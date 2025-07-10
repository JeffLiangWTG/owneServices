using System.Linq;
using CargoWise.Customs.AE.MessageContracts.Mirsal2;
using Enterprise.Customs.AE.Business.MessageSending.Mirsal2.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DeclarationRequestDataProvider))]
sealed class DeclarationDetailsTypeDataProviderTest : Mirsal2DeclarationDetailsTypeDataProviderAbstractClassBase
{
	public override void TestBrokerCustomerCode() => CombineAssertions(() =>
	{
		AssertEquals(0, CreateDataProvider().BrokerCustomerCode);
		var org = Factory.New<OrgHeader>();
		var code = org.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK = org.PK;
		AssertEquals(123, CreateDataProvider().BrokerCustomerCode);
	});

	public override void TestCargoOwnership()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDeclarantReferenceNo()
	{
		Declaration.JE_OwnerRef = "123";
		AssertEquals(Declaration.JE_OwnerRef, CreateDataProvider().DeclarantReferenceNo);
	}

	public override void TestDeclarationNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDeclarationPurpose()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_DeclarationPurpose = AEConstants.RefCusCodeList.Codes.DeclarationPurpose.Others;
		header.CH_CEI_Instruction = instruction.PK;
		AssertEquals((short)4, CreateDataProvider().DeclarationPurpose);
	}

	public override void TestDeclarationReason()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_DeclarationPurposeDetails = "details";
		header.CH_CEI_Instruction = instruction.PK;
		AssertEquals(instruction.CEI_DeclarationPurposeDetails, CreateDataProvider().DeclarationReason);
	}

	public override void TestDeclarationRelatedDocuments()
	{
		Assert("to do in future WI", true);
	}

	public override void TestDeclarationType()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_Style = "2";
		header.CH_CEI_Instruction = instruction.PK;
		AssertEquals((short)2, CreateDataProvider().DeclarationType);
	}

	public override void TestGoodsType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestPaymentDetails()
	{
		Assert("to do in future WI", true);
	}

	public override void TestRegimeType()
	{
		Declaration.JE_MessageType = "1";
		AssertEquals((decimal)1, CreateDataProvider().RegimeType);
	}

	public override void TestTotalNumberHAWBsConsolidated()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTradeType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestTransportDocumentDetails()
	{
		AssertEquals($"{nameof(TransportDocumentDetailsTypeDataProviderAbstractClass)} Type", "TransportDocumentDetailsTypeDataProvider", CreateDataProvider().TransportDocumentDetails.Single().GetType().Name);
	}

	protected override DeclarationDetailsTypeDataProviderAbstractClass CreateDataProvider()
	{
		return DeclarationRequestDataProvider.CreateProvider(header, additionalDataProvider).Declaration.DeclarationDetails;
	}

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		jobDeclaration.ActiveEntryHeaders.Add(header);
		return jobDeclaration;
	}
}
