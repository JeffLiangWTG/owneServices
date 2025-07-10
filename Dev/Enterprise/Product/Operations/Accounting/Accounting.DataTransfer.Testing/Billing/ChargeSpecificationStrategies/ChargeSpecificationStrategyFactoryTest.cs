using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Integration;
using Moq;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	internal sealed class ChargeSpecificationStrategyFactoryTest : TestCaseWithFactory
	{
		public void TestInvalid()
		{
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{
				ChargeSpecificationStrategyFactory.NewStrategy(new Mock<IValueObjectImportContext>().Object, (Xsd.ChargesSpecified)(-1), Xsd.PostedChargeHandling.Abort);
			});
		}
	}
}
