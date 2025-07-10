using System;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT014DataProviderTest : BaseNctsDepartureMessageDataProviderTest<NT014DataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestConstructorNullArgument() => AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NT014DataProvider(null));

	public void TestTransitOperation() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.TransitOperation);
		AssertSame("cached", DataProvider.TransitOperation, DataProvider.TransitOperation);
	});

	public void TestJustification() => CombineAssertions(() =>
	{
		MessageSendingObject.ReasonCode = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others;
		AssertNotNull(DataProvider.Justification);
		AssertSame("cached", DataProvider.Justification, DataProvider.Justification);
	});

	protected override NT014DataProvider CreateDataProvider() => new NT014DataProvider(MessageSendingObject);
}
