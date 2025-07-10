using System.Data;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSXtErrorResponseMessage : COLSMessage
	{
		public COLSXtErrorResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetFormattedMessageText(ZString messageText)
		{
			var result = messageText.StripNonPrintableASCIICharacters();
			try
			{
				result = XDocument.Parse(result).ToString(SaveOptions.None);
			}
			catch
			{
			}

			return result;
		}
	}
}
