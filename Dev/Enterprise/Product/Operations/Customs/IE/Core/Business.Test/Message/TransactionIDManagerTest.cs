using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.TransactionIDResponse;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class TransactionIDManagerTest : TestCaseWithFactory
	{
		public void TestNothingIsCreatedIfNoIrishMessage()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			helper.SetupTransactionIDURL();
			Factory.Save();
			CombineAssertions(() =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});
		}

		public void TestSetTransactionNumbersAsUsed_IECustoms()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ZAC";
			company.GC_Name = "TEST IE COMP 1";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = company.PK;
			transactionNumber1.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber1.TN_IsUsed = false;

			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber2.TN_GC_Company = company.PK;
			transactionNumber2.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber2.TN_IsUsed = false;

			var transactionNumber3 = Factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber3.TN_GC_Company = new ZGuid();
			transactionNumber3.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber3.TN_IsUsed = false;

			var transactionNumber4 = Factory.New<CusTransactionNumber>();
			transactionNumber4.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber4.TN_GC_Company = company.PK;
			transactionNumber4.TN_GP_ExternalPassword = ZGuid.NewZGuid();
			transactionNumber4.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber4.TN_IsUsed = false;

			TransactionIDManager.SetTransactionNumbersAsUsed(company, ZGuid.Empty, CusTransactionNumberTypeList.Codes.IECustoms);

			CombineAssertions(() =>
			{
				AssertEquals("Should be set to used", ZBool.True, transactionNumber1.TN_IsUsed);
				AssertEquals("Should not be set to used - wrong type", ZBool.False, transactionNumber2.TN_IsUsed);
				AssertEquals("Should not be set to used - wrong company", ZBool.False, transactionNumber3.TN_IsUsed);
				AssertEquals("Should not be set to used - wrong credential", ZBool.False, transactionNumber4.TN_IsUsed);
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestAllocate_UsingZZGetNoOfDays()
		{
			TestHelper.SetupTransactionIDNoOfDays(Factory, 5);
			AssertAllocate_GetNoOfDays(transactionID2);
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestAllocate_UsingDefaultGetNoOfDays()
		{
			TestHelper.DeleteTransactionIDNoOfDaysSetting(Factory);
			AssertAllocate_GetNoOfDays(transactionID3);
		}

		void AssertAllocate_GetNoOfDays(string expectedTransactionID)
		{
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany1);
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranch1.PK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = ieCompany1.PK;
			transactionNumber1.TN_TransactionReference = transactionID1;
			transactionNumber1.TN_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-6);
			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = ieCompany1.PK;
			transactionNumber2.TN_TransactionReference = transactionID2;
			transactionNumber2.TN_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-4);
			var transactionNumber3 = Factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber3.TN_GC_Company = ieCompany1.PK;
			transactionNumber3.TN_TransactionReference = transactionID3;
			var transactionNumber4 = Factory.New<CusTransactionNumber>();
			transactionNumber4.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber4.TN_GC_Company = ieCompany1.PK;
			transactionNumber4.TN_TransactionReference = transactionID4;
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				AssertEDIMessage("outgoingMessage1", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage1.PK), AESOutboundEDIMessage.Status.Pending, expectedTransactionID);
			});
		}

		public void TestHandlingMoreTransactionIDsThanExistingBlanks()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var sessionGUID = ZGuid.NewZGuid();
			var trackingReference = sessionGUID.ToString();
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = ieCompany.PK;
			transactionNumber1.TN_TrackingReference = trackingReference;
			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = ieCompany.PK;
			transactionNumber2.TN_TrackingReference = trackingReference;
			var transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID2);
			transactionIDResponse.Transactions.Add(transactionID1);
			transactionIDResponse.Transactions.Add(transactionID3);
			transactionIDResponse.Transactions.Add(transactionID4);
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID, ieBranch.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				var queryCount = new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, SQLComparisonOperator.NotEqual, ZString.Empty);
				var existingTotal = Factory.GetDatabaseCount(typeof(CusTransactionNumber), queryCount);
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				AssertEquals("Two more extra", existingTotal + 2, anotherFactory.GetDatabaseCount(typeof(CusTransactionNumber), queryCount));
				transactionNumber1 = anotherFactory.Load<CusTransactionNumber>(transactionNumber1.PK);
				AssertEquals("transactionNumber1.TN_TransactionReference", transactionID2, transactionNumber1.TN_TransactionReference);
				transactionNumber2 = anotherFactory.Load<CusTransactionNumber>(transactionNumber2.PK);
				AssertEquals("transactionNumber2.TN_TransactionReference", transactionID1, transactionNumber2.TN_TransactionReference);
			});
		}

		public void TestProcessTransactionRequestResponseForEMCS()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var companyWrapper = GlbCompanyWrapper.Get(ieCompany);
			var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;

			var transactionIDs = new List<ZString>();
			var sessionGUID = ZGuid.NewZGuid();
			var trackingReference = sessionGUID.ToString();
			var nonEmptyTransactionNumber = Factory.New<CusTransactionNumber>();
			nonEmptyTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			nonEmptyTransactionNumber.TN_GC_Company = ieCompany.PK;
			nonEmptyTransactionNumber.TN_GP_ExternalPassword = emcsCredential.PK;
			nonEmptyTransactionNumber.TN_TrackingReference = trackingReference;
			var existingTransactionID = ZGuid.NewZGuid().ToString();
			nonEmptyTransactionNumber.TN_TransactionReference = existingTransactionID;
			var transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			var transactionNumbers = new List<CusTransactionNumber>();
			for (var i = 1; i < 51; i++)
			{
				var transactionID = ZGuid.NewZGuid().ToString();
				transactionIDs.Add(transactionID);
				transactionIDResponse.Transactions.Add(transactionID);
				var transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
				transactionNumber.TN_GC_Company = ieCompany.PK;
				transactionNumber.TN_GP_ExternalPassword = emcsCredential.PK;
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumbers.Add(transactionNumber);
			}
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.TransactionID, InterchangeProcessorTestHelper.GetEMCSSOAPEnvelopeXml(IEXmlObjectSerializer.Serialize(transactionIDResponse)), sessionGUID, ieBranch.PK, wrapInSOAPEnvelope: false);
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				TransactionIDManager.New(logger, true).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				foreach (var transactionNumberBO in anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.PK, transactionNumbers.Select(x => x.PK))))
				{
					var transactionID = transactionNumberBO.TN_TransactionReference;
					Assert("transactionIDs should contain " + transactionID, transactionIDs.Remove(transactionID));
					AssertEquals(transactionID + " - transactionNumberBO.TN_IsUsed", ZBool.False, transactionNumberBO.TN_IsUsed);
				}
				AssertEquals("Should have matched all transactionIDs", 0, transactionIDs.Count);
				nonEmptyTransactionNumber = anotherFactory.Load<CusTransactionNumber>(nonEmptyTransactionNumber.PK);
				AssertEquals("nonEmptyTransactionNumber.TN_TransactionReference", existingTransactionID, nonEmptyTransactionNumber.TN_TransactionReference);
				incomingInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("incomingInterchange.EI_Status", EDIInterchange.Status.Received, incomingInterchange.EI_Status);
				AssertContains("Logs", "Processing Interchange (Type:TID, Number:1).\r\nInterchange '1' has been processed successfully.", logger.ToString());
			});
		}

		public void TestProcessTransactionRequestResponse()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			(GlbCompany auCompany, _) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Australia, "A1");
			InterchangeProcessorTestHelper.CreateValidCredential(auCompany);
			var transactionIDs = new List<ZString>();
			var sessionGUID = ZGuid.NewZGuid();
			var trackingReference = sessionGUID.ToString();
			var nonIECTransactionNumber = Factory.New<CusTransactionNumber>();
			nonIECTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			nonIECTransactionNumber.TN_GC_Company = ieCompany.PK;
			nonIECTransactionNumber.TN_TrackingReference = trackingReference;
			var nonEmptyTransactionNumber = Factory.New<CusTransactionNumber>();
			nonEmptyTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			nonEmptyTransactionNumber.TN_GC_Company = ieCompany.PK;
			nonEmptyTransactionNumber.TN_TrackingReference = trackingReference;
			var existingTransactionID = ZGuid.NewZGuid().ToString();
			nonEmptyTransactionNumber.TN_TransactionReference = existingTransactionID;
			var transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			var transactionNumbers = new List<CusTransactionNumber>();
			for (var i = 1; i < 51; i++)
			{
				var transactionID = ZGuid.NewZGuid().ToString();
				transactionIDs.Add(transactionID);
				transactionIDResponse.Transactions.Add(transactionID);
				var transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompany.PK;
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumbers.Add(transactionNumber);
			}
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID, ieBranch.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				TransactionIDManager.New(logger, false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				foreach (var transactionNumberBO in anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.PK, transactionNumbers.Select(x => x.PK))))
				{
					var transactionID = transactionNumberBO.TN_TransactionReference;
					Assert("transactionIDs should contain " + transactionID, transactionIDs.Remove(transactionID));
					AssertEquals(transactionID + " - transactionNumberBO.TN_IsUsed", ZBool.False, transactionNumberBO.TN_IsUsed);
				}
				AssertEquals("Should have matched all transactionIDs", 0, transactionIDs.Count);
				nonIECTransactionNumber = anotherFactory.Load<CusTransactionNumber>(nonIECTransactionNumber.PK);
				AssertEquals("nonIECTransactionNumber.TN_TransactionReference", ZString.Empty, nonIECTransactionNumber.TN_TransactionReference);
				nonEmptyTransactionNumber = anotherFactory.Load<CusTransactionNumber>(nonEmptyTransactionNumber.PK);
				AssertEquals("nonEmptyTransactionNumber.TN_TransactionReference", existingTransactionID, nonEmptyTransactionNumber.TN_TransactionReference);
				incomingInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("incomingInterchange.EI_Status", EDIInterchange.Status.Received, incomingInterchange.EI_Status);
				AssertContains("Logs", "Processing Interchange (Type:TID, Number:1).\r\nInterchange '1' has been processed successfully.", logger.ToString());
			});
		}

		public void TestProcessTransactionRequestResponseWithError()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			(GlbCompany auCompany, _) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Australia, "A1");
			InterchangeProcessorTestHelper.CreateValidCredential(auCompany);
			var transactionIDs = new List<ZString>();
			var sessionGUID = ZGuid.NewZGuid();
			var trackingReference = sessionGUID.ToString();
			var nonIECTransactionNumber = Factory.New<CusTransactionNumber>();
			nonIECTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			nonIECTransactionNumber.TN_GC_Company = ieCompany.PK;
			nonIECTransactionNumber.TN_TrackingReference = trackingReference;
			var nonEmptyTransactionNumber = Factory.New<CusTransactionNumber>();
			nonEmptyTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			nonEmptyTransactionNumber.TN_GC_Company = ieCompany.PK;
			nonEmptyTransactionNumber.TN_TrackingReference = trackingReference;
			var existingTransactionID = ZGuid.NewZGuid().ToString();
			nonEmptyTransactionNumber.TN_TransactionReference = existingTransactionID;
			for (var i = 1; i < 51; i++)
			{
				var transactionID = ZGuid.NewZGuid().ToString();
				transactionIDs.Add(transactionID);
				var transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompany.PK;
				transactionNumber.TN_TrackingReference = trackingReference;
			}
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, @"<?xml version=""1.0"" encoding=""UTF - 8""?><soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><soapenv:Body><soapenv:Fault><faultcode>soapenv:Server</faultcode><faultstring>111005</faultstring><detail><ns1:ROSFaultDetail xmlns:ns1=""http://www.ros.ie/schemas/service/""></ns1:ROSFaultDetail></detail></soapenv:Fault></soapenv:Body></soapenv:Envelope>", sessionGUID, ieBranch.PK, wrapInSOAPEnvelope: false);
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				TransactionIDManager.New(logger, false).Process(CancellationToken.None);
				var query = new ZQuery(CusTransactionNumberSchema.TN_TrackingReference, trackingReference)
					.AddToFilter(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, ieCompany.PK);
				var anotherFactory = new BusinessObjectFactory();
				var transactionNumbers = anotherFactory.Load<CusTransactionNumber>(query);
				AssertEquals("All existing blank records should have been deleted", 1, transactionNumbers.Length);
				AssertEquals("nonEmptyTransactionNumber.TN_TransactionReference", existingTransactionID, transactionNumbers[0].TN_TransactionReference);
				incomingInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("incomingInterchange.EI_Status", EDIInterchange.Status.Error, incomingInterchange.EI_Status);
				AssertContains("Logs", "Processing Interchange (Type:TID, Number:1).\r\nInterchange #1: Status set to 'ERR' due to the following error: EI_BodyText does not contain a valid SOAP Envelope Body or the Body section is empty.", logger.ToString());
			});
		}

		public void TestIgnoreEMCSWithEmptyCredential()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyWrapper = GlbCompanyWrapper.Get(ieCompany);
			var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;

			var message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message.EM_GB = ieBranch.PK;
			message.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			message.EM_MessageNum = "123456";
			message.EM_GP = ZGuid.Empty;
			Factory.Save();
			message.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				var manager = TransactionIDManagerForTest.New(new Logger(), true);
				manager.Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestNotifyWhenCredentialExpired()
		{
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var company1Wrapper = GlbCompanyWrapper.Get(ieCompany1);
			var credential1 = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany1);
			credential1.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-1);
			credential1.GP_MailBoxID = "MBID1231";
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T2");
			var company2Wrapper = GlbCompanyWrapper.Get(ieCompany2);
			var credential2 = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany2);
			credential2.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-2);
			credential2.GP_MailBoxID = "MBID1232";
			(GlbCompany ieCompany3, GlbBranch ieBranch3) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T3");
			var company3Wrapper = GlbCompanyWrapper.Get(ieCompany3);
			var credential3Invalid = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany3);
			credential3Invalid.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-2);
			credential3Invalid.GP_MailBoxID = "MBID1233";
			credential3Invalid.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			(GlbCompany ieCompany4, GlbBranch ieBranch4) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T4");
			var company4Wrapper = GlbCompanyWrapper.Get(ieCompany4);
			var credential4 = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany4);
			credential4.GP_MailBoxID = "MBID1234";
			(GlbCompany ieCompany5, GlbBranch ieBranch5) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T5");
			var company5Wrapper = GlbCompanyWrapper.Get(ieCompany5);
			var credential5NoGroup = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany5);
			credential5NoGroup.GP_MailBoxID = "MBID1235";
			credential5NoGroup.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-2);
			(GlbCompany ieCompany6, GlbBranch ieBranch6) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T6");
			var company6Wrapper = GlbCompanyWrapper.Get(ieCompany1);
			var credential6 = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany6);
			credential6.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-1);
			credential6.GP_MailBoxID = "MBID1236";

			var staff1 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST1", "Staff 1", "st1@email.address");
			var staff2 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST2", "Staff 2", "st2@email.address");
			var staff3 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST3", "Staff 3", "st3@email.address");
			var staff4 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST4", "Staff 4", "st4@email.address");
			var staff5 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST5", "Staff 5", "st5@email.address");
			var staff6 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST6", "Staff 6", "st6@email.address");
			var staff7 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST7", "Staff 7", "st7@email.address");
			var staff8 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST8", "Staff 8", "st8@email.address");
			var staff9 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST9", "Staff 9", ZString.Empty);
			var staff10 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "S10", "Staff 10", "st10@email.address");
			var staff11 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "S11", "Staff 11", "st11@email.address");

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "GP1";
			group1.GG_Desc = "Group 1";
			group1.Staff.Add(staff1);
			group1.Staff.Add(staff2);
			group1.Staff.Add(staff9);

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "GP2";
			group2.GG_Desc = "Group 2";
			group2.Staff.Add(staff3);
			group2.Staff.Add(staff4);

			var group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "GP3";
			group3.GG_Desc = "Group 3";
			group3.Staff.Add(staff5);
			group3.Staff.Add(staff6);

			var group4 = Factory.New<GlbGroup>();
			group4.GG_Code = "GP4";
			group4.GG_Desc = "Group 4";
			group4.Staff.Add(staff7);
			group4.Staff.Add(staff8);

			var group5 = Factory.New<GlbGroup>();
			group5.GG_Code = "GP5";
			group5.GG_Desc = "Group 5";
			group5.Staff.Add(staff10);
			group5.Staff.Add(staff11);

			var message1 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message1.EM_GB = ieBranch1.PK;
			message1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message1.EM_MessageNum = "1234561";
			message1.EM_GP = credential1.PK;
			var transactionNumber = Factory.New<CusTransactionNumber>();
			transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber.TN_GC_Company = ieCompany2.PK;
			transactionNumber.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber.TN_IsUsed = ZBool.True;
			var message3Invalid = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message3Invalid.EM_GB = ieBranch3.PK;
			message3Invalid.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message3Invalid.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message3Invalid.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message3Invalid.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message3Invalid.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message3Invalid.EM_MessageNum = "1234563";
			message3Invalid.EM_GP = credential3Invalid.PK;
			var message4 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message4.EM_GB = ieBranch4.PK;
			message4.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message4.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message4.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message4.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message4.EM_MessageNum = "1234564";
			message4.EM_GP = credential4.PK;
			var message5NoGroup = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message5NoGroup.EM_GB = ieBranch5.PK;
			message5NoGroup.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message5NoGroup.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message5NoGroup.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message5NoGroup.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message5NoGroup.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message5NoGroup.EM_MessageNum = "1234565";
			message5NoGroup.EM_GP = credential5NoGroup.PK;
			var message6 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message6.EM_GB = ieBranch6.PK;
			message6.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message6.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message6.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message6.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message6.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message6.EM_MessageNum = "1234566";
			message6.EM_GP = credential6.PK;
			Factory.Save();

			EUCustomsDataRegistry.Instance.SendExportMessageErrors.SetValue(ieCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, new ExportGroupNotification(Core.Constants.EmailTo.NoEmails, group1.PK));
			ExitControlCustomsDataRegistry.Instance.SendExitControlErrors.SetValue(ieCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, new ExitControlGroupNotification(Core.Constants.EmailTo.NoEmails, group2.PK));
			EUCustomsDataRegistry.Instance.SendNctsErrors.SetValue(ieCompany3.PK.ToGuid(), Guid.Empty, Guid.Empty, new NctsGroupNotification(Core.Constants.EmailTo.NoEmails, group3.PK));
			EUCustomsDataRegistry.Instance.SendExportMessageErrors.SetValue(ieCompany4.PK.ToGuid(), Guid.Empty, Guid.Empty, new ExportGroupNotification(Core.Constants.EmailTo.NoEmails, group4.PK));
			EUCustomsDataRegistry.Instance.SendNctsErrors.SetValue(ieCompany6.PK.ToGuid(), Guid.Empty, Guid.Empty, new NctsGroupNotification(Core.Constants.EmailTo.NoEmails, group5.PK));

			message1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message3Invalid.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message4.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message5NoGroup.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message6.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms);
				var now = ZDateTime.UtcNow;
				query.AddToFilter(CusTransactionNumberSchema.TN_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, now.AddMonths(-3));
				var count = Factory.GetDatabaseCount(typeof(CusTransactionNumber), query);
				var logger = new Logger();
				TransactionIDManager.New(logger, false).Process(CancellationToken.None);
				AssertEquals("Extra 100 seeded for ieCompany4", count + 100, Factory.GetDatabaseCount(typeof(CusTransactionNumber), query));
				query.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, ieCompany4.PK);
				query.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty);
				AssertEquals("100 add for ieCompany4", 100, Factory.GetDatabaseCount(typeof(CusTransactionNumber), query));
				message1.Reload();
				AssertEquals("message1.EM_Status - credential expired", EDIMessage.Status.Discarded, message1.EM_Status);
				credential1.Reload();
				AssertEquals("credential1.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential1.GP_PasswordStatus);
				credential2.Reload();
				AssertEquals("credential2.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential2.GP_PasswordStatus);
				message3Invalid.Reload();
				AssertEquals("message3Invalid.EM_Status - credential expired", EDIMessage.Status.Discarded, message3Invalid.EM_Status);
				credential3Invalid.Reload();
				AssertEquals("credential3Invalid.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential3Invalid.GP_PasswordStatus);
				message4.Reload();
				AssertEquals("message4.EM_Status - waiting for transaction id", EDIMessage.Status.Queued, message4.EM_Status);
				credential4.Reload();
				AssertEquals("credential4.GP_PasswordStatus", PasswordStatusList.Codes.Valid, credential4.GP_PasswordStatus);
				message5NoGroup.Reload();
				AssertEquals("message5NoGroup.EM_Status - credential expired", EDIMessage.Status.Discarded, message5NoGroup.EM_Status);
				credential5NoGroup.Reload();
				AssertEquals("credential5NoGroup.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential5NoGroup.GP_PasswordStatus);
				message6.Reload();
				AssertEquals("message6.EM_Status - credential expired", EDIMessage.Status.Discarded, message6.EM_Status);
				credential6.Reload();
				AssertEquals("credential6.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential6.GP_PasswordStatus);
				AssertEquals(3, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var messageFormat = "The credential for " + credential1.GP_MailBoxIDInfo.Description + " '{0}' expired on {1}.";
				var message = string.Format(messageFormat, credential1.GP_MailBoxID, credential1.GP_ExpiryDate.ToLongTimeString());
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Body.Contains(message));
				AssertEquals("FromDisplayName - Company 1", "TEST T1 COMP", email.FromDisplayName);
				AssertContainsExactElementsInAnyOrder("AES - group 1", new string[] { staff1.GS_EmailAddress, staff2.GS_EmailAddress }, email.Recipients.Cast<RecipientDef>().Select(x => x.Email));
				message = string.Format(messageFormat, credential2.GP_MailBoxID, credential2.GP_ExpiryDate.ToLongTimeString());
				email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Body.Contains(message));
				AssertEquals("FromDisplayName - Company 2", "TEST T2 COMP", email.FromDisplayName);
				AssertContainsExactElementsInAnyOrder("ExitControl - group 2", new string[] { staff3.GS_EmailAddress, staff4.GS_EmailAddress }, email.Recipients.Cast<RecipientDef>().Select(x => x.Email));
				message = string.Format(messageFormat, credential6.GP_MailBoxID, credential6.GP_ExpiryDate.ToLongTimeString());
				email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Body.Contains(message));
				AssertEquals("FromDisplayName - Company 6", "TEST T6 COMP", email.FromDisplayName);
				AssertContainsExactElementsInAnyOrder("NCTS - group 5", new string[] { staff10.GS_EmailAddress, staff11.GS_EmailAddress }, email.Recipients.Cast<RecipientDef>().Select(x => x.Email));
				var logs = logger.ToString();
				AssertContains("Logs", $"The Revenue Online Service Credential for Message Sender EORI 'MBID1231' expired on {credential1.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"The Revenue Online Service Credential for Message Sender EORI 'MBID1235' expired on {credential5NoGroup.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"The Revenue Online Service Credential for Message Sender EORI 'MBID1236' expired on {credential6.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"The Revenue Online Service Credential for Message Sender EORI 'MBID1232' expired on {credential2.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"TEST T4 COMP: 1 TID message(s) created\r\nTEST T4 COMP: Found 1 message(s) waiting for Transaction ID allocation", logs);
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestNotifyWhenCredentialExpired_EMCS()
		{
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var company1Wrapper = GlbCompanyWrapper.Get(ieCompany1);
			var emcsCredential1 = company1Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential1.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential1.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			emcsCredential1.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-1);
			emcsCredential1.GP_MailBoxID = "MBID1231";
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T2");
			var company2Wrapper = GlbCompanyWrapper.Get(ieCompany2);
			var emcsCredential2 = company2Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential2.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential2.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			emcsCredential2.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-2);
			emcsCredential2.GP_MailBoxID = "MBID1232";
			(GlbCompany ieCompany3, GlbBranch ieBranch3) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T3");
			var company3Wrapper = GlbCompanyWrapper.Get(ieCompany3);
			var emcsCredential3Invalid = company3Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential3Invalid.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential3Invalid.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			emcsCredential3Invalid.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-2);
			emcsCredential3Invalid.GP_MailBoxID = "MBID1233";
			emcsCredential3Invalid.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			(GlbCompany ieCompany4, GlbBranch ieBranch4) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T4");
			var company4Wrapper = GlbCompanyWrapper.Get(ieCompany4);
			var emcsCredential4 = company4Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential4.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential4.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			emcsCredential4.GP_MailBoxID = "MBID1234";
			(GlbCompany ieCompany5, GlbBranch ieBranch5) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T5");
			var company5Wrapper = GlbCompanyWrapper.Get(ieCompany5);
			var emcsCredential5NoGroup = company5Wrapper.EMCSGlbExternalPasswordCollection.AddNew();
			emcsCredential5NoGroup.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
			emcsCredential5NoGroup.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
			emcsCredential5NoGroup.GP_MailBoxID = "MBID1235";
			emcsCredential5NoGroup.GP_ExpiryDate = ZDateTime.UtcNow.AddMonths(-2).AddMinutes(1);

			var staff1 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST1", "Staff 1", "st1@email.address");
			var staff2 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST2", "Staff 2", "st2@email.address");
			var staff3 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST3", "Staff 3", "st3@email.address");
			var staff4 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST4", "Staff 4", "st4@email.address");
			var staff5 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST5", "Staff 5", "st5@email.address");
			var staff6 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST6", "Staff 6", "st6@email.address");
			var staff7 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST7", "Staff 7", "st7@email.address");
			var staff8 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST8", "Staff 8", "st8@email.address");
			var staff9 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "ST9", "Staff 9", ZString.Empty);

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "GP1";
			group1.GG_Desc = "Group 1";
			group1.Staff.Add(staff1);
			group1.Staff.Add(staff2);
			group1.Staff.Add(staff9);

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "GP2";
			group2.GG_Desc = "Group 2";
			group2.Staff.Add(staff3);
			group2.Staff.Add(staff4);

			var group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "GP3";
			group3.GG_Desc = "Group 3";
			group3.Staff.Add(staff5);
			group3.Staff.Add(staff6);

			var group4 = Factory.New<GlbGroup>();
			group4.GG_Code = "GP4";
			group4.GG_Desc = "Group 4";
			group4.Staff.Add(staff7);
			group4.Staff.Add(staff8);

			var message1 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message1.EM_GB = ieBranch1.PK;
			message1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			message1.EM_MessageNum = "1234561";
			message1.EM_GP = emcsCredential1.PK;
			var transactionNumber = Factory.New<CusTransactionNumber>();
			transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber.TN_GC_Company = ieCompany2.PK;
			transactionNumber.TN_GP_ExternalPassword = emcsCredential2.PK;
			transactionNumber.TN_TrackingReference = ZGuid.NewZGuid().ToString();
			transactionNumber.TN_IsUsed = ZBool.True;
			var message3Invalid = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message3Invalid.EM_GB = ieBranch3.PK;
			message3Invalid.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message3Invalid.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message3Invalid.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message3Invalid.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message3Invalid.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			message3Invalid.EM_MessageNum = "1234563";
			message3Invalid.EM_GP = emcsCredential3Invalid.PK;
			var message4 = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message4.EM_GB = ieBranch4.PK;
			message4.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message4.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message4.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message4.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			message4.EM_MessageNum = "1234564";
			message4.EM_GP = emcsCredential4.PK;
			var message5NoGroup = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message5NoGroup.EM_GB = ieBranch5.PK;
			message5NoGroup.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			message5NoGroup.EM_MessageText = "<Greeting>HELLO</Greeting>";
			message5NoGroup.EM_Status = AESOutboundEDIMessage.Status.Queued;
			message5NoGroup.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive;
			message5NoGroup.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			message5NoGroup.EM_MessageNum = "1234565";
			message5NoGroup.EM_GP = emcsCredential5NoGroup.PK;
			Factory.Save();

			EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors.SetValue(ieCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, new EmcsGroupNotification(Core.Constants.EmailTo.NoEmails, group1.PK));
			EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors.SetValue(ieCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, new EmcsGroupNotification(Core.Constants.EmailTo.NoEmails, group2.PK));
			EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors.SetValue(ieCompany3.PK.ToGuid(), Guid.Empty, Guid.Empty, new EmcsGroupNotification(Core.Constants.EmailTo.NoEmails, group3.PK));
			EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors.SetValue(ieCompany4.PK.ToGuid(), Guid.Empty, Guid.Empty, new EmcsGroupNotification(Core.Constants.EmailTo.NoEmails, group4.PK));

			message1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message3Invalid.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message4.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			message5NoGroup.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustomsEMCS);
				var now = ZDateTime.UtcNow;
				query.AddToFilter(CusTransactionNumberSchema.TN_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, now.AddMonths(-3));
				var count = Factory.GetDatabaseCount(typeof(CusTransactionNumber), query);
				var logger = new Logger();
				TransactionIDManager.New(logger, true).Process(CancellationToken.None);
				AssertEquals("Extra 100 seeded for ieCompany4", count + 100, Factory.GetDatabaseCount(typeof(CusTransactionNumber), query));
				query.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, ieCompany4.PK);
				query.AddToFilter(CusTransactionNumberSchema.TN_GP_ExternalPassword, emcsCredential4.PK);
				query.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty);
				AssertEquals("100 add for ieCompany4", 100, Factory.GetDatabaseCount(typeof(CusTransactionNumber), query));
				message1.Reload();
				AssertEquals("message1.EM_Status - credential expired", EDIMessage.Status.Discarded, message1.EM_Status);
				emcsCredential1.Reload();
				AssertEquals("emcsCredential1.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, emcsCredential1.GP_PasswordStatus);
				emcsCredential2.Reload();
				AssertEquals("emcsCredential2.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, emcsCredential2.GP_PasswordStatus);
				message3Invalid.Reload();
				AssertEquals("message3.EM_Status - credential expired", EDIMessage.Status.Discarded, message3Invalid.EM_Status);
				emcsCredential3Invalid.Reload();
				AssertEquals("emcsCredential3.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, emcsCredential3Invalid.GP_PasswordStatus);
				message4.Reload();
				AssertEquals("message4.EM_Status - waiting for transaction id", EDIMessage.Status.Queued, message4.EM_Status);
				emcsCredential4.Reload();
				AssertEquals("emcsCredential4.GP_PasswordStatus", PasswordStatusList.Codes.Valid, emcsCredential4.GP_PasswordStatus);
				message5NoGroup.Reload();
				AssertEquals("message5.EM_Status - credential expired", EDIMessage.Status.Discarded, message5NoGroup.EM_Status);
				emcsCredential5NoGroup.Reload();
				AssertEquals("emcsCredential5.GP_PasswordStatus", PasswordStatusList.Codes.Invalid, emcsCredential5NoGroup.GP_PasswordStatus);
				AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var messageFormat = "The credential for " + emcsCredential1.GP_MailBoxIDInfo.Description + " '{0}' expired on {1}.";
				var message = string.Format(messageFormat, emcsCredential1.GP_MailBoxID, emcsCredential1.GP_ExpiryDate.ToLongTimeString());
				var email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Body.Contains(message));
				AssertContainsExactElementsInAnyOrder("group 1", new string[] { staff1.GS_EmailAddress, staff2.GS_EmailAddress }, email1.Recipients.Cast<RecipientDef>().Select(x => x.Email));
				message = string.Format(messageFormat, emcsCredential2.GP_MailBoxID, emcsCredential2.GP_ExpiryDate.ToLongTimeString());
				email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Body.Contains(message));
				AssertContainsExactElementsInAnyOrder("group 2", new string[] { staff3.GS_EmailAddress, staff4.GS_EmailAddress }, email1.Recipients.Cast<RecipientDef>().Select(x => x.Email));
				var logs = logger.ToString();
				AssertContains("Logs", $"The Revenue Online Service Credential for Certificate Identifier 'MBID1235' expired on {emcsCredential5NoGroup.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"The Revenue Online Service Credential for Certificate Identifier 'MBID1231' expired on {emcsCredential1.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"The Revenue Online Service Credential for Certificate Identifier 'MBID1232' expired on {emcsCredential2.GP_ExpiryDate.ToLongTimeString()}.\r\n", logs);
				AssertContains("Logs", $"TEST T4 COMP: 1 TID message(s) created\r\nTEST T4 COMP: Found 1 message(s) waiting for Transaction ID allocation", logs);
			});
		}

		class TransactionIDManagerForTest : TransactionIDManager
		{
			public static new TransactionIDManagerForTest New(ILogger logger, bool useEMCS)
			{
				return useEMCS ?
					new TransactionIDManagerForTest(logger, CusTransactionNumberTypeList.Codes.IECustomsEMCS, new[] { EDIMessage.ApplicationCodes.IECustomsEMCS }, EDIMessage.ApplicationCodes.IECustomsEMCS, true) :
					new TransactionIDManagerForTest(logger, CusTransactionNumberTypeList.Codes.IECustoms, new[] { EDIMessage.ApplicationCodes.IECustomsExport, EDIMessage.ApplicationCodes.IECustomsImport, EDIMessage.ApplicationCodes.IECustomsUCC5Import }, EDIMessage.ApplicationCodes.IECustomsCommon, false);
			}

			TransactionIDManagerForTest(ILogger logger, string transactionType, string[] messageApplicationCodes, string transactionIDApplicationCode, bool filterOnCredential)
				: base(logger, transactionType, messageApplicationCodes, transactionIDApplicationCode, filterOnCredential, Array.Empty<IRegistryItem>())
			{
				factory.Saved += (objectFactory, successfully) =>
				{
					FactoryAddtionSavedAction?.Invoke(objectFactory, successfully);
				};
			}

			public Action<BusinessObjectFactory, bool> FactoryAddtionSavedAction;
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestAllocateCorrectCompanyTransactionNumberIsUsed()
		{
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany1);
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T2");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany2);
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranch2.PK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = ieCompany1.PK;
			transactionNumber1.TN_TransactionReference = transactionID1;
			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = ieCompany2.PK;
			transactionNumber2.TN_TransactionReference = transactionID2;
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				AssertEDIMessage("outgoingMessage1", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage1.PK), AESOutboundEDIMessage.Status.Pending, transactionID2);
			});
		}

		const string transactionID1 = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
		const string transactionID2 = "C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
		const string transactionID3 = "B02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
		const string transactionID4 = "D02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";

		[StressTest]
		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestReplenishUsingAverageDailyUsage()
		{
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var companyPK = ieCompany.PK;
			for (var i = 3; i > 0; i--)
			{
				var utcNow = ZDateTime.UtcNow.AddDays(-i);
				var trackingReference = ZGuid.NewZGuid().ToString();
				for (var j = 540; j > 0; j--)
				{
					var transactionNumber = Factory.New<CusTransactionNumber>();
					transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
					transactionNumber.TN_GC_Company = companyPK;
					transactionNumber.TN_TransactionReference = ZGuid.NewZGuid().ToString();
					transactionNumber.TN_TrackingReference = trackingReference;
					transactionNumber.TN_IsUsed = j > 40;
					transactionNumber.TN_SystemCreateTimeUtc = utcNow.AddSeconds(-j);
				}
			}
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			TestHelper.SetupTransactionIDNoOfDays(Factory, 4);
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranch.PK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals("interchanges.Length", 3, interchanges.Length);
				AssertEDIInterchangeTransactionIDRequest(interchanges[0], ieBranch.PK, companyCredential.PK, url);
				AssertEDIInterchangeTransactionIDRequest(interchanges[1], ieBranch.PK, companyCredential.PK, url);
				AssertEDIInterchangeTransactionIDRequest(interchanges[2], ieBranch.PK, companyCredential.PK, url);
				var blankTransactionNumbers = anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty));
				AssertEquals("blankTransactionNumbers.Length", 300, blankTransactionNumbers.Length);
				var data = blankTransactionNumbers.Where(x => x.TN_GC_Company == companyPK).GroupBy(x => x.TN_TrackingReference).ToDictionary(x => x.Key, y => y.ToArray());
				AssertEquals("data.Count", 3, data.Count);
				AssertEquals("No of CusTransactionNumber linked to interchanges[0]", 100, data[interchanges[0].EI_SessionGUID.ToString()].Length);
				AssertEquals("No of CusTransactionNumber linked to interchanges[1]", 100, data[interchanges[1].EI_SessionGUID.ToString()].Length);
				AssertEquals("No of CusTransactionNumber linked to interchanges[2]", 100, data[interchanges[2].EI_SessionGUID.ToString()].Length);
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestShouldNotReplenishIfAvailableIsMoreThanDaily()
		{
			TestHelper.SetupTransactionIDNoOfDays(Factory, 2);
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var utcNow = ZDateTime.UtcNow.AddDays(-1);
			var ieCompanyPK = ieCompany.PK;
			var ieBranchPK = ieBranch.PK;
			var trackingReference = ZString.Empty;
			for (var j = 500; j > 0; j--)
			{
				if (j % 100 == 0)
				{
					trackingReference = ZGuid.NewZGuid().ToString();
				}
				var transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompanyPK;
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumber.TN_TransactionReference = ZGuid.NewZGuid().ToString();
				transactionNumber.TN_IsUsed = j > 200;
				transactionNumber.TN_SystemCreateTimeUtc = utcNow.AddSeconds(-j);
			}
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranchPK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals("No new transaction request as there is still 200 ids", 0, interchanges.Length);
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestReplenishShouldConsiderPendingRequest()
		{
			TestHelper.SetupTransactionIDNoOfDays(Factory, 2);
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var utcNow = ZDateTime.UtcNow.AddDays(-1);
			var ieCompanyPK = ieCompany.PK;
			var ieBranchPK = ieBranch.PK;
			var trackingReference = ZString.Empty;
			CusTransactionNumber transactionNumber = null;
			for (var j = 600; j > 0; j--)
			{
				if (j % 100 == 0)
				{
					trackingReference = ZGuid.NewZGuid().ToString();
				}
				transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompanyPK;
				transactionNumber.TN_TransactionReference = ZGuid.NewZGuid().ToString();
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumber.TN_IsUsed = ZBool.True;
				transactionNumber.TN_SystemCreateTimeUtc = utcNow.AddSeconds(-j);
			}
			transactionNumber.TN_IsUsed = ZBool.False; // need for outgoingMessage1
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var createTime = ZDateTime.UtcNow.AddHours(-2);
			var interchange1 = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange1.EI_Status = EDIInterchange.Status.Sent;
			interchange1.EI_SystemCreateTimeUtc = createTime.AddMinutes(-2);
			var interchange1Response = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange1Response.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1Response.EI_Status = EDIInterchange.Status.Received;
			interchange1Response.EI_SystemCreateTimeUtc = interchange1.EI_SystemCreateTimeUtc.AddSeconds(10);
			interchange1Response.EI_SessionGUID = interchange1.EI_SessionGUID;
			var interchange2 = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange2.EI_Status = EDIInterchange.Status.Sent;
			interchange2.EI_SystemCreateTimeUtc = createTime.AddMinutes(-1);
			var interchange3 = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange3.EI_SystemCreateTimeUtc = createTime;
			var trackingReferences = new List<ZString>();
			foreach (var interchange in new[] { interchange1, interchange2, interchange3 })
			{
				trackingReference = interchange.EI_SessionGUID.ToString();
				trackingReferences.Add(trackingReference);
				for (var i = 100; i > 0; i--)
				{
					transactionNumber = Factory.New<CusTransactionNumber>();
					transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
					transactionNumber.TN_GC_Company = ieCompanyPK;
					transactionNumber.TN_TrackingReference = trackingReference;
				}
			}
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranchPK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				utcNow = ZDateTime.UtcNow;
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT)
					.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow)
					.AddToFilter(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, new[] { interchange1.PK, interchange2.PK, interchange3.PK }));
				AssertEquals("interchanges.Length", 1, interchanges.Length);
				AssertEDIInterchangeTransactionIDRequest(interchanges[0], ieBranchPK, companyCredential.PK, url);
				var blankTransactionNumbers = anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty)
					.AddToFilter(CusTransactionNumberSchema.TN_TrackingReference, SQLComparisonOperator.NotEqual, trackingReferences.ToArray()));
				AssertEquals("blankTransactionNumbers.Length", 100, blankTransactionNumbers.Length);
				trackingReference = interchanges[0].EI_SessionGUID.ToString();
				AssertEquals("All blankTransactionNumbers", true, blankTransactionNumbers.All(x => x.TN_TrackingReference == trackingReference && x.TN_GC_Company == ieCompanyPK));
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestDoNotReplenishIfThereIsOutgoingAndNewTID()
		{
			TestHelper.SetupTransactionIDNoOfDays(Factory, 2);
			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var systemCreateTime = ZDateTime.UtcNow.AddMinutes(-30);
			var ieCompanyPK = ieCompany.PK;
			var ieBranchPK = ieBranch.PK;
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranchPK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			outgoingMessage1.EM_SystemCreateTimeUtc = systemCreateTime;
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			var interchange1 = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange1.EI_SystemCreateTimeUtc = systemCreateTime.AddMinutes(1);
			interchange1.EI_Status = EDIInterchange.Status.Sent;
			Factory.Save();
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
			query.AddToFilter(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, interchange1.PK);

			CombineAssertions("No Response", () =>
			{
				var utcNow = ZDateTime.UtcNow;
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});

			var interchange1Response = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange1Response.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1Response.EI_SystemCreateTimeUtc = interchange1.EI_SystemCreateTimeUtc.AddSeconds(10);
			interchange1Response.EI_SessionGUID = interchange1.EI_SessionGUID;
			interchange1Response.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			CombineAssertions("Response - Queued", () =>
			{
				var utcNow = ZDateTime.UtcNow;
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});

			interchange1Response.EI_Status = EDIInterchange.Status.Error;
			Factory.Save();
			CombineAssertions("Response - Error", () =>
			{
				var utcNow = ZDateTime.UtcNow;
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});

			interchange1Response.EI_Status = EDIInterchange.Status.Received;
			Factory.Save();
			CombineAssertions("Response - Received", () =>
			{
				var utcNow = ZDateTime.UtcNow;
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 1, interchanges.Length);
				interchanges[0].Delete();
				anotherFactory.Save();
			});

			interchange1.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			CombineAssertions("Outgoing - Queued", () =>
			{
				var utcNow = ZDateTime.UtcNow;
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestDoNotReplenishIfThereIsNoOutgoingButNewTID()
		{
			TestHelper.SetupTransactionIDNoOfDays(Factory, 2);

			(GlbCompany ieCompany, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var systemCreateTime = ZDateTime.UtcToday.AddDays(-1).AddMinutes(-30);
			var ieCompanyPK = ieCompany.PK;
			var ieBranchPK = ieBranch.PK;
			var trackingReference = ZGuid.NewZGuid().ToString();
			CusTransactionNumber transactionNumber = null;
			for (var j = 202; j > 0; j--)
			{
				transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompanyPK;
				transactionNumber.TN_TransactionReference = ZGuid.NewZGuid().ToString();
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumber.TN_IsUsed = ZBool.True;
				transactionNumber.TN_SystemCreateTimeUtc = systemCreateTime.AddSeconds(-j);
			}
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranchPK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Sent;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			outgoingMessage1.EM_SystemCreateTimeUtc = systemCreateTime;
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			var interchange1 = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			systemCreateTime = ZDateTime.UtcNow.AddHours(-1);
			interchange1.EI_SystemCreateTimeUtc = systemCreateTime.AddMinutes(10);
			interchange1.EI_Status = EDIInterchange.Status.Sent;
			trackingReference = interchange1.EI_SessionGUID.ToString();
			for (var j = 100; j > 0; j--)
			{
				transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompanyPK;
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumber.TN_SystemCreateTimeUtc = interchange1.EI_SystemCreateTimeUtc;
			}
			Factory.Save();
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
			query.AddToFilter(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, interchange1.PK);

			CombineAssertions("No Response", () =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, systemCreateTime);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});

			var interchange1Response = TestHelper.CreateTransactionIDRequest(Factory, ieBranchPK);
			interchange1Response.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1Response.EI_SystemCreateTimeUtc = interchange1.EI_SystemCreateTimeUtc.AddSeconds(10);
			interchange1Response.EI_SessionGUID = interchange1.EI_SessionGUID;
			interchange1Response.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			CombineAssertions("Response - Queued", () =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, systemCreateTime);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});

			interchange1Response.EI_Status = EDIInterchange.Status.Error;
			Factory.Save();
			CombineAssertions("Response - Error", () =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, systemCreateTime);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});

			interchange1Response.EI_Status = EDIInterchange.Status.Received;
			Factory.Save();
			CombineAssertions("Response - Received", () =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, systemCreateTime);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 2, interchanges.Length);
				var blankTransactionNumbers = anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty)
					.AddToFilter(CusTransactionNumberSchema.TN_TrackingReference, interchanges.Select(x => x.EI_SessionGUID.ToString())));
				AssertEquals("blankTransactionNumbers.Length", 200, blankTransactionNumbers.Length);
				interchanges.DeleteAll();
				blankTransactionNumbers.DeleteAll();
				anotherFactory.Save();
			});

			interchange1.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();
			CombineAssertions("Outgoing - Queued", () =>
			{
				TransactionIDManager.New(new Logger(), false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				var currentQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, systemCreateTime);
				currentQuery.AddToFilter(query);
				var interchanges = anotherFactory.Load<EDIInterchange>(currentQuery);
				AssertEquals("interchanges.Length", 0, interchanges.Length);
			});
		}

		public void TestAllocate_NCTS()
		{
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var companyPK = company.PK;
			var branchPK = branch.PK;
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = companyPK;
			transactionNumber1.TN_TransactionReference = transactionID1;

			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
			outgoingMessage.EM_GB = branchPK;
			outgoingMessage.EM_GP = companyCredential.PK;
			outgoingMessage.EM_MessageType = "015";
			outgoingMessage.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage.EM_Status = EDIMessage.Status.Queued;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage.EM_MessageNum = "TEST1234567890";
			Factory.Save();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				TransactionIDManager.New(logger, useEMCS: false).Process(CancellationToken.None);
				AssertEDIMessage("outgoingMessage1", NewFactory().Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage.PK), EDIMessage.Status.Pending, transactionID1);
				AssertContains("Logs", "TEST T1 COMP: Found 1 message(s) waiting for Transaction ID allocation\r\nAllocating Transaction ID 'A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567890", logger.ToString());
			});
		}

		[TestDate(2023, 5, 30, 12, 00, 00)]
		public void TestReplenishAndAllocate()
		{
			TestHelper.SetupTransactionIDNoOfDays(Factory, 2);
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var ieCompany1Credential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany1);
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T2");
			InterchangeProcessorTestHelper.CreateValidCredential(ieCompany2);
			var utcNow = ZDateTime.UtcNow.AddDays(-1);
			var ieCompany1PK = ieCompany1.PK;
			var ieBranch1PK = ieBranch1.PK;
			var trackingReference = ZString.Empty;
			for (var j = 300; j > 0; j--)
			{
				if (j == 300 || j == 200 || j == 100)
				{
					trackingReference = ZGuid.NewZGuid().ToString();
				}
				var transactionNumber = Factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
				transactionNumber.TN_GC_Company = ieCompany1PK;
				transactionNumber.TN_TransactionReference = ZGuid.NewZGuid().ToString();
				transactionNumber.TN_TrackingReference = trackingReference;
				transactionNumber.TN_IsUsed = ZBool.True;
				transactionNumber.TN_SystemCreateTimeUtc = utcNow.AddSeconds(-j);
			}
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupTransactionIDURL();
			var outgoingMessage1 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage1.EM_GB = ieBranch1PK;
			outgoingMessage1.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage1.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage1.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage1.EM_MessageNum = "TEST1234567890";
			var outgoingMessage2 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage2.EM_GB = ieBranch1PK;
			outgoingMessage2.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage2.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage2.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage2.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage2.EM_MessageNum = "TEST1234567891";
			var outgoingMessage3 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage3.EM_GB = ieBranch1PK;
			outgoingMessage3.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage3.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage3.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage3.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage3.EM_MessageNum = "TEST1234567892";
			var outgoingMessage4 = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage4.EM_GB = ieBranch2.PK;
			outgoingMessage4.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage4.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage4.EM_Status = AESOutboundEDIMessage.Status.Queued;
			outgoingMessage4.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage4.EM_MessageNum = "TEST1234567892";
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = ieCompany1.PK;
			transactionNumber1.TN_TransactionReference = transactionID1;
			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = ieCompany1.PK;
			transactionNumber2.TN_TransactionReference = transactionID2;
			var transactionNumber3 = Factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber3.TN_GC_Company = ieCompany2.PK;
			transactionNumber3.TN_TransactionReference = transactionID3;
			var transactionNumber4 = Factory.New<CusTransactionNumber>();
			transactionNumber4.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber4.TN_GC_Company = ieCompany2.PK;
			transactionNumber4.TN_TransactionReference = transactionID4;
			Factory.Save();
			outgoingMessage1.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			outgoingMessage2.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			outgoingMessage3.EM_ReceiveTransmit = AESOutboundEDIMessage.Direction.Transmit;
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				TransactionIDManager.New(logger, useEMCS: false).Process(CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				AssertEDIMessage("outgoingMessage1", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage1.PK), AESOutboundEDIMessage.Status.Pending, transactionID1);
				AssertEDIMessage("outgoingMessage2", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage2.PK), AESOutboundEDIMessage.Status.Pending, transactionID2);
				AssertEDIMessage("outgoingMessage3", (AESOutboundEDIMessage)anotherFactory.Load<Enterprise.Messaging.Business.EDIMessage>(outgoingMessage3.PK), AESOutboundEDIMessage.Status.Queued, ZString.Empty);
				var interchanges = anotherFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon)
					.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued)
					.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals("interchanges.Length", 2, interchanges.Length);
				AssertEDIInterchangeTransactionIDRequest(interchanges[0], ieBranch1PK, ieCompany1Credential.PK, url);
				AssertEDIInterchangeTransactionIDRequest(interchanges[1], ieBranch1PK, ieCompany1Credential.PK, url);
				var blankTransactionNumbers = anotherFactory.Load<CusTransactionNumber>(new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.IECustoms)
					.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False)
					.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty));
				AssertEquals("blankTransactionNumbers.Length", 200, blankTransactionNumbers.Length);
				var data = blankTransactionNumbers.Where(x => x.TN_GC_Company == ieCompany1PK).GroupBy(x => x.TN_TrackingReference).ToDictionary(x => x.Key, y => y.ToArray());
				AssertEquals("data.Count", 2, data.Count);
				AssertEquals("No of CusTransactionNumber linked to interchanges[0]", 100, data[interchanges[0].EI_SessionGUID.ToString()].Length);
				AssertEquals("No of CusTransactionNumber linked to interchanges[1]", 100, data[interchanges[1].EI_SessionGUID.ToString()].Length);
				AssertEquals("transactionNumber1.TN_IsUsed", ZBool.True, anotherFactory.Load<CusTransactionNumber>(transactionNumber1.PK).TN_IsUsed);
				AssertEquals("transactionNumber2.TN_IsUsed", ZBool.True, anotherFactory.Load<CusTransactionNumber>(transactionNumber2.PK).TN_IsUsed);
				AssertEquals("transactionNumber3.TN_IsUsed", ZBool.False, anotherFactory.Load<CusTransactionNumber>(transactionNumber4.PK).TN_IsUsed);
				AssertContains("Logs", "TEST T1 COMP: 2 TID message(s) created\r\nTEST T1 COMP: Found 3 message(s) waiting for Transaction ID allocation\r\nAllocating Transaction ID 'A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567890\r\nAllocating Transaction ID 'C02A8604-2D0B-4FEE-8EDF-61DB6AFC7639' to Message #TEST1234567891", logger.ToString());
			});
		}

		void AssertEDIInterchangeTransactionIDRequest(EDIInterchange interchange, ZGuid branchPK, ZGuid credentialPK, string url)
		{
			AssertEquals("interchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", interchange.EI_HeaderText);
			AssertXmlElementValue(
				interchange.EI_BodyText,
				@"http://www.ros.ie/schemas/customs/transactionidrequest/v1",
				["Transactions", "NumberOfTxIds"],
				"100"
			);
			AssertEquals("interchange.EI_GB", branchPK, interchange.EI_GB);
			AssertEquals("interchange.EI_GP", credentialPK, interchange.EI_GP);
		}

		void AssertEDIMessage(string reference, Enterprise.Messaging.Business.EDIMessage message, string expectedStatus, string expectedApplicationReference)
		{
			AssertEquals(reference + ".EM_Status", expectedStatus, message.EM_Status);
			AssertEquals(reference + ".EM_ApplicationReference", expectedApplicationReference, message.EM_ApplicationReference);
		}

		static void AssertXmlElementValue(string xml, string namespaceUri, string[] elementPath, string expectedValue)
		{
			var doc = XDocument.Parse(xml);
			XNamespace ns = namespaceUri;

			XElement current = doc.Root;
			foreach (var name in elementPath)
			{
				current = current?.Element(ns + name);
				if (current == null)
				{
					throw new Exception($"Missing expected element: {string.Join("/", elementPath)}");
				}
			}

			if (!string.Equals(current.Value?.Trim(), expectedValue, StringComparison.Ordinal))
			{
				throw new Exception($"Expected value '{expectedValue}' but found '{current.Value}' at path: {string.Join("/", elementPath)}");
			}
		}
	}
}
