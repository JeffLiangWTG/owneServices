using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DeclarationRequestDataProvider))]
sealed class PersonalDetailsTypeDataProviderTest : Mirsal2PersonalDetailsTypeDataProviderAbstractClassBase
{
	public override void TestAddress()
	{
		Declaration.Importer.MainAddress.Address1 = "123 Main St";
		AssertEquals(Declaration.Importer.MainAddress.Address1, CreateDataProvider().Address);
	}

	public override void TestCity()
	{
		Declaration.Importer.MainAddress.OA_City = "city";
		AssertEquals(Declaration.Importer.MainAddress.OA_City, CreateDataProvider().City);
	}

	public override void TestCountry()
	{
		Declaration.Importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
		AssertEquals(Declaration.Importer.MainAddress.OA_RN_NKCountryCode, CreateDataProvider().Country);
	}

	public override void TestIssuingAuthorityCountry()
	{
		Assert("to do in future WI", true);
	}

	public override void TestName()
	{
		Declaration.Importer.OH_FullName = "name";
		AssertEquals(Declaration.Importer.OH_FullName, CreateDataProvider().Name);
	}

	public override void TestNationalID()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPersonalIdentificationDocumentType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPersonalIdentificationNumber()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPhone()
	{
		Declaration.Importer.MainAddress.PhoneNumber.FormattedForBinding = "1234567890";
		AssertEquals(Declaration.Importer.MainAddress.PhoneNumber.FormattedForBinding, CreateDataProvider().Phone);
	}

	protected override PersonalDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.ImporterDetails;
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
