using System.Collections.Generic;
using System.Text;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDeclarationSearchResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSDeclarationInfoResponseEDIMessage>
	{
		public CDSDeclarationSearchResponseEDIMessagePrettier(CDSDeclarationInfoResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject as DeclarationSearchResponse;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = PrettierSpecificCSS
					+ ToH4IfNotEmpty(Invariant($"Page {response.CurrentPageNumber} of {response.TotalPagesAvailable}: {response.TotalResultsAvailable} results"))
					+ GetDeclarationSearchDetails().JoinAsString();

			return interpretation;
		}

		public IEnumerable<ZString> GetDeclarationSearchDetails()
		{
			foreach (DeclarationSearchResponseDeclarationSearchDetails details in response.DeclarationSearchDetails)
			{
				yield return ToTableSection("", GetTableDisplay(details)) + @"<hr class=""rounded"">";
			}
		}

		HtmlTableCreator GetTableDisplay(DeclarationSearchResponseDeclarationSearchDetails details)
		{
			var tableCreator = GetHtmlTableCreator();

			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting($"MRN: {details.Declaration.ID.Value}", "width", "33%"),
				new CellWithFormatting($"LRN: {details.Declaration.LRN}", "width", "33%"),
				new CellWithFormatting($"Date: {details.Declaration.ReceivedDateTime.Item.ToZDateTime().ToString()}", "width", "33%")
			);

			tableCreator.WriteRow($"Status: route {details.Declaration.ROE}, ICS {details.Declaration.ICS}", $"Type: {details.Declaration1.TypeCode.Value}", $"Location: {GetLocation(details.Declaration1.GoodsShipment.Consignment.GoodsLocation)}");
			tableCreator.WriteRow($"Importer: {details.Declaration1?.GoodsShipment.Importer.ID.Value}", $"Declarant: {details.Declaration1?.Declarant.ID.Value}", $"Submitter: {details.Declaration1?.Submitter.ID.Value}");

			return tableCreator;
		}

		ZString GetLocation(DeclarationGoodsShipmentConsignmentGoodsLocation goodsLocation)
		{
			var location = new StringBuilder();
			location.Append(goodsLocation.Address.CountryCode.Value);
			location.Append(goodsLocation.TypeCode.Value);
			location.Append(goodsLocation.Address.TypeCode.Value);
			location.Append(goodsLocation.Name.Value);

			return location.ToString();
		}

		ZString PrettierSpecificCSS => MessagePrettierCss.CSS.Replace("</style>", @"table, tr, td {border: none;}hr.rounded {border-top: 4px solid #bbb;border-radius: 5px;</style>");

		readonly DeclarationSearchResponse response;
	}
}
