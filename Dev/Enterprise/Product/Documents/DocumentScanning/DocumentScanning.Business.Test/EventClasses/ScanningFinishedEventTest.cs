using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class ScanningFinishedEventTest : TestCaseWithFactory
	{
		public void TestAddFileDetail()
		{
			AssertEquals(0, TestArgs.FileDetailsCount);
			TestArgs.AddFileDetail(DocResult);
			AssertEquals(1, TestArgs.FileDetailsCount);
		}

		public void TestGetFileDetail()
		{
			TestArgs.AddFileDetail(DocResult);
			DocumentResult returnedResult = TestArgs.GetFileDetail(0);
			AssertEquals("same docResult should be returned", DocResult, returnedResult);
		}

		public void TestFileDetailsCount()
		{
			AssertEquals(0, TestArgs.FileDetailsCount);
			TestArgs.AddFileDetail(DocResult);
			AssertEquals(1, TestArgs.FileDetailsCount);
			TestArgs.AddFileDetail(DocResult);
			AssertEquals(2, TestArgs.FileDetailsCount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestArgs = new ScanningFinishedEventArgs();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			DocResult = new DocumentResult(MasterFactory);
		}

		ScanningFinishedEventArgs TestArgs;
		DocumentFactory MasterFactory;
		DocumentResult DocResult;
	}
}
