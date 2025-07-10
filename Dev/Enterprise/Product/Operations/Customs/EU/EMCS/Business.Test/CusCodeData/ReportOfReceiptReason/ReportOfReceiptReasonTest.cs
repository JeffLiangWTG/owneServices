using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReportOfReceiptReason))]
	class ReportOfReceiptReasonTest : Customs.Business.Testing.CusCodeDataTest<ReportOfReceiptReason>
	{
		public void TestHumanReadableName()
		{
			var receiptOfReason = GetNewBusinessObject();
			AssertEquals("Report of Receipt Reason", receiptOfReason.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBizObj(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBizObj(Factory);
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, ReportOfReceiptReason bizObj)
		{
			factory.Load<EMCSInvoiceLineCusOutturn>(bizObj.CY_ParentID);
		}

		static ReportOfReceiptReason GetNewBizObj(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = factory.New<EMCSJobComInvoiceLine>();
			line.JI_JZ = invoiceHeader.PK;
			var outturn = line.Outturn;
			return outturn.ReportOfReceiptReasons.AddNew();
		}
	}
}
