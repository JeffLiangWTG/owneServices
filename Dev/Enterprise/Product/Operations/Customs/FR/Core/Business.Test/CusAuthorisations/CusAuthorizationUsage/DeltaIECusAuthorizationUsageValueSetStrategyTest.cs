using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	class DeltaIECusAuthorizationUsageValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestChangeInAGC_CPH_Authorization()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTTST";
			var header1 = Factory.New<CusAuthorisationHeader>();
			header1.CPH_OH_PermitHolder = orgHeader.PK;
			header1.CPH_Number = "110001";
			header1.CPH_Type = "TST";
			header1.CPH_IsAdHoc = true;
			var header2 = Factory.New<CusAuthorisationHeader>();
			header2.CPH_OH_PermitHolder = orgHeader.PK;
			header2.CPH_Number = "110002";
			header2.CPH_Type = "TST";
			header2.CPH_IsAdHoc = true;
			var header3 = Factory.New<CusAuthorisationHeader>();
			header3.CPH_OH_PermitHolder = orgHeader.PK;
			header3.CPH_Number = "110003";
			header3.CPH_Type = "TST";
			header3.CPH_IsAdHoc = false;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_CPH_Authorization = header1.PK;
			AssertEquals("If the CPH_IsAdHoc of selected Authorization is true, add an INF document with type 00100 in the entry instruction.", 1, entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100));

			cusAuthorizationUsage.AGC_CPH_Authorization = header2.PK;
			AssertEquals("The document already exists and will not be added again.", 1, entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100));

			cusAuthorizationUsage.AGC_CPH_Authorization = header3.PK;
			AssertEquals("If the CPH_IsAdHoc of selected Authorization is false, delete the INF document with type 00100 in the entry instruction.", 0, entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100));

			cusAuthorizationUsage.AGC_CPH_Authorization = header2.PK;
			AssertEquals("If the CPH_IsAdHoc of selected Authorization is true, add an INF document with type 00100 in the entry instruction.", 1, entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			cusAuthorizationUsage.AGC_CPH_Authorization = header3.PK;
			AssertEquals("Declaration is not Delta IE, the value set strategy will not be triggered", 1, entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Count(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100));
		}
	}
}
