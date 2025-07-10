using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageMessageSendingObjectParent))]
sealed class TemporaryStorageMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties()
	{
		var sendingObjectParent = (TemporaryStorageMessageSendingObjectParent)GetNewBusinessObject();
		AssertArrayEqualsByElements(
			"MessageSendingObjectProperties",
			["MessageType", "JobReferenceNumber", "DeclarationType"],
			sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray());
	}

	public void TestSendingObjectsCollection()
	{
		var sendingObjectParent = (TemporaryStorageMessageSendingObjectParent)GetNewBusinessObject();
		var sendingObjectsCollection = sendingObjectParent.SendingObjectsCollection;
		AssertType<EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>>("SendingObjectsCollection Type", sendingObjectsCollection);
		AssertEquals("SendingObjectsCollection Count", 1, sendingObjectsCollection.Count);
		AssertType<TemporaryStorageMessageSendingObject>("SendingObject Type", sendingObjectsCollection[0]);
	}

	public void TestSendMessage()
	{
		SetupStaffCryptoKiCertificate();

		var sendingObjectParent = (TemporaryStorageMessageSendingObjectParent)GetNewBusinessObject();
		var sendingObject = sendingObjectParent.SelectedSendingObjects.Cast<TemporaryStorageMessageSendingObject>().First();
		sendingObject.MessageType = "NEW";
		sendingObject.DeclarationType = "G4";

		sendingObjectParent.SendMessage();
		header.Messages.Reload(reLoadExistingRows: false);
		AssertEquals("Messages Count", 1, header.Messages.Count);
		AssertEquals("EM_MessageType", "NEW", header.Messages[0].EM_MessageType);
		AssertEquals("EM_MessageSubType", "G4", header.Messages[0].EM_MessageSubType);
		AssertEquals("AMA_MessageStatus", "SNT", header.AMA_MessageStatus);
	}

	public void TestCustomsProfile()
	{
		header.AMA_CustomsProfile = ZString.Empty;
		var sendingObjectParent = (TemporaryStorageMessageSendingObjectParent)GetNewBusinessObject();
		AssertEquals("Empty CustomsProfile", ZString.Empty, sendingObjectParent.CustomsProfile);

		header.AMA_CustomsProfile = "12345";
		AssertEquals("CustomsProfile", "12345", sendingObjectParent.CustomsProfile);
	}

	protected override BusinessObject GetNewBusinessObject() => new TemporaryStorageMessageSendingObjectParent(header);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;

	public static void SetupStaffCryptoKiCertificate()
	{
		var staffWrapper = (GlbStaffWrapper)GlbStaff.CurrentUser.GetITWrapper();
		var glbStaffCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		glbStaffCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbStaffCertificate.GP_Name = "BIT4ID";
		glbStaffCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
		certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
		certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Italy;
		certificate.XZ_ExpiryOrDueDate = new ZDateTime(2022, 1, 2);
		certificate.XZ_IssueDate = new ZDateTime(2022, 1, 1);

		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x =>
				x.SignXadesWithToken(
					It.IsAny<byte[]>(),
					It.IsAny<Chipset>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<DateTime>())
				)
			.Returns((
				byte[] input,
				Chipset chipset,
				string serialNumber,
				string tokenPin,
				DateTime signatureTime) => input);

		ObjectFactory.Substitute(cryptoApiMock.Object);
	}
}
