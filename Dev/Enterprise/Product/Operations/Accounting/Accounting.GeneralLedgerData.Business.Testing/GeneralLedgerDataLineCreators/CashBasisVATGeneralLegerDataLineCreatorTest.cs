using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class CashBasisVATGeneralLegerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		[SuspendCriticalValidation]
		public void TestCreateDRCREntries_ARINV()
		{
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var cashBasisVAT = TestObjectCreator.CreateInvoiceWithCashBasisVAT(typeof(ARInvoice), "CA1110001",
				GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 10m, 200m, 20m, 50m, 20m);
			cashBasisVAT.TransactionLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			var creator = new CashBasisVATGeneralLegerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)cashBasisVAT).Row);

			var pendingGSTOutput = AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value;
			var gSTOutputControlAccount = AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem
				{
					AccountPK = pendingGSTOutput,
					LocalAmount = 20m,
					OSAmount = 40m,
					DRCRSign = DebitCredit.DR,
					GLDAccountType = GLDAccountTypes.PendingGSTOutputControlAccount,
					JournalDate = cashBasisVAT.YC_PostDate,
					GLDType = AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT,
					Period = 0
				},
				new DebitCreditEntryItem
				{
					AccountPK = gSTOutputControlAccount,
					LocalAmount = -20m,
					OSAmount = -40m,
					DRCRSign = DebitCredit.CR,
					GLDAccountType = GLDAccountTypes.GSTOutputControlAccount,
					JournalDate = cashBasisVAT.YC_PostDate,
					GLDType = AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT,
					Period = 0
				}
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		[SuspendCriticalValidation]
		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, Guid.Empty);

			var cashBasisVAT = TestObjectCreator.CreateInvoiceWithCashBasisVAT(typeof(ARInvoice), "CA1110001", GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 10m, 200m, 20m, 50m, 20m);
			cashBasisVAT.TransactionLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();
			var creator = new CashBasisVATGeneralLegerDataLineCreator();

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)cashBasisVAT).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)cashBasisVAT).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)cashBasisVAT).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertNoExceptionThrown(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)cashBasisVAT).Row);
			});
			Assert(entry.EntryItems.Any());
		}
	}
}
