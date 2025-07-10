using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GoodsCatalogMessageSendingObject))]
	class GoodsCatalogMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultMessageType()
		{
			var sendingObject = GetNewBusinessObject() as GoodsCatalogMessageSendingObject;
			AssertEquals("Default Message Type", "ORI", sendingObject.MessageType);
		}

		public void TestProperties()
		{
			var sendingObject = GetNewBusinessObject() as GoodsCatalogMessageSendingObject;

			var expectedMessageString = @"{
  ""seq"": 1,
  ""descricao"": """",
  ""denominacao"": ""description"",
  ""cpfCnpjRaiz"": """",
  ""situacao"": ""ATIVADO"",
  ""modalidade"": ""IMPORTACAO"",
  ""ncm"": """",
  ""atributos"": [],
  ""atributosMultivalorados"": [],
  ""atributosCompostos"": [],
  ""codigosInterno"": []
}";

			CombineAssertions(() =>
			{
				AssertEquals("SelectedSendingObjects count", 1, sendingObject.SelectedSendingObjects.Count());
				AssertEquals("SelectedSendingObjects first", sendingObject, sendingObject.SelectedSendingObjects.First());
				AssertEquals("MessageAttachee", sendingObject.GoodsCatalog, sendingObject.MessageAttachee);
				AssertEquals("GetMessageTypeForEDIMessage()", MessageTypeList.Codes.CAT, sendingObject.GetMessageTypeForEDIMessage());
				AssertEquals("GetApplicationReference()", ZString.Empty, sendingObject.GetApplicationReference());
				AssertEquals("GetGlbExternalPasswordPK()", ZGuid.Empty, sendingObject.GetGlbExternalPasswordPK());
				AssertEquals("GetMessageOwner()", ZString.Empty, sendingObject.GetMessageOwner());
				AssertEquals("BrokerCode", ZString.Empty, sendingObject.BrokerCode);
				AssertEquals("GetMessageText()", expectedMessageString, sendingObject.GetMessageText());
			});
		}

		public void TestBrokerCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			Factory.Save();

			var sendingObject = GetNewBusinessObject() as GoodsCatalogMessageSendingObject;
			AssertEquals("BrokerCertificate", null, sendingObject.BrokerCertificate);

			sendingObject.BrokerCode = staff.GS_Code;
			AssertEquals("BrokerCertificate", password, sendingObject.BrokerCertificate);
			AssertEquals("GetGlbExternalPasswordPK()", password.PK, sendingObject.GetGlbExternalPasswordPK());
		}

		public void TestSendMessagesAndSave()
		{
			var sendingObject = GetNewBusinessObject() as GoodsCatalogMessageSendingObject;
			AssertEquals("1 message sent", 1, sendingObject.SendMessagesAndSave());
			AssertEquals("1 message generated", 1, sendingObject.GoodsCatalog.Messages.Count);

			foreach (EDIMessage message in sendingObject.GoodsCatalog.Messages)
			{
				AssertEquals("EDIMessage saved", true, message.IsInDatabase);
				AssertEquals("Interchange saved", true, message.Interchange.IsInDatabase);
				AssertEquals("EDIMessage created", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals("Interchange.EI_TransportType", EDIInterchange.TransportType.xT, message.Interchange.EI_TransportType);
				AssertEquals("Interchange.EI_Status", EDIInterchangeStatusList.Codes.Queued, message.Interchange.EI_Status);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "BRB";
			orgHeader.OH_FullName = "TEST CONSIGNEE";

			var cusGoodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			cusGoodsCatalog.CGC_OH_Owner = orgHeader.PK;
			var catalogProductInfo1 = cusGoodsCatalog.ForeignOperators.AddNew();
			catalogProductInfo1.CGI_CGC_Catalog = cusGoodsCatalog.PK;
			catalogProductInfo1.CountryCode = "1";
			var sendingObject = new GoodsCatalogMessageSendingObject(cusGoodsCatalog);
			sendingObject.Action = ActionList.Codes.Activate;
			return sendingObject;
		}
	}
}
