using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Validation
{
	public static class ExporterExemptionValidationHelper
	{
		public static (ZString, ZString) CheckExporterExemption(OrgHeader orgHeader, ZString ledger, ZDateTime checkDate, InvoicingLineBaseCollection lines = null)
		{
			var warnings = new ZStringBuilder();
			var errors = new ZStringBuilder();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.Italy)
			{
				if (orgHeader != null)
				{
					var requiredDocuments = GetRequiredDocuments(ledger, orgHeader, checkDate);
					if (requiredDocuments != null && requiredDocuments.Any())
					{
						AppendMessage(warnings, CheckCertificateExpireIn30Days(requiredDocuments));
						var ceilingLimitMessage = CheckExemptionCeilingLimitThreshold(ledger, requiredDocuments, orgHeader, lines);
						if (!ceilingLimitMessage.IsEmpty)
						{
							AppendMessage(PreventPosting ? errors : warnings, ceilingLimitMessage);
						}
					}
					else if (PreventPosting && ledger == LedgerTypes.AccountsReceivable && lines != null && lines.Any())
					{
						if (lines.OfType<InvoicingLineBase>().Any(l => l.TaxRate != null && l.TaxRate.AT_Code == "DICH.INT"))
						{
							AppendMessage(errors, GetNoAvailableEXVDocumentsErrorMessage());
						}
					}
				}
			}

			return (warnings.ToString(), errors.ToString());
		}

		static ZString CheckCertificateExpireIn30Days(IEnumerable<JobRequiredDocument> requiredDocuments)
		{
			var result = new ZStringBuilder();
			var expiredRequiredDocuments = requiredDocuments.Where(x => x.EQ_ValidToDate <= ZDate.Today.AddDays(30));
			if (expiredRequiredDocuments.Any())
			{
				foreach (var expiredRequiredDocument in expiredRequiredDocuments)
				{
					AppendMessage(result, Res.GetString("17381A84-419D-489A-B7B3-1EBD6F90CC4A", "The Exporter Exemption Certificate Number {0} is expiring on {1}.", expiredRequiredDocument.EQ_DocNumber, expiredRequiredDocument.EQ_ValidToDate.ToShortDateString()));
				}
			}

			return result.ToString();
		}

		static ZString CheckExemptionCeilingLimitThreshold(ZString ledger, IEnumerable<JobRequiredDocument> requiredDocuments, OrgHeader organization, InvoicingLineBaseCollection lines)
		{
			var result = new ZStringBuilder();

			var threshold = PreventPosting ? 100 : AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.Value;
			if (threshold == 0)
			{
				return ZString.Empty;
			}

			var totalUsedAmount = GetTotalUsedAmount(ledger, requiredDocuments, organization);
			var totalDocCeilingLimit = ZDecimal.Zero;
			var totalThresholdCeilingLimit = ZDecimal.Zero;
			var noCeilingLimit = false;

			foreach (var companyRequiredDocument in requiredDocuments)
			{
				var thresholdCeilingLimit = ZDecimal.Zero;
				var docCeilingLimit = ZDecimal.Zero;

				if (!noCeilingLimit)
				{
					docCeilingLimit = GetCertificateCeilingLimit(companyRequiredDocument);
					thresholdCeilingLimit = (ZDecimal)(threshold / 100 * docCeilingLimit);
					totalDocCeilingLimit += docCeilingLimit;
					totalThresholdCeilingLimit += thresholdCeilingLimit;
					noCeilingLimit = thresholdCeilingLimit == 0;
				}
			}

			if (!PreventPosting)
			{
				if (!noCeilingLimit && totalUsedAmount > totalThresholdCeilingLimit)
				{
					AppendMessage(result, GetCeilingLimitExceedWarningMessage(totalDocCeilingLimit, totalUsedAmount, threshold));
				}
			}

			if (PreventPosting && ledger == LedgerTypes.AccountsReceivable && lines != null && lines.Any())
			{
				var greaterThanZeroTaxIDLines = lines.OfType<InvoicingLineBase>().Where(l => l.TaxRate != null && !l.TaxRate.IsZeroTaxRate);
				var dichIntTaxIDLines = lines.OfType<InvoicingLineBase>().Where(l => l.TaxRate != null && l.TaxRate.AT_Code == "DICH.INT");
				var currentAmount = (ZDecimal)dichIntTaxIDLines.Sum(l => l.AL_LineAmount);
				var greaterThanZeroTaxIDLinesAmount = (ZDecimal)greaterThanZeroTaxIDLines.Sum(l => l.AL_LineAmount);
				var totalAmount = totalUsedAmount + currentAmount;

				if (dichIntTaxIDLines.Any() && !noCeilingLimit && totalAmount > totalThresholdCeilingLimit)
				{
					var exceedingAmount = totalAmount - totalThresholdCeilingLimit;
					AppendMessage(result, GetCeilingLimitExceedErrorMessage(exceedingAmount));
				}

				if (greaterThanZeroTaxIDLines.Any() && (noCeilingLimit || greaterThanZeroTaxIDLinesAmount > 0 && totalAmount < totalThresholdCeilingLimit))
				{
					var stillAvailableAmount = totalThresholdCeilingLimit - totalAmount;
					var taxIdList = string.Join(", ", greaterThanZeroTaxIDLines.Select(l => l.TaxRate.AT_Code).Distinct());
					AppendMessage(result, GetCeilingLimitStillAvailableErrorMessage(organization.OH_Code, taxIdList, noCeilingLimit ? decimal.Zero : stillAvailableAmount));
				}
			}
			return result.ToString();
		}

		static ZDecimal GetTotalUsedAmount(ZString ledger, IEnumerable<JobRequiredDocument> requiredDocuments, OrgHeader organization)
		{
			var minReceivedDate = requiredDocuments.MinBy(x => x.EQ_DateReceived).EQ_DateReceived.ToZDateTime();
			var maxValidToDate = requiredDocuments.MaxBy(x => x.EQ_ValidToDate).EQ_ValidToDate.ToZDateTime();

			var sql = @"SELECT IIF(AH_Ledger = 'AR', SUM(AL_LineAmount), ABS(SUM(AL_LineAmount))) AS TotalAmount
						FROM dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionLines ON AL_AH = AH_PK
						INNER JOIN dbo.AccTaxRate ON AL_AT = AT_PK
						INNER JOIN dbo.OrgHeader  ON AH_OH = OH_PK
						WHERE AH_GC = @CurrentCompany
							AND AH_Ledger = @Ledger AND AH_TransactionType in ('INV', 'CRD', 'ADJ')
							AND AH_PostDate BETWEEN @ReceivedDate AND DATEADD(minute, -1, DATEADD(day, 1, @ValidToDate))
							AND AT_Code = 'DICH.INT' AND AT_RN_NKCountry = 'IT'
							AND AH_OH = @Organization
						GROUP BY AH_Ledger";

			var parameters = new ZSqlParameterCollection
			{
				{ "@CurrentCompany", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC },
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@ReceivedDate", minReceivedDate, AccTransactionHeaderSchema.AH_PostDate },
				{ "@ValidToDate", maxValidToDate, AccTransactionHeaderSchema.AH_PostDate },
				{ "@Organization", organization.PK, AccTransactionHeaderSchema.AH_OH }
			};

			var totalUsedAmount = organization.Factory.LoadScalarValue<ZDecimal>(sql, parameters);

			return totalUsedAmount;
		}

		static ZDecimal GetCertificateCeilingLimit(JobRequiredDocument requiredDocument)
		{
			var ceilingLimitAttribute = requiredDocument.Attributes.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.CeilingLimit)?.D0_AttribValue;
			if (ceilingLimitAttribute.HasValue)
			{
				var isParsed = JobRequiredDocAttrib.TryParseCeilingLimit(ceilingLimitAttribute.Value, out var ceilingLimit);
				return isParsed ? ceilingLimit : ZDecimal.Zero;
			}
			return ZDecimal.Zero;
		}

		static int LocalCurrencyDecimal => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		static bool PreventPosting => AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.Value;

		static IEnumerable<JobRequiredDocument> GetRequiredDocuments(ZString ledger, OrgHeader organization, ZDateTime checkDate)
		{
			var docUsage = ledger == LedgerTypes.AccountsPayable
						|| ledger == LedgerTypes.IncompleteTransactions
						|| ledger == LedgerTypes.TransactionsPendingAllocation
						|| ledger == LedgerTypes.UnapprovedPayableTransactions ? JobRequiredDocument.DocUsage.Creditor : JobRequiredDocument.DocUsage.Debtor;

			return organization.RequiredDocuments.Cast<JobRequiredDocument>().Where(x => x.EQ_DocCategory == ReferenceTypes.ClientSupplierRelationship
																				&& x.EQ_DocType == RefDocTypes.VATExporterExemption
																				&& x.EQ_DocUsage == docUsage
																				&& x.EQ_RN_NKRelatedCountry == GlbCompany.CurrentCompany.Country.Code
																				&& x.EQ_DateReceived.ToZDateTime() <= checkDate
																				&& checkDate <= x.EQ_ValidToDate
																				&& x.Attributes.Any(z => z.IsCompanyCode && ZGuid.TryParse(z.D0_AttribValue, out var companyPK)
																					&& companyPK == GlbCompany.CurrentCompany.PK));
		}

		static string GetCeilingLimitExceedWarningMessage(ZDecimal totalDocCeilingLimit, ZDecimal totalUsedAmount, decimal threshold)
		{
			return Res.GetString("850127A1-BAEC-4649-904F-CDB3FAE2590C", @"The Exporter Exemption Ceiling Limit Threshold of {0}% has exceeded.
The Total Certificate Ceiling Limit is {1} {2}.
Total Posted Transactions Amount using the exporter exemption certificates is {1} {3}.",
													threshold,
													GlbCompany.CurrentCompany.LocalCurrency.Code,
													totalDocCeilingLimit.ToString(LocalCurrencyDecimal),
													totalUsedAmount.ToString(LocalCurrencyDecimal));
		}

		static string GetCeilingLimitExceedErrorMessage(ZDecimal exceedingAmount)
		{
			return Res.GetString("26D59C8A-7CAA-4780-8076-18B5F7A05F0E", @"There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [{0}] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by {1} {2}.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.",
AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.HumanReadableRegistryPath(),
GlbCompany.CurrentCompany.LocalCurrency.Code,
exceedingAmount.ToString(LocalCurrencyDecimal));
		}

		static string GetNoAvailableEXVDocumentsErrorMessage()
		{
			return Res.GetString("A59CE4BB-4F68-4FC4-A53C-CDA5FCBC5DA4", "If [{0}] is set to Yes, the DICH.INT Tax ID can only be used for a Debtor with a valid Exporter Exemption Certificate, where the certificate Ceiling Limit has not been exceeded.",
				AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.HumanReadableRegistryPath());
		}

		static string GetCeilingLimitStillAvailableErrorMessage(ZString orgCode, ZString taxIdList, ZDecimal stillAvailableAmount)
		{
			return Res.GetString("282D051D-8BDD-4FEE-9244-BD573ADC6C7D", @"{0}: There are errors that need to be corrected before this Accounts Receivable Invoice can be saved.
TAX ID: {1} based on the registry [{2}] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption documents that still have available {3} CEILING LIMIT.
To proceed with the post fix Tax ID Code or disable the above registry item.",
orgCode,
taxIdList,
AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.HumanReadableRegistryPath(),
stillAvailableAmount != ZDecimal.Zero
	? GlbCompany.CurrentCompany.LocalCurrency.Code + " " + stillAvailableAmount.ToString(LocalCurrencyDecimal)
	: string.Empty);
		}

		static void AppendMessage(ZStringBuilder sb, ZString message)
		{
			if (!sb.IsEmpty)
			{
				sb.AppendLine();
			}
			sb.Append(message);
		}
	}
}
