using System.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DIFReferenceNumberFountainTest : TestCaseWithFactory
	{
		public void TestGetNextReferenceNumberWithFountain()
		{
			TransactionNumberSetting setting = new TransactionNumberSetting(Factory, "Test", "34567", null, TransactionNumber.DIFNumberDeclarationType, true);
			setting.NextNumber = 100;
			setting.MaxNumber = 200;
			Factory.Save();

			var bo = Factory.New<DummyBOForTesting>();
			bo.NumberFountain = new DIFReferenceNumberFountain("34567", Factory);
			Factory.Save();
			Assert("ReferenceNumber", bo.ReferenceNumber.StartsWith("3456700000100"));
		}

		public void TestGetNextReferenceNumberWithDriedUpFountain()
		{
			TransactionNumberSetting setting = new TransactionNumberSetting(Factory, "Test", "34567", null, TransactionNumber.DIFNumberDeclarationType, true);
			setting.NextNumber = 100;
			setting.MaxNumber = 200;
			Factory.Save();

			DIFReferenceNumberFountain difFountain = new DIFReferenceNumberFountain("34567", Factory);
			for (int i = 100; i < 200; i++)
			{
				difFountain.GetNextReferenceNumber();
			}
			AssertEquals("pre-condition", "34567000002008", difFountain.GetNextReferenceNumber());

			var bo = Factory.New<DummyBOForTesting>();
			bo.NumberFountain = new DIFReferenceNumberFountain("34567", Factory);
			var ex = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
			AssertContains("There are no more available numbers in the number range for DIF.", ex.Message);
		}

		[UseSnapshotProtection]
		public void TestGetNextReferenceNumberWithoutFountain()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name like 'GeneratorFountain-C-%'");

			var bo = Factory.New<DummyBOForTesting>();
			bo.NumberFountain = new DIFReferenceNumberFountain("34567", Factory);
			var ex = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
			AssertEquals(
				"DIF transaction number range for ASEC Number 34567 is not set.\r\n" +
				"Please set this range in the Maintain > Customs > Transaction Number form.",
				ex.Message
			);
		}

		class DummyBOForTesting : JobDeclaration
		{
			public DummyBOForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString ReferenceNumber { get; set; }
			public DIFReferenceNumberFountain NumberFountain { get; set; }

			public override void OnSaving()
			{
				base.OnSaving();
				ReferenceNumber = NumberFountain.GetNextReferenceNumber();
			}
		}
	}
}
