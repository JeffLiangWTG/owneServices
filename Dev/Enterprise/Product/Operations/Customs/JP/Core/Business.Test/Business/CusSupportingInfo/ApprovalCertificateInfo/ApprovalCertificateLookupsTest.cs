using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ApprovalCertificateInfoLookups))]
	sealed class ApprovalCertificateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void ApprovalCertificateNumberList()
		{
			PrepareCodes();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var approvalCertificateInfo = declaration.CustomsEntryInstructions.AddNew().ApprovalCertificateInfos.AddNew();

			approvalCertificateInfo.CSI_Code = "CRNO";
			AssertEquals(1, approvalCertificateInfo.Lookups.ApprovalCertificateNumberList);
			AssertEquals("KIJI", approvalCertificateInfo.Lookups.ApprovalCertificateNumberList.GetAllCodes().FirstOrDefault());

			approvalCertificateInfo.CSI_Code = "TOKG";
			AssertEquals(1, approvalCertificateInfo.Lookups.ApprovalCertificateNumberList);
			AssertEquals("GAITAME", approvalCertificateInfo.Lookups.ApprovalCertificateNumberList.GetAllCodes().FirstOrDefault());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			approvalCertificateInfo.CSI_Code = ApprovalCertificateInfoCodes.ITNO;
			AssertEquals(0, approvalCertificateInfo.Lookups.ApprovalCertificateNumberList);
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

			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportApprovalCertificateType, "Japan Export Approval Certificate Type");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportConstantApprovalCertificateType, "Japan Export Constant Approval Certificate Type");
			var codeForCountry = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportApprovalCertificateType, "TOJP", "Description for TOJP", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingTransportModeForCusCodeList(codeForCountry.PK, TransportTypeList.Codes.Air);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeListAttribute(codeForCountry.PK, "CertificateType", "ITNO");

			Factory.Save();
		}
	}
}
