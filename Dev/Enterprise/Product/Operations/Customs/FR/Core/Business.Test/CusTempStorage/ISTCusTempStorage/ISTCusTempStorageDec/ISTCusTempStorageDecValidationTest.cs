using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class ISTCusTempStorageDecValidationTest : TestCaseWithFactory
	{
		public void TestCheckMandatoryLine()
		{
			tempStorageDec.Validation.ValidateAll();
			AssertHasRowError(tempStorageDec, "You need to supply at least one line.");

			tempStorageDec.CusTempStorageLines.AddNew();
			tempStorageDec.Validation.ValidateAll();
			AssertNoRowError(tempStorageDec, "You need to supply at least one line.");
		}

		public void TestCheckMandatoryLine_WhenRunValidateAllMultipleTimes()
		{
			Enumerable.Range(0, 5).ForEach(x => tempStorageDec.Validation.ValidateAll());
			AssertHasRowError(tempStorageDec, "You need to supply at least one line.");

			tempStorageDec.CusTempStorageLines.AddNew();
			tempStorageDec.Validation.ValidateAll();
			AssertNoRowError(tempStorageDec, "You need to supply at least one line.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			tempStorageDec = Factory.New<ISTCusTempStorageDec>();
		}

		ISTCusTempStorageDec tempStorageDec;
	}
}
