using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CAAddInfoValidationConcreteTest : TestCaseWithFactory
	{
		[TestDate(2020, 09, 16)]
		public virtual void TestCheckCA_OriginalAccountingDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_OriginalAccountingDate = new ZDateTime(2016, 09, 15);
			AssertNoWarning(declaration.CA_OriginalAccountingDateInfo, "The date '15-Sep-2016' is more than 4 years old.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_OriginalAccountingDate = new ZDateTime(2016, 09, 15);
			AssertHasWarning(declaration.CA_OriginalAccountingDateInfo, "The date '15-Sep-2016' is more than 4 years old.");

			declaration.CA_OriginalAccountingDate = new ZDateTime(2020, 09, 15);
			AssertNoWarning(declaration.CA_OriginalAccountingDateInfo, "The date '15-Sep-2016' is more than 4 years old.");
		}

		public void TestCheckCA_ProductionDate()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var validation = new CAAddInfoValidation(new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo));
			validation.Parent.CA_ProductionDate = ZDateTime.Today.AddDays(+1);
			validation.ValidateCA_ProductionDate();
			AssertHasMessageErrorContaining(validation.Parent.CA_ProductionDateInfo, CAAddInfoValidation.ManufactureDateCannotBeInTheFuture);

			validation.Parent.CA_ProductionDate = ZDateTime.Today;
			validation.ValidateCA_ProductionDate();
			AssertNoMessageErrorContaining(validation.Parent.CA_ProductionDateInfo, CAAddInfoValidation.ManufactureDateCannotBeInTheFuture);

			validation.Parent.CA_ProductionDate = ZDateTime.Today.AddDays(-1);
			validation.ValidateCA_ProductionDate();
			AssertNoMessageErrorContaining(validation.Parent.CA_ProductionDateInfo, CAAddInfoValidation.ManufactureDateCannotBeInTheFuture);
		}

		[TestDate(2020, 07, 31)]
		public void TestGetEffectiveDateForDutyRate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2020, 03, 31);
			AssertEquals("31-Mar-20 00:00:00", declaration.AddInfoValidation.GetEffectiveDateForDutyRate().ToString());

			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("31-Mar-20 00:00:00", invoiceHeader.AddInfoValidation.GetEffectiveDateForDutyRate().ToString());

			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("31-Mar-20 00:00:00", invoiceLine.AddInfoValidation.GetEffectiveDateForDutyRate().ToString());
		}
	}
}
