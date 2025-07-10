using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusContainer))]
class CusContainerTest : EU.Business.Declaration.Testing.CusContainerTest
{
	public void TestLookups()
	{
		var declaration = (JobDeclaration)GetJobDeclaration();
		var container = (CusContainer)declaration.CusContainers.AddNew();
		AssertType<CusContainerLookups>(container.Lookups);
	}

	public void TestSealPartyProperties()
	{
		var declaration = (JobDeclaration)GetJobDeclaration();
		var container = (CusContainer)declaration.CusContainers.AddNew();

		AssertEquals(ZString.Empty, container.SealPartyForBinding);
		container.JobContainer.JC_SealParty = Core.Constants.ContainerSealParties.Codes.ConsignorShipper;
		AssertEquals(container.JobContainer.JC_SealParty, container.SealPartyForBinding);
		container.SealPartyForBinding = Core.Constants.ContainerSealParties.Codes.Terminal;
		AssertEquals(container.SealPartyForBinding, container.JobContainer.JC_SealParty);

		AssertEquals(ZString.Empty, container.AdditionalSealPartyForBinding);
		container.JobContainer.JC_AdditionalSealParty = Core.Constants.ContainerSealParties.Codes.ConsignorShipper;
		AssertEquals(container.JobContainer.JC_AdditionalSealParty, container.AdditionalSealPartyForBinding);
		container.AdditionalSealPartyForBinding = Core.Constants.ContainerSealParties.Codes.Terminal;
		AssertEquals(container.AdditionalSealPartyForBinding, container.JobContainer.JC_AdditionalSealParty);
	}

	public void TestCaption()
	{
		var declaration = (JobDeclaration)GetJobDeclaration();
		var container = (CusContainer)declaration.CusContainers.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Sealed By", DataBoundResourceStrings.GetDataForProperty(container.JobContainer.JC_SealPartyInfo).Caption);
			AssertEquals("2nd Sealed By", DataBoundResourceStrings.GetDataForProperty(container.AdditionalSealPartyForBindingInfo).FullDescription);
			AssertEquals("Sealed By", DataBoundResourceStrings.GetDataForProperty(container.AdditionalSealPartyForBindingInfo).Caption);
		});
	}
}
