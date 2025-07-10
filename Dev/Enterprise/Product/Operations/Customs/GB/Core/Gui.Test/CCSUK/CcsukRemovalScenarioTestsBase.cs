using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.ServiceTask;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	abstract class CcsukRemovalTestScenariosBase : TestCaseWithFactory
	{
		protected void AssertNoPrintQueued(string documentClass, string expectedTitle)
		{
			var query = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, expectedTitle);
			query.AddToFilter(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, " " + documentClass + " ");  // e.g. " C1 "
			var printInQueue = Factory.LoadTop1<StmPrintJob>(query);
			AssertNull(printInQueue);
		}

		protected MenuItem AssertWhetherReleaseDocumentPossibleFromMenu(ICcsukCusAwb awb, bool shouldBePossible)
		{
			return AssertWhetherOptionPossibleFromMenu(awb, shouldBePossible, ExpectedReleaseDocumentMenuTitle);
		}

		protected MenuItem AssertWhetherOptionPossibleFromMenu(ICcsukCusAwb awb, bool shouldBePossible, string menuOptionText)
		{
			using (var ccsukMenu = new CcsukMenu(new CusAwbDelegateProvider(delegate
			{ return awb; }), null))
			{
				ccsukMenu.RefreshMenu();
				var releaseMenu = ccsukMenu.MenuItems.FindByText(menuOptionText);
				if (shouldBePossible)
				{
					AssertNotNull(releaseMenu);
				}
				else
				{
					AssertNull(releaseMenu);
				}
				return releaseMenu;
			}
		}

		protected abstract string ExpectedReleaseDocumentMenuTitle { get; }

		protected abstract string RecipientPima { get; }

		protected T AssertAwbExists<T>(string referenceNumber)
			where T : BusinessObject, ICcsukCusAwb
		{
			T[] awbs = Factory.Load<T>(new ZQuery());
			var awb = (from T a in awbs where a.ReferenceNumber == referenceNumber select a).FirstOrDefault();
			AssertNotNull(awb);
			return awb;
		}

		protected StmPrintJob AssertPrintQueued(string expectedClass, string expectedAwbNumber, string[] expectedBodyFragments, string[] bodyMustNotContainFragments)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, expectedAwbNumber);
			query.AddToFilter(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, " " + expectedClass + " ");
			query.OrderBy = StmPrintJobSchema.SP_Sequence.Name + " DESC";
			var printInQueue = factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull(printInQueue);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(expectedBodyFragments, bodyMustNotContainFragments, printInQueue);
			return printInQueue;
		}

		protected delegate void DoStuffToUnderbondDelegate<T>(T underbond) where T : CusUnderbond;

		protected void CreateRemovalObjectAndMessage<TRemoval, TCusDec>(CusUnderbondCollection<TRemoval> collection, DoStuffToUnderbondDelegate<TRemoval> furtherSetupOfUnderbond = null)
			where TRemoval : CusUnderbond
			where TCusDec : CcsukTransmissionMessageFunction.CUSDEC, new()
		{
			var underbond = Factory.NewWithValidTestData<TRemoval>();
			collection.Add(underbond);
			if (furtherSetupOfUnderbond != null)
			{
				furtherSetupOfUnderbond(underbond);
			}
			Factory.Save();
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			UnderbondSenderHelper.SendMessage(underbond, new TCusDec(), shutUp);
			AssertEquals(true, string.IsNullOrEmpty(shutUp.LastErrorsAsString));
		}

		protected EDIMessage ProcessReceivedMessage(string inboundMessageText)
		{
			var message = CreateReceivedMessage(inboundMessageText, RecipientPima);
			Factory.Save();

			new CcsukNonChiefResponseBaseMessageProcessor(new TestServiceLogger()).ExecuteBatch();
			return message;
		}

		protected EDIMessage CreateReceivedMessage(string inboundMessageText, string recipientPima)
		{
			var inboundMocker = Factory.NewMoq<EDIMessage>();
			inboundMocker.Protected().Setup<string>("GetMessageReferenceNumber").Returns("2343233");
			var message = inboundMocker.Object;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_ApplicationCode = "CUK";
			message.EM_MessageText = inboundMessageText.Replace("<<ARRIVALDATE>>", ZDateTime.Now.AddDays(-1).ToString("yyMMdd"));  // to ensure that CM_ArrivalDate is always within a year, without needing to use [TestDate].  Using [TestDate] would lock the current date time so all StmALog records would be created with exactly the same datestamp.   Thus ordering by SL_EventTime (such as we do when looking at the most recently-released batch of pieces) would result in no ordering at all. 
			var receivedInterchange = Factory.New<EDIInterchange>();
			receivedInterchange.EI_ApplicationCode = "CUK";
			receivedInterchange.EI_ReceiveTransmit = "RCV";
			receivedInterchange.EI_To = recipientPima;
			receivedInterchange.EI_From = "blah";
			receivedInterchange.EI_BodyText = "blah blah";
			message.EM_EI = receivedInterchange.PK;
			return message;
		}

		protected void SimulatePurgingOfPrintQueue()
		{
			// Simulate successful printing...Delete prints from queue 
			var allPrints = Factory.Load<StmPrintJob>(new ZQuery());
			foreach (var print in allPrints)
			{
				print.Delete();
			}
			Factory.Save();
		}

		protected override void SetUp()
		{
			branchEnvironment = Environment.DisposableEnvironment.ForBranch(Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDown()
		{
			base.TearDown();
			branchEnvironment.Dispose();
		}
	}
}

