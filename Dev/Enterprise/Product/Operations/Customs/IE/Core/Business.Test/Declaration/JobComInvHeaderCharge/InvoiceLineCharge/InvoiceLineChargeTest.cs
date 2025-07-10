using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<ImportInvoiceLineChargeLookups>(invoiceLineCharge.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertType<EU.Business.Declaration.InvoiceLineChargeLookups>("Lookups for EXP", declaration.Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew().Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			AssertType<EU.Business.Declaration.InvoiceLineChargeLookups>("Lookups for non-IMP or EXP", declaration.Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew().Lookups);
		}
		public void TestShouldResetDefaultIsIncludedInAmount()
		{
			AssertEquals(true, invoiceLineCharge.GetType().GetMethod("ShouldResetDefaultIsIncludedInAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(invoiceLineCharge, new object[] { ZString.Empty, new Mock<ICustomsChargeCode>().Object }));
		}
		protected override BusinessObject GetNewBusinessObject() => invoiceLineCharge;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLineCharge = declaration.Invoices.AddNew().InvoiceLines.AddNew().Charges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceLineCharge invoiceLineCharge;
	}
}
