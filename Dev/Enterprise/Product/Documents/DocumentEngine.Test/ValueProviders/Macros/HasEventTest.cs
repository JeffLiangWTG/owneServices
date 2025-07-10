using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(HasEvent))]
	sealed class HasEventTest : ValueProviderWithLoadControlFactoryTest<HasEvent>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<HasEvent()>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<HasEvent(   )>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<HasEvent(bo,,,,)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<HasEvent(bo  code)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<HasEvent(,)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< HasEvent (Tst) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<HasEvent(bo,abc)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<HasEvent(  bo,  abc)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<HasEvent (bo,abc) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<  HasEvent (bo , abc ) >", Passes.FirstPass));
		}

		public override void TestReplacement()
		{
			var shipment = Factory.New<DummyShipmentBusinessObject>();
			Factory.Save();
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			AssertEquals(0, shipment.Logs.GetAllLogs().Count);
			AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent ({0})>", Events.AuthorisedCode), Report));

			var log = shipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.AuthorisedCode;
				Factory.Save();
				AssertEquals(1, shipment.Logs.GetAllLogs().Count);
				AssertEquals("Y", ValueProviderToTest.GetReplacement(string.Format("<HasEvent ({0})>", Events.AuthorisedCode), Report));
				AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent ({0})>", Events.EmailSentCode), Report));
			}

			log = shipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.EmailSentCode;
				Factory.Save();
				AssertEquals(2, shipment.Logs.GetAllLogs().Count);
				AssertEquals("Y", ValueProviderToTest.GetReplacement(string.Format("<HasEvent ({0})>", Events.EmailSentCode), Report));
			}

			var header = Factory.NewWithValidTestData<OrgHeader>();
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			Factory.Save();

			AssertEquals(1, header.Logs.GetAllLogs().Count);//"ADD" Event
			AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.AuthorisedCode), Report));

			log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.AuthorisedCode;
				Factory.Save();
				AssertEquals(2, header.Logs.GetAllLogs().Count);
				AssertEquals("Y", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.AuthorisedCode), Report));
				AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));
			}

			log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.EmailSentCode;
				Factory.Save();
				AssertEquals(3, header.Logs.GetAllLogs().Count);
				AssertEquals(false, log.SL_IsEstimate);
				AssertEquals(false, log.SL_IsCancelled);
				AssertEquals("Y", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));

				log.SL_IsEstimate = true;
				Factory.Save();
				AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));

				log.SL_IsEstimate = false;
				log.Cancel();
				Factory.Save();
				AssertEquals(true, log.SL_IsCancelled);
				AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));
			}
		}

		public void TestNotInDatabase()
		{
			var shipment = Factory.New<DummyShipmentBusinessObject>();
			Factory.Save();
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
			var header = Factory.NewWithValidTestData<OrgHeader>();
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			Factory.Save();
			var log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.EmailSentCode;
				AssertEquals("Y", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));

				log.SL_IsEstimate = true;
				AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));

				log.SL_IsEstimate = false;
				log.Cancel();
				AssertEquals("N", ValueProviderToTest.GetReplacement(string.Format("<HasEvent (Consignee,{0})>", Events.EmailSentCode), Report));
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var shipment = Factory.New<DummyShipmentBusinessObject>();
			var docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			var header = Factory.NewWithValidTestData<OrgHeader>();
			docDataProvider.Consignor = header;
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
		}
	}
}
