using System;
using System.Linq;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Messages.CONTRL;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class SegmentErrorProviderTest : Customs.Business.Testing.DataProviderTestCase<SegmentErrorProvider, ISegmentErrorProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new SegmentErrorProvider(null), "Null Argument");
		AssertNoExceptionThrown("Valid Arguments", () => new SegmentErrorProvider(new SegmentGroup2()));
	});

	public void TestSegmentPosition() => AssertEquals(ExpectedSegmentPosition, Provider.SegmentPosition);

	public void TestSegmentSyntaxErrorCode() => AssertEquals(ExpectedSyntaxErrorCode, Provider.SegmentSyntaxErrorCode);

	public void TestDataElementErrors() => AssertType<SyntaxErrorProvider>(Provider.DataElementErrors.Single());

	protected override SegmentErrorProvider GetProvider()
	{
		var segmentGroup2 = new SegmentGroup2();
		var uCSSegment = segmentGroup2.UCS.InstantiateAChildAndAddItToChildrenCollection();
		var uCDSegment = segmentGroup2.UCD.InstantiateAChildAndAddItToChildrenCollection();
		uCSSegment.SegmentPositionInMessageBody = ExpectedSegmentPosition;
		uCSSegment.SyntaxErrorCoded = SyntaxErrorCodedList.GetFromString(ExpectedSyntaxErrorCode);
		uCDSegment.SyntaxErrorCoded = SyntaxErrorCodedList.GetFromString("1");
		uCDSegment.DataElementIdentification.ErroneousDataElementPositionInSegment = "2";
		uCDSegment.DataElementIdentification.ErroneousComponentDataElementPosition = "3";

		return new SegmentErrorProvider(segmentGroup2);
	}

	const string ExpectedSegmentPosition = "2";
	const string ExpectedSyntaxErrorCode = "5";
}
