using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUStateCode))]
	public class AUStateCodeTest : NonPersistentBusinessObjectTestCase
	{
		#region Code
		public void TestCode()
		{
			var code = (AUStateCode)GetNewBusinessObject();
			AssertEquals("Code is empty", true, code.Code.IsEmpty);
			code.Code = "ACT";
			AssertEquals("Code is not empty", false, code.Code.IsEmpty);
		}
		#endregion

		#region Validation
		public void TestValidation()
		{
			var code = (AUStateCode)GetNewBusinessObject();
			AssertNotNull("Validation", code.Validation);
		}
		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AUStateCode(invoiceLine, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AUState_Hidden = "ACT,FO";
		}
		JobComInvoiceLine invoiceLine;
	}
}
