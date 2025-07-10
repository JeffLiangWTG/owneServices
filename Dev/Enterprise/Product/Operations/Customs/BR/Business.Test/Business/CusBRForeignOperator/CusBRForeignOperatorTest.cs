using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusBRForeignOperator))]
	public class CusBRForeignOperatorTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsAutoLogged()
		{
			Assert("IsAutoLogged enable", ((IAutoLog)ForeignOperator).IsAutoAdminBusinessObjectLoggerEnabled);
		}

		public void TestUpdateMessagaStatus()
		{
			var brazil = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Brazil);
			var foreignOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOrg1.OH_Code = "001";
			foreignOrg1.OH_FullName = "FOREIGN OPERATOR 1";
			foreignOrg1.MainAddress.OA_Address1 = "PAULISTA AVENUE";
			foreignOrg1.MainAddress.OA_Address2 = "COMPLEMENTARY";
			foreignOrg1.MainAddress.OA_City = "SAO PAULO";
			foreignOrg1.MainAddress.OA_RN_NKCountryCode = "BR";
			foreignOrg1.MainAddress.OA_PostCode = "04248000";
			foreignOrg1.MainAddress.OA_State = "SP";
			foreignOrg1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "33.775.353/0001-12", brazil);
			foreignOrg1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "33775353", brazil);

			var chile = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Chile);
			var foreignOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			foreignOrg2.OH_Code = "002";
			foreignOrg2.OH_FullName = "FOREIGN OPERATOR 2";
			foreignOrg2.MainAddress.OA_Address1 = "AV. ANDR�S BELLO 2425";
			foreignOrg2.MainAddress.OA_Address2 = "";
			foreignOrg2.MainAddress.OA_City = "SANTIAGO";
			foreignOrg2.MainAddress.OA_RN_NKCountryCode = "CL";
			foreignOrg2.MainAddress.OA_PostCode = "7510689";
			foreignOrg2.MainAddress.OA_State = "CL";
			foreignOrg2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "60.339.387/0001-37", chile);
			foreignOrg2.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "60339387", chile);
			Factory.Save();

			ForeignOperator.BFR_OH_ForeignOperator = foreignOrg1.PK;
			Factory.Save();

			ForeignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("Should NOT update BFR_MessageStatus to NST when BFR_MessageStatus changes", BRMessageStatusList.Codes.Accepted, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_OH_ForeignOperator = foreignOrg2.PK;
			Factory.Save();
			AssertEquals("Should update BFR_MessageStatus to NST when BFR_OH_ForeignOperator changes", BRMessageStatusList.Codes.NotSent, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("BFR_MessageStatus should be AwaitingResponse", BRMessageStatusList.Codes.AwaitingResponse, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_OH_ForeignOperator = foreignOrg2.PK;
			Factory.Save();
			AssertEquals("Should NOT update BFR_MessageStatus to NST when BFR_MessageStatus changes", BRMessageStatusList.Codes.AwaitingResponse, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Rejected;
			Factory.Save();
			ForeignOperator.BFR_OH_ForeignOperator = foreignOrg1.PK;
			Factory.Save();
			AssertEquals("Should update BFR_MessageStatus to NST when BFR_OH_ForeignOperator changes", BRMessageStatusList.Codes.NotSent, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
			Factory.Save();

			ForeignOperator.SuspendUpdateMessageStatusOnSavingUntilSaved();
			ForeignOperator.BFR_OH_ForeignOperator = foreignOrg2.PK;
			Assert("UpdateMessageStatusOnSaving Suspended", ForeignOperator.IsUpdateMessageStatusOnSavingSuspended);
			Factory.Save();
			Assert("UpdateMessageStatusOnSaving not Suspended", !ForeignOperator.IsUpdateMessageStatusOnSavingSuspended);
			AssertEquals("Should not update BFR_MessageStatus when suspended", BRMessageStatusList.Codes.Accepted, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_OH_ForeignOperator = ZGuid.Empty;
			ForeignOperator.BFR_Name = "FOREIGN OPERATOR 2";
			ForeignOperator.BFR_City = "SANTIAGO";
			ForeignOperator.BFR_RN_NKCountryCode = "CL";
			Factory.Save();
			AssertEquals("Should update BFR_MessageStatus when NOT suspended", BRMessageStatusList.Codes.NotSent, ForeignOperator.BFR_MessageStatus);

			ForeignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
			Factory.Save();

			ForeignOperator.BFR_OH_ForeignOperator = foreignOrg2.PK;
			Factory.Save();
			AssertEquals("Should not be update BFR_MessageStatus when DB ForeignOperator is null or Empty", BRMessageStatusList.Codes.Accepted, ForeignOperator.BFR_MessageStatus);
		}

		public void TestForeignOperatorDetails()
		{
			var foreignOperatorOrg = Factory.New<OrgHeader>();
			foreignOperatorOrg.OH_Code = "BRB";
			foreignOperatorOrg.OH_FullName = "TEST FOREIGN OPERATOR";
			foreignOperatorOrg.MainAddress.OA_Address1 = "Address foreignOp";
			foreignOperatorOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Comoros;
			var secondaryAddress1 = foreignOperatorOrg.Addresses.AddNew();
			secondaryAddress1.OA_Address1 = " SecAddress foreignOp";
			secondaryAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;

			ForeignOperator.BFR_Name = "FOREIGN COMPANY JOE";
			ForeignOperator.BFR_City = "Bishkek";
			ForeignOperator.BFR_RN_NKCountryCode = Core.Constants.CountryCodes.Kyrgyzstan;

			foreignOperator.OnSaving();
			AssertEquals("ForeignOperatorName should be", "FOREIGN COMPANY JOE", ForeignOperator.ForeignOperatorName);
			AssertEquals("ForeignOperatorCode should be", ZString.Empty, ForeignOperator.ForeignOperatorCode);
			AssertEquals("ForeignOperatorCountry should be", "KG - Kyrgyzstan", ForeignOperator.ForeignOperatorCountry);
			AssertEquals("ForeignOperatorCountryCode should be", "KG", ForeignOperator.ForeignOperatorCountryCode);
			AssertEquals("BFR_Name should be", "FOREIGN COMPANY JOE", ForeignOperator.BFR_Name);
			AssertEquals("BFR_City should be", "Bishkek", ForeignOperator.BFR_City);
			AssertEquals("BFR_RN_NKCountryCode should be", Core.Constants.CountryCodes.Kyrgyzstan, ForeignOperator.BFR_RN_NKCountryCode);

			ForeignOperator.BFR_OH_ForeignOperator = foreignOperatorOrg.PK;
			AssertEquals("ForeignOperatorName should be", "TEST FOREIGN OPERATOR", ForeignOperator.ForeignOperatorName);
			AssertEquals("ForeignOperatorCode should be", foreignOperatorOrg.OH_Code, ForeignOperator.ForeignOperatorCode);
			AssertEquals("ForeignOperatorCountry should be", "KM - Comoros", ForeignOperator.ForeignOperatorCountry);
			AssertEquals("ForeignOperatorCountryCode should be", "KM", ForeignOperator.ForeignOperatorCountryCode);
			AssertEquals("BFR_Name should be", "FOREIGN COMPANY JOE", ForeignOperator.BFR_Name);
			AssertEquals("BFR_City should be", "Bishkek", ForeignOperator.BFR_City);
			AssertEquals("BFR_RN_NKCountryCode should be", Core.Constants.CountryCodes.Kyrgyzstan, ForeignOperator.BFR_RN_NKCountryCode);

			foreignOperator.OnSaving();
			AssertEquals("ForeignOperatorName should be", "TEST FOREIGN OPERATOR", ForeignOperator.ForeignOperatorName);
			AssertEquals("ForeignOperatorCode should be", foreignOperatorOrg.OH_Code, ForeignOperator.ForeignOperatorCode);
			AssertEquals("ForeignOperatorCountry should be", "KM - Comoros", ForeignOperator.ForeignOperatorCountry);
			AssertEquals("ForeignOperatorCountryCode should be", "KM", ForeignOperator.ForeignOperatorCountryCode); 
			Assert("BFR_Name should be Empty", ForeignOperator.BFR_Name.IsEmpty);
			Assert("BFR_City should be Empty", ForeignOperator.BFR_City.IsEmpty);
			Assert("BFR_RN_NKCountryCode should be Empty", ForeignOperator.BFR_RN_NKCountryCode.IsEmpty);
		}

		public void TestReadOnly()
		{
			Assert(ForeignOperator.BFR_MessageStatusInfo.ReadOnly);
			Assert(ForeignOperator.BFR_CustomsStatusInfo.ReadOnly);
			Assert(ForeignOperator.BFR_AuthorityIdentifierInfo.ReadOnly);
			Assert(ForeignOperator.BFR_AuthorityVersionInfo.ReadOnly);
		}

		public void TestIMessageAttachee()
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, ((IMessageAttachee)ForeignOperator).BranchPK);
		}

		public void TestMessages()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			var message = Factory.New<BREDIMessage>();
			message.EM_LinkTable = foreignOperator.TableName;
			message.EM_LinkUniqueID = foreignOperator.PK;

			Assert(foreignOperator.Messages.IsManagedForDataRefresh);
			AssertContainsExactElementsInAnyOrder(new[] { message }, foreignOperator.Messages);
		}

		public void TestMessageStatusProperty()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;
			AssertEquals("IsMessageAwaitingResponse should be false", false, foreignOperator.IsMessageAwaitingResponse);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Rejected;
			AssertEquals("IsMessageAwaitingResponse should be false", false, foreignOperator.IsMessageAwaitingResponse);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
			AssertEquals("IsMessageAwaitingResponse should be false", false, foreignOperator.IsMessageAwaitingResponse);

			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("IsMessageAwaitingResponse should be true", true, foreignOperator.IsMessageAwaitingResponse);
		}

		public void TestIMessageManageableBizObj()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			var messageSendingObject = new ForeignOperatorMessageSendingObject(foreignOperator);

			IMessageManageableBizObj bizObj = foreignOperator;
			AssertType<ForeignOperatorMultiMessageManager>(bizObj.GetMessageManagerForAmendmentDetection());
			AssertEquals(ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());
		}

		public void TestIBackDoorSavingSupportableBizObj()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			var messageSendingObject = new ForeignOperatorMessageSendingObject(foreignOperator);

			IBackDoorSavingSupportableBizObj bizObj = foreignOperator;
			AssertType<AmendmentWithdrawalReason>(bizObj.GetAmendmentWithdrawalReason());
			Assert(bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
		}

		public void TestIsInAStatusAmendmentSendable()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;
			AssertEquals("IsInAStatusAmendmentSendable should be false", false, foreignOperator.IsInAStatusAmendmentSendable);
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Rejected;
			AssertEquals("IsInAStatusAmendmentSendable should be false", false, foreignOperator.IsInAStatusAmendmentSendable);
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
			AssertEquals("IsInAStatusAmendmentSendable should be false", false, foreignOperator.IsInAStatusAmendmentSendable);
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("IsInAStatusAmendmentSendable should be true", true, foreignOperator.IsInAStatusAmendmentSendable);
		}

		public void TestForeignOperatorDTOProperties()
		{
			AssertEquals("TIN number should be", ZString.Empty, ForeignOperator.ForeignOperatorTIN);
			AssertEquals("Email should be", ZString.Empty, foreignOperator.ForeignOperatorEmail);
			AssertEquals("Codigo Interno should be", ZString.Empty, ForeignOperator.ForeignOperatorInternalCode);

			var responseMessage1 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, ForeignOperator, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-10);
			responseMessage1.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageText = responseOZISingleMessage;

			var responseMessage2 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, ForeignOperator, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			responseMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-5);
			responseMessage2.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageText = responseOZISingleMessage.Replace("2133333", "4128753").Replace("otroemail@email.com.br", "newemail@email.com.br");

			Factory.Save();

			AssertEquals("TIN number should be", "4128753", ForeignOperator.ForeignOperatorTIN);
			AssertEquals("Email should be", "newemail@email.com.br", foreignOperator.ForeignOperatorEmail);
			AssertEquals("Codigo Interno should be", "4567898", ForeignOperator.ForeignOperatorInternalCode);

			var foreignOpOrg = Factory.New<OrgHeader>();
			foreignOpOrg.OH_Code = "COD";
			foreignOpOrg.OH_FullName = "Foreign OP Name";

			var contact = foreignOpOrg.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			contact.OC_Email = "email@email.com.br";

			var allocationNFO = contact.Allocations.AddNew();
			allocationNFO.PC_Type = OrgConstants.ContactAllocationType.BRForeignOperator;

			var allocationCUS = contact.Allocations.AddNew();
			allocationCUS.PC_Type = OrgConstants.ContactAllocationType.CUS;

			foreignOpOrg.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.TIN, "12345", Core.Constants.CountryCodes.Brazil);
			foreignOpOrg.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode, "5108", Core.Constants.CountryCodes.Brazil);

			ForeignOperator.BFR_OH_ForeignOperator = foreignOpOrg.PK;

			AssertEquals("TIN number should be", "12345", ForeignOperator.ForeignOperatorTIN);
			AssertEquals("Email should be", "email@email.com.br", foreignOperator.ForeignOperatorEmail);
			AssertEquals("Codigo Interno should be", "5108", ForeignOperator.ForeignOperatorInternalCode);
		}

		public void TestTemplateCopy()
		{
			var clonedForeignOperator = ((ITemplateCopyable)ForeignOperator).TemplateCopy() as CusBRForeignOperator;
			CombineAssertions(() =>
			{
				AssertEquals("BFR_OH_Owner should be", ForeignOperator.Owner.PK, clonedForeignOperator.BFR_OH_Owner);
				AssertEquals("BFR_AuthorityIdentifier should not be cloned", ZString.Empty, clonedForeignOperator.BFR_AuthorityIdentifier);
				AssertEquals("BFR_AuthorityVersion should not be cloned", ZString.Empty, clonedForeignOperator.BFR_AuthorityVersion);
				AssertEquals("BFR_CustomsStatus should not be cloned", ZString.Empty, clonedForeignOperator.BFR_CustomsStatus);
				AssertEquals("BFR_MessageStatus should not be cloned", ZString.Empty, clonedForeignOperator.BFR_MessageStatus);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => ForeignOperator;

		CusBRForeignOperator ForeignOperator
		{
			get
			{
				if (foreignOperator == null)
				{
					var owner1 = Factory.New<OrgHeader>();
					owner1.OH_Code = "BRB";
					owner1.OH_FullName = "TEST CONSIGNEE";

					foreignOperator = Factory.New<CusBRForeignOperator>();
					foreignOperator.BFR_OH_Owner = owner1.PK;
					foreignOperator.BFR_AuthorityIdentifier = "123";
					foreignOperator.BFR_AuthorityVersion = "123";
					foreignOperator.BFR_CustomsStatus = "DEA";
					foreignOperator.BFR_MessageStatus = "123";
				}
				return foreignOperator;
			}
		}
		CusBRForeignOperator foreignOperator;

		const string responseOZISingleMessage = @"{
  ""seq"": 1,
  ""cpfCnpjRaiz"": ""00638881"",
  ""codigo"": ""1"",
  ""versao"": ""2"",
  ""tin"": ""2133333"",
  ""nome"": ""Name"",
  ""situacao"": ""Ativado"",
  ""logradouro"": ""Address"",
  ""nomeCidade"": ""City"",
  ""codigoSubdivisaoPais"": ""CN-MO"",
  ""codigoPais"": ""CN"",
  ""cep"": ""789999999"",
  ""codigoInterno"": ""4567898"",
  ""email"": ""otroemail@email.com.br"",
  ""dataReferencia"": null,
  ""identificacoesAdicionais"": [
    {
      ""numero"": ""321111"",
      ""codigo"": ""10""
    },
    {
      ""numero"": ""322222"",
      ""codigo"": ""102""
    }
  ]
}";
	}
}
