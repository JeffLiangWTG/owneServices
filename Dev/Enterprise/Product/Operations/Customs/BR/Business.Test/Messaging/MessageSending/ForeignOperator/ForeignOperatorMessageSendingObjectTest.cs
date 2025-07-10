using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ForeignOperatorMessageSendingObject))]
	class ForeignOperatorMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var sendingObject = new ForeignOperatorMessageSendingObject(ForeignOperator);

			AssertEquals("Default Message Type", ForeignOperatorMessageTypesList.Codes.ORI, sendingObject.MessageType);
			AssertEquals("Default Should Send", true, sendingObject.ShouldSend);
			AssertEquals("MessageAttachee", ForeignOperator, sendingObject.MessageAttachee);
		}

		public void TestGetMessageText()
		{
			var sendingObject = new ForeignOperatorMessageSendingObject(ForeignOperator);
			AssertContains("\"nome\": \"BRAZIL TEST COMPANY\",", sendingObject.GetMessageText());
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProperties()
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

			var sendingObject = GetNewBusinessObject() as ForeignOperatorMessageSendingObject;
			var expectedMessageString = @"{
  ""seq"": 1,
  ""cpfCnpjRaiz"": """",
  ""codigo"": """",
  ""versao"": """",
  ""tin"": """",
  ""nome"": ""BRAZIL TEST COMPANY"",
  ""situacao"": ""DESATIVADO"",
  ""logradouro"": "" "",
  ""nomeCidade"": """",
  ""codigoSubdivisaoPais"": ""BR-"",
  ""codigoPais"": ""BR"",
  ""cep"": """",
  ""codigoInterno"": """",
  ""email"": """",
  ""dataReferencia"": """",
  ""identificacoesAdicionais"": []
}";

			sendingObject.BrokerCode = staff.GS_Code;
			CombineAssertions(() =>
			{
				AssertEquals("GetMessageTypeForEDIMessage()", MessageTypeList.Codes.OPE, sendingObject.GetMessageTypeForEDIMessage());
				AssertEquals("GetApplicationReference()", ZString.Empty, sendingObject.GetApplicationReference());
				AssertEquals("GetGlbExternalPasswordPK()", password.PK, sendingObject.GetGlbExternalPasswordPK());
				AssertEquals("GetMessageOwner()", ZString.Empty, sendingObject.GetMessageOwner());
				AssertEquals("GetMessageText()", expectedMessageString, sendingObject.GetMessageText());
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ForeignOperatorMessageSendingObject(ForeignOperator);
		}

		CusBRForeignOperator ForeignOperator => fForeignOperator ??= CreateForeignOperator();
		CusBRForeignOperator fForeignOperator;

		CusBRForeignOperator CreateForeignOperator()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "BRAZIL TEST COMPANY";
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "FOREIGNOP";
			manufacturer.OH_FullName = "TEST FOREIGN OPERATOR";

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = manufacturer.PK;
			foreignOperator.BFR_OH_ForeignOperator = owner.PK;

			return foreignOperator;
		}
	}
}
