using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class ARAPCTRGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCRLines_CTR()
		{
			var contra = TestObjectCreator.CreateContra(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK);
			var creator = new ARAPCTRGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)contra.ARRow).Row);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = GLControlAccounts.Instance.ARControlAccount.PK, LocalAmount = -100m, OSAmount = -100m, DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.ARControlAccount, JournalDate = contra.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 }
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);

			entry = creator.CreateDRCREntries(((INeedRow)contra.APRow).Row);
			expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = GLControlAccounts.Instance.APControlAccount.PK, LocalAmount = 100m, OSAmount = 100m, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.APControlAccount, JournalDate = contra.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 }
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var aRContra = TestObjectCreator.CreateContra(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK);
			var creator = new ARAPCTRGeneralLedgerDataLineCreator();

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)aRContra.ARRow).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)aRContra.ARRow).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
			entry = creator.CreateDRCREntries(((INeedRow)aRContra.ARRow).Row);
			Assert(entry.EntryItems.Any());
		}
	}
}
