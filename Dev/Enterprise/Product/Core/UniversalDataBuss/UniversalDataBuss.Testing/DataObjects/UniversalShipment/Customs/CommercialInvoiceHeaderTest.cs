using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(CommercialInvoiceHeader))]
	class CommercialInvoiceHeaderTest : DataObjectTestCase<CommercialInvoiceHeader>
	{
		public void TestSetCommercialInvoiceLineCollection()
		{
			var commercialInvoiceHeader = new CommercialInvoiceHeader(new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection)));
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			commercialInvoiceHeader.SetCommercialChargeCollection(() => new List<CommercialCharge>());
			AssertNull("Should be null when WritingStrategy does allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
			AssertNotNull("CommercialChargeCollection", commercialInvoiceHeader.CommercialChargeCollection);
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(CommercialInvoiceHeader.MarksAndNumbers)
		};
	}
}

