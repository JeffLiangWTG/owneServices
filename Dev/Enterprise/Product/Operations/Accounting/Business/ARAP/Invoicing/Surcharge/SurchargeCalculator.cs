using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface ISurchargeCalculator
	{
		IEnumerable<SurchargeCalculationData> GetSurchargeCalculationData(InvoicingBase invoiceBase);

		IEnumerable<InvoicingLineBase> GetSurchargeLines(InvoicingBase invoiceBase);
	}

	public class SurchargeCalculator : ISurchargeCalculator
	{
		IEnumerable<SurchargeCalculationData> ISurchargeCalculator.GetSurchargeCalculationData(InvoicingBase invoiceBase)
		{
			Argument.NotNull(invoiceBase, nameof(invoiceBase));

			var applicableSurcharges = invoiceBase.Header.CompanyData.OB_ARApplicableSurcharges.Split(',');
			if (applicableSurcharges.FirstOrDefault() != AccountingMasterFilesConstants.ReserveSurchargeCodes.Non)
			{
				var surchargeConfig = ObjectFactory.Get<ISurchargeConfig>();
				var surchargeGroupDataList = new List<SurchargeGroupData>();
				var allApplications = invoiceBase.Factory.Load<AccSurchargeApplication>(new ZQuery(AccSurchargeApplicationSchema.ASP_GC_Company, invoiceBase.AH_GC));

				if (allApplications.Any())
				{
					foreach (TransactionLine line in invoiceBase.Lines.Cast<TransactionLine>().Where(x => !x.AL_AC.IsEmpty))
					{
						var surchargeApplications = GetSurchargeApplications(allApplications, line);
						var surcharges = FilterARApplicableSurcharges(applicableSurcharges, surchargeApplications.Select(x => x.ASP_ASC_NKSurchargeCode));
						foreach (var surcharge in surcharges)
						{
							var (rate, chargeCodePK) = surchargeConfig.GetSurcharge(surcharge, line.AL_AC);
							if (rate != 0m)
							{
								surchargeGroupDataList.Add(new SurchargeGroupData(line, chargeCodePK, surcharge, rate));
							}
						}
					}

					var groupedData = surchargeGroupDataList.GroupBy(x => new { x.JobPK, x.ChargeCodePK, x.SurchargeCode, x.Rate })
						.Select(y => new
						{
							JobPK = y.Key.JobPK,
							Rate = y.Key.Rate,
							SurchargeCode = y.Key.SurchargeCode,
							ChargeCodePK = y.Key.ChargeCodePK,
							Currency = y.First().Currency,
							Amount = y.Sum(s => s.Amount),
							LinePKs = y.Select(z => z.LinePK),
						});

					foreach (var data in groupedData.Where(x => x.Amount > 0))
					{
						var calculatedAmount = Utilities.Round(data.Amount * data.Rate / 100m, data.Currency.Decimals);
						yield return new SurchargeCalculationData(data.LinePKs, data.JobPK, data.ChargeCodePK, calculatedAmount, data.SurchargeCode);
					}
				}
			}
		}

		public IEnumerable<InvoicingLineBase> GetSurchargeLines(InvoicingBase invoiceBase)
		{
			Argument.NotNull(invoiceBase, nameof(invoiceBase));

			var surchargeCodes = invoiceBase.Company.AccSurchargeConfigurations
				.Select(x => x.ASC_AC_ChargeCode)
				.ToHashSet();

			if (!surchargeCodes.Any())
			{
				return Array.Empty<InvoicingLineBase>();
			}

			return invoiceBase.Lines.Cast<InvoicingLineBase>()
				.Where(x => surchargeCodes.Contains(x.AL_AC))
				.ToArray();
		}

		IEnumerable<ZString> FilterARApplicableSurcharges(IEnumerable<ZString> applicableSurcharges, IEnumerable<ZString> surchargesInApplication)
		{
			return applicableSurcharges.FirstOrDefault() == AccountingMasterFilesConstants.ReserveSurchargeCodes.All
				? surchargesInApplication
				: surchargesInApplication.Where(applicableSurcharges.Contains);
		}

		IEnumerable<AccSurchargeApplication> GetSurchargeApplications(AccSurchargeApplication[] surchargeApplications, TransactionLine line)
		{
			var country = line.Company.Country ?? GlbCompany.CurrentCompany.Country;
			var orgCategoryValue = line.TransactionHeader.Header != null ? line.TransactionHeader.Header.OH_Category : ZString.Empty;
			JobInvoicingConsumerType consumerType = line.InvoicingJob?.JobType;
			var jobType = consumerType != null ? consumerType.Code : string.Empty;
			var homeCountryOrZone = AccountingTaxLocations.GetOrganisationHomeCountry(line.Factory, country.Code, line.TransactionHeader.Header);
			var placeOfSupply = AccountingTaxLocations.GetOrganisationHomeCountry(line.Factory, country.Code, fixedPlaceOfSupplyLocation: line.PlaceOfSupplyLocation);

			return surchargeApplications.Where(x =>
				(string.IsNullOrEmpty(jobType) ? x.ASP_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated : (x.ASP_JobType == jobType || x.ASP_JobType == AccSurchargeApplication.ALL))
					&& (x.ASP_OrganizationCategory == orgCategoryValue || x.ASP_OrganizationCategory == AccSurchargeApplication.ALL)
					&& homeCountryOrZone.Contains(x.ASP_HomeCountryOrZone)
					&& (!PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled() || placeOfSupply.Contains(x.ASP_PlaceOfSupply))
					&& (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value || (x.ASP_SupplyType == line.AL_SupplyType || x.ASP_SupplyType == string.Empty))
					&& (x.ASP_AT == ZGuid.Empty || x.ASP_AT == line.AL_AT))
				.GroupBy(x => x.ASP_ASC_NKSurchargeCode).Select(x => x.First());
		}

		class SurchargeGroupData
		{
			public SurchargeGroupData(TransactionLine line, ZGuid chargeCodePK, ZString surchargeCode, ZDecimal rate)
			{
				LinePK = line.PK;
				Currency = line.TransactionCurrency;
				ChargeCodePK = chargeCodePK;
				JobPK = line.AL_JH;
				SurchargeCode = surchargeCode;
				Rate = rate;
				Amount = line.AL_OSExTaxAmount;
			}

			public ZGuid LinePK { get; }
			public RefCurrency Currency { get; }
			public ZGuid ChargeCodePK { get; }
			public ZGuid JobPK { get; }
			public ZString SurchargeCode { get; }
			public ZDecimal Rate { get; }
			public ZDecimal Amount { get; }
		}
	}

	public readonly struct SurchargeCalculationData
	{
		public SurchargeCalculationData(IEnumerable<ZGuid> linePKs, ZGuid jobPK, ZGuid chargePK, ZDecimal calculatedAmount, ZString surchargeCode)
		{
			LinePKs = linePKs;
			JobPK = jobPK;
			ChargePK = chargePK;
			CalculatedAmount = calculatedAmount;
			SurchargeCode = surchargeCode;
		}

		public IEnumerable<ZGuid> LinePKs { get; }
		public ZGuid JobPK { get; }
		public ZGuid ChargePK { get; }
		public ZDecimal CalculatedAmount { get; }
		public ZString SurchargeCode { get; }
	}
}
