using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(ConsolCostLine))]
	class ConsolCostLineTest : DataObjectTestCase<ConsolCostLine>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(ConsolCostLine.CostAPInvoiceNumber), JobConsolCostSchema.E6_InvoiceNum.MaxLength },
				{ nameof(ConsolCostLine.ExternalCreditorCode), OrgCompanyDataSchema.OB_APExternalCreditorCode.MaxLength },
				{ nameof(ConsolCostLine.SupplierReference), JobConsolCostSchema.E6_CostReference.MaxLength },
				{ nameof(ConsolCostLine.ApportionmentMethod), JobConsolCostSchema.E6_ApportionmentMethod.MaxLength },
				{ nameof(ConsolCostLine.PrepaidCollectFilter), JobConsolCostSchema.E6_PPDCLT.MaxLength },
				{ nameof(ConsolCostLine.GovernmentReportingSellChargeCode), JobChargeSchema.JR_SellGovtChargeCode.MaxLength },
				{ nameof(ConsolCostLine.GovernmentReportingCostChargeCode), JobChargeSchema.JR_CostGovtChargeCode.MaxLength },
				{ nameof(ConsolCostLine.RatingBehaviour), JobConsolCostSchema.E6_RatingBehaviour.MaxLength },
				{ nameof(ConsolCostLine.SupplyType), JobConsolCostSchema.E6_SupplyType.MaxLength },
			};
		}
	}
}

