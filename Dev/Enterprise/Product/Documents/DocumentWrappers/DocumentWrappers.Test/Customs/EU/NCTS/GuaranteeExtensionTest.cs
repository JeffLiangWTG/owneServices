using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class GuaranteeExtensionTest : TestCase
	{
		public void TestToStringCertainNumberGuarantees()
		{
			AssertEquals("When guarantees parameters is null, ToStringCertainNumberGuarantees() return value", "", GuaranteeExtension.ToStringCertainNumberGuarantees(null, 3));

			var guaranteeMock1 = new Mock<IGuarantee>();
			var guaranteeMock2 = new Mock<IGuarantee>();
			var guaranteeMock3 = new Mock<IGuarantee>();
			var guaranteeMock4 = new Mock<IGuarantee>();

			guaranteeMock1.Setup(x => x.OtherGuaranteeReference).Returns("OTH REF 1");
			guaranteeMock2.Setup(x => x.GuaranteeReferenceNumber).Returns("REF 2");
			guaranteeMock3.Setup(x => x.GuaranteeReferenceNumber).Returns("REF 3");
			guaranteeMock4.Setup(x => x.GuaranteeReferenceNumber).Returns("REF 4");

			var guarantees = new[]
			{
				guaranteeMock1.Object,
				guaranteeMock2.Object,
				guaranteeMock3.Object,
				guaranteeMock4.Object,
			};
			AssertEquals("ToStringCertainNumberGuarantees()", "OTH REF 1;REF 2;REF 3", guarantees.ToStringCertainNumberGuarantees(3));
		}

		public void TestToStringThreeFirstGuaranteeValidities()
		{
			AssertEquals("When guarantees parameters is null, ToStringThreeFirstGuaranteeValidities() return value", "", GuaranteeExtension.ToStringThreeFirstGuaranteeValidities(null));

			var guaranteeMock1 = new Mock<IGuarantee>();
			var guaranteeMock2 = new Mock<IGuarantee>();
			var guaranteeMock3 = new Mock<IGuarantee>();
			var guaranteeMock4 = new Mock<IGuarantee>();

			guaranteeMock1.Setup(x => x.NotValidForOtherContractingParties).Returns(new ZString[] { "DE", "CH" });
			guaranteeMock2.Setup(x => x.NotValidForOtherContractingParties).Returns(new ZString[] { "IT", "FR" });
			guaranteeMock3.Setup(x => x.NotValidForOtherContractingParties).Returns(new ZString[] { "ES" });
			guaranteeMock4.Setup(x => x.NotValidForOtherContractingParties).Returns(new ZString[] { "NL" });

			var guarantees = new[]
			{
				guaranteeMock1.Object,
				guaranteeMock2.Object,
				guaranteeMock3.Object,
				guaranteeMock4.Object,
			};

			AssertEquals("ToStringThreeFirstGuarantees()", "DE,CH,IT,FR,ES", guarantees.ToStringThreeFirstGuaranteeValidities());
		}

		public void TestToStringThreeFirstGuaranteeCodes()
		{
			AssertEquals("When guarantees parameters is null, ToStringThreeFirstGuaranteeCodes() return value", "", GuaranteeExtension.ToStringThreeFirstGuaranteeCodes(null));

			var guaranteeMock1 = new Mock<IGuarantee>();
			var guaranteeMock2 = new Mock<IGuarantee>();
			var guaranteeMock3 = new Mock<IGuarantee>();
			var guaranteeMock4 = new Mock<IGuarantee>();

			guaranteeMock1.Setup(x => x.GuaranteeType).Returns("1");
			guaranteeMock2.Setup(x => x.GuaranteeType).Returns("2");
			guaranteeMock3.Setup(x => x.GuaranteeType).Returns("3");
			guaranteeMock4.Setup(x => x.GuaranteeType).Returns("4");

			var guarantees = new[]
			{
				guaranteeMock1.Object,
				guaranteeMock2.Object,
				guaranteeMock3.Object,
				guaranteeMock4.Object,
			};

			AssertEquals("ToStringThreeFirstGuaranteeCodes()", "1,2,3", guarantees.ToStringThreeFirstGuaranteeCodes());
		}
	}
}
