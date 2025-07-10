using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionWithGroupRegistryEditorInfo : IRegistryEditorInfo
	{
		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption) : this(groupColumnCaption, true)
		{
		}

		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption, bool isGroupColumnVisible)
			: this(groupColumnCaption, isGroupColumnVisible, false)
		{
		}

		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption, bool isGroupColumnVisible, bool isOnlyGroupColumnEditable)
			: this(groupColumnCaption, null, isGroupColumnVisible, isOnlyGroupColumnEditable)
		{
		}

		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption, bool isGroupColumnVisible, bool isOnlyGroupColumnEditable, bool isDescriptionColumnTranslatable)
			: this(groupColumnCaption, null, isGroupColumnVisible, isOnlyGroupColumnEditable)
		{
			this.IsDescriptionColumnTranslatable = isDescriptionColumnTranslatable;
		}

		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption, ICondition isGroupColumnVisibleCondition)
			: this(groupColumnCaption, isGroupColumnVisibleCondition, false)
		{
		}

		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption, ICondition isGroupColumnVisibleCondition, bool isOnlyGroupColumnEditable)
			: this(groupColumnCaption, isGroupColumnVisibleCondition, false, isOnlyGroupColumnEditable)
		{
		}

		public CodeDescriptionWithGroupRegistryEditorInfo(MultilingualString groupColumnCaption, ICondition isGroupColumnVisibleCondition, bool isGroupColumnVisible, bool isOnlyGroupColumnEditable)
		{
			this.groupColumnCaption = groupColumnCaption;
			this.IsGroupColumnVisibleCondition = isGroupColumnVisibleCondition;
			this.isOnlyGroupColumnEditable = isOnlyGroupColumnEditable;
			this.IsGroupColumnVisibleOverride = isGroupColumnVisible;
		}

		public bool IsGroupColumnVisible
		{
			get { return (IsGroupColumnVisibleCondition == null) ? IsGroupColumnVisibleOverride : IsGroupColumnVisibleCondition.IsMet; }
		}

		public bool IsOnlyGroupColumnEditable
		{
			get { return isOnlyGroupColumnEditable && IsGroupColumnVisible; }
		}

		public string GroupColumnCaption
		{
			get { return groupColumnCaption ?? ""; }
		}

		public bool IsDescriptionColumnTranslatable { get; }

		public readonly MultilingualString groupColumnCaption;
		internal readonly ICondition IsGroupColumnVisibleCondition;
		readonly bool IsGroupColumnVisibleOverride;
		readonly bool isOnlyGroupColumnEditable;

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionWithGroupCollection); }
		}

		#endregion
	}
}
