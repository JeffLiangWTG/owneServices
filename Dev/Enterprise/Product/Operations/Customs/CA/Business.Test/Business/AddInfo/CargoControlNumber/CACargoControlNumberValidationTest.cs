using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACargoControlNumberValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		JobDeclaration declaration;

		#endregion
	}
}
