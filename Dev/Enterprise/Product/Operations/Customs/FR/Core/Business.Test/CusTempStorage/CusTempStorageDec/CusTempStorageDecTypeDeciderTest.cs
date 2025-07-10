using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class CusTempStorageDecTypeDeciderTest : TestCaseWithFactory
	{
		public void TestDefaultTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			AssertEquals(typeof(EU.Business.CusTempStorage.CusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
			AssertEquals("The CusTempStorageDecTypeDecider for FR could not load the object as the STH_DeclarationType '' is unknown. A base EU.CusTempStorageDec was returned instead.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = FRConstants.TemporaryStorage.AppCodeFRC;
			AssertEquals(typeof(FRCCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));

			storageDec.STH_DeclarationType = FRConstants.TemporaryStorage.AppCodeIST;
			AssertEquals(typeof(ISTCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));

			storageDec.STH_DeclarationType = FRConstants.TemporaryStorage.AppCodeLAD;
			AssertEquals(typeof(LADTCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			decider = new CusTempStorageDecTypeDecider();
		}
		CusTempStorageDecTypeDecider decider;

		#endregion
	}
}
