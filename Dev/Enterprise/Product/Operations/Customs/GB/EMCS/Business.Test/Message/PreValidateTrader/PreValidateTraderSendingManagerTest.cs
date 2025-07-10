using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(PreValidateTraderSendingManager))]
	sealed class PreValidateTraderSendingManagerTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorDeclaration()
		{
			_ = new PreValidateTraderSendingManager(null, traderInfo);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorTraderInfo()
		{
			_ = new PreValidateTraderSendingManager(declaration, null);
		}

		public void TestDeliveryFailure()
		{
			using (Factory.AddDisposableService())
			{
				var manager = new PreValidateTraderSendingManagerToTestDeliveryFailure(declaration, traderInfo);
				manager.Send();

				var notification = manager.NotificationCollection.Last();
				AssertEquals("Check message", "An error occurred while trying to queue Pre-Validate Trader", notification.Message);
				AssertEquals("Should be Error", expected: true, notification.IsError);
			}
		}

		public void TestDeliverySuccess()
		{
			using (Factory.AddDisposableService())
			{
				var manager = new PreValidateTraderSendingManager(declaration, traderInfo);
				manager.Send();

				var notification = manager.NotificationCollection.Last();
				AssertEquals("Check message", "Pre-Validate Trader has been queued successfully", notification.Message);
				AssertEquals("Should be Information", expected: true, notification.IsInformation);

				var ediMsg = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkTable, nameof(JobDeclaration)).AddToFilter(EDIMessageSchema.EM_LinkUniqueID, declaration.PK));
				AssertNotNull(ediMsg);
				AssertEquals(EDIMessage.ApplicationCodes.UniversalDataMessaging, ediMsg.EM_ApplicationCode);
				AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalEvent, ediMsg.EM_MessageType);
				AssertEquals(EDIMessage.Direction.Transmit, ediMsg.EM_ReceiveTransmit);
				AssertEquals(EDIMessage.Status.Sent, ediMsg.EM_Status);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.SupplierDocumentaryAddress.OrganisationPK = TestHelper.GetPartyTraderExciseNumberOrg(Factory, "TEN0000001", Core.Constants.CountryCodes.UnitedKingdom).PK;

			var invoice = declaration.Invoices.AddNew();
			for (var i = 1; i < 13; i++)
			{
				var value = i.ToString();
				var line1 = invoice.InvoiceLines.AddNew();
				line1.JI_PartNo = value;
				line1.JI_Tariff = value;
				line1.ZG_ExciseProductCode = value;
				line1.JI_NDescription = value;
			}

			traderInfo = new PreValidateTraderInfo(declaration);
		}

		EU.EMCS.Business.EMCSJobDeclaration declaration;
		PreValidateTraderInfo traderInfo;
	}

	class PreValidateTraderSendingManagerToTestDeliveryFailure : PreValidateTraderSendingManager
	{
		public PreValidateTraderSendingManagerToTestDeliveryFailure(EU.EMCS.Business.EMCSJobDeclaration declaration, PreValidateTraderInfo traderInfo) : base(declaration, traderInfo) { }
		protected override bool Deliver(EU.EMCS.Business.EMCSJobDeclaration declaration, Event universalEvent, MessageSendingNotificationCollection notificationCollection, EDIMessage message) => false;
	}
}
