using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.HotCheque.Testing
{
	public class PrintingFlagTest : TestCaseWithFactory
	{
		public void TestSavingCheque_PrintingWillSetAQ_PrintedOnlyAfterTheFactoryIsSaved()
		{
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(AssertAQ_PrintedIsNotChangedAndChequeIsNotSaved);
			NeedToAssertOnSaving = ZBool.True;
			Cheque = Factory.NewWithValidTestData<AccHotCheque>();
			Assert("AQ_Printed should be set to false by default", !Cheque.AQ_Printed);

			Cheque.DocumentEventSource_DocumentPrinted_ForTestOnly(this, null);
			Assert("AQ_Printed should not change", !Cheque.AQ_Printed);
			Assert("Cheque has not been saved", !Cheque.IsInDatabase);
			Factory.Save();
			Assert("Factory saved - AQ_Printed should be changed", Cheque.AQ_Printed);
			Assert("Cheque should be in database", Cheque.IsInDatabase);
			AssertEquals("Factory.Save() should be called once", 1, DBHitCounter);
		}

		public void TestSavingCheque_ReprintingWillNotCauseMultipleDBHits()
		{
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(AssertAQ_PrintedIsNotChangedAndChequeIsNotSaved);
			NeedToAssertOnSaving = ZBool.False;
			Cheque = Factory.NewWithValidTestData<AccHotCheque>();
			Cheque.AQ_Printed = ZBool.True;

			Cheque.DocumentEventSource_DocumentPrinted_ForTestOnly(this, null);
			Assert("AQ_Printed should keep its value", Cheque.AQ_Printed);
			Assert("Cheque should not be saved", !Cheque.IsInDatabase);
			Factory.Save();
			Assert("AQ_Printed should keep its value", Cheque.AQ_Printed);
			Assert("Cheque should be in database", Cheque.IsInDatabase);
			AssertEquals("Factory.Save() should be called only once", 1, DBHitCounter);
		}

		#region Implementation

		AccHotCheque Cheque;
		int DBHitCounter;
		ZBool NeedToAssertOnSaving;

		void AssertAQ_PrintedIsNotChangedAndChequeIsNotSaved(BusinessObjectFactory factory)
		{
			DBHitCounter++;
			if (DBHitCounter == 1 && NeedToAssertOnSaving)
			{
				Assert("OnSaving is called on the Cheque, but the AQ_Printed should not be changed yet", !Cheque.AQ_Printed);
				Assert("Factory.Save() was not called during the saving", !Cheque.IsInDatabase);
			}
		}

		#endregion
	}
}
