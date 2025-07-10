using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ESNctsMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestNewMessageBuilderNctsDeparture()
		{
			AssertMessageBuilderTypeWithSendingObject<DepartureMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.NctsDeparture);
		}

		public void TestNewMessageBuilderNctsTIR()
		{
			AssertMessageBuilderTypeWithSendingObject<TIRMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.NctsTir);
		}

		public void TestNewMessageBuilderNctsArrivalNotification()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<ArrivalMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival, DeclarationMessageTypeList.Codes.NctsArrivalNotification);

				AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.NctsArrivalNotification, messageBuilder.MessageType);
				AssertContains("MessageText contains the message type", "BGM+AVI", messageBuilder.GetSignedMessageText());
			});
		}

		public void TestNewMessageBuilderNctsUnloadingRemarks()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<ArrivalMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival, DeclarationMessageTypeList.Codes.NctsUnloadingRemarks);

				AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.NctsUnloadingRemarks, messageBuilder.MessageType);
				AssertContains("MessageText contains the message type", "BGM+OBS", messageBuilder.GetSignedMessageText());
			});
		}

		public void TestNewMessageBuilderNctsArrivalNotificationWithDeparture()
		{
			nctsHeader.MovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<ArrivalMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.DepartureAndArrival, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi);

				AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi, messageBuilder.MessageType);
				AssertContains("MessageText contains the message type", "BGM+TNA", messageBuilder.GetSignedMessageText());
			});
		}

		public void TestNewMessageBuilderNctsArrivalNotificationWithUnloadingRemarks()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<ArrivalMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs);

				AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs, messageBuilder.MessageType);
				AssertContains("MessageText contains the message type", "BGM+AVO", messageBuilder.GetSignedMessageText());
			});
		}

		public void TestNewMessageBuilderNctsArrivalNotificationWithDepartureAndUnloadingRemarks()
		{
			nctsHeader.MovementHeader.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<ArrivalMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.DepartureAndArrival, DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb);

				AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb, messageBuilder.MessageType);
				AssertContains("MessageText contains the message type", "BGM+TAO", messageBuilder.GetSignedMessageText());
			});
		}

		public void TestNewMessageBuilderNcts5Departure()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<DepartureNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.Ncts5Departure);
#if NET
				AssertContains("messageText contains the correct value for a departure", "<additionalDeclarationType>A</additionalDeclarationType>", messageBuilder.GetSignedMessageText());
#else
				AssertContains("messageText contains the correct value for a departure", "<q1:additionalDeclarationType>A</q1:additionalDeclarationType>", messageBuilder.GetSignedMessageText());
#endif
			});
		}

		public void TestNewMessageBuilderNcts5DeparturePreDeclaration()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<DepartureNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
#if NET
				AssertContains("messageText contains the correct value for a departure predeclaration", "<additionalDeclarationType>D</additionalDeclarationType>", messageBuilder.GetSignedMessageText());
#else
				AssertContains("messageText contains the correct value for a departure predeclaration", "<q1:additionalDeclarationType>D</q1:additionalDeclarationType>", messageBuilder.GetSignedMessageText());
#endif
			});
		}

		public void TestNewMessageBuilderNcts5Arrival()
		{
			AssertMessageBuilderTypeWithSendingObject<ArrivalNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival, DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification);
		}

		public void TestNewMessageBuilderNcts5NotificationGoods()
		{
			AssertMessageBuilderTypeWithSendingObject<NotifGoodsNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification);
		}

		public void TestNewMessageBuilderNcts5Amendment()
		{
			CombineAssertions(() =>
			{
				var messageBuilder = AssertMessageBuilderTypeWithSendingObject<AmendmentNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);
#if NET
				AssertContains("messageText contains the correct value for an Amendment", "<additionalDeclarationType>D</additionalDeclarationType>", messageBuilder.GetSignedMessageText());
#else
				AssertContains("messageText contains the correct value for an Amendment", "<q1:additionalDeclarationType>D</q1:additionalDeclarationType>", messageBuilder.GetSignedMessageText());
#endif
			});
		}

		public void TestNewMessageBuilderNcts5Cancel()
		{
			var messageBuilder = AssertMessageBuilderTypeWithSendingObject<CancelNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation);
		}

		public void TestNewMessageBuilderNcts5Query()
		{
			AssertMessageBuilderTypeWithSendingObject<QueryNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, DeclarationMessageTypeList.Codes.TransitNcts5Query);
		}

		public void TestNewMessageBuilderNcts5TNN()
		{
			var mrnCode = "1234567890";

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMrnFromUser = mrnCode;
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;

			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = mrnCode;
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			var messageBuilderManager = new ESNctsMessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<TNNNCTSMessageBuilder>("NewMessageBuilder is Ncts5 TNN type", messageBuilder);
		}

		public void TestNewMessageBuilderNcts5NotificationUnloading()
		{
			AssertMessageBuilderTypeWithSendingObject<NotifUnloadingNCTSMessageBuilder>(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival, DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods);
		}

		public void TestNewMessageBuilderNotImplementedException_WithoutSendingObject()
		{
			var messageBuilderManager = new ESNctsMessageBuilderManager(nctsHeader, certificate, ZString.Empty);
			AssertExceptionThrown<NotImplementedException>("NewMessageBuilder throws an exception for any other DeclarationMessageType not yet supported", () => messageBuilderManager.NewMessageBuilder());
		}

		public void TestNewMessageBuilderNotImplementedException_WithSendingObject()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			sendingObject.MessageType = "AAA";
			var messageBuilderManager = new ESNctsMessageBuilderManager(sendingObject, certificate);
			AssertExceptionThrown<NotImplementedException>("NewMessageBuilder throws an exception for any other DeclarationMessageType not yet supported", () => messageBuilderManager.NewMessageBuilder());
		}
		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestGetCompleteMessageNctsDeparture_WithoutSendingObject()
		{
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.BH_JobReference = "NCT00000001";

				var messageBuilderManager = new ESNctsMessageBuilderManager(nctsHeader, certificate, DeclarationMessageTypeList.Codes.NctsDeparture);
				var messageBuilder = messageBuilderManager.NewMessageBuilder();

				CombineAssertions(() =>
				{
					AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.NctsDeparture, messageBuilder.MessageType);
					AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
					AssertEquals("messageBuilder.Provider.IsTest", false, messageBuilder.Provider.IsTest);
					AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
					AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
					AssertEquals("messageBuilder.Provider.BusinessObjectReference", nctsHeader.BH_JobReference, messageBuilder.Provider.BusinessObjectReference);
					AssertMultilineASCIIEquals("messageBuilder.CompleteMessageText", ExpectedStringNctsDeparture, messageBuilder.GetSignedMessageText().Replace("'", "'\n"));
				});
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestGetCompleteMessageNctsDeparture_WithSendingObject()
		{
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				nctsHeader.BH_JobReference = "NCT00000001";

				var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
				sendingObject.MessageType = DeclarationMessageTypeList.Codes.NctsDeparture;

				var messageBuilderManager = new ESNctsMessageBuilderManager(sendingObject, certificate);
				var messageBuilder = messageBuilderManager.NewMessageBuilder();

				CombineAssertions(() =>
				{
					AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.NctsDeparture, messageBuilder.MessageType);
					AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
					AssertEquals("messageBuilder.Provider.IsTest", false, messageBuilder.Provider.IsTest);
					AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
					AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
					AssertEquals("messageBuilder.Provider.BusinessObjectReference", nctsHeader.BH_JobReference, messageBuilder.Provider.BusinessObjectReference);
					AssertMultilineASCIIEquals("messageBuilder.CompleteMessageText", ExpectedStringNctsDeparture, messageBuilder.GetSignedMessageText().Replace("'", "'\n"));
				});
			}
		}

		public void TestNewMessageBuilderNctsAnnex_WithoutSendingObject()
		{
			var eDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = nctsHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			var docPivotList = new List<NctsCusStorageDocPivot>() { pivot };

			var messageBuilderManager = new ESNctsMessageBuilderManager(nctsHeader, certificate, DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes);
			var messageBuilder = messageBuilderManager.NewNCTSAnnexMessageBuilder(docPivotList, "S");
			AssertType<AnnexNCTSMessageBuilder>("NewMessageBuilder is Ncts5DepartureAnnexes type", messageBuilder);
		}

		public void TestNewMessageBuilderNctsAnnex_WithSendingObject()
		{
			var eDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = nctsHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			var docPivotList = new List<NctsCusStorageDocPivot>() { pivot };

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes;

			var messageBuilderManager = new ESNctsMessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewNCTSAnnexMessageBuilder(docPivotList, "S");
			AssertType<AnnexNCTSMessageBuilder>("NewMessageBuilder is Ncts5DepartureAnnexes type", messageBuilder);
		}

		T AssertMessageBuilderTypeWithSendingObject<T>(ZString applicationCode, ZString headerType, ZString messageType)
			where T : IMessageBuilderBase
		{
			nctsHeader.BH_ApplicationCode = applicationCode;
			nctsHeader.BH_HeaderType = headerType;

			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			sendingObject.MessageType = messageType;
			sendingObject.ReasonForCancellation = "Cancel reason";

			var messageBuilderManager = new ESNctsMessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<T>("NewMessageBuilder is " + messageType + " type", messageBuilder);
			return (T)messageBuilder;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.FillWithValidTestData();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.MovementHeader.GoodsItems.AddNew();
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
		}
		NctsHeader nctsHeader;
		CertificateProviderTestClass certificate;
		GlbStaff staff;

		const string ExpectedStringNctsDeparture = @"UNB+UNOA:1+:ZZ+AEATADUE:ZZ+200109:1513+<<MSGNO PLACEHOLDER>>++&EE'
UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:1:921:UN:TEX011'
BGM+969+NCT00000001+9'
CST++++T1:149:141++:113:148'
GIS+0:109:141'
RFF+ABJ:NCT00000001'
NAD+DT+::148++EDI CUSTOMS BROKERS'
MOA+ZZZ::EUR'
UNS+D'
CST+1+:122:148++++:116:141'
UNS+S'
CNT+5:1'
CNT+11:0'
UNT+13+<<MSGNO PLACEHOLDER>>'
UNZ+1+<<MSGNO PLACEHOLDER>>'";
	}
}
