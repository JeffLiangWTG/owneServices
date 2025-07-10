using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ComboBoxRegistryEditorInfo : TextRegistryEditorInfo
	{
		public ComboBoxRegistryEditorInfo(OLookUpEditType lookUpEditType) : this(lookUpEditType, true)
		{
		}

		public ComboBoxRegistryEditorInfo(ICodeDescriptionPairListProvider lookUpList) : this(lookUpList, true)
		{
		}

		public ComboBoxRegistryEditorInfo(OLookUpEditType lookUpEditType, bool showComboDescription)
		{
			fLookUpEditType = lookUpEditType;
			ShowComboDescription = showComboDescription;
		}

		public ComboBoxRegistryEditorInfo(ICodeDescriptionPairListProvider lookUpList, bool showComboDescription)
		{
			fLookUpListProvider = lookUpList;
			ShowComboDescription = showComboDescription;
		}

		public CodeDescriptionPairList LookUpList
		{
			get
			{
				if (fLookUpListProvider == null)
				{
					fLookUpListProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(fLookUpEditType));
				}
				return fLookUpListProvider.CodeDescriptionPairList;
			}
		}

		public bool ShowComboDescription
		{
			get { return fShowComboDescription; }
			set { fShowComboDescription = value; }
		}

		bool fShowComboDescription;
		ICodeDescriptionPairListProvider fLookUpListProvider;
		readonly OLookUpEditType fLookUpEditType;
	}
}
