using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Messages.CUSRES;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class InformationRequestProviderTest : Customs.Business.Testing.DataProviderTestCase<InformationRequestProvider, IInformationRequest>
{
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new InformationRequestProvider(null), "Null Argument");
		AssertNoExceptionThrown("Valid Arguments", () => new InformationRequestProvider(new SegmentGroup4()));
	}

	public void TestErrorSegment() => AssertEquals(ExpectedErrorSegment, Provider.ErrorSegment);

	public void TestRequestType() => AssertEquals(ExpectedRequestType, Provider.RequestType);

	public void TestResponseDetails() => AssertType<ZString>(Provider.ResponseDetails.Single());

	protected override InformationRequestProvider GetProvider()
	{
		var segmentGroup4 = new SegmentGroup4();
		var eRCSegment = segmentGroup4.ERC.InstantiateAChildAndAddItToChildrenCollection();
		var eRPSegment = segmentGroup4.ERP.InstantiateAChildAndAddItToChildrenCollection();
		var fTXSegment = segmentGroup4.FTX.InstantiateAChildAndAddItToChildrenCollection();

		eRCSegment.ApplicationErrorDetail.ApplicationErrorCode = ExpectedRequestType;
		eRPSegment.ErrorSegmentPointDetails.SegmentTagIdentifier = ExpectedErrorSegment;
		fTXSegment.TextLiteral.FreeText1 = "Text";
		return new InformationRequestProvider(segmentGroup4);
	}

	const string ExpectedRequestType = "2";
	const string ExpectedErrorSegment = "NAD";
}
