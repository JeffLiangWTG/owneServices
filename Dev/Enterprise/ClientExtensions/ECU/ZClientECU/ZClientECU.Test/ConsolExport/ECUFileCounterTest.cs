using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.ECU.ConsolExport.Testing
{
	public class ECUFileCounterTest : FileNameNumberFountainTestCase
	{
		public void TestGetFileName()
		{
			ZString fileCounter = ECUFileCounter.New().FileID.PeekPreliminaryFormatted(new BusinessObjectFactory());
			ZString testFileName = ECUFileCounter.GetNewFileName();
			Assert("Shouldn't be empty", !testFileName.IsEmpty);
			AssertEquals("File Name Should be 8 Characters Long", 8, testFileName.Length);
			AssertEquals("should be next number in sequence", fileCounter.PadLeft(8, '0'), testFileName.Left(8));
			ZString testFileName2 = ECUFileCounter.GetNewFileName();
			Assert("Shouldn't equal first FileName", testFileName != testFileName2);
		}

#region Implementation
		protected override int FileIDLength
		{
			get
			{
				return 8;
			}
		}

		protected override FileNameNumberFountain FileNameNumberFountainInstance
		{
			get
			{
				return ECUFileCounter.New();
			}
		}

		protected override long MaxValue
		{
			get
			{
				return 99999999;
			}
		}
#endregion
	}
}
