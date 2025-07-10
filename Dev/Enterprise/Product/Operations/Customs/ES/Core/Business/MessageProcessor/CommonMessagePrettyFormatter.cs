using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public abstract class CommonMessagePrettyFormatter
	{
		protected HtmlTableCreator GetNewTableCreator()
		{
			var htmlAttributes =
				new NameValueCollection
				{
					{ (NoResString)"border", "1" },
					{ (NoResString)"cellpadding", "1" },
					{ (NoResString)"cellspacing", "0" },
					TableInterpretation.Attributes.FullWidth,
					{ (NoResString)"class", (NoResString)"table" }
				};

			return new HtmlTableCreator(htmlAttributes) { EnableHTMLEncoding = false };
		}

		protected void AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(StringBuilder messageDetails, ZDateTime receivedDate, string dateText = null)
		{
			if (!receivedDate.IsEmpty)
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, dateText ?? ReceivedText, receivedDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
			}
		}

		protected ZString CreateMessageDetailsAcceptedCancelCommon(bool correctResponseDataIsNotNull, DateTime? dateInvalidation = null, string csvDeclaration = "", string status = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedCancellationText);

			if (correctResponseDataIsNotNull)
			{
				AppendCancellationDateIfNotEmpty(messageDetails, dateInvalidation);
				messageDetails.Append(blankLine);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, csvDeclaration);
				AppendStatusData(messageDetails, status);
			}

			return messageDetails.ToString();
		}

		void AppendCancellationDateIfNotEmpty(StringBuilder messageDetails, DateTime? cancellationDate)
		{
			if (cancellationDate != null)
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CancellationDateText, ((ZDateTime)cancellationDate).ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected void SetMessageDetailsForRejectedMessageCommon(StringBuilder messageDetails, Collection<IFunctionalError> errorsCollection)
		{
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorCodeColumnText, ErrorPlaceColumnText, ErrorReasonColumnText, ErrorOriginalValueColumnText);

			foreach (var error in errorsCollection)
			{
				tableCreator.WriteRow(error.Code, error.Pointer, "(" + error.Reason + ") " + error.Description, error.OriginalValue);
			}

			messageDetails.Append(tableCreator.ToHtml());
		}

		protected void SetMessageDetailsForErrorMessageCommon(StringBuilder messageDetails, Collection<IXMLError> errorsCollection)
		{
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorLineAndColumnText, ErrorLocationColumnText, ErrorCodeColumnText, ErrorReasonColumnText, ErrorOriginalValueColumnText);

			foreach (var error in errorsCollection)
			{
				tableCreator.WriteRow(error.LineNumber + " / " + error.ColumnNumber, error.Pointer, error.Code, error.Text, error.OriginalValue);
			}

			messageDetails.Append(tableCreator.ToHtml());
		}

		protected ZString SetMessageDetailsRejectedCommon(string responseType, ICommonErrors response)
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(RejectedDeclarationText);
			messageDetails.Append(ListOfErrorsText);

			if (responseType == AESAndNCTS5ResponseTypeCodeList.Codes.RejectedMessage)
			{
				SetMessageDetailsForRejectedMessageCommon(messageDetails, response.FunctionalErrors);
			}
			else if (responseType == AESAndNCTS5ResponseTypeCodeList.Codes.XmlError)
			{
				SetMessageDetailsForErrorMessageCommon(messageDetails, response.XMLErrors);
			}

			return messageDetails.ToString();
		}

		protected void AppendAcceptanceDateIfNotEmpty(StringBuilder messageDetails, ZDateTime acceptanceDate) => AppendDataInNewTableIfNotEmpty(messageDetails, AcceptanceText, acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash());

		protected void AppendMrnDataIfNotEmpty(StringBuilder messageDetails, string mrn, string referenceTextMrn = null) => AppendDataInNewTableIfNotEmpty(messageDetails, referenceTextMrn ?? ReferenceText, mrn);

		protected void AppendStatusData(StringBuilder messageDetails, string statusCode) => AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, StatusText, GetStatusDescription(statusCode));

		protected virtual ZString GetStatusDescription(string statusCode)
		{
			var description = statusCode switch
			{
				AESStatusCodeList.Codes.IndirectDispatch => AESStatusCodeList.Descriptions.IndirectDispatch,
				AESStatusCodeList.Codes.DirectDispatch => AESStatusCodeList.Descriptions.DirectDispatch,
				AESStatusCodeList.Codes.WaitingPcoDecision => AESStatusCodeList.Descriptions.WaitingPcoDecision,
				AESStatusCodeList.Codes.PendingClearance => AESStatusCodeList.Descriptions.PendingClearance,
				AESStatusCodeList.Codes.EffectiveExit => AESStatusCodeList.Descriptions.EffectiveExit,
				AESStatusCodeList.Codes.PendingPresentationOfGoods => AESStatusCodeList.Descriptions.PendingPresentationOfGoods,
				AESStatusCodeList.Codes.DeclarationCancelled => AESStatusCodeList.Descriptions.DeclarationCancelled,
				AESStatusCodeList.Codes.PreDeclarationInvalidated => AESStatusCodeList.Descriptions.PreDeclarationInvalidated,
				AESStatusCodeList.Codes.Invalidated => AESStatusCodeList.Descriptions.Invalidated,
				AESStatusCodeList.Codes.NotCleared => AESStatusCodeList.Descriptions.NotCleared,
				AESStatusCodeList.Codes.ManagedByTransit => AESStatusCodeList.Descriptions.ManagedByTransit,
				AESStatusCodeList.Codes.ManagedInOtherPlace => AESStatusCodeList.Descriptions.ManagedInOtherPlace,
				AESStatusCodeList.Codes.PendingTransitEvidence => AESStatusCodeList.Descriptions.PendingTransitEvidence,
				AESStatusCodeList.Codes.PendingToUe => AESStatusCodeList.Descriptions.PendingToUe,
				AESStatusCodeList.Codes.ReceivedAndUnderControl => AESStatusCodeList.Descriptions.ReceivedAndUnderControl,
				AESStatusCodeList.Codes.RejectedDeviation => AESStatusCodeList.Descriptions.RejectedDeviation,
				AESStatusCodeList.Codes.ReceivedInAnotherCountry => AESStatusCodeList.Descriptions.ReceivedInAnotherCountry,
				AESStatusCodeList.Codes.PendingDeparture => AESStatusCodeList.Descriptions.PendingDeparture,
				AESStatusCodeList.Codes.StopAtExit => AESStatusCodeList.Descriptions.StopAtExit,
				_ => ZString.Empty
			};

			return statusCode + " - " + description;
		}

		protected void AppendCsvElectronicDeclarationDataIfNotEmpty(StringBuilder messageDetails, string csvElectronicDeclaration) => AppendDataInNewTableIfNotEmpty(messageDetails, CSVElectronicDeclarationText, csvElectronicDeclaration);

		protected void AppendCsvClearanceDataAndReleaseDateIfNotEmpty(StringBuilder messageDetails, string csvClearance, ZDateTime? csvClearanceDate, string clearanceText = null, string dateText = null)
		{
			if (!string.IsNullOrEmpty(csvClearance))
			{
				messageDetails.Append(blankLine);
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, clearanceText ?? CSVClearanceText, csvClearance);

				AppendDateWithddMMyyyyFormatIfNotEmpty(csvClearanceDate, tableCreator, dateText ?? ReleaseDateText);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendGroupCSV(StringBuilder messageDetails, string csvClearance, ZDateTime? csvClearanceDate, string csvElectronicDeclaration)
		{
			AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, csvClearance, csvClearanceDate);
			messageDetails.Append(blankLine);
			AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, csvElectronicDeclaration);
		}

		protected void AppendDateWithddMMyyyyFormatIfNotEmpty(ZDateTime? responseDate, HtmlTableCreator tableCreatorExternal, string dateText = null)
		{
			if (responseDate != null)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, dateText ?? ReleaseDateText, ((ZDateTime)responseDate).ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected HtmlTableCreator GetNewNonVisibleTableCreator()
		{
			var htmlAttributes =
				new NameValueCollection
				{
					{ (NoResString)"border", "0" }
				};

			return new HtmlTableCreator(htmlAttributes) { EnableHTMLEncoding = false };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string green = "#64AF00";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string red = "#F00000";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string orange = "#F57800";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string yellow = "#C6C600";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string blankLine = "<br>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected const string nonBreakSpace = "&nbsp;&nbsp;";

		protected void WriteRowIfNotEmpty(HtmlTableCreator tableCreator, ZString rowName, ZString fieldData)
		{
			if (!fieldData.IsEmpty)
			{
				tableCreator.WriteRow(rowName, nonBreakSpace, fieldData);
			}
		}

		protected void AppendDataInNewTableIfNotEmpty(StringBuilder messageDetails, ZString rowName, ZString fieldData)
		{
			if (!fieldData.IsEmpty)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				WriteRowIfNotEmpty(tableCreator, rowName, fieldData);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendDataInNewTableWithBlankLineIfNotEmpty(StringBuilder messageDetails, ZString rowName, ZString fieldData)
		{
			if (!fieldData.IsEmpty)
			{
				messageDetails.Append(blankLine);
				var tableCreator = GetNewNonVisibleTableCreator();
				WriteRowIfNotEmpty(tableCreator, rowName, fieldData);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected virtual void AppendCircuitIfNotEmpty(StringBuilder messageDetails, ZString circuitCodeText) => AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CircuitText, circuitCodeText);

		protected void AppendCircuitIfNotEmpty(StringBuilder messageDetails, ZString circuitCodeText, HtmlTableCreator tableCreatorExternal)
		{
			if (!circuitCodeText.IsEmpty)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, CircuitText, circuitCodeText);
				messageDetails.Append(blankLine);
			}
		}

		protected void AppendCircuitCanIfNotEmpty(ZString circuitText, HtmlTableCreator tableCreatorExternal)
		{
			if (!circuitText.IsEmpty)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, CircuitCanText, circuitText);
			}
		}

		protected ZString HTMLColourString(ZString color, ZString text) => $@"<strong><font color=""{color}"">{text}</font></strong>";

		protected virtual void AppendAcceptanceDataIfNotEmpty(StringBuilder messageDetails, ZString serviceSegmentText, HtmlTableCreator tableCreatorExternal = null)
		{
			if (!string.IsNullOrEmpty(serviceSegmentText))
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				ZDateTime.TryParseExact(serviceSegmentText, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

				WriteRowIfNotEmpty(tableCreator, AcceptanceText, acceptanceDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected ZString GetCircuitFromText(ZString circuitCode) => circuitCodeList.ContainsKey(circuitCode) ? GetColourCircuitStringXML(circuitCode) : ZString.Empty;

		readonly ImmutableDictionary<string, (string Code, string Format)> circuitCodeList = new Dictionary<string, (string, string)>()
		{
			{ MessageFunctionCodeList.Codes.GreenCircuitText, (CircuitCodeList.Descriptions.GREEN, green) },
			{ MessageFunctionCodeList.Codes.RedCircuitText, (CircuitCodeList.Descriptions.RED, red) },
			{ MessageFunctionCodeList.Codes.OrangeCircuitText, (CircuitCodeList.Descriptions.ORANGE, orange) }
		}.ToImmutableDictionary();

		ZString GetColourCircuitStringXML(string circuitCode) => HTMLColourString(circuitCodeList[circuitCode].Format, circuitCodeList[circuitCode].Code);

		protected ZString GetH2Text(string text) => $@"<H2>{text}</H2>";
		protected ZString GetH3Text(string text) => $@"<H3>{text}</H3>";
		protected ZString GetH4Text(string text) => $@"<H4>{text}</H4>";
		protected ZString GetStrongText(string text) => $@"<strong>{text}</strong>";

		protected const string GRNText = "GRN";
		protected const string AEATText = "AEAT";
		protected const string ATCText = "ATC";
		protected const string MrnText = "MRN:";
		protected string AcceptedDeclarationText => GetH3Text(ResString.GetMultilingualString("20BC9530-A60E-4EE5-9B83-4AF61E498FAE", "Accepted Declaration"));
		protected string AcceptedAmendmentText => GetH3Text(ResString.GetMultilingualString("FFF6F735-5F08-4149-AA93-ED416B746E50", "Accepted Amendment"));
		protected string RejectedDeclarationText => GetH3Text(ResString.GetMultilingualString("2275B078-3201-4306-8FFE-E0C0AA26AD9B", "Rejected Declaration"));
		protected string InboxCommunicationText => GetH3Text(ResString.GetMultilingualString("9A821437-60E5-47CA-8A2D-A345619B31B4", "Inbox Communication"));
		protected string AcceptedCancellationText => GetH3Text(ResString.GetMultilingualString("3649C19A-175F-4EB9-BBEE-2C5D1B43448F", "Accepted Cancellation"));

		protected string ListOfErrorsText => GetH4Text(ResString.GetMultilingualString("587D711C-ABFE-4C00-ABB2-6ED7C24FB00E", "List of Errors:"));
		protected string ManagementDataText => GetH4Text(ResString.GetMultilingualString("7477BF0E-5A3D-42EB-8AEE-B9D6785611F5", "Management Data"));
		protected string CSVElectronicDocumentsText => GetH4Text(ResString.GetMultilingualString("2977B2AB-4644-4DD5-A576-02BD7E9D59BC", "CSV Electronic Documents"));
		protected string CSVElectronicDocumentText => GetH4Text(ResString.GetMultilingualString("BB85BF75-729E-46B8-A537-0F3D4DD316E5", "CSV Electronic Document"));

		protected string ReceivedText => ResString.GetMultilingualString("EB9BEB49-ABE1-4505-9145-E72A98D7E08E", "Received:");
		protected string AcceptanceText => ResString.GetMultilingualString("CBCFD0DA-0761-4A9A-92F1-F2505652568D", "Acceptance:");
		protected string CancellationDateText => ResString.GetMultilingualString("5A90E4C5-E510-47AB-BC6D-228D66779CC4", "Cancellation Date:");
		protected string ReferenceExportText => ResString.GetMultilingualString("61E0A17D-D071-44A3-A617-EE82D15EE229", "Export Doc. (MRN):");
		protected string CircuitText => ResString.GetMultilingualString("9E6129AF-2CB6-42C6-B67E-A31077F453FF", "Circuit:");
		protected string CircuitCanText => ResString.GetMultilingualString("C57CB5E7-125A-407E-BBC4-4B687839775A", "ATC Circuit:");
		protected string CSVClearanceText => ResString.GetMultilingualString("A897A1EA-5344-4AE0-85CD-40234D3EC266", "Clearance:");
		protected string ReleaseDateText => ResString.GetMultilingualString("D1B4978B-472D-4BC0-9B4B-7F1B171D620D", "Date:");
		protected string CSVCodeText => ResString.GetMultilingualString("22C749E7-121D-4AF5-87A3-281B495C59F8", "Import cert. (CSV):");
		protected string CsvIdText => ResString.GetMultilingualString("44D0A7C5-9AB8-4984-842C-019E5DA65B18", "CSV ID:");
		protected string DescriptionText => ResString.GetMultilingualString("E11296ED-BCC0-457F-97CB-D3DA14F87CEF", "Description:");
		protected string ReferenceText => ResString.GetMultilingualString("3080E5E6-7C31-4687-BE15-A5908582F99B", "Register (MRN):");
		protected string ArrivalReferenceText => ResString.GetMultilingualString("72ED43C0-21B1-4AEF-A518-1856B6E49AEA", "MRN Arrival:");
		protected string ResponseCodeText => ResString.GetMultilingualString("672EBC99-25F3-47F1-BD80-6378F91896DE", "Response Code:");
		protected string SummaryText => ResString.GetMultilingualString("6A74F95C-BF27-4383-90C6-BB08546EEB03", "Summary Decl:");
		protected string DiscrepanciesPreviousSummaryText => ResString.GetMultilingualString("FF8860BE-1CE2-41C3-AB5D-31175C526D46", "Discrepancies previous summary:");
		protected string CSVElectronicDeclarationText => ResString.GetMultilingualString("0BEB3E80-AD8D-46E5-80C5-18B1F3CD40F2", "CSV Electronic Declaration:");
		protected string CSVExitCertificateText => ResString.GetMultilingualString("1D48F6D0-0181-4D1A-92C7-71076A40510C", "CSV Exit Certificate:");

		protected string TaxesAndFeesDataText => GetH2Text(ResString.GetMultilingualString("5A1A02BF-BDAD-436B-A01D-FFF46F750ECA", "Taxes and fees data"));
		protected string GuaranteesText => GetH2Text(ResString.GetMultilingualString("838864A7-FB4D-4F62-A040-CC53CC733B18", "Guarantees"));
		protected string TaxesAndFeesResponseText => GetH2Text(ResString.GetMultilingualString("50F89032-2632-4E2C-AE24-8F7EBD6CE88C", "Taxes and fees response (Spanish Customs)"));
		protected string TotalTaxesAndFeesDataText => ResString.GetMultilingualString("0E3C6C37-B2B0-4EDF-A8F2-A1D66A53BCBE", "Total:");
		protected string GuaranteedTotalTaxesAndFeesDataText => ResString.GetMultilingualString("0FEFBAF6-7723-4C1D-8B2B-352A8A11F5E7", "Guaranteed Total:");
		protected string ATCTotalText => ResString.GetMultilingualString("F2B6DA9A-F103-4F11-97EA-44E0B93A4D7F", "ATC Total:");
		protected string ATCGuaranteedTotalText => ResString.GetMultilingualString("54A58082-2A64-431D-A7E2-A1902753538B", "ATC Guaranteed Total:");
		protected string TotalDeferredVATText => ResString.GetMultilingualString("604F805A-F536-4AC2-8E07-0B7E919ADEEC", "Total Deferred VAT:");
		protected string ClearanceGuaranteeVATExemptionText => ResString.GetMultilingualString("67C8012D-11E9-4211-89FD-39D1918CDB4B", "Clearance Guarantee VAT Exemption:");
		protected string RealClearanceGuaranteeText => ResString.GetMultilingualString("F6803FAD-A51F-4818-A243-43DFAF631807", "Real Clearance Guarantee:");
		protected string PendencyGuaranteeVatExemptionText => ResString.GetMultilingualString("59EB47AB-B82C-4706-87D0-F9C23B230FFF", "Pendency Guarantee VAT Exemption:");
		protected string RealPendencyGuaranteeText => ResString.GetMultilingualString("B5D9F948-3185-4695-B98E-29683C705303", "Real Pendency Guarantee:");
		protected string PaymentProofNumberText => ResString.GetMultilingualString("4E4FBAD0-D493-4257-AD97-497BAAA19C65", "Payment Proof Number:");
		protected string PaymentDateLimitText => ResString.GetMultilingualString("23A746F4-3023-46F7-85AD-2209493180D7", "Payment date limit:");
		protected string ATCProofOfPaymentNumberText => ResString.GetMultilingualString("C628956B-4408-43DE-9FEE-63E1E661D763", "ATC Proof of Payment Number:");
		protected string ATCPaymentDateLimitText => ResString.GetMultilingualString("0BD2DED0-CBDF-40A3-8CEF-24F0C99A313B", "ATC payment date limit:");

		protected string AdministrationText => ResString.GetMultilingualString("12F2B55F-855E-4679-83E8-F8EC9A8C9BA6", "Administration:");
		protected string DeclarationTypeText => ResString.GetMultilingualString("92A323D6-F04C-4727-9090-5E8527F341C0", "Declaration Type:");
		protected string CustomsClearanceStatusText => ResString.GetMultilingualString("A34322C6-2F26-4121-98E1-D9C0CAD7C11F", "Customs Clearance Status:");
		protected string AccountingStatusText => ResString.GetMultilingualString("891058C9-CAEF-4A94-ABEB-67890F14DD96", "Accounting Status:");
		protected string GuaranteeStatusText => ResString.GetMultilingualString("F714EC61-A54C-49E3-8EEB-B638AC0660FF", "Guarantee Status:");
		protected string UnfinishedPendenciesText => ResString.GetMultilingualString("A91BA2A0-5A49-4136-8DD2-5A0BE38EEDFD", "Unfinished Pendencies:");

		protected string AEATDispatchStatusText => ResString.GetMultilingualString("99028BEC-66CD-4B22-A5A5-D5D286C0CD25", "AEAT Dispatch Status:");
		protected string ATCDispatchStatusText => ResString.GetMultilingualString("F62AA866-7109-4C08-9D69-4A484E91BFAE", "ATC Dispatch Status:");
		protected string AEATAccountingStatusText => ResString.GetMultilingualString("12B71D41-8BD4-48C5-B85B-CDBDDF3C5D0E", "AEAT Accounting Status:");
		protected string ATCAccountingStatusText => ResString.GetMultilingualString("A96FA079-2E2A-451A-895B-7DCD90F72154", "ATC Accounting Status:");
		protected string AEATUnfinishedPendenciesText => ResString.GetMultilingualString("9973FE57-DCC6-403C-8231-2D7A37D75760", "AEAT Unfinished Pendencies:");
		protected string ATCUnfinishedPendenciesText => ResString.GetMultilingualString("D143DD3F-0890-43F2-BF75-22F64CFBC03F", "ATC Unfinished Pendencies:");

		protected string MessageTypeText => ResString.GetMultilingualString("4052C257-AE65-4076-B27A-75C214E634FB", "Message Type:");
		protected string ExitTypeText => ResString.GetMultilingualString("DC7F9F00-C0FE-4B84-B572-1A2C8693713E", "Exit Type:");
		protected string StatusText => ResString.GetMultilingualString("69AB156F-028B-4B0C-A91E-28B81A5B837B", "Status:");
		protected string InvalidationDateText => ResString.GetMultilingualString("DC9C9163-C2B7-4E65-9353-4D941CA6F9D9", "Invalidation Date:");
		protected string InvalidationRequestedDateText => ResString.GetMultilingualString("BB322E16-858F-4419-8A44-56AAF89272E8", "Invalidation Requested Date:");
		protected string InvalidationReasonText => ResString.GetMultilingualString("3F103920-5BEA-4811-A077-4C1795F278E1", "Invalidation Reason:");
		protected string InvalidationByCustomsYesText => ResString.GetMultilingualString("8AD5E557-4E93-4EBB-BD62-694B792E869F", "YES");
		protected string InvalidationByCustomsNoText => ResString.GetMultilingualString("E3DED2B9-0284-4A39-8C47-699F646534AB", "NO");
		protected string ExitResultText => ResString.GetMultilingualString("194413C6-A195-4E4C-B8FC-01D6E85F0029", "Exit Result:");
		protected string EffectiveDepartureClearanceText => ResString.GetMultilingualString("BD671B75-AC35-4CC6-9957-66CE6BD27A96", "Effective Departure Clearance:");
		protected string EffectiveDepartureDateText => ResString.GetMultilingualString("8A890693-2C89-41A4-B4A4-B3B5233202DF", "Effective Departure Date:");
		protected string VersionText => ResString.GetMultilingualString("52C5E8E7-7D59-4AF1-A562-5EF019CB34B4", "Version:");

		protected string OperationText => ResString.GetMultilingualString("A019F9F8-A5CB-4480-AFD2-5DE23FEE6F1F", "Operation:");
		protected string RemarksText => ResString.GetMultilingualString("44C4A909-28D2-4789-82B0-528765E344BF", "Remarks:");
		protected string ArrivalDateText => ResString.GetMultilingualString("37FF1F40-42FE-4FC1-8BD3-03079369432E", "Arrival Date:");
		protected string NonConformityDateText => ResString.GetMultilingualString("85CB3E33-6280-4E97-A314-515FEFFB4057", "Non-Conformity Date:");

		protected string LimitDateOfArrivalText => ResString.GetMultilingualString("A3494462-4769-4043-90FB-35BC95D74CB9", "Limit date of arrival:");

		protected string ResponseCodeSExtraText => ResString.GetMultilingualString("13351C36-BDA1-4988-85D0-C8D18176B929", "Please, send message again after 15 minutes");

		protected string ItemColumnText => GetStrongText(ResString.GetMultilingualString("4E73B387-B40E-4399-9B81-2343CD06A815", "Item"));
		protected string TypeColumnText => GetStrongText(ResString.GetMultilingualString("704A5752-8993-4A47-ACF9-CCB63C0E397B", "Type"));
		protected string DescriptionColumnText => GetStrongText(ResString.GetMultilingualString("489070F4-196F-4A0A-9589-523689BD5E31", "Description"));
		protected string ReferenceColumnText => GetStrongText(ResString.GetMultilingualString("73533979-D455-461D-9910-F83D0FF00869", "Reference"));
		protected string CSVDocumentColumnText => GetStrongText(ResString.GetMultilingualString("77BEDC1A-8E4D-495A-B8A3-8A5D7C909A08", "CSV Document"));
		protected string ErrorCodeColumnText => GetStrongText(ResString.GetMultilingualString("3A9AC9B4-4194-45BB-9029-AEC4D87AE63C", "Code"));
		protected string ErrorPlaceColumnText => GetStrongText(ResString.GetMultilingualString("2AB4DEBD-D6AA-4197-BC81-B8FA62A09B95", "Place"));
		protected string ErrorReasonColumnText => GetStrongText(ResString.GetMultilingualString("299C817F-D285-475D-8433-ECCB77FB623A", "Reason"));
		protected string ErrorPointerColumnText => GetStrongText(ResString.GetMultilingualString("E75540F6-EBF2-48BB-8CF9-7EE6A8EA67D9", "Pointer"));
		protected string ErrorOriginalValueColumnText => GetStrongText(ResString.GetMultilingualString("B1EEF291-3D17-4077-81ED-4300B2C498CD", "Original Value"));
		protected string ErrorErrorColumnText => GetStrongText(ResString.GetMultilingualString("C038A730-7F55-4B85-9866-E819DBB25FFC", "Error"));
		protected string ErrorLocationAndDescriptionColumnText => GetStrongText(ResString.GetMultilingualString("62DBCD3E-3EDF-48AE-B739-6EC1CAD36503", "Location/Description"));
		protected string ErrorLocationColumnText => GetStrongText(ResString.GetMultilingualString("BD537870-E89D-451C-82A2-8FE2AEB3ABCB", "Location"));
		protected string ErrorLineAndColumnText => GetStrongText(ResString.GetMultilingualString("EF1848C5-6513-403F-920A-5256814AC9ED", "Line / Column"));

		protected string CustomsText => ResString.GetMultilingualString("AD7C4554-A5C5-4323-A719-27D469448D7B", "Customs");
		protected string PotentialDebtText => ResString.GetMultilingualString("88B950A2-B211-46CB-BBF9-82FB88BB4589", "Potential Debt");
		protected string ItemText => ResString.GetMultilingualString("E7B61B40-BA97-477D-A970-71B2DC9CE1BC", "Item");

		protected string RequiredCertificatesText => ResString.GetMultilingualString("52E42D1B-B463-43DB-943F-71E53704AB64", "Required Certificates");
		protected string MeasureText => ResString.GetMultilingualString("3A885E43-EA45-40A0-85F4-A5BCBB2562EF", "Measure");
		protected string AgencyText => ResString.GetMultilingualString("6799E862-D9D2-424E-912C-704CD0742160", "Agency");
		protected string DocumentsText => ResString.GetMultilingualString("67FE837E-DC47-45A1-A5DE-E2C8DE7BD65E", "Documents");
	}
}
