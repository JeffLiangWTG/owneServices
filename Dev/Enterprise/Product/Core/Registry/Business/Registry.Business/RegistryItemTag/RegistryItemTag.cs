using System;
using System.Linq;
using System.Net.NetworkInformation;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class RegistryItemTag
	{
		public RegistryItemTag(IRegistryItem item)
		{
			ItemHash = new RegistryItemTagHashtables();
			RegistryItem = item;
		}

		public void SetFallback(FallbackLevel fallback)
		{
			CompanyPK = fallback.CompanyPK(true);
			BranchPK = fallback.BranchPK;
			DepartmentPK = fallback.DepartmentPK;
		}

		public virtual IPGlobalProperties GetIPGlobalProperties()
		{
			return IPGlobalProperties.GetIPGlobalProperties();
		}

		public virtual void ClearAll()
		{
			RegistryItemInternals.ClearProposedCache();
			RegistryItemInternals.ClearCurrentValueToUseCache();

			ItemHash.ClearIsChangedHashtable();
			ItemHash.ClearIsInErrorHashtable();
		}

		#region Validation

		public bool GetIsInErrorCustomPK(FallbackLevel fallback)
		{
			return ItemHash.GetIsInError(fallback.CompanyPK(true), fallback.BranchPK, fallback.DepartmentPK);
		}

		public bool ShouldValidate(bool isValueOverridden = false)
		{
			return (IsRegistryTemplate && isValueOverridden) || (HasValue && NewValue != null) || (!isValueOverridden && DataType.IsValidatedOnSetEvenIfEqualDefaultValue);
		}

		bool IsRegistryTemplate
		{
			get { return DataType.DataType.IsSubclassOf(typeof(RegistryBusinessObjectTemplate)) || DataType.DataType.IsSubclassOf(typeof(RegistryBusinessObjectCollectionTemplate)); }
		}

		public void ValidateItem(bool isPreSaveValidation = false)
		{
			var proposedValue = HasValue ? NewValue : DefaultValue;
			if (isPreSaveValidation)
			{
				DataType.ValidateBeforeRegistryFormSave(RegistryItem, proposedValue, CompanyPK, BranchPK, DepartmentPK);
			}
			else
			{
				DataType.Validate(RegistryItem, proposedValue, CompanyPK, BranchPK, DepartmentPK);
			}
		}

		public bool CurrentFallbackIsInError
		{
			get { return ItemHash.GetIsInError(CompanyPK, BranchPK, DepartmentPK); }
			set { ItemHash.SetIsInError(CompanyPK, BranchPK, DepartmentPK, value); }
		}

		public bool AnyFallbackHasError
		{
			get { return ItemHash.HasAnyError(); }
		}

		#endregion

		#region Getting/Saving Value

		public object GetValue()
		{
			return (!IsChanged) ? RegistryItem.GetValueWithoutFallback(CompanyPK, BranchPK, DepartmentPK) : NewValue;
		}

		public object NewValue
		{
			get
			{
				var result = RegistryItemInternals.GetProposedValue(CompanyPK, BranchPK, DepartmentPK);
				if (result == null && IsRegistryTemplate)
				{
					result = RegistryItem.GetValueWithoutFallback(CompanyPK, BranchPK, DepartmentPK); // changes and validation are on the template object
				}
				return result;
			}
			set { RegistryItemInternals.SetProposedValue(CompanyPK, BranchPK, DepartmentPK, value); }
		}

		public bool HasValue
		{
			get { return RegistryItemInternals.GetCurrentValueToUse(CompanyPK, BranchPK, DepartmentPK) != ValueToUse.DefaultValue; }
			set
			{
				var value1 = (value) ? ValueToUse.ProposedValue : ValueToUse.DefaultValue;
				RegistryItemInternals.SetCurrentValueToUse(CompanyPK, BranchPK, DepartmentPK, value1);
			}
		}

		public void SaveAllValues()
		{
			Db.Connection.RunTransactioned(() =>
			{
				foreach (var key in ItemHash.GetIsChangedKeys().Cast<string>().ToArray())
				{
					var keyArray = key.Split('+');
					Guid companyGuid = (Guid)System.Data.SqlTypes.SqlGuid.Parse(keyArray[0]);
					Guid branchGuid = (Guid)System.Data.SqlTypes.SqlGuid.Parse(keyArray[1]);
					Guid departmentGuid = (Guid)System.Data.SqlTypes.SqlGuid.Parse(keyArray[2]);

					var wrapper = RegistryItem as RegistryItemWrapper;
					object originalValue = null;
					object newValue = null;

					if (SystemDataRegistry.Instance.EnableEnhancedLogging.Value)
					{
						RegistryItemUpdateLoggerFactory.BindLogger(wrapper);
					}

					var needLoadOriginalValue = wrapper?.OnBuildLogReference != null || RegistryItemInternals.BeforeUpdateAction != null || RegistryItemInternals.OnDeleteAction != null;
					if (needLoadOriginalValue)
					{
						originalValue = RegistryItem.GetFallBackValueAtAllLevels(companyGuid, branchGuid, departmentGuid);
					}

					RegistryItemInternals.BeforeUpdate(companyGuid, branchGuid, departmentGuid, originalValue);

					Event eventCode;
					if (RegistryItemInternals.GetCurrentValueToUse(companyGuid, branchGuid, departmentGuid) == ValueToUse.ProposedValue)
					{
						newValue = RegistryItemInternals.GetProposedValue(companyGuid, branchGuid, departmentGuid);
						RegistryItem.SetValue(companyGuid, branchGuid, departmentGuid, newValue);
						eventCode = Events.EditedARecord;
					}
					else
					{
						RegistryItemInternals.OnDelete(companyGuid, branchGuid, departmentGuid, originalValue);

						newValue = new RegistryItemProposedValueAccessor(RegistryItem, new FallbackLevel(companyGuid, branchGuid, departmentGuid)).GetCurrentValue().Value;
						RegistryItemInternals.DeleteValue(companyGuid, branchGuid, departmentGuid);
						eventCode = Events.ResetEntryMessageItemFunction;
					}

					RegistryItemInternals.OnUpdate(companyGuid, branchGuid, departmentGuid, newValue); // On any update to the registry item value, call the hooked action to carry out the corresponding post-processing

					AddLog(RegistryItemInternals.GetRegistryItemPK(companyGuid, branchGuid, departmentGuid), eventCode, originalValue, needLoadOriginalValue ? newValue : null);
				}

				RegistryItemInternals.OnAllValuesSaved();

				ClearAll();
			});
		}

		#region Logging

		void AddLog(Guid registryItemPk, Event eventCode, object originalValue, object newValue)
		{
			if (registryItemPk != Guid.Empty)
			{
				var factory = new BusinessObjectFactory();
				var parent = factory.Load<StmData>(registryItemPk);

				ZString logRef = ZString.Empty;

				var wrapper = RegistryItem as RegistryItemWrapper;
				if (wrapper != null && wrapper.OnBuildLogReference != null && SystemDataRegistry.Instance.EnableEnhancedLogging.Value)
				{
					var args = new RegistryItemWrapper.BuildLogReferenceArgs(RegistryItem, originalValue, newValue);
					logRef = wrapper.OnBuildLogReference(args);
				}

				var logs = parent.GetLogs();

				if (logRef.IsEmpty)
				{
					logs.AddNew(eventCode);
				}
				else
				{
					foreach (var line in logRef.Split('\n'))
					{
						var trimmed = line.Trim();
						if (!trimmed.IsEmpty)
						{
							logs.AddNew(eventCode, trimmed);
						}
					}
				}

				factory.Save();
			}
		}

		#endregion

		public bool IsChanged
		{
			get { return ItemHash.GetIsChanged(CompanyPK, BranchPK, DepartmentPK); }
			set { ItemHash.SetIsChanged(CompanyPK, BranchPK, DepartmentPK, value); }
		}

		public ZGuid GetRegistryItemPK()
		{
			return new ZGuid(RegistryItemInternals.GetRegistryItemPK(CompanyPK, BranchPK, DepartmentPK));
		}

		#endregion

		#region Exposed Registry Item Properties

		public RegistryStorageFlags Storage
		{
			get { return RegistryItem.Storage; }
		}

		public string Hint
		{
			get { return RegistryItem.Hint; }
		}

		public string Category
		{
			get { return RegistryItem.Category; }
		}

		public string Caption
		{
			get { return RegistryItem.Caption; }
		}

		public IRegistryDataType DataType
		{
			get { return RegistryItem.DataType; }
		}

		public IRegistryEditorInfo EditorInfo
		{
			get { return RegistryItem.EditorInfo; }
		}

		public bool IsReadOnly
		{
			get { return RegistryItem.IsReadOnly; }
		}

		public bool HasReadOnlyOption
		{
			get { return RegistryItem.HasOption(RegistryOptions.IsReadOnly); }
		}

		public bool IsLockedDown
		{
			get { return RegistryItem.IsLockedDown; }
		}

		public object DefaultValue
		{
			get { return RegistryItemInternals.GetDefaultValue(CompanyPK, BranchPK, DepartmentPK); }
		}

		public bool MustOverrideDefaultValue
		{
			get
			{
				bool mustOverrideDefault;

				if (CompanyPK != Guid.Empty && BranchPK == Guid.Empty && DepartmentPK == Guid.Empty) // In the fallback level of company
				{
					mustOverrideDefault = !RegistryItem.HasOption(RegistryOptions.NotMustOverrideDefaultValueForCompanies) && RegistryItem.HasOption(RegistryOptions.MustOverrideDefaultValue);
				}
				else
				{
					mustOverrideDefault = RegistryItem.HasOption(RegistryOptions.MustOverrideDefaultValue);
				}

				return mustOverrideDefault;
			}
		}

		public ZString Name
		{
			get { return RegistryItem.Name; }
		}

		public ZString LastValidationErrorMessage { get; set; }

		#endregion

		IRegistryItemInternals RegistryItemInternals
		{
			get { return (IRegistryItemInternals)RegistryItem; }
		}

		protected Guid CompanyPK;
		protected Guid BranchPK;
		protected Guid DepartmentPK;
		readonly RegistryItemTagHashtables ItemHash;
		public readonly IRegistryItem RegistryItem;
	}
}
