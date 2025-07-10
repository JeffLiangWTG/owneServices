using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(ChargeLine))]
	class ChargeLineTest : DataObjectTestCase<ChargeLine>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(ChargeLine.Description), JobChargeSchema.JR_Desc.MaxLength },
				{ nameof(ChargeLine.CostAPInvoiceNumber), JobChargeSchema.JR_APInvoiceNum.MaxLength },
				{ nameof(ChargeLine.SellPostedTransactionType), AccTransactionHeaderSchema.AH_TransactionType.MaxLength },
				{ nameof(ChargeLine.SellPostedTransactionNumber), AccTransactionHeaderSchema.AH_TransactionNum.MaxLength },
				{ nameof(ChargeLine.SellInvoiceType), JobChargeSchema.JR_InvoiceType.MaxLength },
				{ nameof(ChargeLine.ExternalDebtorCode), OrgCompanyDataSchema.OB_ARExternalDebtorCode.MaxLength },
				{ nameof(ChargeLine.ExternalCreditorCode), OrgCompanyDataSchema.OB_APExternalCreditorCode.MaxLength },
				{ nameof(ChargeLine.SupplierReference), JobChargeSchema.JR_CostReference.MaxLength },
				{ nameof(ChargeLine.SellReference), JobChargeSchema.JR_SellReference.MaxLength },
				{ nameof(ChargeLine.GovernmentReportingSellChargeCode), JobChargeSchema.JR_SellGovtChargeCode.MaxLength },
				{ nameof(ChargeLine.GovernmentReportingCostChargeCode), JobChargeSchema.JR_CostGovtChargeCode.MaxLength },
				{ nameof(ChargeLine.CostSupplyType), JobChargeSchema.JR_CostSupplyType.MaxLength },
				{ nameof(ChargeLine.SellSupplyType), JobChargeSchema.JR_SellSupplyType.MaxLength },
			};
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(ChargeLine.Description)
		};
	}
}

