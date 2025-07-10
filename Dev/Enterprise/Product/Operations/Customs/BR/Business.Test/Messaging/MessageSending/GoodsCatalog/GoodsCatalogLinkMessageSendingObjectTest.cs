using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GoodsCatalogLinkMessageSendingObject))]
	class GoodsCatalogLinkMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestDefaultMessageType()
		{
			var(sendingObject, goodcatalog) = GetNewBusinessObject();
			AssertEquals("Default Message Type", "LIN", sendingObject.MessageType);
			AssertEquals("Message Attache", goodcatalog, sendingObject.MessageAttachee);
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

			var (sendingObject, goodcatalog) = GetNewBusinessObject();
			var expectedMessageString = @"[
  {
    ""cpfCnpjRaiz"": ""75400331"",
    ""codigoOperadorEstrangeiro"": ""OPE_1"",
    ""cpfCnpjFabricante"": """",
    ""conhecido"": true,
    ""codigoProduto"": 0,
    ""vincular"": true,
    ""codigoPais"": ""BR""
  },
  {
    ""cpfCnpjRaiz"": ""75400331"",
    ""cpfCnpjFabricante"": """",
    ""conhecido"": false,
    ""codigoProduto"": 0,
    ""vincular"": false,
    ""codigoPais"": ""UY""
  }
]";

			sendingObject.Parent.BrokerCode = staff.GS_Code;
			CombineAssertions(() =>
			{
				AssertEquals("GetMessageTypeForEDIMessage()", MessageTypeList.Codes.CAT, sendingObject.GetMessageTypeForEDIMessage());
				AssertEquals("GetApplicationReference()", ZString.Empty, sendingObject.GetApplicationReference());
				AssertEquals("GetGlbExternalPasswordPK()", password.PK, sendingObject.GetGlbExternalPasswordPK());
				AssertEquals("GetMessageOwner()", ZString.Empty, sendingObject.GetMessageOwner());
				AssertEquals("GetMessageText()", expectedMessageString, sendingObject.GetMessageText());
			});
		}

		protected (GoodsCatalogLinkMessageSendingObject, CusGoodsCatalog) GetNewBusinessObject()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";
			owner.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75400331", Core.Constants.CountryCodes.Brazil);

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_OH_Owner = owner.PK;
			var productionInfo = goodsCatalog.ForeignOperators.AddNew();
			productionInfo.CGI_SystemCreateTimeUtc = ZDateTime.Now;
			productionInfo.CountryCode = "BR";
			productionInfo.AuthorityCode = "OPE_1";
			productionInfo = goodsCatalog.ForeignOperators.AddNew();
			productionInfo.CGI_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			productionInfo.CountryCode = "UY";
			productionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			var parent = new GoodsCatalogMessageSendingObject(goodsCatalog);
			parent.Action = Constants.Situation.Active;
			var sendingObject = new GoodsCatalogLinkMessageSendingObject(parent, goodsCatalog.ForeignOperators);

			return (sendingObject, goodsCatalog);
		}
	}
}
