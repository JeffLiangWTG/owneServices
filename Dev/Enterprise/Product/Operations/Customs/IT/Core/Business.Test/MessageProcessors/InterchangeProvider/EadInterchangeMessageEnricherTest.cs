using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EadInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();
		AssertEquals("PRE-CONDITION: EI_InterchangeType", "", interchange.EI_InterchangeType);

		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new EadInterchangeMessageEnricher();
		interchangeMessageEnricher.Enrich(interchange);
		AssertEquals("POST-CONDITION: EI_InterchangeType", "EAD", interchange.EI_InterchangeType);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new EadInterchangeMessageEnricher();
		AssertExceptionThrown<ArgumentNullException>(() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
