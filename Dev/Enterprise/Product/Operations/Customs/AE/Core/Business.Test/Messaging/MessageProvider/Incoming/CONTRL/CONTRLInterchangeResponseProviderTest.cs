using System;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Segments;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLInterchangeResponseProviderTest : Customs.Business.Testing.DataProviderTestCase<CONTRLInterchangeResponseProvider, ICONTRLInterchangeResponseProvider>
{
	[ExpectNoExceptions]
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new CONTRLInterchangeResponseProvider(null));
	}

	public void TestOutgoingReference() => AssertEquals(ExpectedOutgoingReference, Provider.OutgoingReference);

	public void TestActionCode() => AssertEquals(ExpectedActionCode, Provider.ActionCode);

	public void TestSyntaxErrorCode() => AssertEquals(ExpectedSyntaxErrorCode, Provider.SyntaxErrorCode);

	public void TestErrorDataElementPosition() => AssertEquals(ExpectedErrorDataElementPosition, Provider.ErrorDataElementPosition);

	public void TestErrorDataElementComponentPosition() => AssertEquals(ExpectedErrorDataElementComponentPosition, Provider.ErrorDataElementComponentPosition);

	protected override CONTRLInterchangeResponseProvider GetProvider()
	{
		var uCISegment = new UCISegment();
		uCISegment.InterchangeControlReference = ExpectedOutgoingReference;
		uCISegment.ActionCoded = ActionCodedList.GetFromString(ExpectedActionCode);
		uCISegment.SyntaxErrorCoded = SyntaxErrorCodedList.GetFromString(ExpectedSyntaxErrorCode);
		uCISegment.DataElementIdentification.ErroneousDataElementPositionInSegment = ExpectedErrorDataElementPosition;
		uCISegment.DataElementIdentification.ErroneousComponentDataElementPosition = ExpectedErrorDataElementComponentPosition;

		return new CONTRLInterchangeResponseProvider(uCISegment);
	}

	const string ExpectedOutgoingReference = "INT001";
	const string ExpectedActionCode = "4";
	const string ExpectedSyntaxErrorCode = "12";
	const string ExpectedErrorDataElementPosition = "2";
	const string ExpectedErrorDataElementComponentPosition = "5";
}
