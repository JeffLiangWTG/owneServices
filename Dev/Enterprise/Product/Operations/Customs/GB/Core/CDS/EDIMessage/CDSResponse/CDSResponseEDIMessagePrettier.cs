using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSResponseEDIMessage>
	{
		public CDSResponseEDIMessagePrettier(CDSResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject;
			responseFunction = Message.ResponseFunction;
		}

		HtmlTableCreator StatementTableCreator
		{
			get
			{
				HtmlTableCreator tableCreator = null;

				var additionalInformations = response.AdditionalInformation ?? Array.Empty<ResponseAdditionalInformation>();
				if (additionalInformations.Any())
				{
					tableCreator = GetHtmlTableCreator();
					tableCreator.WriteRow("Statement Type", "Statement Code/Description", "Limit Date Time");

					foreach (var responseAdditionalInformation in additionalInformations)
					{
						var statementType = responseAdditionalInformation.StatementTypeCode?.Value ?? ZString.Empty;
						var statementTypeDescription = CDSResponseStatementTypeList.GetDescriptionFromCode(statementType);
						var statementCode = responseAdditionalInformation.StatementCode?.Value ?? ZString.Empty;
						var statementDescription = responseAdditionalInformation.StatementDescription?.Value ?? CDSResponseStatementCodeList.GetDescriptionFromCode(statementCode);
						var limitDateTime = responseAdditionalInformation.LimitDateTime?.Item?.ToZDateTime().ToString() ?? ZString.Empty;

						tableCreator.WriteRow(ToCodeDescriptionDisplay(statementType, statementTypeDescription),
							ToCodeDescriptionDisplay(statementCode, statementDescription),
							limitDateTime
						);
					}
				}

				return tableCreator;
			}
		}

		HtmlTableCreator ErrorHtmlTableCreator
		{
			get
			{
				HtmlTableCreator tableCreator = null;

				if (FriendlyErrors.Any())
				{
					tableCreator = GetHtmlTableCreator();
					tableCreator.WriteRow("Header/Item", "Error Code", "Error Description", "Field Name", "Original Value", "Data Element", "Path (for technical support only)");

					foreach (var pointerHelper in FriendlyErrors)
					{
						var errorDescription = GetErrorDescription(pointerHelper.Code);
						if (!string.IsNullOrEmpty(pointerHelper.Description))
						{
							errorDescription = Invariant($"{pointerHelper.Description} ({errorDescription})");
						}

						tableCreator.WriteRow(GetHeaderItem(pointerHelper.LineNumber)
							, pointerHelper.Code
							, errorDescription
							, pointerHelper.FinalFieldName
							, pointerHelper.ErrorPointIsToGovernmentAgencyGoodsItem ? string.Empty : pointerHelper.FinalFieldValue
							, pointerHelper.DataElement
							, pointerHelper.PseudoXpath + BR + pointerHelper.TagIdsPath
						);
					}
				}

				return tableCreator;
			}
		}

		FriendlyCodeWithPointers[] FriendlyErrors => fFriendlyErrors ?? (fFriendlyErrors = response.GetAllFriendlyErrorsForRequestAndRejection(RequestXML).ToArray());
		FriendlyCodeWithPointers[] fFriendlyErrors;

		public IEnumerable<FriendlyCodeWithPointers> GetAllFriendlyErrors()
		{
			return FriendlyErrors;
		}

		HtmlTableCreator AmendmentTableCreator
		{
			get
			{
				HtmlTableCreator tableCreator = null;

				var newDeclarationXMLString = response.Declaration1?.Serialize() ?? ZString.Empty;
				var newDeclarationXElment = newDeclarationXMLString.IsEmpty ? null : XElement.Parse(Invariant($"<Root>{newDeclarationXMLString}</Root>"));
				var friendlyAmendments = response.GetAllFriendlyAmendments(newDeclarationXElment);

				if (friendlyAmendments.Any())
				{
					tableCreator = GetHtmlTableCreator();
					tableCreator.WriteRow("Header/Item", "Change Reason Code", "Field Name", "New Value", "Path");

					foreach (var pointerHelper in friendlyAmendments)
					{
						tableCreator.WriteRow(GetHeaderItem(pointerHelper.LineNumber)
							, pointerHelper.Code
							, pointerHelper.FinalFieldName
							, pointerHelper.FinalFieldValue
							, pointerHelper.PseudoXpath + BR + pointerHelper.TagIdsPath);
					}
				}

				return tableCreator;
			}
		}

		HtmlTableCreator StatusTableCreator
		{
			get
			{
				HtmlTableCreator tableCreator = null;

				var statuses = response.Status ?? Array.Empty<ResponseStatus>();
				if (statuses.Any())
				{
					tableCreator = GetHtmlTableCreator();
					tableCreator.WriteRow("Status", "Status Description", "Effective Date Time");

					foreach (var responseStatus in statuses)
					{
						var status = responseStatus.NameCode?.Value ?? ZString.Empty;
						var statusDescription = CDSResponseStatusList.GetDescriptionFromCode(status) ?? CDSResponseStatementCodeList.GetDescriptionFromCode(status);
						var effectiveDataTime = responseStatus.EffectiveDateTime?.Item?.ToZDateTime().ToString() ?? ZString.Empty;

						tableCreator.WriteRow(status, statusDescription, effectiveDataTime);
					}
				}

				return tableCreator;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's really not that complicated")]
		HtmlTableCreator PaymentTableCreator
		{
			get
			{
				HtmlTableCreator tableCreator = null;
				var dataIsSet = false;
				var goodsItems = response.GetGovernmentAgencyGoodsItem() ?? Array.Empty<ResponseDeclarationGovernmentAgencyGoodsItem>();
				var dutyTaxFees = response?.Declaration?.DutyTaxFee ?? Array.Empty<ResponseDeclarationDutyTaxFee>();

				if (goodsItems.Any() || dutyTaxFees.Any())
				{
					tableCreator = GetHtmlTableCreator();
					var needToShowAssessedAmount = goodsItems.Where(x => x.Commodity != null).SelectMany(x => x.Commodity).Any(x => x.Payment?.PaymentAmount?.Value != x.Payment?.TaxAssessedAmount?.Value)
						|| dutyTaxFees.Any(x => x.Payment?.PaymentAmount?.Value != x.Payment?.TaxAssessedAmount?.Value);

					if (needToShowAssessedAmount)
					{
						tableCreator.WriteRow("Item", "Tax Type or Reference", "Payment Amount", "Assessed Amount", "Tax Rate", "Tax Base");
					}
					else
					{
						tableCreator.WriteRow("Item", "Tax Type or Reference", "Payment Amount", "Tax Rate", "Tax Base");
					}

					foreach (var goodsItem in goodsItems)
					{
						var item = "Item " + goodsItem.SequenceNumeric.ToString(CultureInfo.CurrentCulture);

						foreach (var commodity in goodsItem.Commodity ?? Array.Empty<ResponseDeclarationGovernmentAgencyGoodsItemDutyTaxFee>())
						{
							var cTypeCode = commodity.TypeCode?.Value ?? ZString.Empty;
							var paymentAmount = Invariant($"{commodity.Payment?.PaymentAmount?.currencyID ?? ZString.Empty}{commodity.Payment?.PaymentAmount?.Value ?? decimal.Zero}");
							var taxRate = Invariant($"{commodity.TaxRateNumeric}%");
							var taxBase = commodity.AdValoremTaxBaseAmount == null || commodity.AdValoremTaxBaseAmount.Value == 0m ? string.Empty : commodity.AdValoremTaxBaseAmount.Value.ToString(CultureInfo.CurrentCulture);

							if (needToShowAssessedAmount)
							{
								var assessedAmount = Invariant($"{commodity.Payment?.TaxAssessedAmount?.currencyID ?? ZString.Empty}{commodity.Payment?.TaxAssessedAmount?.Value ?? decimal.Zero}");
								tableCreator.WriteRow(item, cTypeCode, paymentAmount, assessedAmount, taxRate, taxBase);
							}
							else
							{
								tableCreator.WriteRow(item, cTypeCode, paymentAmount, taxRate, taxBase);
							}

							dataIsSet = true;
						}
					}

					foreach (var dutyTaxFee in dutyTaxFees)
					{
						if (needToShowAssessedAmount)
						{
							tableCreator.WriteRow("Total", dutyTaxFee?.Payment?.ReferenceID?.Value, dutyTaxFee?.Payment?.PaymentAmount?.Value, dutyTaxFee?.Payment?.TaxAssessedAmount?.Value, string.Empty, string.Empty);
						}
						else
						{
							tableCreator.WriteRow("Total", dutyTaxFee?.Payment?.ReferenceID?.Value, dutyTaxFee?.Payment?.PaymentAmount?.Value, string.Empty, string.Empty);
						}
						dataIsSet = true;
					}
				}

				return !dataIsSet ? null : tableCreator;
			}
		}

		public override ZString MakeHumanReadable()
		{
			var owner = Message.EM_MessageOwner;
			var senderOfMessage = string.IsNullOrEmpty(owner) ? (ZString)"CDS" : owner;
			return MessagePrettierCss.CSS
					+ ToH3IfNotEmpty(Invariant($"Response from {senderOfMessage}: {responseFunction.GetDescription(Message.LinkedEntry)}"))
					+ ToKeyValuePairSection(new (ZString key, ZString value)[]
						{
							("Function Code", Invariant($"{responseFunction.NumericFunctionCode}-{responseFunction.ThreeCharFunctionCode}")),
							//("Summary", ToStrongIfNotEmpty(responseFunction.Description)),
							("Old CHIEF Report Code", responseFunction.OldCHIEFReportCode),
							("Bank", response.Bank?.ID?.Value ?? ZString.Empty),
							("Declaration Version", response.Declaration?.VersionID?.Value ?? ZString.Empty),
							("MRN", response.GetMovementReferenceNumber()),
							("LRN", response.GetDeclarationFunctionalReferenceID()),
							("Issued Date", response.GetIssueDate().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
						})
					+ ToTableSection("Additional Information", StatementTableCreator)
					+ ToTableSection("Amendment Information", AmendmentTableCreator)
					+ ToTableSection("Status Information", StatusTableCreator)
					+ ToTableSection("Payment Information", PaymentTableCreator)
					+ ToTableSection("Error Information", ErrorHtmlTableCreator);
		}

		XElement RequestXML
		{
			get
			{
				var outgoingMessageText = Entry?.GetLastOutgoingMessage(Message.EM_ApplicationReference)?.EM_MessageText ?? ZString.Empty;
				return !outgoingMessageText.IsEmpty
						? XElement.Parse(outgoingMessageText)
						: null;
			}
		}

		ZString GetErrorDescription(ZString errorCode)
		{
			var strippedErrorCode = errorCode.SubstringSafe(3);

			var query = new ZQuery();
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, SQLComparisonOperator.Equal, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode);

			var codeQuery = new ZQuery();
			codeQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.Equal, errorCodePrefix.Select(x => x + strippedErrorCode));

			query.AddToFilter(codeQuery);

			return Factory.LoadTop1<RefCusCodeList>(query)?.ZZD_Description ?? string.Empty;
		}

		static ZString GetHeaderItem(ZString lineNumber)
		{
			return lineNumber.IsEmpty ? "Header" : Invariant($"Item {lineNumber}");
		}

		CodeDescriptionPairList CDSResponseStatusList => Factory.GetCachedValue<CDSResponseStatusList>();

		CodeDescriptionPairList CDSResponseStatementTypeList => Factory.GetCachedValue<CDSResponseStatementTypeList>();

		CodeDescriptionPairList CDSResponseStatementCodeList => Factory.GetCachedValue<CDSResponseStatementCodeList>();

		readonly string[] errorCodePrefix = { "CDS", "DMS" };
		readonly Response response;
		readonly ResponseFunction responseFunction;
	}
}
