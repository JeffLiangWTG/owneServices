using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class TextTypeWrapper : ITextType
	{
		readonly string text;
		readonly string languageId;

		TextTypeWrapper(ZString text)
		{
			this.text = text;
			this.languageId = null;
		}

		TextTypeWrapper(ZString text, ZString languageId)
			: this(text)
		{
			this.languageId = languageId;
		}

		public static TextTypeWrapper NewOrNull(ZString text) => !text.IsEmpty ? new TextTypeWrapper(text) : null;
		public static TextTypeWrapper NewOrNull(ZString text, ZString languageId) => !text.IsEmpty ? new TextTypeWrapper(text, languageId) : null;

		public string LanguageID => languageId;

		public string Value => text;
	}
}
