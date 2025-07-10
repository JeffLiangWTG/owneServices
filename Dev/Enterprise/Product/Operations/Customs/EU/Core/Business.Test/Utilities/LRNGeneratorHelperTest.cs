using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class LRNGeneratorHelperTest : TestCaseWithFactory
	{
		[TestDate(2021, 10, 20)]
		public void TestGenerateLocalReferenceNumber()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");

			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber();
			CombineAssertions(() =>
			{
				AssertEquals("Length of Local Reference Number should be 22 characters", 22, lrnNumber.Length);

				AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the current year.", "21", lrnNumber);

				string partThatShouldStartWithEORI = lrnNumber.Substring(2);
				AssertStartsWith("Expected Local Reference Number, after the first 2 characters, to contain the EORI of the Declarant.", "1230789654", partThatShouldStartWithEORI);

				string partThatShouldBe10RandomDigits = lrnNumber.Substring(12);
				AssertEquals("Expected Local Reference Number, after the first 12 characters, sequence 0000000001", "0000000001", partThatShouldBe10RandomDigits);
				AssertEquals("LRN", "2112307896540000000001", lrnNumber);

				lrnGeneratorObject.LrnNumberFountain.SetNext(Factory, 10);
				AssertEquals("LRN when the next sequence number is 10", "2112307896540000000010", lrnGeneratorObject.GenerateLocalReferenceNumber());
			});
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_Greece()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EL123654789", "GR");

			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber();
			AssertEquals("2212365478900000000001", lrnNumber);
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_EoriDoesNotStartWithCountryCode()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1230789654", "BE");

			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber();
			AssertEquals("22EORI1230789654000001", lrnNumber);
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_EoriFromCompany()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1230789654", "BE");

			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber();
			AssertEquals("2212307896540000000001", lrnNumber);
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_NoEori()
		{
			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber();
			AssertEquals(ZString.Empty, lrnNumber);
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_InvalidEori()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1234567890123456", "BE");

			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber();
			AssertEquals(ZString.Empty, lrnNumber);
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_GivenEori()
		{
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1234567890123456", "BE");

			var lrnGeneratorObject = GetLRNGenerator();
			var lrnNumber = lrnGeneratorObject.GenerateLocalReferenceNumber("EORI1230789654");
			AssertEquals("22EORI1230789654000001", lrnNumber);
		}

		ILRNGenerator GetLRNGenerator()
		{
			var lrnGenerator = new Mock<ILRNGenerator>();
			lrnGenerator.Setup(x => x.Branch).Returns(GlbBranch.CurrentBranch);
			lrnGenerator.Setup(x => x.Factory).Returns(Factory);
			var numberFountain = Env.NumberFountains.EULocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());
			lrnGenerator.Setup(x => x.LrnNumberFountain).Returns(numberFountain);
			return lrnGenerator.Object;
		}
	}
}
