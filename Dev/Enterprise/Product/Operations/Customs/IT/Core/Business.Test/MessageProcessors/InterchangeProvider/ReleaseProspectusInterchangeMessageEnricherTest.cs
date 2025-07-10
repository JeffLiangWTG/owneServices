using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ReleaseProspectusInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();
		AssertEquals("PRE-CONDITION: EI_InterchangeType", "", interchange.EI_InterchangeType);

		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new ReleaseProspectusInterchangeMessageEnricher();
		interchangeMessageEnricher.Enrich(interchange);
		AssertEquals("POST-CONDITION: EI_InterchangeType", "SVI", interchange.EI_InterchangeType);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new ReleaseProspectusInterchangeMessageEnricher();
		AssertExceptionThrown<ArgumentNullException>(() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
