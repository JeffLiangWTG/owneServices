using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class CusEntryHeaderEntryLinesToNctsAttacherTest : TestCaseForAttachGUI
	{
		[RequiresSTA]
		public void TestAttach()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			invoiceline.JI_CL = entryLine.PK;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var houseConsignment = nctsHeader.Bills.AddNew();

			var entryHeadersCollection = new CusEntryHeadersToAttachCollection(nctsHeader);
			entryHeadersCollection.Add(entryHeader);

			using (var form = new ZForm(houseConsignment))
			{
				var attacher = new CusEntryHeaderEntryLinesToNctsAttacherForTest(houseConsignment, entryHeadersCollection);
				attacher.Show(form);

				CombineAssertions("PRE-CONDITION", () =>
				{
					AssertEquals("Should have called through to base Show", 1, attacher.BaseShowCoreCallCountForTesting);
					AssertEquals("GoodsItems Count", 0, houseConsignment.GoodsItems.Count);
				});

				attacher.AttachCoreExposed(entryHeader, null);
				AssertEquals("POST-CONDITION: GoodsItems Count", 1, houseConsignment.GoodsItems.Count);
			}
		}

		sealed class CusEntryHeaderEntryLinesToNctsAttacherForTest : CusEntryHeaderEntryLinesToNctsAttacher
		{
			public CusEntryHeaderEntryLinesToNctsAttacherForTest(NctsBill houseConsignment, IBusinessObjectCollection findBoxList) : base(houseConsignment, findBoxList)
			{
			}

			public bool AttachCoreExposed(BusinessObject bizO, List<BusinessObject> listToBulkAdd) => AttachCore(bizO, listToBulkAdd);
		}
	}
}
