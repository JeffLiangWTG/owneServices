using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CancellationInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();

		AssertEquals($"[PRE-CONDITION] {nameof(EDIInterchange.EI_HeaderText)}", ZString.Empty, interchange.EI_HeaderText);

		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new CancellationInterchangeMessageEnricher();
		interchangeMessageEnricher.Enrich(interchange);

		AssertEquals($"[POST-CONDITION] {nameof(EDIInterchange.EI_HeaderText)}", "{\"custom.MessageSubType\":\"CAN\"}", interchange.EI_HeaderText);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new CancellationInterchangeMessageEnricher();
		AssertExceptionThrown<ArgumentNullException>(() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
