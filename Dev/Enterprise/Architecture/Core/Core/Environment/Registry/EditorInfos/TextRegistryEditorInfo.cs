using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Flags]
	public enum TextEditorType
	{
		Unknown = 0,
		TextBox = 1 << 0,
		Password = 1 << 1,
		Memo = 1 << 2,
		ScheduleControl = 1 << 3,
		DirectoryBrowser = 1 << 4,
		AutoRatingPriorityControl = 1 << 5,
		OrgHeaderCodeListEdit = 1 << 6,
		OrgDebtorGroupCodeListEdit = 1 << 7,
		AWBCustomisableText = 1 << 8,
		AWBCustomisableMultilineText = 1 << 9,
		Url = 1 << 10,
		Guid = 1 << 11,
		HTML = 1 << 12,
	}

	public class TextRegistryEditorInfo : RegistryEditorInfo
	{
		public TextRegistryEditorInfo()
			: this(TextEditorType.Unknown)
		{
		}

		public TextRegistryEditorInfo(TextEditorType editorType)
			: base()
		{
			EditorType = editorType;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(StringRegistryDataType); }
		}

		public TextEditorType EditorType { get; set; }
	}
}
