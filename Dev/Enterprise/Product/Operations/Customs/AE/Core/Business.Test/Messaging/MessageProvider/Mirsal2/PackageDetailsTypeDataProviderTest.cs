using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DeclarationRequestDataProvider))]
sealed class PackageDetailsTypeDataProviderTest : Mirsal2PackageDetailsTypeDataProviderAbstractClassBase
{
	public override void TestMarksAndNumber()
	{
		Declaration.JE_MarksAndNumbersShort = "123";
		AssertEquals(Declaration.JE_MarksAndNumbersShort, CreateDataProvider().MarksAndNumber);
	}

	public override void TestPackageType()
	{
		Declaration.JE_TotalNoOfPacksPackType = "123";
		AssertEquals(Declaration.JE_TotalNoOfPacksPackType, CreateDataProvider().PackageType);
	}

	public override void TestTotalNumberOfPackages()
	{
		Declaration.JE_TotalNoOfPacks = 100;
		AssertEquals((long)Declaration.JE_TotalNoOfPacks, CreateDataProvider().TotalNumberOfPackages);
	}

	protected override PackageDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.DeclarationDetails.TransportDocumentDetails.First().PackageDetails.First();
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
