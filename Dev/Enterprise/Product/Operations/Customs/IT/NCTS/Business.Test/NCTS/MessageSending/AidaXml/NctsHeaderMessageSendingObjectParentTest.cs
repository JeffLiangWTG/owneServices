using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
public sealed class NctsHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties()
	{
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
		AssertArrayEqualsByElements("MessageSendingObjectProperties", new ZString[] { "MessageType", "Reason", "LegislativeReference", "JobReferenceNumber", "MessageSubType" }, sendingObjectPropertyNames);
	}

	public void TestSendingObjectsCollection()
	{
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObjectsCollection = sendingObjectParent.SendingObjectsCollection;
		AssertType<NctsHeaderMessageSendingObjectCollection>("SendingObjectsCollection Type", sendingObjectsCollection);
		AssertEquals("SendingObjectsCollection Count", 1, sendingObjectsCollection.Count);
		AssertType<NctsHeaderMessageSendingObject>("SendingObject Type", sendingObjectsCollection[0]);
	}

	public void TestBizObjValidationMessageErrors_ForMessageType()
	{
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = (NctsHeaderMessageSendingObject)sendingObjectParent.SelectedSendingObjects.FirstOrDefault();

		CombineAssertions("BizObjValidationMessageErrors", () =>
		{
			sendingObject.MessageType = "NEW";
			AssertNotNullOrEmpty("When Message type = NEW", sendingObjectParent.BizObjValidationMessageErrors);

			sendingObject.MessageType = "CAN";
			AssertNullOrEmpty("When Message type = CAN", sendingObjectParent.BizObjValidationMessageErrors);
		});
	}

	public void TestBizObjValidationMessageErrorsShouldNotResetAfterDisposeIsCalled()
	{
		NctsHeaderMessageSendingObjectParent sendingObjectParent;
		NctsHeaderMessageSendingObject sendingObject = null;

		using (sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader))
		{
			sendingObject = (NctsHeaderMessageSendingObject)sendingObjectParent.SelectedSendingObjects.FirstOrDefault();

			CombineAssertions("BizObjValidationMessageErrors with events attached", () =>
			{
				sendingObject.MessageType = "CAN";
				AssertNullOrEmpty("When Message type = CAN", sendingObjectParent.BizObjValidationMessageErrors);

				sendingObject.MessageType = "NEW";
				AssertNotNullOrEmpty("When Message type = NEW", sendingObjectParent.BizObjValidationMessageErrors);
			});
		}

		CombineAssertions("BizObjValidationMessageErrors without events attached", () =>
		{
			sendingObject.MessageType = "CAN";
			AssertNotNullOrEmpty("When Message type = CAN", sendingObjectParent.BizObjValidationMessageErrors);

			sendingObject.MessageType = "NEW";
			AssertNotNullOrEmpty("When Message type = NEW", sendingObjectParent.BizObjValidationMessageErrors);
		});
	}

	public void TestBizObjValidationMessageErrors_CheckRuleC0001()
	{
		CombineAssertions("Header Level", () =>
		{
			using var testContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory);
			testContext.EnableRule(r => r.IsRuleC0001_1Active);
			testContext.DisableRule(r => r.IsRuleB1823Active);
			testContext.EnableRule(r => r.IsRuleC0001_6Active);

			var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			_ = nctsHeader.Bills.AddNew();

			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			var sendingObject = (NctsHeaderMessageSendingObject)sendingObjectParent.SelectedSendingObjects.First();

			var messageErrors = sendingObjectParent.BizObjValidationMessageErrors;
			AssertContains("C0001-1", messageErrors);
			AssertContains("C0001-6", messageErrors);
		});

		CombineAssertions("Bill Level", () =>
		{
			using var testContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			testContext.EnableRule(r => r.IsRuleC0001_6Active);
			testContext.EnableRule(r => r.IsRuleC0001_7Active);

			var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			_ = nctsHeader.Bills.AddNew();
			var bill = nctsHeader.Bills.AddNew();
			var additionalInfo = bill.AdditionalDocuments.AddNew();
			additionalInfo.CSI_SubType = "INF";
			additionalInfo.CSI_Code = "30600";

			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			var sendingObject = (NctsHeaderMessageSendingObject)sendingObjectParent.SelectedSendingObjects.First();

			var messageErrors = sendingObjectParent.BizObjValidationMessageErrors;
			AssertContains("C0001-6", messageErrors);
			AssertContains("C0001-7", messageErrors);
		});
	}

	public void TestMessageSendingObjectProperty_Reason()
	{
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var prop = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == "Reason");
		AssertNotNull(prop);
		AssertEquals("Reason Column Width", 80, prop.ColumnWidth);
	}

	public void TestMessageSendingObjectProperty_LegislativeReference()
	{
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var prop = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == "LegislativeReference");
		AssertNotNull(prop);
		AssertEquals("LegislativeReference Column Width", 80, prop.ColumnWidth);
	}

	public void TestMessageSendingObjectProperty_MessageSubType()
	{
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var prop = sendingObjectParent.MessageSendingObjectProperties.FirstOrDefault(x => x.PropertyName == "MessageSubType");
		AssertNotNull(prop);
		AssertEquals("MessageSubType Column Width", 80, prop.ColumnWidth);
	}

	public void TestSaveAndSaveMessages_WhenMessageTypeIsCAN()
	{
		SetupStaffCryptoKiCertificate();

		var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = "CAN";

		messageSendingObjectParent.SendAndSaveMessages();
		movementHeader.Messages.Reload(reLoadExistingRows: false);
		AssertEquals(1, movementHeader.Messages.Count);
		AssertContains("Cancellation Message", "<tns:serviceId>annullaDichiarazione</tns:serviceId>", movementHeader.Messages[0].EM_MessageText);
	}

	public void TestSaveAndSaveMessages_AssignDeclarationGoodsItemNumbersWhenMessageTypeIsNEW()
	{
		SetupStaffCryptoKiCertificate();

		var bill1 = nctsHeader.Bills.AddNew();
		var bill2 = nctsHeader.Bills.AddNew();

		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill1.GoodsItems.AddNew();
		var goodsItem3 = bill2.GoodsItems.AddNew();

		var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = "NEW";

		messageSendingObjectParent.SendAndSaveMessages();
		CombineAssertions("", () =>
		{
			AssertEquals("House 1, Goods Item LineNo 1, BY_DeclarationGoodsItemNumber", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("House 1, Goods Item LineNo 2, BY_DeclarationGoodsItemNumber", 2, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("House 2, Goods Item LineNo 1, BY_DeclarationGoodsItemNumber", 3, goodsItem3.BY_DeclarationGoodsItemNumber);
		});

		nctsHeader.BH_MessageStatus = "";
		nctsHeader.MovementHeader.BM_CustomsStatus = "";

		var goodsItem4 = bill1.GoodsItems.AddNew();
		messageSendingObjectParent.SendAndSaveMessages();
		CombineAssertions("When a NCTS is resent (DeclarationGoodsItemNumber are already assigned)", () =>
		{
			AssertEquals("House 1, Goods Item LineNo 1, BY_DeclarationGoodsItemNumber", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("House 1, Goods Item LineNo 2, BY_DeclarationGoodsItemNumber", 2, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("House 1, Goods Item LineNo 3, BY_DeclarationGoodsItemNumber", 3, goodsItem4.BY_DeclarationGoodsItemNumber);
			AssertEquals("House 2, Goods Item LineNo 1, BY_DeclarationGoodsItemNumber", 4, goodsItem3.BY_DeclarationGoodsItemNumber);
		});
	}

	public void TestSaveAndSaveMessages_AssignDeclarationGoodsItemNumbersWhenMessageTypeIsCAN()
	{
		SetupStaffCryptoKiCertificate();

		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();

		var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = "CAN";

		messageSendingObjectParent.SendAndSaveMessages();
		AssertEquals("When MessageType is CAN, BY_DeclarationGoodsItemNumber", true, goodsItem1.BY_DeclarationGoodsItemNumber.IsEmpty);
	}

	public void TestSaveAndSaveMessages_AssignDeclarationGoodsItemNumbersWhenMessageTypeIsAMD()
	{
		SetupStaffCryptoKiCertificate();

		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		goodsItem1.BY_DeclarationGoodsItemNumber = 2;
		var goodsItem2 = bill1.GoodsItems.AddNew();

		var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = "AMD";

		messageSendingObjectParent.SendAndSaveMessages();
		CombineAssertions("When MessageType is AMD", () =>
		{
			AssertEquals("Goods Item 1, BY_DeclarationGoodsItemNumber", 2, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Goods Item 2, BY_DeclarationGoodsItemNumber", 3, goodsItem2.BY_DeclarationGoodsItemNumber);
		});
	}

	[TestDate(2023, 03, 25)]
	public void TestSaveAndSaveMessages_PopulateMessageWithLRN()
	{
		SetupStaffCryptoKiCertificate();

		var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = "NEW";
		sendingObject.MessageSubType = "D1";

		messageSendingObjectParent.SendAndSaveMessages();
		movementHeader.Messages.Reload(reLoadExistingRows: false);
		AssertEquals("Messages Count", 1, movementHeader.Messages.Count);

		var base64Content = Regex.Match(movementHeader.Messages[0].EM_MessageText, "<tns:xml>(.+)<\\/tns:xml>").Groups[1].Value;
		var base64Bytes = Convert.FromBase64String(base64Content);
		var effectiveMessageContent = Encoding.UTF8.GetString(base64Bytes);
		AssertContains("Generated Message Content", "<LRN>2023EDIDAT000000000001</LRN>", effectiveMessageContent);
	}

	public void TestSaveAndSaveMessages_AddLockForEditLogWithDefaultLockForEditConfiguration()
	{
		SetupStaffCryptoKiCertificate();

		var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
		sendingObject.MessageType = "NEW";
		sendingObject.MessageSubType = "D1";

		messageSendingObjectParent.SendAndSaveMessages();

		CombineAssertions(() =>
		{
			AssertHasLockForEditLog(hasLockForEditLog: true);
			AssertEquals("Ncts Has Changes?", false, nctsHeader.HasChanges);
		});
	}

	public void TestSaveAndSaveMessages_AddLockForEditLogWithEmptyLockForEditConfiguration()
	{
		SetupStaffCryptoKiCertificate();

		var itemSet = CustomsDataRegistry.Instance;
		var itConfigCollection = itemSet.DeclarationLockForEdit.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		itConfigCollection.RemoveAndDeleteAll();

		using (itemSet.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itConfigCollection))
		{
			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			var sendingObject = messageSendingObjectParent.SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().First();
			sendingObject.MessageType = "NEW";
			sendingObject.MessageSubType = "D1";

			messageSendingObjectParent.SendAndSaveMessages();

			CombineAssertions("When there are no Lock for Edit configuration", () =>
			{
				AssertHasLockForEditLog(hasLockForEditLog: false);
				AssertEquals("Ncts Has Changes?", false, nctsHeader.HasChanges);
			});
		}
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		movementHeader = nctsHeader.MovementHeader;
	}

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

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	void AssertHasLockForEditLog(bool hasLockForEditLog)
	{
		var hasLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.LockForEdit.Code)
			.AddToFilter(StmALogSchema.SL_Reference, "The tabs are locked for editing because the message has been sent.");
		AssertEquals("Ncts Has Lock for Edit log?", hasLockForEditLog, nctsHeader.Logs.HasLogWith(hasLogQuery));
	}
}
