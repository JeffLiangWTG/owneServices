using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ImportGenericCommonMessagePrettyFormatter<TResponse> : CommonMessagePrettyFormatter, IMessagePrettyFormatter
		where TResponse : IImportCommonGeneric
	{
		public ImportGenericCommonMessagePrettyFormatter(TResponse response, CusEntryHeader entryHeader)
		{
			this.response = Argument.NotNull(response, nameof(response));
			Argument.NotNull(entryHeader, nameof(entryHeader));
			differedData = entryHeader.TotalAmount != response.TotalAmountToPay;
			countryCode = entryHeader.CountryCode;
			factory = entryHeader.Factory;
		}
		protected readonly TResponse response;
		readonly ZBool differedData;
		readonly ZString countryCode;
		readonly BusinessObjectFactory factory;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => CreateMessageDetailsAcceptedCore(extraDataFromProcessing);

		protected virtual ZString CreateMessageDetailsAcceptedCore(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			AppendDeclarationData(messageDetails);
			AppendTaxesTitle(messageDetails);

			var tableCreator = GetNewNonVisibleTableCreator();
			AppendTotalAmountToPayAndGuaranteedTotal(messageDetails, tableCreator);
			AppendATCAmountToPayAndGuaranteedTotal(messageDetails, tableCreator);
			AppendVATData(messageDetails, tableCreator);
			messageDetails.Append(tableCreator.ToHtml());
			AppendPaymentInfoTitle(messageDetails);
			var tableCreatorPayment = GetNewNonVisibleTableCreator();
			AppendPaymentInfo(messageDetails, tableCreatorPayment);
			AppendPaymentInfoATC(messageDetails, tableCreatorPayment);
			messageDetails.Append(tableCreatorPayment.ToHtml());
			messageDetails.Append(blankLine);
			AppendGuarantees(messageDetails, response.GRNGuarantees, ((IImportCommon)response).GRNGuaranteesCan);
			AppendFees(messageDetails, true);
			AppendCertificates(messageDetails);

			return messageDetails.ToString();
		}

		protected void AppendDeclarationData(StringBuilder messageDetails)
		{
			messageDetails.Append(AcceptedDeclarationText);

			AppendDescriptionDataIfNotEmpty(messageDetails);
			messageDetails.Append(blankLine);
			AppendGroupAcceptanceAndMRN(messageDetails);
			AppendGroupCircuit(messageDetails);
			messageDetails.Append(blankLine);
			AppendGroupCSV(messageDetails);
		}

		void AppendGroupAcceptanceAndMRN(StringBuilder messageDetails)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			AppendAcceptanceDataIfNotEmpty(messageDetails, GetAcceptanceDateString(), tableCreator);
			AppendMrnDataIfNotEmpty(messageDetails, tableCreator);
			AppendExportMrnDataIfNotEmpty(messageDetails, tableCreator);
			messageDetails.Append(tableCreator.ToHtml());
		}

		void AppendGroupCircuit(StringBuilder messageDetails)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			AppendCircuitIfNotEmpty(messageDetails, GetCircuit(), tableCreator);
			AppendCircuitCanIfNotEmpty(GetCircuitCan(), tableCreator);
			messageDetails.Append(tableCreator.ToHtml());
		}

		void AppendGroupCSV(StringBuilder messageDetails)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			AppendCsvClearanceDataIfNotEmpty(messageDetails, tableCreator);
			AppendReleaseDateIfNotEmpty(messageDetails, GetReleaseDateString(), GetReleaseDateFormat(), tableCreator);
			AppendCsvImportCertificateDataIfNotEmpty(messageDetails, tableCreator);
			messageDetails.Append(tableCreator.ToHtml());
		}

		protected virtual void AppendDescriptionDataIfNotEmpty(StringBuilder messageDetails) { }

		protected virtual ZString GetAcceptanceDateString() => response.AcceptanceDate;

		protected virtual void AppendMrnDataIfNotEmpty(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal) { }

		protected virtual void AppendExportMrnDataIfNotEmpty(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(response.ExportMRN))
			{
				WriteRowIfNotEmpty(tableCreatorExternal, ReferenceExportText, response.ExportMRN);
			}
		}

		protected virtual ZString GetReleaseDateString() => response.ReleaseDate;

		protected virtual string GetReleaseDateFormat() => CustomsDateTimeExtension.DateFormat;

		void AppendReleaseDateIfNotEmpty(StringBuilder messageDetails, ZString releaseDateString, string dateFormat, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(releaseDateString))
			{
				ZDateTime.TryParseExact(releaseDateString, out var releaseDate, dateFormat);

				WriteRowIfNotEmpty(tableCreatorExternal, ReleaseDateText, releaseDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
			}
		}

		void AppendCsvClearanceDataIfNotEmpty(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(response.CSVClearance))
			{
				WriteRowIfNotEmpty(tableCreatorExternal, CSVClearanceText, response.CSVClearance);
			}
		}

		void AppendCsvImportCertificateDataIfNotEmpty(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(response.CSVImportCertificate))
			{
				WriteRowIfNotEmpty(tableCreatorExternal, CSVCodeText, response.CSVImportCertificate);
			}
		}

		ZString GetCircuit() => response.CircuitSpecified ? circuitoTdList.ContainsKey(response.Circuit) ? GetColourCircuitStringXML(response.Circuit) : ZString.Empty : ZString.Empty;

		protected virtual ZString GetCircuitCan() => ZString.Empty;

		protected readonly ImmutableDictionary<CircuitoTd, (string Code, string Format)> circuitoTdList = new Dictionary<CircuitoTd, (string, string)>()
		{
			{ CircuitoTd.V, (CircuitCodeList.Descriptions.GREEN, green) },
			{ CircuitoTd.R, (CircuitCodeList.Descriptions.RED, red) },
			{ CircuitoTd.N, (CircuitCodeList.Descriptions.ORANGE, orange) },
			{ CircuitoTd.A, (CircuitCodeList.Descriptions.YELLOW, yellow) }
		}.ToImmutableDictionary();

		protected ZString GetColourCircuitStringXML(CircuitoTd circuitoTd) => HTMLColourString(circuitoTdList[circuitoTd].Format, circuitoTdList[circuitoTd].Code);

		protected void AppendTaxesTitle(StringBuilder messageDetails)
		{
			messageDetails.Append(blankLine + TaxesAndFeesDataText);
			if (differedData)
			{
				messageDetails.Append(GetH3Text(ResString.GetMultilingualString("DD023FD7-866E-47CD-B523-63C115484AE9", "Warning: Taxes and fees data received differ from sent data")));
			}
		}

		protected void AppendTotalAmountToPayAndGuaranteedTotal(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			WriteRowIfNotEmpty(tableCreatorExternal, TotalTaxesAndFeesDataText, response.TotalAmountToPay.ToString());

			WriteRowIfNotEmpty(tableCreatorExternal, GuaranteedTotalTaxesAndFeesDataText, (response.TotalGuaranteed - response.ClearanceGuaranteeVATExemption - response.PendencyGuaranteeVATExemption).ToString(CultureInfo.InvariantCulture));
		}

		protected void AppendATCAmountToPayAndGuaranteedTotal(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			var totalAmountCan = GetTotalAmountToPayCan();
			var totalGuaranteedCan = GetTotalGuaranteedCan();

			if (totalAmountCan != 0)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, ATCTotalText, totalAmountCan.ToString());
			}
			if (totalGuaranteedCan != 0)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, ATCGuaranteedTotalText, totalGuaranteedCan.ToString());
			}
		}

		protected void AppendVATData(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			var totalAPagar = response.TotalAmountToPay;
			var excencionIVALevante = response.ClearanceGuaranteeVATExemption;
			var excencionIVAPendencia = response.PendencyGuaranteeVATExemption;

			if (response.TotalDeferredVAT != 0)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, TotalDeferredVATText, response.TotalDeferredVAT.ToString());
			}
			if (excencionIVALevante != 0)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, ClearanceGuaranteeVATExemptionText, excencionIVALevante.ToString());

				WriteRowIfNotEmpty(tableCreatorExternal, RealClearanceGuaranteeText, (totalAPagar - excencionIVALevante).ToString());
			}
			if (excencionIVAPendencia != 0)
			{
				var totalGuaranteed = response.TotalGuaranteed;
				WriteRowIfNotEmpty(tableCreatorExternal, PendencyGuaranteeVatExemptionText, excencionIVAPendencia.ToString());
				WriteRowIfNotEmpty(tableCreatorExternal, RealPendencyGuaranteeText, (totalGuaranteed - totalAPagar - excencionIVAPendencia).ToString(CultureInfo.InvariantCulture));
			}

			messageDetails.Append(blankLine);
		}

		protected void AppendPaymentInfoTitle(StringBuilder messageDetails)
		{
			messageDetails.Append(blankLine + PaymentInformationText + blankLine);
		}

		protected void AppendPaymentInfo(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			var paymentProofNumber = GetPaymentProofNumber();
			if (!paymentProofNumber.IsEmpty)
			{
				WriteRowIfNotEmpty(tableCreatorExternal, PaymentProofNumberText, paymentProofNumber);
				var limitPaymentDateCorrect = ZDateTime.TryParseExact(response.LimitPaymentDate, out var limitPaymentDate, CustomsDateTimeExtension.DateFormat);
				var limitDate = limitPaymentDateCorrect && !limitPaymentDate.IsEmpty ? limitPaymentDate : ZDateTime.MaxSmallDateTime;
				WriteRowIfNotEmpty(tableCreatorExternal, PaymentDateLimitText, limitDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected void AppendPaymentInfoATC(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			var paymentProofNumberCan = ((IImportCommon)response).PaymentProofNumberCan;
			if (!paymentProofNumberCan.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreatorExternal, ATCProofOfPaymentNumberText, paymentProofNumberCan);
				var limitPaymentCanDateCorrect = ZDateTime.TryParseExact(((IImportCommon)response).LimitPaymentDateCan, out var limitPaymentCanDate, CustomsDateTimeExtension.DateFormat);
				var aTCLimitDate = limitPaymentCanDateCorrect && !limitPaymentCanDate.IsEmpty ? limitPaymentCanDate : ZDateTime.MaxSmallDateTime;
				WriteRowIfNotEmpty(tableCreatorExternal, ATCPaymentDateLimitText, aTCLimitDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected void AppendGuarantees(StringBuilder messageDetails, Collection<GarantiaGrNutilizadaTd> aeatGuarantees, Collection<GarantiaGrNutilizadaTd> atcGuarantees, string extraData = "")
		{
			messageDetails.Append(blankLine + GuaranteesText + blankLine);
			AppendExtraDataToGuarantees(messageDetails, extraData);
			var tableCreatorGuarantees = new HtmlTableCreator(new[] { CustomsText, GRNText, RealDebtText, PotentialDebtText, UndeterminedRealDebtText });
			CreateGuaranteeRow(aeatGuarantees, tableCreatorGuarantees, AEATText);
			CreateGuaranteeRow(atcGuarantees, tableCreatorGuarantees, ATCText);
			messageDetails.Append(tableCreatorGuarantees.ToHtml());
			messageDetails.Append(blankLine);
		}

		protected virtual void AppendExtraDataToGuarantees(StringBuilder messageDetails, ZString extraData) { }

		protected void AppendFees(StringBuilder messageDetails, ZBool filterFees)
		{
			var tableCreatorTaxes = new HtmlTableCreator(new[] { ItemText, TypeText, MaxMinRateText, BaseAmountText, TaxRateText, TotalAmountText, TotalGuaranteedAmountText });

			bool shouldAddFees;
			if (filterFees)
			{
				var rowsCreatedAEAT = CreateFeeTableRows(tableCreatorTaxes, response, false, filterFees);
				var rowsCreatedATC = CreateFeeTableRows(tableCreatorTaxes, response, true, filterFees);
				shouldAddFees = rowsCreatedAEAT || rowsCreatedATC;
			}
			else
			{
				shouldAddFees = CreateFeeTableRows(tableCreatorTaxes, response, false, filterFees);
			}

			if (shouldAddFees)
			{
				messageDetails.Append(blankLine + TaxesAndFeesResponseText + blankLine);
				messageDetails.Append(tableCreatorTaxes.ToHtml());
				messageDetails.Append(blankLine);
			}
		}

		protected void AppendCertificates(StringBuilder messageDetails)
		{
			var shouldAddCertificates = false;
			var tableCreatorCerts = new HtmlTableCreator(new[] { ItemText, MeasureText, AgencyText, DocumentsText });

			if (response.Lines != null && response.Lines.Any())
			{
				foreach (var item in response.Lines)
				{
					var itemNumber = item.LineNumber;
					var certificates = item.Certificates;
					if (certificates != null && certificates.Count > 0)
					{
						shouldAddCertificates = true;
						foreach (var cert in certificates)
						{
							var measure = cert.Medida;
							var agency = cert.Organismo;
							var agencyName = cert.NombreOrganismo;
							var certificateTypes = cert.TipoCertificado;
							if (certificateTypes.Count > 0)
							{
								var certTypes = new List<ZString?>();
								foreach (var certTypeNode in certificateTypes)
								{
									var certType = certTypeNode;
									var certDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, certType, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today)?.ZZD_Description;
									certTypes.Add(certType + ((certDescription.HasValue && certDescription.Value != "") ? " - " + certDescription : ""));
								}
								tableCreatorCerts.WriteRow(itemNumber, measure, agency + " - " + agencyName, string.Join(", ", certTypes));
							}
						}
					}
				}
			}

			if (shouldAddCertificates)
			{
				messageDetails.Append(blankLine + GetH2Text(RequiredCertificatesText) + blankLine);
				messageDetails.Append(tableCreatorCerts.ToHtml());
			}
		}

		void CreateGuaranteeRow(Collection<GarantiaGrNutilizadaTd> usedGuarantees, HtmlTableCreator tableCreatorGuarantees, ZString customs)
		{
			if (usedGuarantees != null)
			{
				foreach (var guarantee in usedGuarantees)
				{
					tableCreatorGuarantees.WriteRow(customs, guarantee.CBgarantiaGrn, guarantee.CBimporteReal ?? 0, guarantee.CBimportePotencial ?? 0, guarantee.CBimporteRealSinDeterminar ?? 0);
				}
			}
		}

		ZBool CreateFeeTableRows(HtmlTableCreator tableCreatorTaxes, IImportCommonGeneric response, ZBool isATC, ZBool filterFees)
		{
			ZDecimal totalAmount = 0;
			ZDecimal totalGuaranteedAmount = 0;
			var shouldAddRow = false;
			if (response.Lines != null && response.Lines.Any())
			{
				foreach (var line in response.Lines)
				{
					var filteredTributes = line.Tributes;
					if (filterFees)
					{
						filteredTributes = new Collection<Cas47TributoLiquidadoTd>((isATC ? line.Tributes.Where(x => x.C47TributoClase == "3IG" || x.C47TributoClase == "3AI") : line.Tributes.Where(x => x.C47TributoClase != "3IG" && x.C47TributoClase != "3AI")).ToList());
					}
					if (filteredTributes != null && filteredTributes.Count > 0)
					{
						shouldAddRow = true;
						foreach (var tax in filteredTributes)
						{
							var taxrateunit = string.IsNullOrEmpty(tax.C47TributoUnidadFiscal) || tax.C47TributoUnidadFiscal.Contains("%") ? " %" : (NoResString)" €/" + tax.C47TributoUnidadFiscal;
							var taxrate = tax.C47TributoTipoImpositivo.ToString(CultureInfo.InvariantCulture) + taxrateunit;
							totalAmount += tax.C47TributoCuotaPaga;
							totalGuaranteedAmount += tax.C47TributoCuotaGarantiza;
							tableCreatorTaxes.WriteRow(line.LineNumber, tax.C47TributoClase, tax.C47TributoIndicadorMaxMinNor, tax.C47TributoBaseImponible, taxrate, tax.C47TributoCuotaPaga, tax.C47TributoCuotaGarantiza);
						}
					}
				}
				if (totalAmount > 0 || totalGuaranteedAmount > 0)
				{
					shouldAddRow = true;
					tableCreatorTaxes.WriteRow("", "", "", "", TotalText, totalAmount, totalGuaranteedAmount);
				}
			}
			return shouldAddRow;
		}

		public ZString CreateMessageDetailsRejected()
		{
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorErrorColumnText, ErrorLocationAndDescriptionColumnText);

			foreach (var error in response.Errors)
			{
				var errorCode = error.CodigoError;
				var errorDescription = error.DescripcionError;
				var goodsItemWithError = error.NumeroOrdenPartidaConError ?? 0;
				var errorElementNum = error.NumeroOrdenElementoErroneo ?? 0;
				var errorTag = error.EtiquetaConError;
				var errorValue = error.ValorErroneo;

				tableCreator.WriteRow(errorCode, goodsItemWithError + "." + errorElementNum + blankLine + errorDescription + "." + errorTag + "." + errorValue);
			}

			return tableCreator.ToHtml();
		}

		protected virtual ZDecimal GetTotalAmountToPayCan() => ((IImportCommon)response).TotalAmountToPayCan;
		protected virtual ZDecimal GetTotalGuaranteedCan() => ((IImportCommon)response).TotalGuaranteedCan;
		protected virtual ZString GetPaymentProofNumber() => ((IImportCommon)response).PaymentProofNumber;

		protected string PaymentInformationText => GetH2Text(ResString.GetMultilingualString("1D37FE53-CCBD-4EFC-A1F4-19019F58F13D", "Payment information"));
		string RealDebtText => ResString.GetMultilingualString("E2377DEE-F637-4D69-BBE5-18659B872B12", "Real Debt");
		string UndeterminedRealDebtText => ResString.GetMultilingualString("BE2E5AAB-B0EE-4CEB-B39F-8CF90D172EDA", "Undetermined Real Debt");
		string TypeText => ResString.GetMultilingualString("584AD150-33A8-4902-8C70-6A5F09D77A66", "Type");
		string MaxMinRateText => ResString.GetMultilingualString("FAFBE656-42B5-4D01-99B9-F3E297EE771D", "MAX/MIN Rate");
		string BaseAmountText => ResString.GetMultilingualString("691EA893-E001-42B1-AC52-A551452A34CE", "Base Amount");
		string TaxRateText => ResString.GetMultilingualString("95631E8A-CBAB-486D-8CE2-C5B7D67A2E5F", "Tax Rate");
		string TotalAmountText => ResString.GetMultilingualString("96569A03-DF04-4B82-A493-1AFCFD9DEAFE", "Total Amount");
		string TotalGuaranteedAmountText => ResString.GetMultilingualString("965B3974-CC10-4F89-9FA2-B31FFBCC4E2C", "Total Guaranteed Amount");
		string TotalText => ResString.GetMultilingualString("7B5E2462-7C73-41D9-8B3D-9EE298321490", "Total: ");
	}
}
