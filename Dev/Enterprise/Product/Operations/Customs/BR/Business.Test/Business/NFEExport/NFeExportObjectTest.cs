using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFeExportObject))]
	class NFeExportObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRefresh()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.Invoices.AddNew();

			var nfe = declaration.NFeExportObject;
			nfe.Refresh();

			AssertEquals(0, nfe.Entries.Count);

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("2000010001", ZDateTime.Today);
			entryheader1.CH_BGMReference = "000001";

			var entryLine1 = entryheader1.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);

			nfe.Refresh();

			AssertEquals("Entries reloaded", 1, nfe.Entries.Count);
			AssertEquals("Lines reloaded", 1, nfe.Entries[0].Lines.Count);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NFeExportObject(Factory.New<JobDeclaration>());
		}

		#endregion
	}
}
