using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class AEInterchangeNumberStrategyTest : TestCaseWithFactory
{
	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestGetMessageReferenceNumber()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "AEC";
		interchange.EI_InterchangeType = "CAR";
		var numberStrategy = new AEInterchangeNumberStrategy(interchange) as IMessageNumberStrategy;
		CombineAssertions(() =>
		{
			AssertEquals("First Interchange for AEC, CAR", "012345B0000001", numberStrategy.GetMessageReferenceNumber());
			AssertEquals("Same application code and type", "012345B0000002", numberStrategy.GetMessageReferenceNumber());

			interchange.EI_ApplicationCode = "XYZ";
			AssertEquals("Different application code, same type", "012345B0000001", numberStrategy.GetMessageReferenceNumber());
			interchange.EI_ApplicationCode = "AEC";
			interchange.EI_InterchangeType = "XYZ";
			AssertEquals("Same application code, different type", "012345B0000001", numberStrategy.GetMessageReferenceNumber());
		});
	}
}
