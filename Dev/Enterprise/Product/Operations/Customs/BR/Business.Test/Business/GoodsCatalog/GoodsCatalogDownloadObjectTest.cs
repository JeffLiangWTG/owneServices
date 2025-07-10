using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GoodsCatalogDownloadObject))]
	public class GoodsCatalogDownloadObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBrokerCertificate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERDOWNLOADCATALOG";
			staff.GS_Code = "DOW";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var goodsCatalogDownloadObject = new GoodsCatalogDownloadObject(Factory);
			goodsCatalogDownloadObject.BrokerCode = "DOW";

			AssertEquals(staff, goodsCatalogDownloadObject.Broker);
			AssertEquals(password, goodsCatalogDownloadObject.BrokerCertificate);
			AssertEquals(password.PK, goodsCatalogDownloadObject.GetGlbExternalPasswordPK());
		}

		public void TestIMessageSendingObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "XVBQP68S";
			orgHeader.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75.400.331/0001-15", Core.Constants.CountryCodes.Brazil);

			var goodsCatalogDownloadObject = new GoodsCatalogDownloadObject(Factory);
			goodsCatalogDownloadObject.OwnerCode = orgHeader.OH_Code;
			goodsCatalogDownloadObject.MessageType = EDIMessageSubTypeList.Codes.CatalogZipFile;
			goodsCatalogDownloadObject.DownloadDeactivated = ZBool.True;
			CombineAssertions("CatalogZipFile1", () =>
			{
				AssertEquals(MessageTypeList.Codes.CAT, goodsCatalogDownloadObject.GetMessageTypeForEDIMessage());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageOwner());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageText());
				AssertEquals("75400331|true", goodsCatalogDownloadObject.GetApplicationReference());
			});

			goodsCatalogDownloadObject.DownloadDeactivated = ZBool.False;
			CombineAssertions("CatalogZipFile2", () =>
			{
				AssertEquals(MessageTypeList.Codes.CAT, goodsCatalogDownloadObject.GetMessageTypeForEDIMessage());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageOwner());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageText());
				AssertEquals("75400331|false", goodsCatalogDownloadObject.GetApplicationReference());
			});

			goodsCatalogDownloadObject.MessageType = EDIMessageSubTypeList.Codes.ManufacturerZipFile;
			CombineAssertions("ManufacturerZipFile", () =>
			{
				AssertEquals(MessageTypeList.Codes.CAT, goodsCatalogDownloadObject.GetMessageTypeForEDIMessage());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageOwner());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageText());
				AssertEquals("75400331", goodsCatalogDownloadObject.GetApplicationReference());
			});

			goodsCatalogDownloadObject.MessageType = EDIMessageSubTypeList.Codes.OperatorZipFile;
			CombineAssertions("OperatorZipFile1", () =>
			{
				AssertEquals(MessageTypeList.Codes.CAT, goodsCatalogDownloadObject.GetMessageTypeForEDIMessage());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageOwner());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageText());
				AssertEquals("75400331|false", goodsCatalogDownloadObject.GetApplicationReference());
			});

			goodsCatalogDownloadObject.DownloadDeactivated = ZBool.True;
			CombineAssertions("OperatorZipFile1", () =>
			{
				AssertEquals(MessageTypeList.Codes.CAT, goodsCatalogDownloadObject.GetMessageTypeForEDIMessage());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageOwner());
				AssertEquals(ZString.Empty, goodsCatalogDownloadObject.GetMessageText());
				AssertEquals("75400331|true", goodsCatalogDownloadObject.GetApplicationReference());
			});

			AssertEquals("Message Attache", orgHeader, goodsCatalogDownloadObject.MessageAttachee);
		}

		public void TestOwnerRootCNPJAndGetApplicationReference()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TEST1";
			orgHeader.OH_FullName = "TEST 1";
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "58.500.398/-04", Core.Constants.CountryCodes.Brazil);
			orgHeader.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "58500398", Core.Constants.CountryCodes.Brazil);

			var goodsCatalogDownloadObject = new GoodsCatalogDownloadObject(Factory);
			goodsCatalogDownloadObject.MessageType = EDIMessageSubTypeList.Codes.ManufacturerZipFile;
			AssertEquals("Owner", null, goodsCatalogDownloadObject.Owner);
			AssertEquals("OwnerRootCNPJ", ZString.Empty, goodsCatalogDownloadObject.OwnerRootCNPJ);
			AssertEquals("GetApplicationReference", ZString.Empty, goodsCatalogDownloadObject.GetApplicationReference());

			goodsCatalogDownloadObject.OwnerCode = "TEST1";
			AssertEquals("Owner", orgHeader, goodsCatalogDownloadObject.Owner);
			AssertEquals("OwnerRootCNPJ", "58500398", goodsCatalogDownloadObject.OwnerRootCNPJ);
			AssertEquals("GetApplicationReference", "58500398", goodsCatalogDownloadObject.GetApplicationReference());
		}

		public void TestDownloadDeactivated_ReadOnly()
		{
			var goodsCatalogDownloadObject = new GoodsCatalogDownloadObject(Factory);

			goodsCatalogDownloadObject.DownloadCatalog = ZBool.False;
			AssertEquals("Download Deactivated should be Read Only", true, goodsCatalogDownloadObject.DownloadDeactivatedInfo.ReadOnly);

			goodsCatalogDownloadObject.DownloadCatalog = ZBool.True;
			AssertEquals("Download Deactivated should be Read Only", false, goodsCatalogDownloadObject.DownloadDeactivatedInfo.ReadOnly);
		}

		public void TestClearDownloadDeactivated()
		{
			var goodsCatalogDownloadObject = new GoodsCatalogDownloadObject(Factory);

			goodsCatalogDownloadObject.DownloadDeactivated = ZBool.True;
			AssertEquals("Download Deactivated should be true", true, goodsCatalogDownloadObject.DownloadDeactivated);

			goodsCatalogDownloadObject.DownloadCatalog = ZBool.True;
			AssertEquals("Download Deactivated should be Clear to false", false, goodsCatalogDownloadObject.DownloadDeactivated);
		}

		public void TestDownloadForeignOperator_ReadOnly()
		{
			var goodsCatalog = new GoodsCatalogDownloadObject(Factory);
			Assert("Download Catalog is not selected", !goodsCatalog.DownloadCatalog);
			Assert("Download Foreign Operator is not selected", !goodsCatalog.DownloadForeignOperator);
			Assert("Download Foreign Operator is not readonly", !goodsCatalog.DownloadForeignOperatorInfo.ReadOnly);

			goodsCatalog.DownloadCatalog = ZBool.True;
			Assert("Download Foreign Operator is selected", goodsCatalog.DownloadForeignOperator);
			Assert("Download Foreign Operator is readonly", goodsCatalog.DownloadForeignOperatorInfo.ReadOnly);
		}
	}
}
