using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CTOCusMAWBDocumentSupporter))]
	sealed class CTOCusMAWBDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			AssertEquals("This shipment is not associated with a MAWB data.", Mawb.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Core.Constants.DataContext.CTOCusMAWB), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals(true, Mawb.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.CTOCusMAWB, null));
		}

		public void TestUsingCorrectDocumentSupporter()
		{
			AssertEquals(typeof(CTOCusMAWBDocumentSupporter), Mawb.DocumentSupporter.GetType());
		}

		public void TestCTOCusMAWBWrapper()
		{
			DocumentWrapper[] wrappers = Mawb.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CTOCusMAWB, null);

			AssertEquals("Should have returned 1 wrapper", 1, wrappers.Length);
			AssertEquals("Should have returned the correct wrapper", "DocCTOCusMAWB", wrappers[0].GetType().Name);
			AssertEquals("Wrapper should be wrapping the correct object", Mawb, wrappers[0].WrappedObject);
		}

		public void TestCTOCusMAWBGetDataStateBeforeRun()
		{
			CTOCusHAWBCollection hawbc = Mawb.ChildBills;
			CTOCusHAWB hawb = hawbc.AddNew();
			hawb.CS_WeightUQ = "KG";
			hawb = hawbc.AddNew();
			hawb.CS_WeightUQ = "11";

			CTOCusMAWBDocumentSupporter documentSupporter = (CTOCusMAWBDocumentSupporter)Mawb.DocumentSupporter;
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Flight Manifest";

			DocumentSupporterDataState state = documentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals(false, state.IsValid);
			AssertEquals("There is one of more invalid weight unit(s)", state.ErrorMessage);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Mawb;

		CTOCusMAWB mawb;
		CTOCusMAWB Mawb => mawb ?? (mawb = Factory.New<CTOCusMAWB>());
	}
}
