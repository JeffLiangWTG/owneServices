using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IvistoInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();
		AssertEquals("PRE-CONDITION: EI_InterchangeType", "", interchange.EI_InterchangeType);

		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new IvistoInterchangeMessageEnricher();
		interchangeMessageEnricher.Enrich(interchange);
		AssertEquals("POST-CONDITION: EI_InterchangeType", "IVI", interchange.EI_InterchangeType);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new IvistoInterchangeMessageEnricher();
		AssertExceptionThrown<ArgumentNullException>(() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
