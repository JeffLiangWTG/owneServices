using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestLookups()
		{
			AssertType<ImportInvoiceChargeLookups>(invoiceCharge.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertType<InvoiceChargeLookups>("Lookups for EXP", declaration.Invoices.AddNew().Charges.AddNew().Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			AssertType<InvoiceChargeLookups>("Lookups for non-IMP or EXP", declaration.Invoices.AddNew().Charges.AddNew().Lookups);
		}

		public void TestShouldResetDefaultIsIncludedInAmount()
		{
			AssertEquals(true, invoiceCharge.GetType().GetMethod("ShouldResetDefaultIsIncludedInAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(invoiceCharge, new object[] { ZString.Empty, new Mock<ICustomsChargeCode>().Object }));
		}

		public void TestGetNewValidation()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertType<InvoiceChargeValidation>(invoiceCharge.Validation);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertType<ImportInvoiceChargeValidation>(invoiceCharge.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => declaration.Invoices.AddNew().Charges.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceCharge = declaration.Invoices.AddNew().Charges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceCharge invoiceCharge;
	}
}
