using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DropDownCodeDescriptionBoolRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<DropDownCodeDescriptionBoolCollection, DropDownCodeDescriptionBoolCollection>
	{
		public DropDownCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString boolColumnCaption, DropDownCodeDescriptionBoolCollection defaultValue, int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList descriptionLookup)
			: this(name, category, caption, hint, storage, new DropDownCodeDescriptionBoolRegistryEditorInfo(boolColumnCaption), defaultValue, codeMaxLength, codeLookup, descriptionLookup)
		{
		}
		public DropDownCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DropDownCodeDescriptionBoolRegistryEditorInfo editorInfo, DropDownCodeDescriptionBoolCollection defaultValue, int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList descriptionLookup)
			: this(new RegistryItemImpl(name, category, caption, hint, new DropDownCodeDescriptionBoolRegistryDataType(codeMaxLength, codeLookup, descriptionLookup), storage, defaultValue), editorInfo)
		{
		}

		public DropDownCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DropDownCodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter, int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList descriptionLookup)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new DropDownCodeDescriptionBoolRegistryDataType(codeMaxLength, codeLookup, descriptionLookup), storage, defaultValueGetter), editorInfo)
		{
		}

		public DropDownCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DropDownCodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter, int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList descriptionLookup)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new DropDownCodeDescriptionBoolRegistryDataType(codeMaxLength, codeLookup, descriptionLookup), storage, options, defaultValueGetter), editorInfo)
		{
		}

		public DropDownCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DropDownCodeDescriptionBoolRegistryEditorInfo editorInfo, DropDownCodeDescriptionBoolCollection defaultValue, int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList descriptionLookup)
			: this(new RegistryItemImpl(name, category, caption, hint, new DropDownCodeDescriptionBoolRegistryDataType(codeMaxLength, codeLookup, descriptionLookup), storage, options, defaultValue), editorInfo)
		{
		}

		protected DropDownCodeDescriptionBoolRegistryItem(IRegistryItem item, IRegistryEditorInfo editorInfo)
			: base(item, editorInfo)
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	public class DropDownCodeDescriptionBoolRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DropDownCodeDescriptionBoolCollection>
	{
		public DropDownCodeDescriptionBoolRegistryDataType(int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList decriptionLookup)
		{
			this.codeMaxLength = codeMaxLength;
			this.codeLookup = codeLookup;
			this.decriptionLookup = decriptionLookup;
		}

		readonly int codeMaxLength;
		public readonly ReadOnlyCodeDescriptionPairList codeLookup;
		public readonly ReadOnlyCodeDescriptionPairList decriptionLookup;

		protected override DropDownCodeDescriptionBoolCollection DeserialiseCore(byte[] value)
		{
			var collection = base.DeserialiseCore(value);
			SetupCollection(collection);
			return collection;
		}

		void SetupCollection(DropDownCodeDescriptionBoolCollection collection)
		{
			collection.CodeMaxLength = codeMaxLength;
			collection.CodeLookup = codeLookup;
			collection.DescriptionLookup = decriptionLookup;
		}

		protected override void ValidateCore(IRegistryItem registryItem, DropDownCodeDescriptionBoolCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue != null)
			{
				if (proposedValue.Count > 3)
				{
					throw new RegistryValidationException(Res.GetString("8A5FEE6B-D510-4DD7-88DD-2101CA247AB9", "There are {0} values. The number of values cannot exceed 3.", proposedValue.Count));
				}

				var pattern = @"MANAGERSECURITY[1|2|3]";
				var regexp = new System.Text.RegularExpressions.Regex(pattern);
				var reportingRoles = SystemDataRegistry.Instance.StaffReportingRoles.Value.ToArray<StaffReportingRole>().Select(x => x.Code);

				for (var i = 0; i < proposedValue.Count; i++)
				{
					if (proposedValue[i].Description.ToString().Length != 3)
					{
						throw new RegistryValidationException(Res.GetString("04BD2123-E5C8-4F62-8870-78930757AC68", "Description '{0}' is wrong.\r\nDescription must be a 3 letter code that matches a value in Staff Reporting Roles.", proposedValue[i].Description));
					}

					if (!regexp.IsMatch(proposedValue[i].Code))
					{
						throw new RegistryValidationException(Res.GetString("7BAD0347-A273-4C1A-83FB-C39AC2C4C1F3", "Manager Security type '{0}' is wrong.\r\nManager Security type must be in the format '{1}' where X is 1, 2, or 3.", proposedValue[i].Code, "MANAGERSECURITYX"));
					}

					if (!reportingRoles.Contains(proposedValue[i].Description))
					{
						throw new RegistryValidationException(Res.GetString("5C368045-B2CF-4324-A5DA-89A6D476044C", "Description '{0}' is wrong.\r\nDescription must be a 3 letter code that matches a value in Staff Reporting Roles.", proposedValue[i].Description));
					}
				}
			}
		}
	}
}
