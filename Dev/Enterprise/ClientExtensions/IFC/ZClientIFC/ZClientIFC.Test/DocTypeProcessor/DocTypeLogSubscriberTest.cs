using System.IO;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.IFC.Testing
{
	[TestedType(typeof(DocTypeLogSubscriber))]
	class DocTypeLogSubscriberTest : LogSubscriberTest<DocTypeLogSubscriber>
	{
		public void TestProcess()
		{
			try
			{
				RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
				Factory.Save();
				RunLogWalkerCycleForTest();
				AssertEquals("File created", 1, Directory.GetFiles(Env.TempPath).Length);
			}
			finally
			{
				TempDirectory.DeleteDirectory(Env.TempPath);
			}
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get
			{
				return true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			IFCDataRegistry.Instance.FSCExportDirectory = Env.TempPath;
		}
	}
}
