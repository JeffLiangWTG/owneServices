using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.TransactionIDResponse;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class TransactionIDRequestProcessorTest
		: TestCaseWithFactory
	{
		public void TestHandlingError()
		{
			(var ieCompany, var ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(ieCompany);
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, "<Data>invalid data</Data>", branchPK: ieBranch.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				var errorCompanyPK = ZGuid.Empty;
				var errorBranchPK = ZGuid.Empty;
				var errorSubject = string.Empty;
				var errorBody = string.Empty;
				var sendErrorNotification = new TransactionIDManager.SendErrorNotificationDelegate((ZGuid companyPK, ZGuid branchPK, string subject, string body) =>
				{
					errorCompanyPK = companyPK;
					errorBranchPK = branchPK;
					errorSubject = subject;
					errorBody = body;
				});
				var processor = new TransactionIDRequestProcessor(sendErrorNotification, 3, CusTransactionNumberTypeList.Codes.IECustoms, EDIMessage.ApplicationCodes.IECustomsCommon);
				processor.ExecuteBatch(System.Threading.CancellationToken.None);
				var logger = processor.Logger;
				var anotherFactory = new BusinessObjectFactory();
				incomingInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("incomingInterchange.EI_Status", EDIInterchange.Status.Error, incomingInterchange.EI_Status);
				var log = incomingInterchange.Logs.GetAllLogs()[0];
				AssertEquals("SL_SE_NKEvent", Events.ErrorReport.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "Error Processing Transaction ID Request Response.\r\nThere is an error in XML document (1, 2).", log.SL_Reference);
				AssertEquals("errorCompanyPK", ieCompany.PK, errorCompanyPK);
				AssertEquals("errorBranchPK", ieBranch.PK, errorBranchPK);
				AssertEquals("errorSubject", $"TID Interchange #{incomingInterchange.EI_InterchangeNum} Set To 'ERR'", errorSubject);
				AssertEquals("errorBody", "The following error occurred: Error Processing Transaction ID Request Response.\r\nThere is an error in XML document (1, 2).", errorBody);
				AssertContains("Error Log", "Interchange #1: Status set to 'ERR' due to the following error: Error Processing Transaction ID Request Response.\r\nThere is an error in XML document (1, 2).", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestOnlyInterchangeWithMatchingApplicationCodeAndInterchangeTypeIsProcessed()
		{
			(_, GlbBranch ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var transactionID = ZGuid.NewZGuid().ToString();
			var transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID);
			var xmlData = IEXmlObjectSerializer.Serialize(transactionIDResponse);
			var incomingInterchangeExport = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsExport, CommonInterchangeTypeList.Codes.TransactionID, xmlData, branchPK: ieBranch.PK);
			var incomingInterchangeImport = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsImport, CommonInterchangeTypeList.Codes.TransactionID, xmlData, branchPK: ieBranch.PK);
			var incomingInterchangeCommonNonTransactionID = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, xmlData, branchPK: ieBranch.PK);

			Factory.Save();
			CombineAssertions(() =>
			{
				new TransactionIDRequestProcessor(null, 3, CusTransactionNumberTypeList.Codes.IECustoms, EDIMessage.ApplicationCodes.IECustomsCommon).ExecuteBatch(System.Threading.CancellationToken.None);
				var anotherFactory = new BusinessObjectFactory();
				void AssertNotProcessed(EDIInterchange interchange)
				{
					interchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
					AssertEquals("interchange.EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				}
				AssertNotProcessed(incomingInterchangeImport);
				AssertNotProcessed(incomingInterchangeExport);
				AssertNotProcessed(incomingInterchangeCommonNonTransactionID);
			});
		}

		public void TestProcess()
		{
			(GlbCompany ieCompany1, GlbBranch ieBranch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			(GlbCompany ieCompany2, GlbBranch ieBranch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T2");
			(GlbCompany auCompany, GlbBranch auBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Australia, "A1");
			var sessionGUID1 = ZGuid.NewZGuid();
			var trackingReference1 = sessionGUID1.ToString();
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = ieCompany1.PK;
			transactionNumber1.TN_TrackingReference = trackingReference1;
			var sessionGUID2 = ZGuid.NewZGuid();
			var trackingReference2 = sessionGUID2.ToString();
			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = ieCompany2.PK;
			transactionNumber2.TN_TrackingReference = trackingReference2;
			var sessionGUID3 = ZGuid.NewZGuid();
			var trackingReference3 = sessionGUID3.ToString();
			var transactionNumber3 = Factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber3.TN_GC_Company = ieCompany1.PK;
			transactionNumber3.TN_TrackingReference = trackingReference3;
			var sessionGUID4 = ZGuid.NewZGuid();
			var trackingReference4 = sessionGUID4.ToString();
			var transactionNumber4 = Factory.New<CusTransactionNumber>();
			transactionNumber4.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber4.TN_GC_Company = auCompany.PK;
			transactionNumber4.TN_TrackingReference = trackingReference4;
			var sessionGUID5 = ZGuid.NewZGuid();
			var trackingReference5 = sessionGUID5.ToString();
			var transactionNumber5 = Factory.New<CusTransactionNumber>();
			transactionNumber5.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber5.TN_GC_Company = ieCompany1.PK;
			transactionNumber5.TN_TrackingReference = trackingReference5;
			transactionNumber5.TN_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-4);

			var transactionID1 = ZGuid.NewZGuid().ToString();
			var transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID1);
			var incomingInterchange1 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID1, ieBranch1.PK);
			var transactionID2 = ZGuid.NewZGuid().ToString();
			transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID2);
			var incomingInterchange2 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID2, ieBranch2.PK);
			var transactionID3 = ZGuid.NewZGuid().ToString();
			transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID3);
			var incomingInterchange3 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, "X12", IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID3, ieBranch1.PK);
			var transactionID4 = ZGuid.NewZGuid().ToString();
			transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID4);
			var incomingInterchange4 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID4, auBranch.PK);
			var transactionID5 = ZGuid.NewZGuid().ToString();
			transactionIDResponse = new TransactionIdResponse()
			{
				Transactions = new System.Collections.ObjectModel.Collection<string>()
			};
			transactionIDResponse.Transactions.Add(transactionID5);
			var incomingInterchange5 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, IEXmlObjectSerializer.Serialize(transactionIDResponse), sessionGUID5, ieBranch1.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				var sendErrorNotificationCount = 0;
				var sendErrorNotification = new TransactionIDManager.SendErrorNotificationDelegate((ZGuid companyPK, ZGuid branchPK, string subject, string body) =>
				{
					sendErrorNotificationCount++;
				});
				var processor = new TransactionIDRequestProcessor(sendErrorNotification, 3, CusTransactionNumberTypeList.Codes.IECustoms, EDIMessage.ApplicationCodes.IECustomsCommon);
				processor.ExecuteBatch(System.Threading.CancellationToken.None);
				var logger = processor.Logger;
				var anotherFactory = new BusinessObjectFactory();
				transactionNumber1 = anotherFactory.Load<CusTransactionNumber>(transactionNumber1.PK);
				AssertEquals("transactionNumber1.TN_TransactionReference", transactionID1, transactionNumber1.TN_TransactionReference);
				transactionNumber2 = anotherFactory.Load<CusTransactionNumber>(transactionNumber2.PK);
				AssertEquals("transactionNumber2.TN_TransactionReference", transactionID2, transactionNumber2.TN_TransactionReference);
				transactionNumber3 = anotherFactory.Load<CusTransactionNumber>(transactionNumber3.PK);
				AssertEquals("Inccorect EI_Interchange - transactionNumber3.TN_TransactionReference", ZString.Empty, transactionNumber3.TN_TransactionReference);
				transactionNumber4 = anotherFactory.Load<CusTransactionNumber>(transactionNumber4.PK);
				AssertEquals("not IE - transactionNumber4.TN_TransactionReference", transactionID4, transactionNumber4.TN_TransactionReference);
				transactionNumber5 = anotherFactory.Load<CusTransactionNumber>(transactionNumber5.PK);
				AssertEquals("too old - transactionNumber5.TN_TransactionReference", ZString.Empty, transactionNumber5.TN_TransactionReference);
				incomingInterchange1 = anotherFactory.Load<EDIInterchange>(incomingInterchange1.PK);
				AssertEquals("incomingInterchange1.EI_Status", EDIInterchange.Status.Received, incomingInterchange1.EI_Status);
				incomingInterchange2 = anotherFactory.Load<EDIInterchange>(incomingInterchange2.PK);
				AssertEquals("incomingInterchange2.EI_Status", EDIInterchange.Status.Received, incomingInterchange2.EI_Status);
				incomingInterchange3 = anotherFactory.Load<EDIInterchange>(incomingInterchange3.PK);
				AssertEquals("Inccorect EI_Interchange - incomingInterchange3.EI_Status", EDIInterchange.Status.Queued, incomingInterchange3.EI_Status);
				incomingInterchange4 = anotherFactory.Load<EDIInterchange>(incomingInterchange4.PK);
				AssertEquals("not IE - incomingInterchange4.EI_Status", EDIInterchange.Status.Received, incomingInterchange4.EI_Status);
				incomingInterchange5 = anotherFactory.Load<EDIInterchange>(incomingInterchange5.PK);
				AssertEquals("too old - incomingInterchange5.EI_Status", EDIInterchange.Status.Received, incomingInterchange5.EI_Status);
				AssertEquals("sendErrorNotificationCount", 0, sendErrorNotificationCount);
				AssertMultilineASCIIEquals("Error Log", "\tProcessing Interchange (Type:TID, Number:1).\r\n\tInterchange '1' has been processed successfully.\r\n\tProcessing Interchange (Type:TID, Number:2).\r\n\tInterchange '2' has been processed successfully.\r\n\tProcessing Interchange (Type:TID, Number:4).\r\n\tInterchange '4' has been processed successfully.\r\n\tProcessing Interchange (Type:TID, Number:5).\r\n\tInterchange '5' has been processed successfully.\r\n", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}
	}
}
