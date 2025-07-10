using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public sealed class GLPresentationJournalCategoryRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<GLPresentationJournalCategoryCollection, GLPresentationJournalCategoryCollection>, ICodeDescriptionPairListProvider
	{
		public GLPresentationJournalCategoryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, 3)
		{
		}

		public GLPresentationJournalCategoryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, int codeMaxLength)
			: base(new RegistryItemImpl(name, category, caption, hint, GetDataType(codeMaxLength), storage, GetDefaultValue()))
		{
		}

		static GLPresentationJournalCategoryRegistryDataType GetDataType(int maxLength)
		{
			return new GLPresentationJournalCategoryRegistryDataType(maxLength);
		}

		static GLPresentationJournalCategoryCollection GetDefaultValue()
		{
			var defaultValue = new GLPresentationJournalCategoryCollection();
			var defaultRow = new GLPresentationJournalCategory();
			defaultRow.Code = "ELM";
			defaultRow.Description = ResString.GetMultilingualString("b8aae2e1-47e5-44db-8313-99a45bc17e16", "Elimination Category");
			defaultRow.Bool = true;
			defaultRow.Bool2 = true;
			defaultRow.Bool3 = false;
			defaultRow.Bool4 = false;

			defaultValue.Add(defaultRow);
			return defaultValue;
		}

		public override int MaxLength
		{
			get { return 256; }
		}

		#region ICodeDescriptionPairListProvider

		CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
		{
			get { return this.Value.GetCodeDescriptionPairList(); }
		}

		#endregion
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.GLPresentationJournalCategoryRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class GLPresentationJournalCategoryRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GLPresentationJournalCategoryCollection>
	{
		public GLPresentationJournalCategoryRegistryDataType(int codeMaxLength)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, GLPresentationJournalCategoryCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue.Count == 0)
			{
				throw new RegistryValidationException((NoResString)"Please enter something in the list.");
			}
		}
	}
}
