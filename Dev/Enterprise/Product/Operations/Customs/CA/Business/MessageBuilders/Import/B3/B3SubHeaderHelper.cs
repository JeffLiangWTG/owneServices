using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using static Enterprise.Customs.CA.Business.MessageBuilders.B3ImportMessageWrapper;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public static class B3SubHeaderHelper
	{
		public static IEnumerable<B3SubHeader> GetB3SubHeaders(this IB3Header b3Header)
		{
			B3SubHeader[] b3SubHeaders = null;
			if (b3Header.TopLevelBusinessObject is JobDeclaration declaration)
			{
				if (declaration.IsLVX)
				{
					b3SubHeaders = (from JobComInvoiceLine line in declaration.InvoiceLines
									orderby line.B3SubHeaderNumberForLVX, line.JI_LineNo
									group line by line.B3SubHeaderNumberForLVX into lines
									select new B3SubHeader(lines.Key, lines, b3Header)).ToArray();
				}
				else
				{
					b3SubHeaders = (from JobComInvoiceLine line in declaration.InvoiceLines
									orderby line.CA_B3SubHeaderNumber, line.JI_LineNo
									group line by line.CA_B3SubHeaderNumber into lines
									select new B3SubHeader(lines.Key, lines, b3Header)).ToArray();
				}
			}
			return b3SubHeaders;
		}

		public static IEnumerable<IClassificationLine1> GetB3SubHeaderLines(this IB3SubHeader b3SubHeader)
		{
			var b3Header = b3SubHeader.B3Header;
			if (b3Header.MessageType == JobMessageTypeList.Codes.LVSForConsolidation)
			{
				return from line1 in b3Header.PositiveClassificationLines
					   where line1.B3SubHeaderNumberForLVX == b3SubHeader.B3SubHeaderNumber
					   orderby line1.B3LineNumber
					   select line1;
			}
			else
			{
				return from line1 in b3Header.PositiveClassificationLines
					   where line1.B3SubHeaderNumber == b3SubHeader.B3SubHeaderNumber
					   orderby line1.B3LineNumber
					   select line1;
			}
		}

		public static void SetB3SubHeaderNumbers(this JobDeclaration declaration)
		{
			if (declaration.IsLVS || declaration.IsIM2 || (declaration.IsImport && !declaration.IsCADEnabled))
			{
				var subHeaderNumbers = new Dictionary<string, int>();
				var invoiceLines = from JobComInvoiceLine line in declaration.InvoiceLines orderby line.InvoiceHeaderSequence, line.JI_LineNo select line;
				foreach (var line in invoiceLines)
				{
					ZString subHeaderKey;
					var header = line.InvoiceHeader;
					var treatmentCode = line.EffectiveTreatmentCode;
					if (declaration.IsConsolidatedLVS)
					{
						subHeaderKey = treatmentCode + header.CA_TimeLimit + header.CA_TimeLimitCode;
						if (TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(treatmentCode))
						{
							subHeaderKey += line.EffectiveCountryAndStateOfOrigin + line.EffectiveCountryAndStateOfExport;
						}
					}
					else if (declaration.CA_MergeBy == B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices)
					{
						subHeaderKey = LineMerger.GetTRMMergeKey(header, line);
					}
					else
					{
						subHeaderKey = header.PK + treatmentCode + line.EffectiveCountryAndStateOfOrigin + line.JI_RX_NKLinePriceCurr;
					}

					int subHeaderNumber;
					if (!subHeaderNumbers.TryGetValue(subHeaderKey, out subHeaderNumber))
					{
						subHeaderNumber = subHeaderNumbers.Count + 1;
						subHeaderNumbers[subHeaderKey] = subHeaderNumber;
					}

					if (declaration.IsLVX)
					{
						line.B3SubHeaderNumberForLVX = subHeaderNumber;
					}
					else
					{
						line.CA_B3SubHeaderNumber = subHeaderNumber;
					}
				}
			}
		}

		public static void CopyB3SubHeaderToInvoice(this JobComInvoiceHeader invoice, IB3SubHeader subHeader)
		{
			invoice.JZ_InvoiceNumber = subHeader.B3SubHeaderNumber.ToString();
			var originCountry = subHeader.CountryOfOrigin;
			if (originCountry.Length > 2 && originCountry.StartsWith("U", StringComparison.CurrentCulture))
			{
				invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
				invoice.JZ_RW_NKOriginState = originCountry.SubstringSafe(1, 2);
			}
			else
			{
				invoice.JZ_RN_NKDefaultOrigin = originCountry.Left(2);
			}
			var placeOfExit = subHeader.PlaceOfExport;
			if (placeOfExit.Length > 2 && placeOfExit.StartsWith("U", StringComparison.CurrentCulture))
			{
				invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
				invoice.CA_USStateOfExport = placeOfExit.SubstringSafe(1, 2);
			}
			else
			{
				invoice.CA_RN_NKExport = placeOfExit.Left(2);
			}
			invoice.CA_TreatmentCode = subHeader.TariffTreatmentCode;
			invoice.JZ_ValuationDateOverride = subHeader.DateOfDirectShipment;
			invoice.JZ_RX_NKInvoice_Currency = subHeader.CurrencyCode;
			invoice.CA_TimeLimit = subHeader.B3TimeLimits;
			invoice.CA_TimeLimitCode = subHeader.TimeLimitUnit;
			invoice.CA_TradeZone = subHeader.TradeZone;
		}

		public static void CopyB3SubHeaderLineToInvoiceLine(this JobComInvoiceLine invoiceLine, IClassificationLine1 classificationLine)
		{
			invoiceLine.CA_OriginalLineNo = classificationLine.B3LineNumber.ToString();
			invoiceLine.JI_Description = classificationLine.PartNumberDescriptions.FirstOrDefault().TrimEnd('*').Left(invoiceLine.JI_DescriptionInfo.MaxLength);
			invoiceLine.CA_AuthorityNumber = classificationLine.AuthorityNumber;
			invoiceLine.JI_Tariff = classificationLine.ClassificationNumber;
			invoiceLine.CA_99TariffCode = classificationLine.TariffCode;
			invoiceLine.CA_ValueForDutyCode = classificationLine.ValueForDutyCode;
			invoiceLine.CA_CustomsValue = classificationLine.ValueForDuty;
			invoiceLine.CA_CVforCurrConv = classificationLine.ValueForCurrency;

			var classificationLines2 = classificationLine.ClassificationLines.ToList();
			if (classificationLines2.Count > 0)
			{
				invoiceLine.JI_CustomsUnitQty = classificationLines2[0].UnitOfMeasureCode;
				invoiceLine.JI_CustomsQuantity = classificationLines2[0].ClassificationLineQuantity;
			}
			if (classificationLines2.Count > 1)
			{
				var secondClassificationQty = classificationLines2[1].ClassificationLineQuantity;
				invoiceLine.JI_CustomsSecondUnitQty = classificationLines2[1].UnitOfMeasureCode;
				if (secondClassificationQty > 0 && invoiceLine.JI_CustomsSecondUnitQty.IsEmpty)
				{
					invoiceLine.JI_CustomsSecondUnitQty = invoiceLine.JI_CustomsUnitQty;
				}
				invoiceLine.JI_CustomsSecondQuantity = secondClassificationQty;
			}
			if (classificationLines2.Count > 2)
			{
				var thirdClassificationQty = classificationLines2[2].ClassificationLineQuantity;
				invoiceLine.JI_CustomsThirdUnitQty = classificationLines2[2].UnitOfMeasureCode;
				if (thirdClassificationQty > 0 && invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
				{
					invoiceLine.JI_CustomsThirdUnitQty = invoiceLine.JI_CustomsUnitQty;
				}
				invoiceLine.JI_CustomsThirdQuantity = thirdClassificationQty;
			}
		}
	}
}
