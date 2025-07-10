using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	public class ExportInformationTest : TestCase
	{
		public void TestConstructor2()
		{
			ExportInformation = new ExportInformation(12, ZDateTime.Now, ZDateTime.Now);
			AssertEquals(12, ExportInformation.BatchNumber);
		}

		public void TestConstructor3()
		{
			ExportInformation = new ExportInformation(12, ZDateTime.Now, ZDateTime.Now);
			ExportInformation.RunEveryDayExport = true;
			ExportInformation.EveryDayStartDate = new ZDateTime(2006, 1, 1);
			ExportInformation.EveryDayEndDate = new ZDateTime(2007, 1, 1);
			AssertEquals(12, ExportInformation.BatchNumber);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.EveryDayStartDate);
			AssertEquals(new ZDateTime(2007, 1, 1), ExportInformation.EveryDayEndDate);
		}

		public void TestEveryDayDateOfArrivalStartDate()
		{
			ExportInformation.EveryDayDateOfArrivalStartDate = new ZDateTime(2006, 1, 1, 10, 10, 10);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.EveryDayDateOfArrivalStartDate);
		}

		public void TestEveryDayDateOfArrivalEndDate()
		{
			ExportInformation.EveryDayDateOfArrivalEndDate = new ZDateTime(2006, 1, 1, 10, 10, 10);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.EveryDayDateOfArrivalEndDate);
		}

		public void TestWorkingDayDateOfArrivalMetroStartDate()
		{
			ExportInformation.WorkingDayDateOfArrivalMetroStartDate = new ZDateTime(2006, 1, 1, 10, 10, 10);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.WorkingDayDateOfArrivalMetroStartDate);
		}

		public void TestWorkingDayDateOfArrivalMetroEndDate()
		{
			ExportInformation.WorkingDayDateOfArrivalMetroEndDate = new ZDateTime(2006, 1, 1, 10, 10, 10);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.WorkingDayDateOfArrivalMetroEndDate);
		}

		public void TestWorkingDayDateOfArrivalOtherStartDate()
		{
			ExportInformation.WorkingDayDateOfArrivalOtherStartDate = new ZDateTime(2006, 1, 1, 10, 10, 10);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.WorkingDayDateOfArrivalOtherStartDate);
		}

		public void TestWorkingDayDateOfArrivalOtherEndDate()
		{
			ExportInformation.WorkingDayDateOfArrivalOtherEndDate = new ZDateTime(2006, 1, 1, 10, 10, 10);
			AssertEquals(new ZDateTime(2006, 1, 1), ExportInformation.WorkingDayDateOfArrivalOtherEndDate);
		}

		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			ExportInformation = new ExportInformation(1, ZDateTime.Now, ZDateTime.Now);
		}

		ExportInformation ExportInformation;
		#endregion
	}
}
