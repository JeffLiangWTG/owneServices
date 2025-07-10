using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableEouDataProviderTest : ExportSbTableEouDataProviderAbstractClassBase
{
	public override void TestAmendmentDate()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentDate);
	}

	public override void TestAmendmentNo()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentNo);
	}

	public override void TestAmendmentType()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentType);
	}

	public override void TestBranchSrNumberOfIe()
	{
		var expectCode = "123";
		var supplier = Factory.New<OrgHeader>();
		var address = supplier.Addresses.AddNew();
		var cusCode = supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, expectCode, Core.Constants.CountryCodes.India);
		cusCode.OK_OA_PremisesAddress = address.PK;
		var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
		supplierDocumentaryAddress.E2_OA_Address = address.PK;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
		Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
		AssertEquals(expectCode, CreateDataProvider().BranchSrNumberOfIe);
	}

	public override void TestCommissionerate()
	{
		Declaration.JE_Commissionerate = "123";
		AssertEquals("123", CreateDataProvider().Commissionerate);
	}

	public override void TestCustomHouseCode()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestDivision()
	{
		Declaration.JE_Division = "123";
		AssertEquals("123", CreateDataProvider().Division);
	}

	public override void TestExaminationDate()
	{
		AssertNull(CreateDataProvider().ExaminationDate);

		Declaration.JE_ExaminationDate = new ZDateTime(2025, 2, 08);
		AssertEquals(new ZDateTime(2025, 2, 08), CreateDataProvider().ExaminationDate);
	}

	public override void TestExaminingOfficerDesignation()
	{
		Declaration.JE_ExaminingOfficerDesignation = "123";
		AssertEquals("123", CreateDataProvider().ExaminingOfficerDesignation);
	}

	public override void TestExaminingOfficerName()
	{
		Declaration.JE_ExaminingOfficerName = "123";
		AssertEquals("123", CreateDataProvider().ExaminingOfficerName);
	}

	public override void TestIeCodeOfTheEou()
	{
		var expectCode = "123";
		var supplier = Factory.New<OrgHeader>();
		var address = supplier.Addresses.AddNew();
		var cusCode = supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, expectCode, Core.Constants.CountryCodes.India);
		cusCode.OK_OA_PremisesAddress = address.PK;
		var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
		supplierDocumentaryAddress.E2_OA_Address = address.PK;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
		Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
		AssertEquals(expectCode, CreateDataProvider().IeCodeOfTheEou);
	}

	public override void TestItemValuesVerifiedByExaminingOfficer()
	{
		Declaration.JE_Verified = YesNoList.Codes.Yes;
		AssertEquals(YesNoList.Codes.Yes, CreateDataProvider().ItemValuesVerifiedByExaminingOfficer);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestRange()
	{
		Declaration.JE_Range = "123";
		AssertEquals("123", CreateDataProvider().Range);
	}

	public override void TestSampleForwarded()
	{
		Declaration.JE_SampleForwarded = YesNoList.Codes.Yes;
		AssertEquals(YesNoList.Codes.Yes, CreateDataProvider().SampleForwarded);
	}

	public override void TestSbDate()
	{
		AssertEquals("TBA", CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		AssertEquals("TBA", CreateDataProvider().SbNo);
	}

	public override void TestSealNo()
	{
		Declaration.JE_SealNo = "123";
		AssertEquals("123", CreateDataProvider().SealNo);
	}

	public override void TestSupervisingOfficerDesignation()
	{
		Declaration.JE_SupervisingOfficerDesignation = "123";
		AssertEquals("123", CreateDataProvider().SupervisingOfficerDesignation);
	}

	public override void TestSupervisingOfficerName()
	{
		Declaration.JE_SupervisingOfficerName = "123";
		AssertEquals("123", CreateDataProvider().SupervisingOfficerName);
	}

	protected override TableEouDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableEou.First();
	}

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new ExportSbCACHE01AdditionalDataProvider(messageSendingObject);

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}
}
