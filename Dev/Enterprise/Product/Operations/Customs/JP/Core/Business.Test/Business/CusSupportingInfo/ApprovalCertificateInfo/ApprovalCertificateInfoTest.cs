using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ApprovalCertificateInfo))]
	class ApprovalCertificateInfoTest : Customs.Business.Testing.CusSupportingInfoTest<ApprovalCertificateInfo>
	{
		public void TestMaxLength()
		{
			var approvalCertificateInfo = Factory.New<ApprovalCertificateInfo>();
			AssertEquals(4, approvalCertificateInfo.CSI_CodeInfo.MaxLength);
			AssertEquals(20, approvalCertificateInfo.CSI_ReferenceNumberInfo.MaxLength);
		}

		public void TestSetDefaultValues()
		{
			var approvalCertificateInfo = Factory.New<ApprovalCertificateInfo>();
			AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.ApprovalCertificate, approvalCertificateInfo.CSI_Type);
		}

		public void TestCSI_Code()
		{
			PrepareCodes();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var approvalCertificateInfo = declaration.CustomsEntryInstructions.AddNew().ApprovalCertificateInfos.AddNew();

			approvalCertificateInfo.CSI_Code = "CRNO";
			AssertEquals("Should not set number automatically.", ZString.Empty, approvalCertificateInfo.CSI_ReferenceNumber);

			approvalCertificateInfo.CSI_Code = "TOKG";
			AssertEquals("Should set number automatically.", "GAITAME", approvalCertificateInfo.CSI_ReferenceNumber);
		}

		public void TestReferenceNumberDescription()
		{
			PrepareCodes();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var approvalCertificateInfo = declaration.CustomsEntryInstructions.AddNew().ApprovalCertificateInfos.AddNew();

			approvalCertificateInfo.CSI_Code = "CRNO";
			AssertEquals("Description should be the description for code", "Description for CRNO", approvalCertificateInfo.ReferenceNumberDescription);

			approvalCertificateInfo.CSI_Code = "TOKG";
			AssertEquals("Should set number automatically.", "GAITAME", approvalCertificateInfo.CSI_ReferenceNumber);
			AssertEquals("Description should be the description for reference number", "Description for GAITAME", approvalCertificateInfo.ReferenceNumberDescription);

			approvalCertificateInfo.CSI_ReferenceNumber = "INVALID";
			AssertEquals("Description should be the description for code", "Description for TOKG", approvalCertificateInfo.ReferenceNumberDescription);
		}

		protected override IEnumerable<ApprovalCertificateInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.NewWithValidTestData<CusEntryInstruction>();
			yield return entryInstruction.ApprovalCertificateInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			return entryInstruction.ApprovalCertificateInfos.AddNew();
		}

		void PrepareCodes()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "Japan Import Approval Certificate Number");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportConstantApprovalCertificateNumber, "Japan Import Constant Approval Certificate Number");
			var normalCode = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "CRNO", "Description for CRNO", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(normalCode.PK, TransportTypeList.Codes.Air);
			var specialCode = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber, "TOKG", "Description for TOKG", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(specialCode.PK, TransportTypeList.Codes.Air);
			var codeForConstantNumber = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportConstantApprovalCertificateNumber, "GAITAME", "Description for GAITAME", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeForConstantNumber.PK, TransportTypeList.Codes.Air);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(codeForConstantNumber.PK, "CertificateType", "TOKG");

			Factory.Save();
		}
	}
}
