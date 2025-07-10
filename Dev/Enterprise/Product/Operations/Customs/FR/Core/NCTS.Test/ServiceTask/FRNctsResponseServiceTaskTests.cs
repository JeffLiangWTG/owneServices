using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	sealed class FRNctsResponseServiceTaskTests : TestCaseWithFactory
	{
		#region Test

		public void TestOrderAndHint()
		{
			var processor = new FRNctsResponseMessageProcessorForTest(new TestServiceLogger());
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc, hint.IndexName);
		}

		public void TestNctsResponseServiceTaskExists()
		{
			var parser = new NctsDownloadPoller("FR");
			AssertNotNull(parser);
			var responseParser = parser.CountrySpecificChooser;
			AssertType<FRNctsResponseServiceTask>(responseParser);
			AssertEquals("FR", parser.NctsDomainCountryCode);
		}

		public void TestRunNctsResponseDownloader()
		{
			var frBranch = CreateFrCompanyAndBranchForTest(Factory);
			var responseServiceTask = new FRNctsResponseServiceTask();
			AssertNotNull(responseServiceTask);
			responseServiceTask.ExecuteDownload(serviceLogger, frBranch, CancellationToken.None);
		}

		public void TestNctsResponseProcessorCanExecute()
		{
			serviceLogger = new TestServiceLogger();
			var processor = new FRNctsResponseMessageProcessor(serviceLogger);
			AssertNotNull(processor);
			processor.ExecuteBatch();
		}

		#region ValidBranchesForMessageFilter

		public void TestValidBranchesForMessageFilter()
		{
			serviceLogger = new TestServiceLogger();
			var processor = new FRNctsResponseMessageProcessorForTest(serviceLogger);
			AssertEquals($"{EDIMessage.Schema.EM_MessageType} = 'FR' and {EDIMessage.Schema.EM_Status} = 'QUE' and {EDIMessage.Schema.EM_ApplicationCode} = 'NCT'", processor.ValidBranchesForMessageFilterForTesting.FilterPartsHashKey);
		}

		#endregion

		class FRNctsResponseMessageProcessorForTest : FRNctsResponseMessageProcessor
		{
			public FRNctsResponseMessageProcessorForTest(Integration.ILogger serviceLogger) : base(serviceLogger)
			{
			}

			public ZQuery ValidBranchesForMessageFilterForTesting => ValidBranchesForMessageFilter;
			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}

		public void TestCanInstanciateAllDeltaTProcessor()
		{
			serviceLogger = new TestServiceLogger();
			var logging = new LoggingInformation();

			var processorCC004A = new DTCC004AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC004A);
			var processorCC005A = new DTCC005AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC005A);
			var processorCC008A = new DTCC008AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC008A);
			var processorCC009A = new DTCC009AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC009A);
			var processorCC013B = new DTCC013BProcessor(serviceLogger, logging);
			AssertNotNull(processorCC013B);
			var processorCC014A = new DTCC014AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC014A);
			var processorCC015B = new DTCC015BProcessor(serviceLogger, logging);
			AssertNotNull(processorCC015B);
			var processorCC016A = new DTCC016AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC016A);
			var processorCC019A = new DTCC019AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC019A);
			var processorCC021A = new DTCC021AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC021A);
			var processorCC025A = new DTCC025AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC025A);
			var processorCC028A = new DTCC028AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC028A);
			var processorCC029B = new DTCC029BProcessor(serviceLogger, logging);
			AssertNotNull(processorCC029B);
			var processorCC035A = new DTCC035AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC035A);
			var processorCC043A = new DTCC043AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC043A);
			var processorCC044A = new DTCC044AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC044A);
			var processorCC045A = new DTCC045AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC045A);
			var processorCC055A = new DTCC055AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC055A);
			var processorCC058A = new DTCC058AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC058A);
			var processorCC140A = new DTCC140AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC140A);
			var processorCC141A = new DTCC141AProcessor(serviceLogger, logging);
			AssertNotNull(processorCC141A);
			var processorCCF02A = new DTCCF02AProcessor(serviceLogger, logging);
			AssertNotNull(processorCCF02A);
			var processorCCF03A = new DTCCF03AProcessor(serviceLogger, logging);
			AssertNotNull(processorCCF03A);
			var processorCCF15A = new DTCCF15AProcessor(serviceLogger, logging);
			AssertNotNull(processorCCF15A);
			var processorCCF96A = new DTCCF96AProcessor(serviceLogger, logging);
			AssertNotNull(processorCCF96A);
			var processorCCF97A = new DTCCF97AProcessor(serviceLogger, logging);
			AssertNotNull(processorCCF97A);
		}

		#endregion

		public static GlbBranch CreateFrCompanyAndBranchForTest(BusinessObjectFactory factory)
		{
			var frCompany = factory.New<GlbCompany>();
			frCompany.GC_Code = "PAR";
			frCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var ukBranch = frCompany.Branches.AddNew();
			ukBranch.GB_City = "Paris";
			factory.Save();
			return ukBranch;
		}

		TestServiceLogger serviceLogger;
	}
}
