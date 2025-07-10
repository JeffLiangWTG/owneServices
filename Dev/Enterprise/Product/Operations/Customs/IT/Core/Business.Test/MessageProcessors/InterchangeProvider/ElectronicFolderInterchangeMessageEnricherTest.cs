using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ElectronicFolderInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();

		AssertEquals($"[PRE-CONDITION] {nameof(EDIInterchange.EI_To)}", ZString.Empty, interchange.EI_To);
		AssertEquals($"[PRE-CONDITION] {nameof(EDIInterchange.EI_InterchangeType)}", ZString.Empty, interchange.EI_InterchangeType);

		var interchangeMessageEnricher = new ElectronicFolderInterchangeMessageEnricher();
		interchangeMessageEnricher.Enrich(interchange);

		AssertEquals(nameof(EDIInterchange.EI_To), "ITCustomsSOAP", interchange.EI_To);
		AssertEquals(nameof(EDIInterchange.EI_InterchangeType), "EFQ", interchange.EI_InterchangeType);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		var interchangeMessageEnricher = new ElectronicFolderInterchangeMessageEnricher();
		AssertExceptionThrown<ArgumentNullException>(() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
