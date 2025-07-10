using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.CodeDescriptionBoolRelatedItemRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CodeDescriptionBoolRelatedItemRegistryEditorInfo : IRegistryEditorInfo
	{
		public CodeDescriptionBoolRelatedItemRegistryEditorInfo()
		{
		}

		public CodeDescriptionBoolRelatedItemRegistryEditorInfo(MultilingualString boolColumnCaption)
			: this(boolColumnCaption, true)
		{
		}

		public CodeDescriptionBoolRelatedItemRegistryEditorInfo(MultilingualString boolColumnCaption, bool isBoolColumnVisible)
		{
			this.boolColumnCaption = boolColumnCaption;
			this.isBoolColumnVisible = isBoolColumnVisible;
		}

		public CodeDescriptionBoolRelatedItemRegistryEditorInfo(MultilingualString boolColumnCaption, bool isBoolColumnVisible, MultilingualString relatedItemCaption)
		{
			this.boolColumnCaption = boolColumnCaption;
			this.isBoolColumnVisible = isBoolColumnVisible;
			this.relatedItemCaption = relatedItemCaption;
		}

		readonly MultilingualString boolColumnCaption;
		readonly bool isBoolColumnVisible;
		readonly MultilingualString relatedItemCaption;

		public string BoolColumnCaption
		{
			get { return boolColumnCaption ?? ""; }
		}

		public bool IsBoolColumnVisible
		{
			get { return isBoolColumnVisible; }
		}

		public string RelatedItemColumnCaption
		{
			get { return relatedItemCaption; }
		}

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionBoolRelatedItemCollection); }
		}

		#endregion
	}
}
