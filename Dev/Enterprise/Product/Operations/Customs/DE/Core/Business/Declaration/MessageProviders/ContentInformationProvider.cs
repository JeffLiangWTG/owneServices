using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ContentInformationProvider : IContentInformation
	{
		public ContentInformationProvider(ContentInformationType content)
		{
			this.content = content;
		}
		readonly ContentInformationType content;

		public string ContentType => content.CY_Code;

		public decimal DegreePercentage => ZDecimal.ParseSafe(content.CY_Data, ZDecimal.Zero);
	}
}
