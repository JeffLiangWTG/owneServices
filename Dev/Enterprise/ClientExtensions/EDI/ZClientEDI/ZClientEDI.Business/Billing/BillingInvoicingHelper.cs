using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class BillingInvoicingHelper
	{
		public static byte[] GetRawDocumentInPdf(StmTemplate template, DocBaseWrapper wrapper, string language = null)
		{
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			using (DocumentPack documentPack = new DocumentPack())
			{
				if (!string.IsNullOrEmpty(language))
				{
					documentPack.Language = language;
				}

				using (Report report = new Report(documentPack, excelTemplate, wrapper, "Report", null, DocumentDirection.ANY, false))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					ValidateReport(report);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						using (MemoryStream pdfStream = new MemoryStream())
						{
							excelInterface.ExportToPdfAndScale(pdfStream, 100);
							return pdfStream.ToArray();
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public static (byte[] rawDocument, string fileExtensionInLowerCase) GetRawDocumentInExcel(StmTemplate template, DocBaseWrapper wrapper, string language = null)
		{
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			using (DocumentPack documentPack = new DocumentPack())
			{
				if (!string.IsNullOrEmpty(language))
				{
					documentPack.Language = language;
				}

				using (Report report = new Report(documentPack, excelTemplate, wrapper, "Report", null, DocumentDirection.ANY, false))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					ValidateReport(report);
					return (outputStream.ToArray(), ((string)((IDeliverable)report).FileExtension).ToLowerInvariant());
				}
			}
		}

		public static void AddAttachmentInPdf(IDocManagerSupport docManagerSupport, StmTemplate template, DocBaseWrapper wrapper, string attachmentNameWithoutExtension, string documentType, string language = null)
		{
			if (!Globals.IsTest)
			{
				byte[] rawDocument = GetRawDocumentInPdf(template, wrapper, language);
				var nameWithExtension = attachmentNameWithoutExtension + '.' + BillingConstants.FileExtensions.Pdf;
				docManagerSupport.DocManagerInfo.AddFileOrDocument(rawDocument, nameWithExtension, documentType);
			}
		}

		public static void AddAttachmentInExcel(IDocManagerSupport docManagerSupport, StmTemplate template, DocBaseWrapper wrapper, string attachmentNameWithoutExtension, string documentType, string language = null)
		{
			if (!Globals.IsTest)
			{
				(byte[] rawDocument, string fileExtension) = GetRawDocumentInExcel(template, wrapper, language);
				var nameWithExtension = attachmentNameWithoutExtension + '.' + fileExtension;
				docManagerSupport.DocManagerInfo.AddFileOrDocument(rawDocument, nameWithExtension, documentType);
			}
		}

		public static AccChargeCode GetChargeCode(GlbBranch branch, string code)
		{
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_GC, branch.Company.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, code);

			AccChargeCode chargeCode = branch.Factory.LoadTop1<AccChargeCode>(query);
			if (chargeCode == null)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Could not load charge code '{0}' for branch '{1}'", code, branch.GB_BranchName);
				throw new ArgumentException(message);
			}

			return chargeCode;
		}

		public static ZGuid GetChargeCodePK(GlbBranch branch, string code)
		{
			return GetChargeCode(branch, code).PK;
		}

		public static ZGuid GetChargeCodePKOrEmpty(GlbBranch branch, string code)
		{
			return !string.IsNullOrEmpty(code)
				? GetChargeCode(branch, code).PK
				: ZGuid.Empty;
		}

		public static ZDecimal GetAmountInInvoiceCurrency(ZDecimal amount, ZDateTime dateForExchangeRate, ZString sourceCurrencyCode, ZString invoiceCurrencyCode, GlbBranch invoicingBranch, BusinessObjectFactory branchFactory)
		{
			RefCurrency invoiceCurrency = branchFactory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, invoiceCurrencyCode);

			if (invoiceCurrency != null)
			{
				ZDecimal exchangeRate = GetExchangeRate(dateForExchangeRate, sourceCurrencyCode, invoiceCurrencyCode, invoicingBranch, branchFactory);
				return ZArchitecture.Core.Utilities.Round(amount * exchangeRate, invoiceCurrency.Decimals);
			}
			else
			{
				return 0m;
			}
		}

		public static ZDecimal GetExchangeRate(ZDateTime dateForExchangeRate, ZString sourceCurrencyCode, ZString invoiceCurrencyCode, GlbBranch invoicingBranch)
		{
			return GetExchangeRate(dateForExchangeRate, sourceCurrencyCode, invoiceCurrencyCode, invoicingBranch, invoicingBranch.Factory);
		}

		public static ZDecimal GetExchangeRate(ZDateTime dateForExchangeRate, ZString sourceCurrencyCode, ZString invoiceCurrencyCode, GlbBranch invoicingBranch, BusinessObjectFactory branchFactory)
		{
			RefCurrency sourceCurrency = branchFactory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, sourceCurrencyCode);
			RefCurrency invoiceCurrency = branchFactory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, invoiceCurrencyCode);

			if (sourceCurrency == null || invoiceCurrency == null || invoicingBranch == null)
			{
				return 0;
			}
			else if (sourceCurrencyCode == invoiceCurrencyCode)
			{
				return 1;
			}

			ZDecimal bigEnoughAmount = 100500;
			ZDecimal amountInInvoiceCurrency = 0;
			using ((invoicingBranch.PK != Env.CurrentBranch.PK) ? invoicingBranch.SetAsTemporaryContext() : null)
			{
				amountInInvoiceCurrency = sourceCurrency.ConvertUsingSellRate(dateForExchangeRate, bigEnoughAmount, invoiceCurrency);
			}

			return ZArchitecture.Core.Utilities.Round(amountInInvoiceCurrency / bigEnoughAmount, 5);
		}

		public static ZDateTime GetDateForExchangeRate(ARInvoice invoice) => ExchangeRateCalculator.GetExchangeRateDate(invoice);

		public static ZDateTime GetDateForExchangeRate(ZDateTime date, bool isLocalCurrency, ZGuid companyPK)
			=> ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, isLocalCurrency, companyPK, date, date, date);

		public static InvoicingLineBase AddAmountLine(InvoicingBase invoice, ZDecimal amountInInvoiceCurrency, ZString amountChargeCode, AccTaxRate taxRate, ZString description)
		{
			Argument.NotNull(invoice, "Invoice");

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, amountChargeCode);
			line.AL_LineType = Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_OSExTaxAmount = amountInInvoiceCurrency;
			if (!description.IsEmpty)
			{
				line.AL_Desc = description;
			}

			if (taxRate != null)
			{
				line.AL_AT = taxRate.PK;
			}
			else if (line.ChargeCode.AC_AT_GSTRate.IsValid)
			{
				line.AL_AT = line.ChargeCode.AC_AT_GSTRate;
			}

			return line;
		}

		public static InvoicingLineBase AddAmountLine(InvoicingBase invoice, ZDecimal amount, ZDateTime dateForExchangeRate, ZString amountChargeCodeName, ZString currencyCode, AccTaxRate taxRate, ZString description)
		{
			ZDecimal amountInInvoiceCurrency = BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, currencyCode, invoice.TransactionCurrency.RX_Code, invoice.Branch, invoice.Branch.Factory);
			return BillingInvoicingHelper.AddAmountLine(invoice, amountInInvoiceCurrency, amountChargeCodeName, taxRate, description);
		}

		public static void AddCommentLine(InvoicingBase invoice, ZString comment)
		{
			comment = comment.Trim();
			if (!comment.IsEmpty)
			{
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.GenericCharge = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, EDIDataRegistry.Instance.CommentChargeCode.Value);
				line.AL_AT = ZGuid.Empty;
				line.AL_AG = ZGuid.Empty;
				line.AL_Desc = comment;
			}
		}

		public static IDisposable BranchContext(ZGuid branchPk)
		{
			return !branchPk.IsEmpty ? BranchContext(branchPk.ToGuid()) : null;
		}

		public static IDisposable BranchContext(Guid branchPk)
		{
			IDisposable result = null;
			if (branchPk != Env.CurrentBranch.PK)
			{
				var context = new TemporaryUserContext();
				context.BranchPK = branchPk;
				result = context.Set();
			}
			return result;
		}

		/// <summary>
		/// Synchronize a new lazy created, unique, child bizobj with an existing child.
		/// If two users cause the child object to be created synchronize is needed
		/// to prevent them both saving new child records. The second user to save
		/// must check for an existing child record and overwrite that record rather than
		/// creating a second child.
		/// </summary>
		public static void SynchronizeChild(BusinessObject parent, BusinessObject child, CargoWise.Schema.SchemaColumn childParentKey)
		{
			Argument.NotNull(parent, "parent");
			Argument.NotNull(parent.Factory, "Factory");
			if (parent.IsInDatabase && !child.IsInDatabase && !child.IsDeleted && child.HasChanges)
			{
				BusinessObject[] existing = parent.Factory.Load(child.GetType(), new ZQuery(childParentKey, parent.PK));
				foreach (var item in existing)
				{
					if (item.PK != child.PK)
					{
						item.CopyPersistentValuesFrom(child);
						child.Delete();
						break;
					}
				}
			}
		}

		public static void CreateOdplMinimumFeeRevenueBreakdown(IEnumerable<SystemMinimumFee> minimumFees, ARInvoice invoice, ZDateTime periodStart, ZDecimal feePercentage)
		{
			foreach (var group in minimumFees.GroupBy(x =>
						new { ChargeCode = x.ChargeCode,
							  DatabasePk = x.DatabasePk,
							  PriceItemPk = x.PriceItem?.PK ?? ZGuid.Empty,
							  PriceCode = x.PriceItem?.L7_Code ?? ZString.Empty,
							  Currency = x.Currency,
							  IsNonProductionSystemFee = x.IsNonProductionSystemFee
						}))
			{
				var amount = group.Sum(x => x.Amount);

				if (amount != 0m)
				{
					var groupKey = group.Key;
					var preDiscount = amount;
					var processingFee = feePercentage / 100m * amount;
					var postDiscount = amount + processingFee;
					var dateForExchangeRate = GetDateForExchangeRate(invoice);
					var currency = groupKey.Currency;
					var chargeCodePk = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, groupKey.ChargeCode);
					var discountChargeCodePk = ZGuid.Empty;
					if (processingFee != 0m)
					{
						var discountChargeCode = processingFee > 0m
						? EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode.Value
						: EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
						discountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, discountChargeCode);
					}

					var billed = invoice.Factory.New<EdiBilledUsage>();
					billed.BU9_LD = groupKey.DatabasePk;
					billed.BU9_L7 = groupKey.PriceItemPk;
					billed.BU9_PriceCode = groupKey.PriceCode;
					billed.BU9_AC_AmountChargeCode = chargeCodePk;
					billed.BU9_AC_DiscountChargeCode = discountChargeCodePk;
					billed.BU9_AH_Invoice = invoice.PK;
					billed.BU9_UsageCode = BillingConstants.BillingSystem.ODM;
					billed.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
					billed.BU9_UsageSubCode = groupKey.IsNonProductionSystemFee ? "#NP" : "#MF";
					billed.BU9_UnitCount = group.Count();
					billed.BU9_UnitPrice = amount / group.Count();
					billed.BU9_LocalAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_LocalAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_LocalProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_PeriodStart = periodStart.Date;
					billed.BU9_PriceCurrency = currency;
					billed.BU9_TransactionAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_TransactionAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_TransactionProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
				}
			}
		}

		static void ValidateReport(Report report)
		{
			var reportError = report.GetReportErrors();
			if (reportError?.Any() ?? false)
			{
				var bizObj = report.BODocDataProvider?.ParentBusinessObject;
				var stlMonthlyUsage = bizObj as StlMonthlyUsage;
				var reportErrorsAsString =
					string.Join(System.Environment.NewLine, reportError
						.Select(x => $"IsFatal={x.Type?.IsFatal}|Severity={x.Type?.Severity}|EnumValueName={x.Type?.EnumValueName}|Message={x.Message}"));

				var errorMessage = new ZStringBuilder();
				errorMessage.AppendLine($"TemplateName={report.Template?.TemplateName}");
				errorMessage.AppendLine($"BizObj={bizObj?.GetType().FullName}");
				if (stlMonthlyUsage != null)
				{
					errorMessage.AppendLine($"StlMonthlyUsage.PeriodStart={stlMonthlyUsage.PeriodStart.ToBestReadableDateTimeString()}");
					errorMessage.AppendLine($"StlMonthlyUsage.Bill.OrganisationCode={stlMonthlyUsage.Bill?.OrganisationCode}");
				}
				errorMessage.AppendLine($"ReportErrors={reportErrorsAsString}");

				ExceptionReporter.Instance.ReportDeveloperException("BillingInvoicingHelper.ValidateReport()", errorMessage.ToString(), new InvalidOperationException(errorMessage.ToString()));
			}
		}
	}
}

