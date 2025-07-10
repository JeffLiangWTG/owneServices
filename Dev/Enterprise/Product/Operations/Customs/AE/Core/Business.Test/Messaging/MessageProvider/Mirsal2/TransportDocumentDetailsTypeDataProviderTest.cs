using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DeclarationRequestDataProvider))]
sealed class TransportDocumentDetailsTypeDataProviderTest : Mirsal2TransportDocumentDetailsTypeDataProviderAbstractClassBase
{
	public override void TestCargoTypePackageCode() => CombineAssertions(() =>
	{
		AssertEquals(string.Empty, CreateDataProvider().CargoTypePackageCode);
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = "ABC";
		AssertEquals(Declaration.JE_ContainerMode, CreateDataProvider().CargoTypePackageCode);
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		AssertEquals("General", CreateDataProvider().CargoTypePackageCode);
	});

	public override void TestContainerDetails()
	{
		Declaration.CusContainers.AddNew();
		AssertEquals($"{nameof(ContainerDetailsTypeDataProviderAbstractClass)} Type", "ContainerDetailsTypeDataProvider", CreateDataProvider().ContainerDetails.Single().GetType().Name);
	}

	public override void TestGrossWeightUnit()
	{
		Declaration.JE_TotalWeightUnit = "KG";
		AssertEquals(Declaration.JE_TotalWeightUnit, CreateDataProvider().GrossWeightUnit);
	}

	public override void TestInboundMasterDocumentNo()
	{
		Declaration.JE_MasterBill = "Test";
		AssertEquals(Declaration.JE_MasterBill, CreateDataProvider().InboundMasterDocumentNo);
	}

	public override void TestInboundTransportDocumentNo()
	{
		Declaration.JE_HouseBill = "Test";
		AssertEquals(Declaration.JE_HouseBill, CreateDataProvider().InboundTransportDocumentNo);
	}

	public override void TestNetWeightUnit()
	{
		Assert("to do in future WI", true);
	}

	public override void TestOutboundMasterDocumentNo()
	{
		Declaration.JE_MasterBill = "Test";
		AssertEquals(Declaration.JE_MasterBill, CreateDataProvider().OutboundMasterDocumentNo);
	}

	public override void TestOutboundTransportDocumentNo()
	{
		Declaration.JE_HouseBill = "Test";
		AssertEquals(Declaration.JE_HouseBill, CreateDataProvider().OutboundTransportDocumentNo);
	}

	public override void TestPackageDetails()
	{
		AssertEquals($"{nameof(PackageDetailsTypeDataProviderAbstractClass)} Type", "PackageDetailsTypeDataProvider", CreateDataProvider().PackageDetails.Single().GetType().Name);
	}

	public override void TestTotalGrossWeight()
	{
		Declaration.JE_TotalWeight = 100;
		AssertEquals(Declaration.JE_TotalWeight, CreateDataProvider().TotalGrossWeight);
	}

	public override void TestTotalNetWeight()
	{
		Assert("to do in future WI", true);
	}

	public override void TestVolume()
	{
		Declaration.JE_TotalVolume = 100;
		AssertEquals(Declaration.JE_TotalVolume, CreateDataProvider().Volume);
	}

	public override void TestVolumeUnit()
	{
		Declaration.JE_TotalVolumeUnit = "KG";
		AssertEquals(Declaration.JE_TotalVolumeUnit, CreateDataProvider().VolumeUnit);
	}

	protected override TransportDocumentDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.DeclarationDetails.TransportDocumentDetails.First();
	}

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		var importer = Factory.New<OrgHeader>();
		jobDeclaration.JE_OH_Importer = importer.PK;
		return jobDeclaration;
	}
}
