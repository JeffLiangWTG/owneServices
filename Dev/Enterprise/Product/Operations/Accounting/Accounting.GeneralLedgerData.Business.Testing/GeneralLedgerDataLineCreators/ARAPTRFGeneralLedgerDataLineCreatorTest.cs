using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class ARAPTRFGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		public void TestCreateDRCREntries_ARTRF()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());

			var aRTransfer = TestObjectCreator.CreateTransfer<ARTransfer>(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK);
			var creator = new ARAPTRFGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)aRTransfer.TransferFrom).Row);

			var aRControlAccount = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = aRControlAccount, LocalAmount = -100m, OSAmount = -100m, DRCRSign = DebitCredit.CR, GLDAccountType = GLDAccountTypes.ARFromTRFControlAccount, JournalDate = aRTransfer.TransferFrom.AH_PostDate, GLDType = AccountingConstants.GLDTypeCodes.Posting, Period = 0 },
			};
			AssertGeneratedDRCRLines(entry.EntryItems, expectedDRCRLines);
		}

		public override void TestHasValidControlAccount()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var aRTransfer = TestObjectCreator.CreateTransfer<ARTransfer>(100m, new ZDateTime(2023, 5, 10), TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK);
			var creator = new ARAPTRFGeneralLedgerDataLineCreator();

			var entry = new DebitCreditEntry { EntryItems = Array.Empty<DebitCreditEntryItem>() };
			AssertExceptionThrown<MissingGLHeaderException>(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)aRTransfer.TransferFrom).Row);
			});
			Assert(!entry.EntryItems.Any());

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AssertNoExceptionThrown(() =>
			{
				entry = creator.CreateDRCREntries(((INeedRow)aRTransfer.TransferFrom).Row);
			});
			Assert(entry.EntryItems.Any());
		}
	}
}
