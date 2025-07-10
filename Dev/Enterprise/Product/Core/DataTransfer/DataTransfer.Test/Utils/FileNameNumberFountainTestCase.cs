using System;
using CargoWise.Data;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	public abstract class FileNameNumberFountainTestCase : TransactionedTestCase
	{
		protected abstract int FileIDLength { get; }
		protected abstract FileNameNumberFountain FileNameNumberFountainInstance { get; }
		protected abstract long MaxValue { get; }

		protected virtual long MinValue
		{
			get { return 1; }
		}

		protected virtual bool LengthOfFileIDIsAlwaysTheSame
		{
			get { return true; }
		}

		public void TestGenerateFileID()
		{
			ZString fileID = FileNameNumberFountainInstance.GetGenerateFileID(Db.Connection, false);

			if (LengthOfFileIDIsAlwaysTheSame)
			{
				Assert("Should not be Empty", !fileID.IsEmpty);
				AssertEquals("FileID Should be " + FileIDLength + " characters long", FileIDLength, fileID.Length);
				Assert("FileID is Numbers only", fileID.IsNumbersOnlyOrEmpty);
			}

			ZString tempFileID = fileID;
			fileID = FileNameNumberFountainInstance.GetGenerateFileID(Db.Connection, true);
			AssertEquals("FileID should still be the same", tempFileID, fileID);

			tempFileID = FileNameNumberFountainInstance.GetGenerateFileID(Db.Connection, false);
			AssertEquals("FileID has Incremented", (Convert.ToInt16(fileID)) + 1, Convert.ToInt16(tempFileID));
		}

		public void TestFileNameNumberFountain()
		{
			long testMaxValue = MaxValue;

			ZString nextNumber = FileNameNumberFountainInstance.FileID.PeekPreliminaryFormatted(Db.Connection);
			Assert("Get Next should return a number", nextNumber.IsNumbersOnlyOrEmpty);
			int cacheableFountainStrategyCacheSize = 100;
			long maxSetValue = testMaxValue - cacheableFountainStrategyCacheSize - 1;
			FileNameNumberFountainInstance.FileID.SetNext(Db.Connection, maxSetValue);

			// Get next value from the Fountain (from MaxSetValue to TestMaxValue - 2)
			for (long i = maxSetValue; i < testMaxValue - 1; i++)
			{
				FileNameNumberFountainInstance.FileID.GetNextFormatted(Db.Connection);
			}

			AssertEquals("GetNext Max-1", (testMaxValue - 1).ToString(), FileNameNumberFountainInstance.FileID.GetNextFormatted(Db.Connection));
			AssertEquals("GetNext Max", testMaxValue.ToString(), FileNameNumberFountainInstance.FileID.GetNextFormatted(Db.Connection));
			AssertEquals("GetNext 1 again", MinValue.ToString(), FileNameNumberFountainInstance.FileID.GetNextFormatted(Db.Connection));
		}

		#region Setup And TearDown

		protected override void SetUp()
		{
			base.SetUp();
			CargoWise.Data.Db.Connection.BeginTransaction();
		}

		protected override void TearDown()
		{
			base.TearDown();
			CargoWise.Data.Db.Connection.RollbackTransaction();
		}

		#endregion
	}
}
