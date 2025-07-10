using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolRegistryEditorInfo : IRegistryEditorInfo
	{
		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption) : this(boolColumnCaption, true)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, bool isBoolColumnVisible)
			: this(boolColumnCaption, isBoolColumnVisible, false)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, bool isBoolColumnVisible, bool isOnlyBoolColumnEditable)
			: this(boolColumnCaption, null, isBoolColumnVisible, isOnlyBoolColumnEditable)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, bool isBoolColumnVisible, bool isOnlyBoolColumnEditable, bool isDescriptionColumnTranslatable)
			: this(boolColumnCaption, null, isBoolColumnVisible, isOnlyBoolColumnEditable)
		{
			this.IsDescriptionColumnTranslatable = isDescriptionColumnTranslatable;
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, ICondition isBoolColumnVisibleCondition)
			: this(boolColumnCaption, isBoolColumnVisibleCondition, false)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, ICondition isBoolColumnVisibleCondition, bool isOnlyBoolColumnEditable)
			: this(boolColumnCaption, isBoolColumnVisibleCondition, false, isOnlyBoolColumnEditable)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, ICondition isBoolColumnVisibleCondition, bool isBoolColumnVisibleOverride, bool isOnlyBoolColumnEditable)
			: this(boolColumnCaption, isBoolColumnVisibleCondition, isBoolColumnVisibleOverride, isOnlyBoolColumnEditable, false)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, ICondition isBoolColumnVisibleCondition, bool isBoolColumnVisibleOverride, bool isOnlyBoolColumnEditable, bool isCodeColumnAlwaysVisible)
			: this(boolColumnCaption, null, isBoolColumnVisibleCondition, isBoolColumnVisibleOverride, isOnlyBoolColumnEditable, isCodeColumnAlwaysVisible)
		{
		}

		public CodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption, MultilingualString codeColumnCaption, ICondition isBoolColumnVisibleCondition, bool isBoolColumnVisibleOverride, bool isOnlyBoolColumnEditable, bool isCodeColumnAlwaysVisible)
		{
			this.boolColumnCaption = boolColumnCaption;
			this.codeColumnCaption = codeColumnCaption;
			this.IsBoolColumnVisibleCondition = isBoolColumnVisibleCondition;
			this.isOnlyBoolColumnEditable = isOnlyBoolColumnEditable;
			this.IsBoolColumnVisibleOverride = isBoolColumnVisibleOverride;
			this.isCodeColumnAlwaysVisible = isCodeColumnAlwaysVisible;
		}

		public bool IsBoolColumnVisible
		{
			get { return (IsBoolColumnVisibleCondition == null) ? IsBoolColumnVisibleOverride : IsBoolColumnVisibleCondition.IsMet; }
		}

		public bool IsOnlyBoolColumnEditable
		{
			get { return isOnlyBoolColumnEditable && IsBoolColumnVisible; }
		}

		public bool IsCodeColumnVisible => isCodeColumnAlwaysVisible || !IsOnlyBoolColumnEditable;

		public string BoolColumnCaption
		{
			get { return boolColumnCaption ?? ""; }
		}

		public string CodeColumnCaption
		{
			get { return codeColumnCaption ?? ""; }
		}

		public bool IsDescriptionColumnTranslatable { get; }

		public readonly MultilingualString boolColumnCaption;
		public readonly MultilingualString codeColumnCaption;
		internal readonly ICondition IsBoolColumnVisibleCondition;
		readonly bool IsBoolColumnVisibleOverride;
		readonly bool isOnlyBoolColumnEditable;
		readonly bool isCodeColumnAlwaysVisible;

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionBoolCollection); }
		}

		#endregion
	}
}
