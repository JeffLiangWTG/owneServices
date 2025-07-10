using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodeDescriptionPairListEditorInfo : IRegistryEditorInfo
	{
		#region Construction

		public CodeDescriptionPairListEditorInfo() : this(true, true, CharacterCasing.Normal)
		{
		}

		public CodeDescriptionPairListEditorInfo(MultilingualString codeColumnCaption) : this(true, true, CharacterCasing.Upper, CharacterCasing.Normal, codeColumnCaption)
		{
		}

		public CodeDescriptionPairListEditorInfo(CharacterCasing descriptionFieldCasing) : this(true, true, descriptionFieldCasing)
		{
		}

		public CodeDescriptionPairListEditorInfo(bool showCodeColumn, bool showDescriptionColumn) : this(showCodeColumn, showDescriptionColumn, CharacterCasing.Normal)
		{
		}

		public CodeDescriptionPairListEditorInfo(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing descriptionFieldCasing)
			: this(showCodeColumn, showDescriptionColumn, CharacterCasing.Upper, descriptionFieldCasing)
		{
		}

		public CodeDescriptionPairListEditorInfo(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing)
			: this(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, ResString.GetMultilingualString("B2BA6404-5BD5-40dd-8EFB-7811FED6E9F3", "Code"))
		{
		}

		public CodeDescriptionPairListEditorInfo(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, MultilingualString codeColumnCaption)
			: this(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, ResString.GetMultilingualString("813E8AAD-C5FE-49fe-8890-A6094DB8654C", "Description"))
		{
		}

		public CodeDescriptionPairListEditorInfo(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, MultilingualString codeColumnCaption, MultilingualString descriptionColumnCaption)
		{
			ShowCodeColumn = showCodeColumn;
			ShowDescriptionColumn = showDescriptionColumn;
			CodeFieldCasing = codeFieldCasing;
			DescriptionFieldCasing = descriptionFieldCasing;
			this.codeColumnCaption = codeColumnCaption;
			this.descriptionColumnCaption = descriptionColumnCaption;
		}

		#endregion

		public enum CharacterCasing
		{
			Upper,
			Normal,
			Lower
		}

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionPairListRegistryDataType); }
		}

		public bool ShowCodeColumn
		{
			get { return fShowCodeColumn; }
			set { fShowCodeColumn = value; }
		}

		bool fShowCodeColumn;

		public readonly bool ShowDescriptionColumn;

		public readonly CharacterCasing DescriptionFieldCasing;

		public CharacterCasing CodeFieldCasing
		{
			get { return codeFieldCasing; }
			set { codeFieldCasing = value; }
		}

		public string CodeColumnCaption
		{
			get { return codeColumnCaption; }
		}

		public void SetCodeColumnCaption(MultilingualString value)
		{
			codeColumnCaption = value;
		}

		public string DescriptionColumnCaption
		{
			get { return descriptionColumnCaption; }
		}

		public void SetDescriptionColumnCaption(MultilingualString value)
		{
			descriptionColumnCaption = value;
		}

		CharacterCasing codeFieldCasing;
		MultilingualString codeColumnCaption;
		MultilingualString descriptionColumnCaption;

		public bool IsMultilineDescriptionColumn
		{
			get; set;
		}
	}
}
