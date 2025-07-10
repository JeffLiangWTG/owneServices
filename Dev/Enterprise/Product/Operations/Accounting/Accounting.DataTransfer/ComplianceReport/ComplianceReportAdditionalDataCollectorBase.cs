//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CargoWise.Common;
//using CargoWise.Types;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
//using Enterprise.UniversalDataBuss.DataObjects.Accounting;
//using UniversalCodeDescription = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
//using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
//using UniversalDebitCredit = Enterprise.UniversalDataBuss.DataObjects.Accounting.DebitCredit;
//using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
//using UniversalRegNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
//using UniversalRegNumberType = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumberType;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	public abstract class ComplianceReportAdditionalDataCollectorBase<H> : ComplianceReportAdditionalDataCollector where H : class, IComplianceReportTransactionHeaderDetails
	{
		public ComplianceReportAdditionalDataCollectorBase(AccComplianceReport report, ComplianceReportDataCollectionMode mode = ComplianceReportDataCollectionMode.None) : base(report, mode)
		{
			if (mode != ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				HeaderData = GetTransactionHeaderDetails(Report);
			}
		}

		public override IComplianceReportTransactionHeaderDetails GetTransactionHeader(ZGuid headerPK) => GetTransactionHeaderCore(headerPK);

		internal protected Dictionary<ZGuid, H> HeaderData { get; protected set; }

		internal protected abstract Dictionary<ZGuid, H> GetTransactionHeaderDetails(AccComplianceReport report);

		internal H GetTransactionHeaderCore(ZGuid headerPK)
		{
			HeaderData.TryGetValue(headerPK, out var result);
			return result;
		}
	}
}
