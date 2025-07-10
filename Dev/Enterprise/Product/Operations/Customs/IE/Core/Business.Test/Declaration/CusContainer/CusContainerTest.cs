using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : EU.Business.Declaration.Testing.CusContainerTest
	{
		public override void TestCO_ContainerNumber_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<CusContainer>().CO_ContainerNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[7/10] Container Number", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Container Num.", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "Cont. Num.", resourceStringDataAttribute.ShortCaption);
			});

			resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<CusContainer>().CO_ContainerNumberInfo, JobDeclaration.CaptionKeyImportUCC5);
			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("Caption", "[7/10] Container Number", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "[7/10] Container identification number", resourceStringDataAttribute.FullDescription);
			});
		}

		public override void TestContainerNumberForBinding_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<CusContainer>().ContainerNumberForBindingInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[7/10] Container Number", resourceStringDataAttribute.Caption);
				AssertEquals("MediumCaption", "Container Num.", resourceStringDataAttribute.MediumCaption);
				AssertEquals("ShortCaption", "Cont. Num.", resourceStringDataAttribute.ShortCaption);
			});

			resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<CusContainer>().ContainerNumberForBindingInfo, JobDeclaration.CaptionKeyImportUCC5);
			CombineAssertions("Import UCC5", () =>
			{
				AssertEquals("Caption", "[7/10] Container Number", resourceStringDataAttribute.Caption);
				AssertEquals("FullDescription", "[7/10] Container identification number", resourceStringDataAttribute.FullDescription);
			});
		}

		public override void TestCO_Seal_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<CusContainer>().CO_SealInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("Import UCC5 Caption", "[7/18] Seal Number", resourceStringDataAttribute.Caption);
		}

		public void TestSealNumberForBinding_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<CusContainer>().SealNumberForBindingInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("Import UCC5 Caption", "[7/18] Seal Number", resourceStringDataAttribute.Caption);
		}
	}
}
