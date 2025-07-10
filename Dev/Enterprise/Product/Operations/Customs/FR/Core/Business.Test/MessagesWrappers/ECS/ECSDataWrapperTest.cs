using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Messaging.Interfaces.ECS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.ECS.Testing
{
	public abstract class ECSDataWrapperTest : TestCaseWithFactory
	{
		public void TestECSDataWrapper()
		{
			var ecsMessageHelper = new ECSMessageTestHelper();
			var emptyDetail = Factory.New<CusExitDetail>();

			var emptyWrapper = GetNewECSDataWrapper(emptyDetail);
			AssertECSDataWrapper(emptyWrapper, "", "", "", "", "", "Empty ECSDataWrapper");

			var exitHeader = Factory.New<EU.Business.CusExitControlHeader>();
			var exitDetail = Factory.New<CusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_MovementReferenceNumber = "MRN001";
			exitDetail.CED_CustomsOffice = "OFC";
			exitDetail.CED_LocationOfGoods = "LOG";

			var testWrapper = GetNewECSDataWrapper(exitDetail);
			AssertECSDataWrapper(testWrapper, "MRN001", "OFC", "LOG", "", "", "ECSDataWrapper with empty AgentOrDeclarant");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OH_ExportBroker = shipmentExportBroker.PK;
			exitHeader.CEH_ParentID = shipment.PK;
			testWrapper = GetNewECSDataWrapper(exitDetail);
			AssertECSDataWrapper(testWrapper, "MRN001", "OFC", "LOG", "SEB_ACC", "SEB001");

			var declaration = Factory.New<EU.Business.Declaration.JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarationDeclarantAddress.PK;
			exitHeader.CEH_ParentID = declaration.PK;
			testWrapper = GetNewECSDataWrapper(exitDetail);
			AssertECSDataWrapper(testWrapper, "MRN001", "OFC", "LOG", "DCD_ACC2", "DCD000");

			exitHeader.CEH_OA_Agent = exitHeaderAgentAddress.PK;
			testWrapper = GetNewECSDataWrapper(exitDetail);
			AssertECSDataWrapper(testWrapper, "MRN001", "OFC", "LOG", "EHA_ACC2", "EHA003");
		}

		protected abstract ECSDataWrapper GetNewECSDataWrapper(CusExitDetail exitDetail);

		public void TestMessageWrapper()
		{
			var exitHeader = Factory.New<EU.Business.CusExitControlHeader>();
			var exitDetail = Factory.New<CusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_MovementReferenceNumber = "MRN001";
			exitDetail.CED_CustomsOffice = "OFC";
			exitDetail.CED_LocationOfGoods = "LOG";
			AssertType(GetMessageWrapperType(), GetNewECSDataWrapper(exitDetail).MessageWrapper);
		}

		protected abstract Type GetMessageWrapperType();

		void AssertECSDataWrapper(IECSData wrapper, string mrn = "", string office = "", string location = "", string agreement = "", string eori = "", string message = "")
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("MRN", mrn, wrapper.MRN);
				AssertEquals("Office", office, wrapper.Office);
				AssertEquals("Location", location, wrapper.Location);
				AssertEquals("Agreement", agreement, wrapper.Agreement);
				AssertEquals("eori", eori, wrapper.EORI);
			});
		}

		OrgHeader exitHeaderAgent;
		OrgAddress exitHeaderAgentAddress;
		OrgHeader declarationDeclarant;
		OrgAddress declarationDeclarantAddress;
		OrgHeader shipmentExportBroker;

		protected override void SetUp()
		{
			base.SetUp();

			exitHeaderAgent = Factory.NewWithValidTestData<OrgHeader>();
			exitHeaderAgent.OH_Code = "EHA";
			exitHeaderAgentAddress = exitHeaderAgent.Addresses.AddNew();
			exitHeaderAgentAddress.Address1 = "Address1";
			var exitHeaderAgentRegNo1 = exitHeaderAgent.CustomsCodes.AddNew();
			exitHeaderAgentRegNo1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			exitHeaderAgentRegNo1.OK_CodeType = OrgCusCode.FranceCodeTypes.Siret;
			exitHeaderAgentRegNo1.OK_CustomsRegNo = "EHA001";
			var exitHeaderAgentRegNo2 = exitHeaderAgent.CustomsCodes.AddNew();
			exitHeaderAgentRegNo2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			exitHeaderAgentRegNo2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			exitHeaderAgentRegNo2.OK_CustomsRegNo = "EHA002";
			var exitHeaderAgentRegNo3 = exitHeaderAgent.CustomsCodes.AddNew();
			exitHeaderAgentRegNo3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			exitHeaderAgentRegNo3.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			exitHeaderAgentRegNo3.OK_CustomsRegNo = "EHA003";
			var exitHeaderAgentAccount = exitHeaderAgent.DeltaAgreementNumberCollection.AddNew();
			exitHeaderAgentAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			exitHeaderAgentAccount.CZ_Account = "EHA_ACC1";
			exitHeaderAgentAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var exitHeaderAgentAccount2 = exitHeaderAgent.DeltaAgreementNumberCollection.AddNew();
			exitHeaderAgentAccount2.CZ_Code = "ECS";
			exitHeaderAgentAccount2.CZ_Account = "EHA_ACC2";
			exitHeaderAgentAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			declarationDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			declarationDeclarant.OH_Code = "DCD";
			declarationDeclarantAddress = declarationDeclarant.Addresses.AddNew();
			declarationDeclarantAddress.Address1 = "Address1";
			var declarationDeclarantRegNo = declarationDeclarant.CustomsCodes.AddNew();
			declarationDeclarantRegNo.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			declarationDeclarantRegNo.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			declarationDeclarantRegNo.OK_CustomsRegNo = "DCD000";
			var declarationDeclarantAccount1 = declarationDeclarant.DeltaAgreementNumberCollection.AddNew();
			declarationDeclarantAccount1.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			declarationDeclarantAccount1.CZ_Account = "DCD_ACC1";
			declarationDeclarantAccount1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var declarationDeclarantAccount2 = declarationDeclarant.DeltaAgreementNumberCollection.AddNew();
			declarationDeclarantAccount2.CZ_Code = "ECS";
			declarationDeclarantAccount2.CZ_Account = "DCD_ACC2";
			declarationDeclarantAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			shipmentExportBroker = Factory.NewWithValidTestData<OrgHeader>();
			shipmentExportBroker.OH_Code = "SEB";
			var shipmentExportBrokerRegNo = shipmentExportBroker.CustomsCodes.AddNew();
			shipmentExportBrokerRegNo.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			shipmentExportBrokerRegNo.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			shipmentExportBrokerRegNo.OK_CustomsRegNo = "SEB001";
			var shipmentExportBrokerAccount = shipmentExportBroker.DeltaAgreementNumberCollection.AddNew();
			shipmentExportBrokerAccount.CZ_Code = "ECS";
			shipmentExportBrokerAccount.CZ_Account = "SEB_ACC";
			shipmentExportBrokerAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		}
	}
}
