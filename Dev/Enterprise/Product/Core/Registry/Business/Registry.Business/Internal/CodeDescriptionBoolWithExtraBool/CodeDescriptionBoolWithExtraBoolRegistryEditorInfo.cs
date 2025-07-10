using System;
using Enterprise.Integration;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolWithExtraBoolRegistryEditorInfo : IRegistryEditorInfo
	{
		public CodeDescriptionBoolWithExtraBoolRegistryEditorInfo(string boolColumnCaption, string bool2ColumnCaption) : this(boolColumnCaption, true, bool2ColumnCaption, true)
		{
		}

		public CodeDescriptionBoolWithExtraBoolRegistryEditorInfo(string boolColumnCaption, bool isBoolColumnVisible, string bool2ColumnCaption, bool isBool2ColumnVisible)
			: this(boolColumnCaption, isBoolColumnVisible, false)
		{
			this.Bool2ColumnCaption = bool2ColumnCaption;
			this.fIsBool2ColumnVisible = isBool2ColumnVisible;
		}

		public CodeDescriptionBoolWithExtraBoolRegistryEditorInfo(string boolColumnCaption, bool isBoolColumnVisible, bool isOnlyBoolColumnEditable)
			: this(boolColumnCaption, null, isOnlyBoolColumnEditable)
		{
			this.IsBoolColumnVisibleOverride = isBoolColumnVisible;
		}

		public CodeDescriptionBoolWithExtraBoolRegistryEditorInfo(string boolColumnCaption, ICondition isBoolColumnVisibleCondition)
			: this(boolColumnCaption, isBoolColumnVisibleCondition, false)
		{
		}

		public CodeDescriptionBoolWithExtraBoolRegistryEditorInfo(string boolColumnCaption, ICondition isBoolColumnVisibleCondition, bool isOnlyBoolColumnEditable)
		{
			this.BoolColumnCaption = boolColumnCaption;
			this.IsBoolColumnVisibleCondition = isBoolColumnVisibleCondition;
			this.isOnlyBoolColumnEditable = isOnlyBoolColumnEditable;
		}

		public bool IsBoolColumnVisible
		{
			get { return (IsBoolColumnVisibleCondition == null) ? IsBoolColumnVisibleOverride : IsBoolColumnVisibleCondition.IsMet; }
		}

		public bool IsOnlyBoolColumnEditable
		{
			get { return isOnlyBoolColumnEditable && IsBoolColumnVisible; }
		}

		public bool IsBool2ColumnVisible
		{
			get { return fIsBool2ColumnVisible; }
		}

		public readonly string BoolColumnCaption;
		public readonly string Bool2ColumnCaption;
		internal readonly ICondition IsBoolColumnVisibleCondition;
		readonly bool IsBoolColumnVisibleOverride;
		readonly bool isOnlyBoolColumnEditable;
		readonly bool fIsBool2ColumnVisible;

		#region IRegistryEditorInfo Members

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionBoolWithExtraBoolCollection); }
		}

		#endregion
	}
}
