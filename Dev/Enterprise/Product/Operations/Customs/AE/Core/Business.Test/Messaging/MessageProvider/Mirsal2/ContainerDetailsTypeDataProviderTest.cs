using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DeclarationRequestDataProvider))]
sealed class ContainerDetailsTypeDataProviderTest : Mirsal2ContainerDetailsTypeDataProviderAbstractClassBase
{
	public override void TestContainerNo()
	{
		Container.CO_ContainerNumber = "123";
		AssertEquals(Container.CO_ContainerNumber, CreateDataProvider().ContainerNo);
	}

	public override void TestContainerSealNo()
	{
		Container.CO_Seal = "123";
		AssertEquals(Container.CO_Seal, CreateDataProvider().ContainerSealNo);
	}

	public override void TestContainerSize()
	{
		Container.Container.RC_Length = 20;
		AssertEquals((decimal)Container.Container.RC_Length, CreateDataProvider().ContainerSize);
	}

	public override void TestContainerType()
	{
		var map = Container.Container.CodeMapCollection.AddNew();
		map.RCM_RN_NKCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		map.RCM_Code = "123";
		AssertEquals(map.RCM_Code, CreateDataProvider().ContainerType);
	}

	protected override ContainerDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.DeclarationDetails.TransportDocumentDetails.First().ContainerDetails.First();
	}

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;
	CusContainer Container => container ??= GetContainer();
	CusContainer container;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		var importer = Factory.New<OrgHeader>();
		jobDeclaration.JE_OH_Importer = importer.PK;
		return jobDeclaration;
	}

	CusContainer GetContainer()
	{
		var container = Declaration.CusContainers.AddNew();
		var refContainer = Factory.New<RefContainer>();
		container.CO_RC = refContainer.PK;
		return container;
	}
}
