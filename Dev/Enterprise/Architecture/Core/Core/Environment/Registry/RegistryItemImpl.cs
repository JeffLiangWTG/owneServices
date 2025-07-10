using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryItemImpl : IRegistryItem, IRegistryItemInternals, IMultilingualRegistryItem, IConnectionAdjustable
	{
		#region Constructors

		public RegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: this(name, category, caption, hint, dataType, storage, RegistryOptions.Default)
		{
		}

		public RegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, dataType, null, storage, options, null, true)
		{
		}

		public RegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, object defaultValue)
			: this(name, category, caption, hint, dataType, null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public RegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
			: this(name, category, caption, hint, dataType, null, storage, options, defaultValue)
		{
		}

		public RegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
			: this(name, category, caption, hint, dataType, editorInfo, storage, options, defaultValue, false)
		{
		}

		public RegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, bool useDefaultDefaultValue)
			: this(name, caption, hint, dataType, editorInfo, storage, options, defaultValue, useDefaultDefaultValue, category)
		{
		}

		public RegistryItemImpl(string name, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, bool useDefaultDefaultValue, params MultilingualString[] categories)
		{
			if (name == null)
			{
				Globals.Message.ShowDeveloperErrorAlways("Name cannot be null", ""); // Developer Error
			}

			this.name = name;
			this.caption = caption;
			this.hint = hint;
			this.dataType = dataType;
			this.categories = categories;
			this.storage = storage;
			this.options = options;
			this.editorInfo = editorInfo;
			CountryFilterPKs = Enumerable.Empty<Guid>();

			this.useDefaultDefaultValue = useDefaultDefaultValue;
			if (!useDefaultDefaultValue)
			{
				CheckValueDataType(defaultValue);
				this.defaultValue = defaultValue;
			}
		}

		#endregion

		#region IConnectionAdjustable

		IDisposable IConnectionAdjustable.SetTemporaryConnection(DbConnection connection)
		{
			tmpConnection = connection;
			return new DisposableAction(() => tmpConnection = null);
		}

		DbConnection tmpConnection;

		#endregion

		#region IRegistryItem

		public string Name
		{
			get { return name; }
			set
			{
				if (value == null)
				{
					Globals.Message.ShowDeveloperErrorAlways("Name cannot be null", ""); // Developer Error
				}
				name = value;
			}
		}

		public MultilingualString CategoryMultilingual
		{
			get { return categories?.FirstOrDefault() ?? (NoResString)""; }
		}

		public string Category
		{
			get { return CategoryMultilingual; }
		}

		public MultilingualString[] CategoriesMultilingual
		{
			get { return categories; }
		}

		public string[] CategoriesUntranslated => categories?.Select(s => s.GetUnresolvedString()).ToArray();

		public string[] Categories => categories?.Select(s => (string)s).ToArray();

		public string Hint
		{
			get { return hint ?? ""; }
		}

		public MultilingualString CaptionMultilingual
		{
			get { return caption ?? (NoResString)""; }
		}

		public string Caption
		{
			get { return CaptionMultilingual; }
		}

		public IRegistryDataType DataType
		{
			get { return dataType; }
			set { dataType = value; }
		}

		public IRegistryEditorInfo EditorInfo
		{
			get
			{
				IRegistryEditorInfo result = editorInfo;

				if (result == null)
				{
					IRegistryDataType dataType = DataType;
					if (dataType.HasDefaultEditorInfo)
					{
						result = dataType.DefaultEditorInfo;
					}
				}
				return result;
			}
			set { editorInfo = value; }
		}

		public RegistryStorageFlags Storage
		{
			get { return storage; }
		}

		public RegistryOptions Options
		{
			get { return options; }
			set { options = value; }
		}

		public bool HasOption(RegistryOptions option)
		{
			return (Options & option) != 0;
		}

		public DepartmentFlags DepartmentsAllowed
		{
			get { return departmentsAllowed; }
			set { departmentsAllowed = value; }
		}

		public bool IsReadOnly
		{
			get { return HasOption(RegistryOptions.IsReadOnly) || IsLockedDown; }
		}

		public bool CanBeExported
		{
			get
			{
				return !(IsReadOnly || Options.HasAnyFlag(RegistryOptions.IsHidden, RegistryOptions.IsOnlyForDevelopers, RegistryOptions.IsOnlyForSupport));
			}
		}

		public bool IsLockedDown
		{
			get
			{
				IEnvironment env = EnvProxy.Instance;
				bool isOnlyEditableBySupportIfHosted = HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted);
				bool isNonEdiSupportLogin = !env.CurrentUser.IsDeveloper;
				return isOnlyEditableBySupportIfHosted && isNonEdiSupportLogin && EnvProxy.IsHostedWithCargowise;
			}
		}

		public object DefaultValue
		{
			get
			{
				if (storage == RegistryStorageFlags.System)
				{
					return GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				else
				{
					IEnvironment env = EnvProxy.Instance;
					var companyPK = env.CurrentCompany?.PK ?? Guid.Empty;
					var branchPK = env.CurrentBranch?.PK ?? Guid.Empty;
					var departmentPK = env.CurrentDepartment?.PK ?? Guid.Empty;
					return GetDefaultValue(companyPK, branchPK, departmentPK);
				}
			}
		}

		public object GetDefaultValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var val = GetDefaultValueCore(companyPK, branchPK, departmentPK);
			if (DataType.IsDefaultValueImmutable)
			{
				return val;
			}
			else
			{
				return DataType.CloneValue(val);
			}
		}

		protected virtual object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return useDefaultDefaultValue ? DataType.DefaultValue : defaultValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public virtual bool IsVisible(Guid companyPk, Guid branchPk, Guid departmentPk, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
		{
			var result = false;

			if (!HasOption(RegistryOptions.IsHidden))
			{
				result = IsVisibleForDepartment(departmentPk, department);

				if (result && CountryFilterPKs.Any())
				{
					if (registryItemVisibility != null)
					{
						result = registryItemVisibility.IsVisible(companyPk, branchPk, CountryFilterPKs);
					}
					else
					{
						// To prevent unit tests from failing if they only create the registry items without forms.
						// Do not expect to enter this condition in business scenarios.
						var collection = RegistryLoadingHelper.GetJoinedCountryCompanyBranchCollection(new BusinessObjectFactory());
						result = RegistryLoadingHelper.IsVisible(companyPk, branchPk, CountryFilterPKs, collection);
					}
				}
			}

			if (HasOption(RegistryOptions.IsOnlyForSupport) && !EnvProxy.Instance.CurrentUser.IsSupportUser)
			{
				result = false;
			}

			return result;
		}

		public string GetValidationErrorMessage(object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue != DBNull.Value)
			{
				CheckValueDataType(proposedValue);
			}

			string message = "";
			try
			{
				DataType.Validate(this, proposedValue, companyPK, branchPK, departmentPK);
			}
			catch (RegistryValidationException e)
			{
				message = e.Message;
			}

			return message;
		}

		public virtual IEnumerable<Guid> CountryFilterPKs
		{
			get;
			set;
		}

		public object Value
		{
			get
			{
				lock (valueLock)
				{
					if (HasOption(RegistryOptions.CannotCallParameterlessValueGetter))
					{
						throw new NotSupportedException(Name + " requires GetValue or GetFallBackValueAtAllLevels to be called - Value is not supported");
					}
					return GetRetriever().GetFallBackValue().Value;
				}
			}
		}

		/// <summary>
		/// Updating the value in the StmData table. If DefaultValue is used as NewValue, it will clear the record from the stmdata.
		/// </summary>
		public void SetValue(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			lock (valueLock)
			{
				SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
			}
		}

		protected virtual void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			CheckStorageFlags(companyOrOwnerPK, branchPK, departmentPK);
			CheckValueDataType(newValue);

			if (newValue != null && (DataType.IsValidatedOnSetEvenIfEqualDefaultValue || !newValue.Equals(GetDefaultValueCore(companyOrOwnerPK, branchPK, departmentPK))))
			{
				DataType.Validate(this, newValue, companyOrOwnerPK, branchPK, departmentPK);
			}

			var isLogged = !HasOption(RegistryOptions.NotLogged);
			var preserveTestValue = HasOption(RegistryOptions.PreserveTestValue);

			Guid ownerPK = RegistryCacheKey.GetOwnerPK(companyOrOwnerPK, branchPK);

			byte[] raw;
			using (Culture.SetTemporarily(Culture.Default))
			{
				raw = DataType.Serialise(newValue);
			}

			Guid guidValue = DataType.GetGuidValue(newValue);

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.SetBinaryValue(Name, ownerPK, departmentPK, raw, guidValue, DataType.Code, isLogged, preserveTestValue);
			}

			if (!HasOption(RegistryOptions.NotCached))
			{
				Cache.Clear();
				var cacheKey = new RegistryCacheKey(Name, companyOrOwnerPK, branchPK, departmentPK);

				if (newValue == null)
				{
					Cache.Add(cacheKey.Key, raw);
				}
				else
				{
					Cache.Add(cacheKey.Key, Deserialise(raw));
				}
			}

			ClearAuditDetailsCache();
		}

		public object GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			lock (valueLock)
			{
				CheckStorageFlags(companyPK, branchPK, departmentPK);
				RegistryCacheKey cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK);
				return GetValue(cacheKey, false);
			}
		}

		public object GetFallBackValueAtAllLevels(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			lock (valueLock)
			{
				RegistryCacheKey cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK, true);
				return GetValue(cacheKey, true);
			}
		}

		public string GetLocationInEnglish()
		{
			return string.Join(" -> ", CategoryMultilingual.GetUnresolvedString().Replace("/", " -> "), caption.GetUnresolvedString());
		}

		public string GetLocation()
		{
			return string.Join(" -> ", Category.Replace("/", " -> "), Caption);
		}

		bool IRegistryItem.IsValueMandatory
		{
			get { return HasOption(RegistryOptions.IsValueMandatory); }
		}

		#endregion

		#region IRegistryItemInternals

		#region Location

		public MultilingualString LocationMultilingual
		{
			get { return GetLocation(CategoryMultilingual); }
		}

		string IRegistryItemInternals.Location
		{
			get { return LocationMultilingual; }
		}

		string IRegistryItemInternals.LocationForCategory(string category)
		{
			string result = null;
			var categories = Categories;
			if (categories != null)
			{
				if (categories.Contains(category))
				{
					result = GetLocation((NoResString)category);
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format("{0}.LocationForCategory('{1}')", Name, category), string.Format("Cannot get the location path for registry '{0}' as it has no category matching '{1}'.", Name, category));
				}
			}
			return result ?? ((IRegistryItemInternals)this).Location;
		}

		MultilingualString GetLocation(MultilingualString category)
		{
			return MultilingualString.Join(" -> ", category.Replace("/", " -> "), CaptionMultilingual);
		}

		#endregion

		void IRegistryItemInternals.CheckConfigurationValid()
		{
			if (DefaultValue == null && !DataType.AllowNull)
			{
				throw new ArgumentException("You can't have null as a default value for registry data type " + DataType.GetType().FullName);
			}
			try
			{
				DataType.Validate(this, DefaultValue, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			catch (RegistryValidationException e)
			{
				throw new ArgumentException("DefaultValue not valid: " + e.Message, e);
			}
			DataType.CheckConfigurationValid();
		}

		RegistryStorageFlags IRegistryItemInternals.GetFallBackLevel(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			int levelResult;
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				levelResult = registryDataAccessor.GetFallBackLevel(Name, companyPK, branchPK, departmentPK);
			}

			var result = RegistryStorageFlags.System;
			switch (levelResult)
			{
				case 6:
					result = RegistryStorageFlags.System;
					break;
				case 5:
					result = RegistryStorageFlags.SystemDepartment;
					break;
				case 4:
					result = RegistryStorageFlags.Company;
					break;
				case 3:
					result = RegistryStorageFlags.CompanyDepartment;
					break;
				case 2:
					result = RegistryStorageFlags.Branch;
					break;
				case 1:
					result = RegistryStorageFlags.BranchDepartment;
					break;
				case -1:
					if ((Storage & RegistryStorageFlags.System) == RegistryStorageFlags.System)
					{
						result = RegistryStorageFlags.System;
					}
					else if ((Storage & RegistryStorageFlags.SystemDepartment) == RegistryStorageFlags.SystemDepartment)
					{
						result = RegistryStorageFlags.SystemDepartment;
					}
					else if ((Storage & RegistryStorageFlags.Company) == RegistryStorageFlags.Company)
					{
						result = RegistryStorageFlags.Company;
					}
					else if ((Storage & RegistryStorageFlags.CompanyDepartment) == RegistryStorageFlags.CompanyDepartment)
					{
						result = RegistryStorageFlags.CompanyDepartment;
					}
					else if ((Storage & RegistryStorageFlags.Branch) == RegistryStorageFlags.Branch)
					{
						result = RegistryStorageFlags.Branch;
					}
					else if ((Storage & RegistryStorageFlags.BranchDepartment) == RegistryStorageFlags.BranchDepartment)
					{
						result = RegistryStorageFlags.BranchDepartment;
					}

					break;
			}

			return result;
		}

		object IRegistryItemInternals.GetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			RegistryCacheKey cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK);
			return ProposedCache[cacheKey.Key];
		}

		void IRegistryItemInternals.SetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK, object value)
		{
			RegistryCacheKey cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK);
			ProposedCache[cacheKey.Key] = value;
		}

		bool IRegistryItemInternals.HasValueForAnyLevel()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				return registryDataAccessor.HasValue(Name);
			}
		}

		bool HasActualValue(RegistryCacheKey cacheKey)
		{
			return GetValueOrBytes(cacheKey, false) != null;
		}

		bool IRegistryItemInternals.HasActualValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			RegistryCacheKey cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK);
			return HasActualValue(cacheKey);
		}

		bool IRegistryItemInternals.HasActualValueAtThisLevelForAnyDepartment(Guid companyPK, Guid branchPK)
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				return registryDataAccessor.HasActualValueAtThisLevelForAnyDepartment(Name, RegistryCacheKey.GetOwnerPK(companyPK, branchPK));
			}
		}

		object IRegistryItemInternals.GetCurrentValueFromProposedValueAccessor(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			RegistryItemProposedValueAccessor retriever = new RegistryItemProposedValueAccessor(
				this, companyPK, branchPK, departmentPK);
			return retriever.GetCurrentValueWithoutFallback(companyPK, branchPK, departmentPK);
		}

		ValueToUse IRegistryItemInternals.GetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			ValueToUse result = ValueToUse.DefaultValue;
			RegistryCacheKey cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK);
			if (CurrentValueToUseCache.Contains(cacheKey.Key))
			{
				result = (ValueToUse)CurrentValueToUseCache[cacheKey.Key];
			}
			else if (HasActualValue(cacheKey))
			{
				result = ValueToUse.SavedValue;
			}

			return result;
		}

		void IRegistryItemInternals.SetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK, ValueToUse value)
		{
			CurrentValueToUseCache[CacheKeyName(companyPK, branchPK, departmentPK)] = value;
		}

		void IRegistryItemInternals.DeleteRecord(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.DeleteRecord(Name, RegistryCacheKey.GetOwnerPK(companyPk, branchPk), departmentPk);
			}

			((IRegistryItemInternals)this).ClearCache();
			ClearAuditDetailsCache();
		}

		void IRegistryItemInternals.DeleteValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			DeleteValueCore(companyPK, branchPK, departmentPK);
		}

		protected virtual void DeleteValueCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.SetBinaryValue(Name, RegistryCacheKey.GetOwnerPK(companyPk, branchPk), departmentPk, null, Guid.Empty, DataType.Code, false, HasOption(RegistryOptions.PreserveTestValue));
			}

			((IRegistryItemInternals)this).ClearCache();
			ClearAuditDetailsCache();
		}

		void IRegistryItemInternals.ClearCache()
		{
			if (cache != null)
			{
				Cache.Clear();
			}
		}

		void IRegistryItemInternals.ClearCache(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (cache != null)
			{
				var cacheKey = new RegistryCacheKey(Name, companyPK, branchPK, departmentPK);
				Cache.Remove(cacheKey.Key);
			}
		}

		void IRegistryItemInternals.ClearProposedCache()
		{
			if (proposedCache != null)
			{
				proposedCache.Clear();
			}
		}

		void IRegistryItemInternals.ClearCurrentValueToUseCache()
		{
			if (currentValueToUseCache != null)
			{
				currentValueToUseCache.Clear();
			}
		}

		Guid IRegistryItemInternals.GetRegistryItemPK(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			return GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL expression")]
		protected virtual Guid GetRegistryItemPKCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			Guid ownerPk = RegistryCacheKey.GetOwnerPK(companyPk, branchPk);

			string ownerWhereSql = " AND " + StmDataSchema.Constants.SD_Owner + (ownerPk == Guid.Empty ? " is null " : " = @SD_Owner ");
			string departmentWhereSql = " AND " + StmDataSchema.Constants.SD_DepartmentGuid + (departmentPk == Guid.Empty ? " is null " : " = @SD_DepartmentGuid ");

			string selectPkSql = "select SD_PK from dbo.StmData where SD_Name = @SD_Name " + ownerWhereSql + departmentWhereSql;

			using (Db.DisposableActionForDbConnection())
			using (DbCommand command = Db.Connection.Command(selectPkSql))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", Name, StmDataSchema.SD_Name);
				if (ownerPk != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@SD_Owner", ownerPk, StmDataSchema.SD_Owner);
				}
				if (departmentPk != Guid.Empty)
				{
					command.AddParameterBasedOnDbColumn("@SD_DepartmentGuid", departmentPk, StmDataSchema.SD_DepartmentGuid);
				}

				object result = command.ExecuteScalar();
				return (result != null && result != DBNull.Value) ? (Guid)result : Guid.Empty;
			}
		}

		public RegistryUpdateActionDelegate BeforeUpdateAction { get; set; }

		public RegistryUpdateActionDelegate OnDeleteAction { get; set; }

		public RegistryUpdateActionDelegate OnUpdateAction { get; set; }

		public Action OnAllValuesSavedAction { get; set; }

		#endregion

		#region Implementation

		bool IsVisibleForDepartment(Guid departmentPK, IGlbDepartment department)
		{
			bool result = (DepartmentsAllowed == DepartmentFlags.All) || (departmentPK == Guid.Empty);

			if (!result)
			{
				bool isAir = department.GE_Air;
				bool isSea = department.GE_Sea;

				bool isImport = department.GE_Import;
				bool isExport = department.GE_Export;

				if (DepartmentsAllowed == DepartmentFlags.Air)
				{
					result = isAir;
				}
				else if (DepartmentsAllowed == DepartmentFlags.ImportAir)
				{
					result = isAir && isImport;
				}
				else if (DepartmentsAllowed == DepartmentFlags.ExportAir)
				{
					result = isAir && isExport;
				}
				else if (DepartmentsAllowed == DepartmentFlags.Sea)
				{
					result = isSea;
				}
				else if (DepartmentsAllowed == DepartmentFlags.ImportSea)
				{
					result = isSea && isImport;
				}
				else if (DepartmentsAllowed == DepartmentFlags.ExportSea)
				{
					result = isSea && isExport;
				}
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
 RegistryItemFallBackValueAccessor GetRetriever()
		{
			RegistryItemFallBackValueAccessor result;
			if (Storage == RegistryStorageFlags.System)
			{
				result = new RegistryItemFallBackValueAccessor(this, RegistryStorageFlags.System, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			else
			{
				var env = EnvProxy.Instance;
				result = new RegistryItemFallBackValueAccessor(this, env.CurrentCompany?.PK ?? Guid.Empty, env.CurrentBranch?.PK ?? Guid.Empty, env.CurrentDepartment?.PK ?? Guid.Empty);
			}
			return result;
		}

		object GetValue(RegistryCacheKey cacheKey, bool useFallback)
		{
			object result = GetValueOrBytes(cacheKey, useFallback);

			if (result == null)
			{
				result = GetDefaultValue(cacheKey.CompanyPK, cacheKey.BranchPK, cacheKey.DepartmentPK);
				if (HasOption(RegistryOptions.CacheExpensiveDefaultValue))
				{
					Cache.Add(cacheKey.Key, result);
				}
			}
			else
			{
				if (!DataType.IsDeserializedDataAlive(result))
				{
					Cache.Remove(cacheKey.Key);
					result = GetValueOrBytes(cacheKey, useFallback);
				}

				if (result != null && DataType.IsNullDataRepresentation(result))
				{
					result = null;
				}
			}

			return result;
		}

#if DEBUG
		public
#endif
 object Deserialise(byte[] data)
		{
			object result = null;
			try
			{
				using (Culture.SetTemporarily(Culture.Default))
				{
					result = DataType.Deserialise(data);
				}
			}
			catch (OutOfMemoryException) //GDI+ generic error, rethrown from ImageRegistryDataType.DeserialiseCore
			{
				if (DataType is ImageRegistryDataType)
				{
					//report error message to user, use name, etc
					Globals.Message.ShowError(Res.GetString("2047f14d-0283-4b28-b41d-96c9f9a0f0b3",
						"Image registry item ({0} - {1}) contains a malformed or corrupt image that is not recognized by GDI+. Try saving the image as a different format or with a different program.", Name, Caption), Res.GetString("342f45c2-b083-49c1-b46e-0c721bb2f9e4", "Error"));
				}
				else
				{
					throw;
				}
			}
			catch (JsonException)
			{
				if (!Globals.CanShowDialogs)
				{
					throw new RegistryJsonException(Res.GetString("a1449f0e-e509-416c-a07d-aa3eb45e68ac", "Invalid JSON Registry!"), Name, Caption);
				}

				Globals.Message.ShowError(Res.GetString("f8db00de-3474-42b4-bc34-96e29383e766",
					"The value for registry item ({0} - {1}) contains invalid JSON. (Maybe you copied data from an old database and it missed the data transformation that converted it from Binary to JSON format.) Fix or delete and re-enter it.",
					Name,
					Caption),
					Res.GetString("342f45c2-b083-49c1-b46e-0c721bb2f9e4",
						"Error"));
			}
			catch (Exception ex) when (!ex.IsCriticalException() && (IsXmlException(ex) || IsInvalidOperationException(ex)))
			{
				if (!Globals.CanShowDialogs)
				{
					if (EnvProxy.Instance.IsProductionSystem)
					{
						ExceptionReporter.Instance.ReportException("RegistryItemXmlException: " + Name + " (" + ex.GetType().Name + ")",// Developer error message
							new Exception(
							@"An Exception occurred while deserializing a registry item value. Check for inconsistent storage formats. This probably means that the XML being deserialised was written under a different schema than that which is now being used to deserialise it; since we read node-by-node, if an extra field is present when we expect to see the closing tag of the document root or the opening tag of another field, we puke; this can happen if you make a schema change to ao complex registry item, save a new value, then undo your changes and try to read the pre-alpha value.\r\n" + // Developer error message
							string.Format("The registry item was '{0}'\r\n", Name) +
							string.Format("The Unicode-string value of the binary data was [{0}] \r\n", Encoding.Unicode.GetString(data)) + // Developer error message
							string.Format("The stored binary value was [{0}]", ByteArrayValue(data)), // Developer error message
							ex));
					}
					throw;
				}
				Globals.Message.ShowError(Res.GetString("f7afbdd7-d499-42e3-a035-8119e671aa67",
					"The value for registry item ({0} - {1}) contains invalid content. (Maybe you copied data from an old database and it missed the data transformation that converted it from Binary to XML format.) Fix or delete and re-enter it.",
					Name,
					Caption),
					Res.GetString("03499c61-8e9e-409c-9366-797c42799958",
						"Error"));
			}
			return result;
		}

		string ByteArrayValue(byte[] data)
		{
			StringBuilder sb = new StringBuilder(data.Length * 4);
			if (data.Length > 0)
			{
				sb.Append(data[0]);
			}
			for (int i = 1; i < data.Length; i++)
			{
				sb.Append(", ").Append(data[i]);
			}
			return sb.ToString();
		}

		bool IsInvalidOperationException(Exception ex)
		{
			return ex is InvalidOperationException || (ex.InnerException != null && IsInvalidOperationException(ex.InnerException));
		}

		bool IsXmlException(Exception ex)
		{
			return ex is XmlException || (ex.InnerException != null && IsXmlException(ex.InnerException));
		}

		object GetValueOrBytes(RegistryCacheKey cacheKey, bool useFallback)
		{
			object result = null;

			if (!HasOption(RegistryOptions.NotCached) && Cache.TryGetValue(cacheKey.Key, out result))
			{
				return result;
			}
			else
			{
				byte[] bytes = GetBinaryValue(Name, cacheKey, useFallback);
				result = bytes != null ? Deserialise(bytes) : bytes;

				if (!HasOption(RegistryOptions.NotCached))
				{
					Cache.Add(cacheKey.Key, result);
				}

				return result;
			}
		}

		byte[] GetBinaryValue(string name, RegistryCacheKey cacheKey, bool useFallback)
		{
			var span = HasOption(RegistryOptions.NotCached) ? TimeSpan.Zero : new TimeSpan(0, 30, 0);
			using (var registryDataAccessor = tmpConnection == null ? RegistryDataAccessor.DisposableInstance : RegistryDataAccessor.CreateDisposableInstance(tmpConnection))
			{
				if (useFallback)
				{
					return registryDataAccessor.GetBinaryValueFallBack(name, span, cacheKey.CompanyPK, cacheKey.BranchPK, cacheKey.DepartmentPK);
				}
				else
				{
					return registryDataAccessor.GetBinaryValue(name, span, cacheKey.OwnerPK, cacheKey.DepartmentPK);
				}
			}
		}

		IDictionary CurrentValueToUseCache
		{
			get
			{
				if (currentValueToUseCache == null)
				{
					lock (propertyLock)
					{
						if (currentValueToUseCache == null)
						{
							currentValueToUseCache = new System.Collections.Specialized.HybridDictionary();
						}
					}
				}
				return currentValueToUseCache;
			}
		}

		IDictionary ProposedCache
		{
			get
			{
				if (proposedCache == null)
				{
					lock (propertyLock)
					{
						if (proposedCache == null)
						{
							proposedCache = new System.Collections.Specialized.HybridDictionary();
						}
					}
				}
				return proposedCache;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		void UpdateCache(RegistryCacheKey cacheKey, object value)
		{
			Cache.Clear();
			Cache.Add(cacheKey.Key, value);
		}

		string CacheKeyName(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return new RegistryCacheKey(Name, companyPK, branchPK, departmentPK).Key;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Error")]
		const string defaultErrorMessage = "The following arguments must be equals to Guid.Empty for this RegistryItem because of Storage constraint:";
		void CheckStorageFlags(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (Storage != RegistryStorageFlags.All)
			{
				string errorMessage = null;

				if (companyPK != Guid.Empty && (Storage & (RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment)) == 0)
				{
					errorMessage = defaultErrorMessage + " CompanyPK";
				}

				if (branchPK != Guid.Empty && (Storage & (RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment)) == 0)
				{
					errorMessage = defaultErrorMessage + " BranchPK";
				}

				if (departmentPK != Guid.Empty && (Storage & (RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.SystemDepartment)) == 0)
				{
					errorMessage = defaultErrorMessage + " DepartmentPK";
				}

				if (errorMessage != null)
				{
					throw new ArgumentException(errorMessage);
				}
			}
		}

#if DEBUG
		internal
#endif
		void CheckValueDataType(object value)
		{
			if (!CheckValueDataTypeCore(value) && (value != null || !DataType.AllowNull))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
					"Input value data type is incorrect, it must be {0}. Name = {1}, Category = {2}, Caption = {3}, DataType = {4}, DataType.DataType = {5}, Value = {6}, Value.GetType() = {7}.",
					DataType.DataType.FullName, Name, Category, Caption, DataType.GetType(), DataType.DataType, value, value.GetType()));
			}
		}

		protected virtual bool CheckValueDataTypeCore(object value)
		{
			return DataType.DataType.IsInstanceOfType(value);
		}

		readonly object propertyLock = new object();
		readonly object valueLock = new object();
		readonly object cacheLock = new object();
		readonly object auditLock = new object();

		string name = "";
		readonly MultilingualString[] categories;
		readonly MultilingualString hint;
		readonly MultilingualString caption;
		IRegistryDataType dataType;
		IRegistryEditorInfo editorInfo;
		readonly RegistryStorageFlags storage;
		RegistryOptions options;
		DepartmentFlags departmentsAllowed = DepartmentFlags.All;
		readonly object defaultValue;
		readonly bool useDefaultDefaultValue;
		IDictionary currentValueToUseCache;

		ExpiringConcurrentDictionary<string, object> Cache
		{
			get
			{
				if (cache == null)
				{
					lock (cacheLock)
					{
						if (cache == null)
						{
							cache = new ExpiringConcurrentDictionary<string, object>(TimeSpan.FromSeconds((double)RegistryRefresh.FrequencyInSeconds));
						}
					}
				}
				return cache;
			}
		}
		ExpiringConcurrentDictionary<string, object> cache;

		IDictionary proposedCache;

		#endregion

		#region IAuditDetails members

		public ZDateTime SystemCreateTimeUtc => new ZDateTime(GetAuditColumnValue(StmDataSchema.Constants.SD_SystemCreateTimeUtc), DateTimeKind.Utc);

		public ZString SystemCreateUser => new ZString(GetAuditColumnValue(StmDataSchema.Constants.SD_SystemCreateUser));

		public ZDateTime SystemLastEditTimeUtc => new ZDateTime(GetAuditColumnValue(StmDataSchema.Constants.SD_SystemLastEditTimeUtc), DateTimeKind.Utc);

		public ZString SystemLastEditUser => new ZString(GetAuditColumnValue(StmDataSchema.Constants.SD_SystemLastEditUser));

		object GetAuditColumnValue(string columnName)
		{
			lock (auditLock)
			{
				RegistryAuditColumnCacheKey auditColumnCacheKey;

				var companyPk = EnvProxy.Instance.CurrentCompany?.PK ?? Guid.Empty;
				var branchPk = EnvProxy.Instance.CurrentBranch?.PK ?? Guid.Empty;
				var departmentPk = EnvProxy.Instance.CurrentDepartment?.PK ?? Guid.Empty;

				var level = ((IRegistryItemInternals)this).GetFallBackLevel(companyPk, branchPk, departmentPk);
				switch (level)
				{
					case RegistryStorageFlags.BranchDepartment:
						auditColumnCacheKey = new RegistryAuditColumnCacheKey(Name, columnName, Guid.Empty, branchPk, departmentPk);
						break;
					case RegistryStorageFlags.Branch:
						auditColumnCacheKey = new RegistryAuditColumnCacheKey(Name, columnName, Guid.Empty, branchPk, Guid.Empty);
						break;
					case RegistryStorageFlags.CompanyDepartment:
						auditColumnCacheKey = new RegistryAuditColumnCacheKey(Name, columnName, companyPk, Guid.Empty, departmentPk);
						break;
					case RegistryStorageFlags.Company:
						auditColumnCacheKey = new RegistryAuditColumnCacheKey(Name, columnName, companyPk, Guid.Empty, Guid.Empty);
						break;
					case RegistryStorageFlags.SystemDepartment:
						auditColumnCacheKey = new RegistryAuditColumnCacheKey(Name, columnName, Guid.Empty, Guid.Empty, departmentPk);
						break;
					default:
						auditColumnCacheKey = new RegistryAuditColumnCacheKey(Name, columnName, Guid.Empty, Guid.Empty, Guid.Empty);
						break;
				}

				if (AuditDetailsCache.Contains(auditColumnCacheKey.Key))
				{
					return AuditDetailsCache[auditColumnCacheKey.Key];
				}

				using (var registryDataAccessor = tmpConnection == null ? RegistryDataAccessor.DisposableInstance : RegistryDataAccessor.CreateDisposableInstance(tmpConnection))
				{
					var result = registryDataAccessor.GetAuditColumnValue(name, auditColumnCacheKey.AuditColumnName, auditColumnCacheKey.OwnerPK, auditColumnCacheKey.DepartmentPK);
					AuditDetailsCache[auditColumnCacheKey.Key] = result;
					return result;
				}
			}
		}

		void ClearAuditDetailsCache()
		{
			lock (auditLock)
			{
				auditDetailsCache?.Clear();
			}
		}

		IDictionary AuditDetailsCache
		{
			get
			{
				if (auditDetailsCache == null)
				{
					lock (auditLock)
					{
						if (auditDetailsCache == null)
						{
							auditDetailsCache = new System.Collections.Specialized.HybridDictionary();
						}
					}
				}
				return auditDetailsCache;
			}
		}

		IDictionary auditDetailsCache;

		#endregion
	}

	public interface IConnectionAdjustable
	{
		IDisposable SetTemporaryConnection(DbConnection connection);
	}
}
