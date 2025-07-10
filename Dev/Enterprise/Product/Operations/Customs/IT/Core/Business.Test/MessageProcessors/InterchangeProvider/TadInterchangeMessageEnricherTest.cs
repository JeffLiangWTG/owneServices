using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class TadInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();
		AssertEquals("PRE-CONDITION: EI_InterchangeType", "", interchange.EI_InterchangeType);

		IEDIInterchangeEnricher interchangeMessageEnricher = new TadInterchangeMessageEnricher();
		_ = interchangeMessageEnricher.Enrich(interchange);
		AssertEquals("POST-CONDITION: EI_InterchangeType", "TAD", interchange.EI_InterchangeType);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		IEDIInterchangeEnricher interchangeMessageEnricher = new TadInterchangeMessageEnricher();
		_ = AssertExceptionThrown<ArgumentNullException>(() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
