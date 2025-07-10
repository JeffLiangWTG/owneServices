using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class CusEntryHeaderInvoiceLinesToNctsAttacherTest : TestCaseForAttachGUI
	{
		[RequiresSTA]
		public void TestAttach()
		{
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			var entryHeader = jobdeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
			var nctsheader = Factory.NewWithValidTestData<NctsHeader>();
			nctsheader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var entryline = entryHeader.MergedLines.AddNew();
			entryline.FillWithValidTestData();
			var invoiline = entryline.InvoiceLines.AddNew();
			invoiline.FillWithValidTestData();
			var entryinstruction = jobdeclaration.CustomsEntryInstructions.AddNew();
			entryinstruction.FillWithValidTestData();
			invoiline.JI_JZ = invoice.PK;
			invoiline.JI_CEI = entryinstruction.PK;
			invoiline.JI_CL = entryline.PK;
			entryHeader.CH_CEI_Instruction = entryinstruction.PK;

			nctsheader.SetMovementType(NctsMovementType.Codes.Departure);
			CusEntryHeadersToAttachCollection collection = new CusEntryHeadersToAttachCollection(nctsheader);
			collection.Add(entryHeader);

			Factory.Save();

			using (var form = new ZForm(nctsheader))
			{
				var count = nctsheader.MovementHeader.GoodsItems.Count;
				var attacher = new CusEntryHeaderInvoiceLinesToNctsAttacherForTest(nctsheader, collection);
				attacher.Show(form);
				AssertEquals("Should have called through to base Show", 1, attacher.BaseShowCoreCallCountForTesting);
				attacher.AttachCoreExposed(entryHeader, null);

				AssertEquals("The ncts should have been added to the GoodsItemsForIntegration collection.", count + 1, nctsheader.MovementHeader.GoodsItems.Count);
			}
		}
		public class CusEntryHeaderInvoiceLinesToNctsAttacherForTest : CusEntryHeaderInvoiceLinesToNctsAttacher
		{
			public CusEntryHeaderInvoiceLinesToNctsAttacherForTest(NctsHeader nctsHeader, IBusinessObjectCollection findBoxList) : base(nctsHeader, findBoxList)
			{
			}

			public bool AttachCoreExposed(BusinessObject bizO, List<BusinessObject> listToBulkAdd) => base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
