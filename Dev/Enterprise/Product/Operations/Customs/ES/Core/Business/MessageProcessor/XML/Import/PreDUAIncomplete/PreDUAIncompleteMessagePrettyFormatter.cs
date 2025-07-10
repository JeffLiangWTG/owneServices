using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class PreDUAIncompleteMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public PreDUAIncompleteMessagePrettyFormatter(PreDeclaIncompletaV1Sal response, CusEntryHeader entryHeader)
		{
			this.response = Argument.NotNull(response, nameof(response));
			Argument.NotNull(entryHeader, nameof(entryHeader));
			countryCode = entryHeader.CountryCode;
			factory = entryHeader.Factory;
		}
		readonly PreDeclaIncompletaV1Sal response;
		readonly ZString countryCode;
		readonly BusinessObjectFactory factory;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetailsBuilders = new StringBuilder();

			messageDetailsBuilders.Append(AcceptedDeclarationText);

			AppendDescriptionDataIfNotEmpty(messageDetailsBuilders);
			messageDetailsBuilders.Append(blankLine);
			AppendGroupCSV(messageDetailsBuilders);

			var goodsItems = response.Partida;
			if (goodsItems != null && goodsItems.Any())
			{
				messageDetailsBuilders.Append(blankLine + blankLine + GetH3Text(RequiredCertificatesText));
				var tableCreator = GetNewTableCreator();
				tableCreator.WriteRow(ItemColumnText, GetStrongText(MeasureText), GetStrongText(AgencyText), GetStrongText(DocumentsText));

				foreach (var item in goodsItems)
				{
					foreach (var cert in item.CertificadosRequeridos)
					{
						AppendItemCertDataRow(item.C32NumeroDePartida, cert, tableCreator);
					}
				}
				messageDetailsBuilders.Append(tableCreator.ToHtml());
			}
			return messageDetailsBuilders.ToString();
		}

		void AppendDescriptionDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.CodigoRespuesta))
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, DescriptionText, "(" + response.CodigoRespuesta + ")" + response.DescripcionRespuesta);
			}
		}

		void AppendGroupCSV(StringBuilder messageDetails)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			AppendCsvClearanceDataIfNotEmpty(messageDetails, tableCreator);
			messageDetails.Append(tableCreator.ToHtml());
		}

		void AppendCsvClearanceDataIfNotEmpty(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(response.CsVdeDeclaracionElectronica))
			{
				WriteRowIfNotEmpty(tableCreatorExternal, CSVClearanceText, response.CsVdeDeclaracionElectronica);
			}
		}

		void AppendItemCertDataRow(int itemDepartureNumber, CertiControlTd cert, HtmlTableCreator tableCreator)
		{
			var measure = cert.Medida;
			var agency = cert.Organismo;
			var agencyName = cert.NombreOrganismo;
			var certificateTypes = cert.TipoCertificado;
			if (certificateTypes != null && certificateTypes.Any())
			{
				var tableCreatorCert = new HtmlTableCreator(TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };
				foreach (var certType in certificateTypes)
				{
					var certDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, certType, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today)?.ZZD_Description;
					tableCreatorCert.WriteRow(certType + " - " + certDescription);
				}

				tableCreator.WriteRow(itemDepartureNumber, measure, agency + " " + agencyName, tableCreatorCert.ToHtml());
			}
		}

		public ZString CreateMessageDetailsRejected()
		{
			var responseCode = response.CodigoRespuesta;
			var responseDescription = response.DescripcionRespuesta;
			var goodsItemWithError = response.NumeroOrdenPartidaConError;

			var messageDetails = ErrorDetailsText(responseCode, responseDescription) + ItemDetailsText(goodsItemWithError);

			return messageDetails;
		}

		string ErrorDetailsText(ZString responseCode, ZString responseDescription) => GetH4Text(ResString.GetMultilingualString("B09E33D7-9E86-475B-BC32-C94715F7ADA6", "Error = {0} - {1}", responseCode, responseDescription));
		string ItemDetailsText(int? goodsItemWithError) => GetH4Text(ResString.GetMultilingualString("7B08B641-80C1-45AE-A547-578B551FDF02", "Item = {0}", goodsItemWithError));
	}
}
