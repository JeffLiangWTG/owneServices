using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(UserChoiceCDWriter))]
	public class UserChoiceCDWriterTest : CDWriterTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UserChoiceCDWriter(MasterFactory, ArchiveManager);
		}

		public override CDWriter Writer
		{
			get
			{
				if (fWriter == null)
				{
					fWriter = new UserChoiceCDWriter(MasterFactory, ArchiveManager);
				}
				return fWriter;
			}
		}

		CDWriter fWriter;

		public override void TestBurn()
		{
			Assert("this class does not burn anything", true);
		}

		public override void TestCurrentDrive()
		{
			Assert("this class does not have a current drive", true);
		}

		public override void TestDriveList()
		{
			Assert("this class does not use the drive list", true);
		}

		public override void TestWriteSpeed()
		{
			Assert("this class does not use the write speed", true);
		}

		public override void TestWriteSpeedList()
		{
			Assert("this class does not use the write speed list", true);
		}
	}
}
