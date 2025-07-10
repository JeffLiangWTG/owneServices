using System;
using System.Linq;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Messages.CONTRL;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLMessageResponseProviderTest : Customs.Business.Testing.DataProviderTestCase<CONTRLMessageResponseProvider, ICONTRLMessageResponseProvider>
{
	[ExpectNoExceptions]
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new CONTRLMessageResponseProvider(null));
	}

	public void TestOutgoingReference() => AssertEquals(ExpectedOutgoingReference, Provider.OutgoingReference);

	public void TestActionCode() => AssertEquals(ExpectedActionCode, Provider.ActionCode);

	public void TestSyntaxErrorCode() => AssertEquals(ExpectedSyntaxErrorCode, Provider.SyntaxErrorCode);

	public void TestErrorDataElementPosition() => AssertEquals(ExpectedErrorDataElementPosition, Provider.ErrorDataElementPosition);

	public void TestErrorDataElementComponentPosition() => AssertEquals(ExpectedErrorDataElementComponentPosition, Provider.ErrorDataElementComponentPosition);

	public void TestSegmentErrors() => AssertType<SegmentErrorProvider>(Provider.SegmentErrors.Single());

	protected override CONTRLMessageResponseProvider GetProvider()
	{
		var segmentGroup1 = new SegmentGroup1();
		var uCMSegment = segmentGroup1.UCM.InstantiateAChildAndAddItToChildrenCollection();
		uCMSegment.MessageReferenceNumber = ExpectedOutgoingReference;
		uCMSegment.ActionCoded = ActionCodedList.GetFromString(ExpectedActionCode);
		uCMSegment.SyntaxErrorCoded = SyntaxErrorCodedList.GetFromString(ExpectedSyntaxErrorCode);
		uCMSegment.DataElementIdentification.ErroneousDataElementPositionInSegment = ExpectedErrorDataElementPosition;
		uCMSegment.DataElementIdentification.ErroneousComponentDataElementPosition = ExpectedErrorDataElementComponentPosition;

		var segmentGroup2 = segmentGroup1.Group2.InstantiateAChildAndAddItToChildrenCollection();
		var uCSSegment = segmentGroup2.UCS.InstantiateAChildAndAddItToChildrenCollection();
		uCSSegment.SyntaxErrorCoded = SyntaxErrorCodedList.GetFromString("1");
		uCSSegment.SegmentPositionInMessageBody = "2";

		return new CONTRLMessageResponseProvider(segmentGroup1);
	}

	const string ExpectedOutgoingReference = "INT001";
	const string ExpectedActionCode = "4";
	const string ExpectedSyntaxErrorCode = "12";
	const string ExpectedErrorDataElementPosition = "2";
	const string ExpectedErrorDataElementComponentPosition = "5";
}
