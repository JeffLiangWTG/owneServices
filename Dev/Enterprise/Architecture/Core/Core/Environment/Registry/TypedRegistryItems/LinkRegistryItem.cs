using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public sealed class LinkRegistryItem : IRegistryItemInternals, IMultilingualRegistryItem
	{
		public LinkRegistryItem(MultilingualString category, MultilingualString caption, MultilingualString buttonCaption, MultilingualString hint, ModuleIdentifier moduleID)
		{
			this.category = category;
			this.caption = caption;
			this.hint = hint;
			this.moduleID = moduleID;
			this.buttonCaption = buttonCaption;
		}

		public LinkRegistryItem(MultilingualString category, MultilingualString caption, MultilingualString hint, ModuleIdentifier moduleID)
			: this(category, caption, MultilingualString.Join(" ", ResString.GetMultilingualString("07caede1-4eeb-4e85-81f8-ab50984c64dc", "Edit"), caption), hint, moduleID)
		{
		}

		public string ButtonCaption
		{
			get { return buttonCaption ?? ""; }
		}

		public string ModuleName
		{
			get { return caption ?? ""; }
		}

		public ModuleIdentifier ModuleID
		{
			get { return moduleID; }
		}

		public string Hint
		{
			get { return hint ?? ""; }
		}
		readonly MultilingualString category;
		readonly MultilingualString caption;
		readonly MultilingualString hint;
		readonly MultilingualString buttonCaption;
		readonly ModuleIdentifier moduleID;
		public const string Name = "LINK_REGISTRY_ITEM";

		#region IMultilingualRegistryItem Members

		MultilingualString IMultilingualRegistryItem.CaptionMultilingual => caption;

		MultilingualString[] IMultilingualRegistryItem.CategoriesMultilingual => new MultilingualString[] { category };

		MultilingualString IMultilingualRegistryItem.CategoryMultilingual => category;

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		MultilingualString IMultilingualRegistryItem.LocationMultilingual => throw new NotImplementedException("The method or operation is not implemented.");

		#endregion

		#region IRegistryItemInternals Members

		void IRegistryItemInternals.CheckConfigurationValid()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IRegistryItemInternals.ClearCache()
		{
		}

		void IRegistryItemInternals.ClearCache(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IRegistryItemInternals.ClearCurrentValueToUseCache()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		Guid IRegistryItemInternals.GetRegistryItemPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return Guid.Empty;
		}

		void IRegistryItemInternals.ClearProposedCache()
		{
		}

		void IRegistryItemInternals.DeleteRecord(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IRegistryItemInternals.DeleteValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		object IRegistryItemInternals.GetCurrentValueFromProposedValueAccessor(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		ValueToUse IRegistryItemInternals.GetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		object IRegistryItemInternals.GetDefaultValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		RegistryStorageFlags IRegistryItemInternals.GetFallBackLevel(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		object IRegistryItemInternals.GetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool IRegistryItemInternals.HasActualValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool IRegistryItemInternals.HasActualValueAtThisLevelForAnyDepartment(Guid companyPK, Guid branchPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool IRegistryItemInternals.HasValueForAnyLevel()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string IRegistryItemInternals.Location
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string IRegistryItemInternals.LocationForCategory(string category)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IRegistryItemInternals.SetCurrentValueToUse(Guid companyPK, Guid branchPK, Guid departmentPK, ValueToUse value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IRegistryItemInternals.SetProposedValue(Guid companyPK, Guid branchPK, Guid departmentPK, object value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public RegistryUpdateActionDelegate BeforeUpdateAction { get; set; }

		public RegistryUpdateActionDelegate OnDeleteAction { get; set; }

		public RegistryUpdateActionDelegate OnUpdateAction { get; set; }

		public Action OnAllValuesSavedAction { get; set; }

		#endregion

		#region IRegistryItem Members

		string IRegistryItem.Caption
		{
			get { return caption ?? ""; }
		}

		string[] IRegistryItem.CategoriesUntranslated
		{
			get { return new string[] { category?.GetUnresolvedString() ?? "" }; }
		}

		string[] IRegistryItem.Categories
		{
			get { return new string[] { category ?? "" }; }
		}

		string IRegistryItem.Category
		{
			get { return category ?? ""; }
		}

		IEnumerable<Guid> IRegistryItem.CountryFilterPKs
		{
			get { return Enumerable.Empty<Guid>(); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		IRegistryDataType IRegistryItem.DataType
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		public bool CanBeExported { get { return false; } }

		object IRegistryItem.DefaultValue
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		DepartmentFlags IRegistryItem.DepartmentsAllowed
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		IRegistryEditorInfo IRegistryItem.EditorInfo
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		object IRegistryItem.GetFallBackValueAtAllLevels(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		string IRegistryItem.GetValidationErrorMessage(object proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		object IRegistryItem.GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			throw new NotImplementedException();
		}

		bool IRegistryItem.HasOption(RegistryOptions option)
		{
			return false;
		}

		string IRegistryItem.Hint
		{
			get { return hint ?? ""; }
		}

		bool IRegistryItem.IsReadOnly
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool IRegistryItem.IsLockedDown
		{
			get { return false; }
		}

		bool IRegistryItem.IsValueMandatory
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool IRegistryItem.IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		string IRegistryItem.Name
		{
			get { return Name; }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		RegistryOptions IRegistryItem.Options
		{
			get { throw new NotImplementedException("The method or operation is not implemented."); }
			set { throw new NotImplementedException("The method or operation is not implemented."); }
		}

		void IRegistryItem.SetValue(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		RegistryStorageFlags IRegistryItem.Storage
		{
			get { return RegistryStorageFlags.System; }
		}

		object IRegistryItem.Value
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public string GetLocationInEnglish() => throw new NotImplementedException();

		public string GetLocation() => throw new NotImplementedException();

		#endregion

		#region IAuditDetails members

		public ZDateTime SystemCreateTimeUtc
		{
			get { throw new NotImplementedException(); }
		}

		public ZString SystemCreateUser
		{
			get { throw new NotImplementedException(); }
		}

		public ZDateTime SystemLastEditTimeUtc
		{
			get { throw new NotImplementedException(); }
		}

		public ZString SystemLastEditUser
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
