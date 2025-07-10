using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class RegistryItemWrapper : IRegistryItem, IRegistryItemInternals, IMultilingualRegistryItem
	{
		protected RegistryItemWrapper(IRegistryItem inner)
		{
			Inner = (IRegistryItemInternals)inner;
		}

		public readonly IRegistryItemInternals Inner;

		#region BuildLogReference

		public sealed class BuildLogReferenceArgs
		{
			public BuildLogReferenceArgs(IRegistryItem registryItem, object originalValue, object newValue)
			{
				RegistryItem = registryItem;
				OriginalValue = originalValue;
				NewValue = newValue;
			}

			public readonly IRegistryItem RegistryItem;
			public readonly object OriginalValue;
			public readonly object NewValue;
		}

		public delegate string BuildLogReferenceHandler(BuildLogReferenceArgs args);
		public BuildLogReferenceHandler OnBuildLogReference;

		#endregion

		public override string ToString()
		{
#if DEBUG
			NotSupported();
#endif
			return base.ToString();
		}

#if DEBUG
		static void NotSupported()
		{
			throw new NotSupportedException("Do not call this method - use .Value instead");
		}
#endif

		#region IRegistryItem Members

		public string Name
		{
			get { return Inner.Name; }
			set { Inner.Name = value; }
		}

		public string Category
		{
			get { return Inner.Category; }
		}

		public string[] Categories
		{
			get { return Inner.Categories; }
		}

		public string[] CategoriesUntranslated
		{
			get { return Inner.CategoriesUntranslated; }
		}

		public string Hint
		{
			get { return Inner.Hint; }
		}

		public string Caption
		{
			get { return Inner.Caption; }
		}

		public IRegistryDataType DataType
		{
			get { return Inner.DataType; }
			set { Inner.DataType = value; }
		}

		public IRegistryEditorInfo EditorInfo
		{
			get { return Inner.EditorInfo; }
			set { Inner.EditorInfo = value; }
		}

		public RegistryStorageFlags Storage
		{
			get { return Inner.Storage; }
		}

		public RegistryOptions Options
		{
			get { return Inner.Options; }
			set { Inner.Options = value; }
		}

		public bool HasOption(RegistryOptions option)
		{
			return Inner.HasOption(option);
		}

		public DepartmentFlags DepartmentsAllowed
		{
			get { return Inner.DepartmentsAllowed; }
			set { Inner.DepartmentsAllowed = value; }
		}

		public bool IsReadOnly
		{
			get { return Inner.IsReadOnly; }
		}

		public bool IsLockedDown
		{
			get { return Inner.IsLockedDown; }
		}

		public bool CanBeExported
		{
			get { return Inner.CanBeExported; }
		}

		public virtual object DefaultValue
		{
			get { return Inner.DefaultValue; }
		}

		public bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department = null, IRegistryItemVisibility registryItemVisibility = null)
		{
			return Inner.IsVisible(companyPK, branchPK, departmentPK, department, registryItemVisibility);
		}

		public string GetValidationErrorMessage(object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetValidationErrorMessage(proposedValue, companyPK, branchPK, departmentPK);
		}

		public IEnumerable<Guid> CountryFilterPKs
		{
			get { return Inner.CountryFilterPKs; }
			set { Inner.CountryFilterPKs = value; }
		}

		public object GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);
		}

		protected virtual object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
		}

		void IRegistryItem.SetValue(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		protected virtual void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			Inner.SetValue(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		#region Temporary

		public virtual object GetFallBackValueAtAllLevels(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		#endregion

		object IRegistryItem.Value
		{
			get { return ValueCore; }
		}

		protected virtual object ValueCore
		{
			get { return Inner.Value; }
		}

		bool IRegistryItem.IsValueMandatory
		{
			get { return Inner.IsValueMandatory; }
		}

		#endregion

		#region IRegistryItemInternals

		Guid IRegistryItemInternals.GetRegistryItemPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetRegistryItemPK(companyPK, branchPK, departmentPK);
		}

		string IRegistryItemInternals.Location
		{
			get { return Inner.Location; }
		}

		string IRegistryItemInternals.LocationForCategory(string category)
		{
			return Inner.LocationForCategory(category);
		}

		object IRegistryItemInternals.GetDefaultValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetDefaultValue(companyPK, branchPK, departmentPK);
		}

		object IRegistryItemInternals.GetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetProposedValue(companyPK, branchPK, departmentPK);
		}

		void IRegistryItemInternals.SetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK, object value)
		{
			Inner.SetProposedValue(companyPK, branchPK, departmentPK, value);
		}

		RegistryStorageFlags IRegistryItemInternals.GetFallBackLevel(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetFallBackLevel(companyPK, branchPK, departmentPK);
		}

		bool IRegistryItemInternals.HasActualValueAtThisLevelForAnyDepartment(Guid companyPK, Guid branchPK)
		{
			return Inner.HasActualValueAtThisLevelForAnyDepartment(companyPK, branchPK);
		}

		object IRegistryItemInternals.GetCurrentValueFromProposedValueAccessor(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetCurrentValueFromProposedValueAccessor(companyPK, branchPK, departmentPK);
		}

		ValueToUse IRegistryItemInternals.GetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.GetCurrentValueToUse(companyPK, branchPK, departmentPK);
		}

		void IRegistryItemInternals.SetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK, ValueToUse value)
		{
			Inner.SetCurrentValueToUse(companyPK, branchPK, departmentPK, value);
			OnOverrideDefaultChanged(companyPK, branchPK, departmentPK, value == ValueToUse.ProposedValue);
		}

		bool IRegistryItemInternals.HasValueForAnyLevel()
		{
			return Inner.HasValueForAnyLevel();
		}

		bool IRegistryItemInternals.HasActualValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Inner.HasActualValue(companyPK, branchPK, departmentPK);
		}

		void IRegistryItemInternals.DeleteRecord(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			Inner.DeleteRecord(companyPK, branchPK, departmentPK);
		}

		void IRegistryItemInternals.DeleteValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			DeleteValueCore(companyPK, branchPK, departmentPK);
		}

		protected virtual void DeleteValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			Inner.DeleteValue(companyPK, branchPK, departmentPK);
		}

		void IRegistryItemInternals.ClearCache()
		{
			Inner.ClearCache();
		}

		void IRegistryItemInternals.ClearCache(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			Inner.ClearCache(companyPK, branchPK, departmentPK);
		}

		void IRegistryItemInternals.ClearProposedCache()
		{
			Inner.ClearProposedCache();
		}

		void IRegistryItemInternals.ClearCurrentValueToUseCache()
		{
			Inner.ClearCurrentValueToUseCache();
		}

		void IRegistryItemInternals.CheckConfigurationValid()
		{
			Inner.CheckConfigurationValid();
		}

		public RegistryUpdateActionDelegate BeforeUpdateAction { get; set; }

		public RegistryUpdateActionDelegate OnDeleteAction { get; set; }

		public RegistryUpdateActionDelegate OnUpdateAction { get; set; }

		public Action OnAllValuesSavedAction { get; set; }

		#endregion

		protected virtual void OnOverrideDefaultChanged(Guid companyPK, Guid branchPK, Guid departmentPK, bool state)
		{
		}

#if DEBUG

		public IDisposable SetTemporaryValue(Guid companyPk, Guid branchPk, Guid departmentPk, object temporaryValue)
		{
			if (!Globals.IsTest && !Globals.InTransactionedTestCase)
			{
				throw new InvalidOperationException("SetTemporaryValue can only be used in a test case");
			}

			var previousValue = GetValueWithoutFallback(companyPk, branchPk, departmentPk);
			SetValueCore(companyPk, branchPk, departmentPk, temporaryValue);

			DisposableAction action = null;

			action = new DisposableAction(delegate
			{
				SetValueCore(companyPk, branchPk, departmentPk, previousValue);
				DisposableLeakListener.Instance.UnRegisterDisposable(action);
			});

			if (!Globals.InTransactionedTestCase) // A transaction test case will roll back any changes made to the registry.
			{
				DisposableLeakListener.Instance.RegisterDisposable(action);
			}

			return action;
		}

#endif

		public string GetLocationInEnglish() => Inner.GetLocationInEnglish();

		public string GetLocation() => Inner.GetLocation();

		MultilingualString IMultilingualRegistryItem.CaptionMultilingual
		{
			get { return ((IMultilingualRegistryItem)Inner).CaptionMultilingual; }
		}

		MultilingualString[] IMultilingualRegistryItem.CategoriesMultilingual
		{
			get { return ((IMultilingualRegistryItem)Inner).CategoriesMultilingual; }
		}

		MultilingualString IMultilingualRegistryItem.CategoryMultilingual
		{
			get { return ((IMultilingualRegistryItem)Inner).CategoryMultilingual; }
		}

		MultilingualString IMultilingualRegistryItem.LocationMultilingual
		{
			get { return ((IMultilingualRegistryItem)Inner).LocationMultilingual; }
		}

		/// <summary>
		/// Set this to exclude your item from being flagged by the test that warns you when you combine RegistryOptions.NotCached with one of the RegistryOption.{Is for cargowise/ediSupport/dev only} options
		/// </summary>
		public bool IsExcludedFromCwOnlyNonCachedTest;

		#region IAuditDetails members

		public ZDateTime SystemCreateTimeUtc => Inner.SystemCreateTimeUtc;

		public ZString SystemCreateUser => Inner.SystemCreateUser;

		public ZDateTime SystemLastEditTimeUtc => Inner.SystemLastEditTimeUtc;

		public ZString SystemLastEditUser => Inner.SystemLastEditUser;

		#endregion
	}
}
