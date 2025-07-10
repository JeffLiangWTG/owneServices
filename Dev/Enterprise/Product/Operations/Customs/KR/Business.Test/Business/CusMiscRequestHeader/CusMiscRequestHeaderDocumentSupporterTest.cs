using CargoWise.Definitions;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusMiscRequestHeaderDocumentSupporter))]
	sealed class CusMiscRequestHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowShowReasonForNotPrinting()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			var docSupporter = ((IDocumentSupportable)cusMiscRequestHeader).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			var docSupporter = ((IDocumentSupportable)cusMiscRequestHeader).DocumentSupporter;
			AssertEquals(Env.Security.MiscRequestCustomiseDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			var docSupporter = ((IDocumentSupportable)cusMiscRequestHeader).DocumentSupporter;
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.MiscRequest)));
		}

		public void TestBusinessContext()
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			var docSupporter = ((IDocumentSupportable)cusMiscRequestHeader).DocumentSupporter;
			AssertEquals(BusinessContext.CusMiscRequestHeader, docSupporter.BusinessContext);
		}

		void AssertGetBODocDataProviders(string messageType, string menuName)
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_MessageType = messageType;
			var docSupporter = ((IDocumentSupportable)cusMiscRequestHeader).DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = menuName;

			var bODocDataProviders = GetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO);
			AssertNull("Can't get BODocDataProviders", bODocDataProviders);

			bODocDataProviders = GetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EXPEntryHeaderBO);
			AssertNull("Can't get BODocDataProviders", bODocDataProviders);

			bODocDataProviders = GetBODocDataProviders(CusMiscRequestHeaderDocumentSupporter.DataContexts.CusMiscRequest);
			AssertEquals("Can get BODocDataProviders", 1, bODocDataProviders.Length);

			var miscRequestDocumentWrapper = (MiscRequestDocumentWrapper)bODocDataProviders[0];
			AssertEquals(messageType, miscRequestDocumentWrapper.MiscRequestHeader.CMR_MessageType);

			IBODocDataProvider[] GetBODocDataProviders(string dataContaxt)
			{
				var dataContext = new DataContextValue(dataContaxt);
				var bODocDataProviders = docSupporter.GetBODocDataProviders(dataContext, menuItem);
				return bODocDataProviders;
			}
		}

		void AssertGenerateQuestionsToAskUsersBeforeRunningDocumentCore(string messageType, string menuName)
		{
			var cusMiscRequestHeader = Factory.New<CusMiscRequestHeader>();
			cusMiscRequestHeader.CMR_MessageType = messageType;
			cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var requestHeader = (IDocumentSupportable)cusMiscRequestHeader;
			var supporter = (CusMiscRequestHeaderDocumentSupporter)requestHeader.DocumentSupporter;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = menuName;
			AssertEquals(1, supporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem).Count);
			AssertEquals("This document has been rejected by Customs. Are you sure you wish to print this document?", supporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem)[0].QuestionText);

			cusMiscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			requestHeader = cusMiscRequestHeader;
			supporter = (CusMiscRequestHeaderDocumentSupporter)requestHeader.DocumentSupporter;
			AssertEquals(0, supporter.GenerateQuestionsToAskUsersBeforeRunningDocument(menuItem).Count);
		}

		public void Test5GW()
		{
			AssertGetBODocDataProviders(ElectronicDocumentTypeList.Codes._5GW, CusMiscRequestHeaderDocumentSupporter.MenuNames.ApplicationforExtendedOfficeHours);
			AssertGenerateQuestionsToAskUsersBeforeRunningDocumentCore(ElectronicDocumentTypeList.Codes._5GW, CusMiscRequestHeaderDocumentSupporter.MenuNames.ApplicationforExtendedOfficeHours);
		}

		public void Test5AC()
		{
			AssertGetBODocDataProviders(ElectronicDocumentTypeList.Codes._5AC, CusMiscRequestHeaderDocumentSupporter.MenuNames.ApplicationforExtendedOfficeHours);
			AssertGenerateQuestionsToAskUsersBeforeRunningDocumentCore(ElectronicDocumentTypeList.Codes._5AC, CusMiscRequestHeaderDocumentSupporter.MenuNames.ApplicationforExtendedOfficeHours);
		}

		public void Test5SG()
		{
			AssertGetBODocDataProviders(ElectronicDocumentTypeList.Codes._5SG, CusMiscRequestHeaderDocumentSupporter.MenuNames.FinalPricePeriodExtensionApplication);
			AssertGenerateQuestionsToAskUsersBeforeRunningDocumentCore(ElectronicDocumentTypeList.Codes._5SG, CusMiscRequestHeaderDocumentSupporter.MenuNames.FinalPricePeriodExtensionApplication);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusMiscRequestHeader>();
		}
		public void TestGetFilterValueMSGBKRCTY()
		{
			var header = Factory.New<CusMiscRequestHeader>();

			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;

			var requestHeader = (IDocumentSupportable)header;
			AssertEquals("For filter 'MSGBKRCTY' result is '5SG' + Current Country Code", "5SG" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, requestHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}
	}
}
