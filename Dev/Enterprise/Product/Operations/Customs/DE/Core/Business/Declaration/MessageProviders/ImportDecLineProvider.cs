using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.DE.Business
{
	public abstract class ImportDecLineProvider : IImportDecLine
	{
		protected ImportDecLineProvider(CusEntryLine entryLine)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			EntryHeader = EntryLine.Header;
			Declaration = EntryHeader.Declaration;
			RandomInvoiceLine = EntryLine.RandomLine;
			RandomInvoiceHeader = RandomInvoiceLine.InvoiceHeader;
			InvoiceLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>();
		}
		protected readonly CusEntryLine EntryLine;
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;
		protected readonly JobComInvoiceLine RandomInvoiceLine;
		protected readonly JobComInvoiceHeader RandomInvoiceHeader;
		protected readonly IEnumerable<JobComInvoiceLine> InvoiceLines;

		public int SequenceNumber => EntryLine.CL_LineNumber;

		public string RequestedPreviousProcedure => RandomInvoiceLine.JI_Procedure.Left(4);

		public string GoodsDescription => RandomInvoiceLine.JI_Description;

		public decimal NetMassMeasure => CachedValueHelper.GetValue(ref netMassMeasure, () => InvoiceLines.Sum(x => x.JI_CustomsQuantity));
		CachedValue<decimal> netMassMeasure;

		public string OriginCountry => RandomInvoiceLine.JI_CountryOfOrigin;

		public string SupplementaryInformation => RandomInvoiceLine.SupplementaryInformation;

		public string CommodityCode => RandomInvoiceLine.JI_Tariff;

		public IReadOnlyCollection<string> AdditionalProcedure => additionalProcedure ?? (additionalProcedure = GetAdditionalProcedure());
		IReadOnlyCollection<string> additionalProcedure;

		string[] GetAdditionalProcedure()
		{
			var list = new List<string>();
			var addProcedure = RandomInvoiceLine.JI_Procedure;
			if (!addProcedure.IsEmpty)
			{
				list.Add(addProcedure.SubstringSafe(4, 3));
			}
			var addProcedures = RandomInvoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>()
				.Where(x => x.CY_Type == EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode)
				.Select(x => (string)x.CY_Code.SubstringSafe(4, 3));
			return list.Concat(addProcedures).ToArray();
		}

		public IReadOnlyCollection<string> SupplementaryCodes => supplementaryCodes ?? (supplementaryCodes = GetSupplementaryCodes());
		IReadOnlyCollection<string> supplementaryCodes;

		string[] GetSupplementaryCodes()
		{
			var list = new List<string>();
			if (!RandomInvoiceLine.JI_SupplementaryCode1.IsEmpty)
			{
				list.Add(RandomInvoiceLine.JI_SupplementaryCode1);
			}
			if (!RandomInvoiceLine.JI_SupplementaryCode2.IsEmpty)
			{
				list.Add(RandomInvoiceLine.JI_SupplementaryCode2);
			}

			var suppCodes = RandomInvoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>()
				.Where(x => x.CY_Type == EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode).Select(x => (string)x.CY_Code);

			return list.Concat(suppCodes).ToArray();
		}

		public IImportPackage Package => CachedValueHelper.GetValue(ref package, () => ImportPackageProvider.NewOrNull(EntryLine.PackagingDetails));
		CachedValue<IImportPackage> package;

		public decimal ForeignTradeStatisticsQuantity => CachedValueHelper.GetValue(ref foreignTradeStatisticsQuantity, () =>
		{
			var result = InvoiceLines.Sum(x => x.JI_Calc_StatisticalValue).Round(0);
			return result == decimal.Zero ? 1m : result;
		});
		CachedValue<decimal> foreignTradeStatisticsQuantity;

		public decimal ForeignTradeStatisticsGrossMassMeasure => CachedValueHelper.GetValue(ref foreignTradeStatisticsGrossMassMeasure, () =>
		{
			if (InvoiceLines.Any(l => l.InvoiceHeader.JZ_Weight > 0))
			{
				return decimal.Zero;
			}
			else
			{
				var summedGrossWeightInKg = InvoiceLines.Sum(x => new ZWeight(x.JI_Weight, x.JI_WeightUQ).InKilogramsSafe).Round(1).Normalize();
				return Math.Max(summedGrossWeightInKg, 0.1m);
			}
		});
		CachedValue<decimal> foreignTradeStatisticsGrossMassMeasure;

		public decimal AssessmentCustomsValue => CachedValueHelper.GetValue(ref assessmentCustomsValue, () =>
		{
			if (!IsHighValueOvrdValidForAssessmentCustomsValue || RandomInvoiceLine.IsProcedureInE01OrE02)
			{
				return decimal.Zero;
			}
			return InvoiceLines.Sum(x => x.JI_CustomsValue).RoundAndNormalize(2);
		});
		CachedValue<decimal> assessmentCustomsValue;

		public IReadOnlyCollection<IAmount> AssessmentAmount
		{
			get
			{
				if (assessmentAmount == null)
				{
					var list = new List<IAmount>();

					var thirdQty = InvoiceLines.Sum(x => x.JI_CustomsThirdQuantity);
					var fourthQty = InvoiceLines.Sum(x => x.JI_CustomsFourthQuantity);

					if (thirdQty > 0)
					{
						var thirdQtyProvider = new AmountProvider(thirdQty, RandomInvoiceLine.JI_CustomsThirdUnitQty);
						list.Add(thirdQtyProvider);
					}
					if (fourthQty > 0)
					{
						var fourthQtyProvider = new AmountProvider(fourthQty, RandomInvoiceLine.JI_CustomsFourthUnitQty);
						list.Add(fourthQtyProvider);
					}

					assessmentAmount = list.ToArray();
				}
				return assessmentAmount;
			}
		}
		IReadOnlyCollection<IAmount> assessmentAmount;

		public IReadOnlyCollection<IImportSpecificRate> AssessmentSpecificRate => assessmentSpecificRate ?? (assessmentSpecificRate =
			InvoiceLines.SelectMany(l => l.Charges.Cast<InvoiceLineCharge>())
			.Where(c => ImportChargeCodeList.IsSpecificRate(c.J7_ChargeType))
			.GroupBy(c => c.J7_ChargeType)
			.Select(g => new ImportSpecificRateProvider(g.Key, g.Sum(c => c.J7_Amount)))
			.ToArray());
		IReadOnlyCollection<IImportSpecificRate> assessmentSpecificRate;

		public IReadOnlyCollection<IContentInformation> AssessmentContentInformation => assessmentContentInformation ?? (assessmentContentInformation = RandomInvoiceLine.JI_CustomsThirdQuantity > 0
			? RandomInvoiceLine.ContentInformationTypes.Cast<ContentInformationType>()
					.Where(c => c.CY_Type == CusCodeDataTypeList.Codes.ContentInformationType)
					.Select(c => new ContentInformationProvider(c))
					.ToArray()
			: Array.Empty<IContentInformation>());
		IReadOnlyCollection<IContentInformation> assessmentContentInformation;

		public IReadOnlyCollection<IExciseDuty> ExciseDuty => exciseDuty ?? (exciseDuty = RandomInvoiceLine.CusLineTariffDetails
			.Where(c => c.BZ_Type == "EXC")
			.Select(c => new ExciseDutyProvider(c))
			.ToArray());
		IReadOnlyCollection<IExciseDuty> exciseDuty;

		public IReadOnlyCollection<IImportLineDocument> Documents => documents ?? (documents =
			InvoiceLines.SelectMany(l => l.SupportingDocuments.Cast<SupportingDocument>())
			.GroupBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber })
			.Select(g => new ImportLineDocumentProvider(g.First(), g.Sum(d => d.CSI_Quantity)))
			.ToArray());
		IReadOnlyCollection<IImportLineDocument> documents;

		protected BusinessObjectFactory Factory => EntryLine.Factory;

		protected virtual bool IsHighValueOvrdValidForAssessmentCustomsValue => !Declaration.ZG_IsHighValueOvrd;
	}
}
