using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	[TestedType(typeof(CommercialInvoiceLine))]
	class CommercialInvoiceLineTest : DataObjectTestCase<CommercialInvoiceLine>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(CommercialInvoiceLine.Description),
			nameof(CommercialInvoiceLine.DetailedDescription),
			nameof(CommercialInvoiceLine.LocalDescription)
		};

		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>()
		{
			{ nameof(CommercialInvoiceLine.Description), 525 },
			{ nameof(CommercialInvoiceLine.DataImportMatchingKey), 38 },
			{ nameof(CommercialInvoiceLine.HarmonisedCode), 35 },
			{ nameof(CommercialInvoiceLine.PartNo), 35 },
			{ nameof(CommercialInvoiceLine.ClassificationCode), 35 },
			{ nameof(CommercialInvoiceLine.ConcessionOrder), 15 },
			{ nameof(CommercialInvoiceLine.ContainerNumber), 20 },
			{ nameof(CommercialInvoiceLine.EntryNumber), 35 },
			{ nameof(CommercialInvoiceLine.OrderNumber), 25 },
			{ nameof(CommercialInvoiceLine.Procedure), 7 },
			{ nameof(CommercialInvoiceLine.PrimaryPreference), 10 },
			{ nameof(CommercialInvoiceLine.SecondaryPreference), 10 },
			{ nameof(CommercialInvoiceLine.BrandName), 50 },
			{ nameof(CommercialInvoiceLine.Model), 50 },
			{ nameof(CommercialInvoiceLine.LocalDescription), 525 },
			{ nameof(CommercialInvoiceLine.PreviousEntryNumber), 35 },
			{ nameof(CommercialInvoiceLine.ClassUsageComment), 200 },
			{ nameof(CommercialInvoiceLine.DetailedDescription), 2147483646 },
			{ nameof(CommercialInvoiceLine.BondedWarehouseRemarks), 35 },
			{ nameof(CommercialInvoiceLine.BondedWHSOrderNumber), 35 },
			{ nameof(CommercialInvoiceLine.EntryStatus), 3 },
			{ nameof(CommercialInvoiceLine.FormattedTariff), 35 },
		};
	}
}
