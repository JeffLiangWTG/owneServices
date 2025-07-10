using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ParentAndChildCodeDescriptionBoolRegistryEditorInfo : IRegistryEditorInfo
	{
		public ParentAndChildCodeDescriptionBoolRegistryEditorInfo(MultilingualString parentListCaption, MultilingualString childListCaption, CodeDescriptionBoolRegistryEditorInfo parentListEditorInfo, CodeDescriptionBoolRegistryEditorInfo childListEditorInfo)
			: this(parentListCaption, childListCaption, parentListEditorInfo, childListEditorInfo, false)
		{
		}

		public ParentAndChildCodeDescriptionBoolRegistryEditorInfo(MultilingualString parentListCaption, MultilingualString childListCaption, CodeDescriptionBoolRegistryEditorInfo parentListEditorInfo, CodeDescriptionBoolRegistryEditorInfo childListEditorInfo, bool isParentListReadOnly)
		{
			this.ParentListCaption = parentListCaption;
			this.ChildListCaption = childListCaption;
			this.ParentListEditorInfo = parentListEditorInfo;
			this.ChildListEditorInfo = childListEditorInfo;
			this.IsParentListReadOnly = isParentListReadOnly;
		}

		public readonly MultilingualString ParentListCaption;
		public readonly MultilingualString ChildListCaption;
		public readonly bool IsParentListReadOnly;
		public readonly CodeDescriptionBoolRegistryEditorInfo ParentListEditorInfo;
		public readonly CodeDescriptionBoolRegistryEditorInfo ChildListEditorInfo;

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(ParentCodeDescriptionBoolCollection); }
		}

		#endregion
	}
}
