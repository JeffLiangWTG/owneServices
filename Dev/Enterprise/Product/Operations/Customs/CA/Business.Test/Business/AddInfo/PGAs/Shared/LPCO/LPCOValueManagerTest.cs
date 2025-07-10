using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LPCOValueManagerTest : TestCaseWithFactory
	{
		public void TestGetValueSafe()
		{
			var pgaHeader = Factory.New<DFOPGAHeader>();
			var pgaCollection = new CusCALPCOCollection(pgaHeader);
			var viewCollection = new LPCOViewCollection(pgaCollection, null, pgaHeader);

			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.CLP_Type = "0000";

			var lazyCollection = new Lazy<LPCOViewCollection>(() => viewCollection);
			var manager = new LPCOValueManager(lazyCollection, ZString.Empty, () => lpco);

			AssertNotNull(lpco);
			AssertEquals(false, lpco.IsDeleted);

			var type = manager.GetValueSafe<ZString>(AutoCusCALPCO.Schema.CLP_Type);
			AssertEquals("0000", type);

			lpco.Delete();

			AssertNotNull(lpco);
			AssertEquals(true, lpco.IsDeleted);

			type = manager.GetValueSafe<ZString>(AutoCusCALPCO.Schema.CLP_Type);
			AssertEquals(ZString.Empty, type);
		}

		public void TestSetValueSafe()
		{
			var pgaHeader = Factory.New<DFOPGAHeader>();
			var pgaCollection = new CusCALPCOCollection(pgaHeader);
			var viewCollection = new LPCOViewCollection(pgaCollection, null, pgaHeader);

			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.CLP_Type = "0000";
			lpco.CLP_RefNo = "A0000";

			var lazyCollection = new Lazy<LPCOViewCollection>(() => viewCollection);

			var lpcoType = "5555";
			var manager = new LPCOValueManager(lazyCollection, lpcoType, () => lpco);
			manager.SetValueSafe<ZString>(AutoCusCALPCO.Schema.CLP_RefNo, "B0000");

			AssertEquals(false, lpco.IsDeleted);
			AssertEquals("B0000", lpco.CLP_RefNo);

			manager.SetValueSafe(AutoCusCALPCO.Schema.CLP_RefNo, ZString.Empty);
			var newLpco = viewCollection.Cast<LPCOView>().FirstOrDefault(c => c.CLP_Type == lpcoType);
			AssertNull(newLpco);
			AssertEquals(true, lpco.IsDeleted);

			manager.SetValueSafe<ZString>(AutoCusCALPCO.Schema.CLP_RefNo, "D0000");
			newLpco = viewCollection.Cast<LPCOView>().FirstOrDefault(c => c.CLP_Type == lpcoType);

			AssertEquals(true, lpco.IsDeleted);
			AssertEquals(false, newLpco.IsDeleted);
			AssertEquals("D0000", newLpco.CLP_RefNo);
		}

		public void TestNoExtraLPCOAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			var dfoPGAHeader = invoiceLine.DFOPGAHeader;
			var lpco0 = dfoPGAHeader.LPCOViews.AddNew();
			lpco0.CLP_Type = DFODocumentTypes.Codes.AquaticBiotechnologyNewSubstancesNotification;
			_ = dfoPGAHeader.NSNNumber;
			dfoPGAHeader.LPCOViews.RemoveAndDelete(lpco0);

			var lpco1 = dfoPGAHeader.LPCOViews.AddNew();
			lpco1.CLP_Type = DFODocumentTypes.Codes.AquaticBiotechnologyNewSubstancesNotification;
			dfoPGAHeader.NSNNumber = "XXXXX";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDFO = newFactory.Load<DFOPGAHeader>(dfoPGAHeader.PK);
			AssertEquals("XXXXX", loadedDFO.NSNNumber);
			AssertEquals(1, loadedDFO.LPCOViews.Count);
			AssertEquals("XXXXX", loadedDFO.LPCOViews[0].CLP_RefNo);
		}
	}
}
