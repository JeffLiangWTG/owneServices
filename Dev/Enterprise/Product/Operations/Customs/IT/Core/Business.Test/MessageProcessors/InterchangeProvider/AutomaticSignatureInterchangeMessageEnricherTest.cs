using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AutomaticSignatureInterchangeMessageEnricherTest : TestCaseWithFactory
{
	public void TestEnrich()
	{
		var interchange = Factory.New<EDIInterchange>();
		AssertEquals("Pre-condition: EDIInterchange.EI_HeaderText should be empty initially", ZString.Empty, interchange.EI_HeaderText);

		var mock = new Mock<IAutomaticSignatureExternalPassword>();
		mock.Setup(m => m.DelegateName).Returns("FirstName.LastName");
		mock.Setup(m => m.GP_UserID).Returns("UserName");
		var automaticSignature = mock.Object;

		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new AutomaticSignatureInterchangeMessageEnricher(automaticSignature, SignatureOperation.Amendment);
		interchangeMessageEnricher.Enrich(interchange);

		AssertEquals("When SignatureOperation is Amendment, EI_HeaderText should include MessageSubType=AMD",
			"{\"custom.MessageSubType\":\"AMD\",\"custom.IT.Signature\":\"Y\",\"custom.IT.SignatureDelegatedUser\":\"FirstName.LastName\",\"custom.IT.SignatureUser\":\"UserName\"}",
			interchange.EI_HeaderText);

		interchangeMessageEnricher = new AutomaticSignatureInterchangeMessageEnricher(automaticSignature, SignatureOperation.Cancellation);
		interchangeMessageEnricher.Enrich(interchange);

		AssertEquals("When SignatureOperation is Cancellation, EI_HeaderText should include MessageSubType=CAN",
			"{\"custom.MessageSubType\":\"CAN\",\"custom.IT.Signature\":\"Y\",\"custom.IT.SignatureDelegatedUser\":\"FirstName.LastName\",\"custom.IT.SignatureUser\":\"UserName\"}",
			interchange.EI_HeaderText);

		interchangeMessageEnricher = new AutomaticSignatureInterchangeMessageEnricher(automaticSignature, SignatureOperation.New);
		interchangeMessageEnricher.Enrich(interchange);

		AssertEquals("When SignatureOperation is New, EI_HeaderText should not include MessageSubType",
			"{\"custom.IT.Signature\":\"Y\",\"custom.IT.SignatureDelegatedUser\":\"FirstName.LastName\",\"custom.IT.SignatureUser\":\"UserName\"}",
			interchange.EI_HeaderText);
	}

	public void TestEnrich_WithNullEdiInterchange()
	{
		var automaticSignature = Factory.New<AutomaticSignatureExternalPassword>();
		var interchangeMessageEnricher = (IEDIInterchangeEnricher)new AutomaticSignatureInterchangeMessageEnricher(automaticSignature, SignatureOperation.New);

		AssertExceptionThrown<ArgumentNullException>("Enricher should throw ArgumentNullException when ediInterchange is null",
			() => interchangeMessageEnricher.Enrich(ediInterchange: null));
	}
}
