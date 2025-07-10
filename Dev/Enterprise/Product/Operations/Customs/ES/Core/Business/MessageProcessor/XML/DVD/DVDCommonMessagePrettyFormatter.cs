using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Incoming;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public abstract class DVDCommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		protected void AppendDeclarationDataIfNotEmpty(StringBuilder messageDetails, string declarationTypeCode, string operationCode)
		{
			if (!string.IsNullOrEmpty(declarationTypeCode) || !string.IsNullOrEmpty(operationCode))
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, DeclarationTypeText, GetDeclarationTypeDescription(declarationTypeCode));
				WriteRowIfNotEmpty(tableCreator, OperationText, GetOperationDescription(operationCode));

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected ZString GetDeclarationTypeDescription(string declarationTypeCode)
		{
			var description = declarationTypeCode switch
			{
				DVDResponseDeclarationTypeCodeList.Codes.DvdIntoWarehouseDeclaration => DVDResponseDeclarationTypeCodeList.Descriptions.DvdIntoWarehouseDeclaration,
				DVDResponseDeclarationTypeCodeList.Codes.IdaIntoDeclarantsRecordsWarehouseDeclaration => DVDResponseDeclarationTypeCodeList.Descriptions.IdaIntoDeclarantsRecordsWarehouseDeclaration,
				_ => string.Empty
			};
			return description;
		}

		protected ZString GetOperationDescription(string operationCode)
		{
			var description = operationCode switch
			{
				DVDResponseOperationCodeList.Codes._0DvdAccepted => DVDResponseOperationCodeList.Descriptions._0DvdAccepted,
				DVDResponseOperationCodeList.Codes._1PreDeclarationAccepted => DVDResponseOperationCodeList.Descriptions._1PreDeclarationAccepted,
				DVDResponseOperationCodeList.Codes._2PreDeclarationModification => DVDResponseOperationCodeList.Descriptions._2PreDeclarationModification,
				DVDResponseOperationCodeList.Codes._3DvdAcceptedByPreDeclarationModification => DVDResponseOperationCodeList.Descriptions._3DvdAcceptedByPreDeclarationModification,
				_ => string.Empty
			};
			return description;
		}

		protected void AppendGroupCircuit(StringBuilder messageDetails, TdCircuito? responseCircuitCode, TdCircuito? responseCircuitATCCode)
		{
			var circuitAEATCode = responseCircuitCode != null ? GetCircuit(responseCircuitCode ?? default) : ZString.Empty;
			var circuitATCCode = responseCircuitATCCode != null ? GetCircuit(responseCircuitATCCode ?? default) : ZString.Empty;
			if (!circuitAEATCode.IsEmpty || !circuitATCCode.IsEmpty)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				AppendCircuitIfNotEmpty(messageDetails, circuitAEATCode, tableCreator);
				AppendCircuitCanIfNotEmpty(circuitATCCode, tableCreator);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendCsvClearanceDataAndReleaseDateIfNotEmpty(StringBuilder messageDetails, string csvClearance, string releaseDateTime)
		{
			if (!string.IsNullOrEmpty(csvClearance))
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, CSVClearanceText, csvClearance);

				AppendReleaseDateIfNotEmpty(releaseDateTime, tableCreator);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendReleaseDateIfNotEmpty(string releaseDateTime, HtmlTableCreator tableCreatorExternal)
		{
			if (releaseDateTime != null)
			{
				ZDateTime.TryParseExact(releaseDateTime, out var releaseDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

				WriteRowIfNotEmpty(tableCreatorExternal, ReleaseDateText, releaseDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
			}
		}

		protected void AppendTaxesAndFeesDataIfNotEmpty(StringBuilder messageDetails, decimal? guaranteedTotal, decimal? guaranteedTotalATC)
		{
			if (guaranteedTotal != null || guaranteedTotalATC != null)
			{
				messageDetails.Append(blankLine);
				messageDetails.Append(TaxesAndFeesDataText);
				var tableCreator = GetNewNonVisibleTableCreator();

				if (guaranteedTotal != null)
				{
					WriteRowIfNotEmpty(tableCreator, GuaranteedTotalTaxesAndFeesDataText, ((decimal)guaranteedTotal).ToString(CultureInfo.InvariantCulture));
				}

				if (guaranteedTotalATC != null)
				{
					WriteRowIfNotEmpty(tableCreator, ATCGuaranteedTotalText, ((decimal)guaranteedTotalATC).ToString(CultureInfo.InvariantCulture));
				}

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendGuaranteesDataIfNotEmpty(StringBuilder messageDetails, Collection<TdGarantiaGrNutilizada> guaranteesAEAT, Collection<TdGarantiaGrNutilizada> guaranteesATC)
		{
			if (!guaranteesAEAT.IsNullOrEmpty() || !guaranteesATC.IsNullOrEmpty())
			{
				messageDetails.Append(blankLine);
				messageDetails.Append(GuaranteesText);
				var tableCreatorGuarantees = new HtmlTableCreator(new[] { CustomsText, GRNText, PotentialDebtText });
				CreateGuaranteeRow(guaranteesAEAT, tableCreatorGuarantees, AEATText);
				CreateGuaranteeRow(guaranteesATC, tableCreatorGuarantees, ATCText);
				messageDetails.Append(tableCreatorGuarantees.ToHtml());
			}
		}

		protected void CreateGuaranteeRow(Collection<TdGarantiaGrNutilizada> usedGuarantees, HtmlTableCreator tableCreatorGuarantees, ZString customs)
		{
			if (usedGuarantees != null)
			{
				foreach (var guarantee in usedGuarantees)
				{
					tableCreatorGuarantees.WriteRow(customs, guarantee.CBgarantiaGrn, guarantee.CBimportePotencial ?? 0);
				}
			}
		}

		protected void AppendGoodsItemsDataIfNotEmpty(StringBuilder messageDetails, IDVDCommonGoodsItems responseGoodsItems)
		{
			var goodsItems = responseGoodsItems.GoodsItems;
			if (!goodsItems.IsNullOrEmpty())
			{
				messageDetails.Append(blankLine);
				messageDetails.Append(TaxesAndFeesResponseText);
				var tableCreatorGuarantees = new HtmlTableCreator(new[] { ItemText, VatText, DutyText, ExciseText, GuaranteedAmountText });
				CreateGoodsItemRow(goodsItems, tableCreatorGuarantees);
				messageDetails.Append(tableCreatorGuarantees.ToHtml());
			}
		}

		void CreateGoodsItemRow(Collection<IDVDGoodsItem> goodsItems, HtmlTableCreator tableCreatorGuarantees)
		{
			if (goodsItems != null)
			{
				foreach (var item in goodsItems)
				{
					tableCreatorGuarantees.WriteRow(item.Item, item.AmountVAT, item.AmountDuty, item.AmountExcise, item.AmountGuaranteed);
				}
			}
		}

		protected ZString GetCircuit(TdCircuito circuitCode) => GetCircuitFromText(circuitCode.ToString());

		protected ZString SetMessageDetailsRejectedComplete(IDVDCommonErrors responseWithErrors)
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(RejectedDeclarationText);
			messageDetails.Append(ListOfErrorsText);

			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorCodeColumnText, ErrorReasonColumnText, ErrorLocationColumnText, ItemColumnText, ErrorOriginalValueColumnText);

			foreach (var error in responseWithErrors.Errors)
			{
				tableCreator.WriteRow(error.Code, error.Description, error.Tag, error.GoodsItem, error.OriginalValue);
			}

			messageDetails.Append(tableCreator.ToHtml());

			return messageDetails.ToString();
		}

		string VatText => ResString.GetMultilingualString("BF904930-8D0E-4C46-AFDF-E6E3F604A0EB", "VAT");
		string DutyText => ResString.GetMultilingualString("9B6FC35F-0DB1-43B3-ADE2-CA3895E07D6A", "Duty");
		string ExciseText => ResString.GetMultilingualString("D44502DC-F2F5-40FA-AC05-66F77BF3CB05", "Excise");
		string GuaranteedAmountText => ResString.GetMultilingualString("11789B8E-03A9-48F0-B67C-4199B159019C", "Guaranteed Amount");
	}
}
