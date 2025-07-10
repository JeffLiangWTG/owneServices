using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	[TestedType(typeof(CSSLogSubscriber))]
	internal class CSSLogSubscriberTest : LogSubscriberTest<CSSLogSubscriber>
	{
		public void TestName()
		{
			AssertEquals("TGECSSCustomsResponse", LogSubscriber.Name);
			AssertEquals("TGE CSS Customs Response", LogSubscriber.FriendlyName);
		}

		public void TestOverrides()
		{
			AssertEquals(3, LogSubscriber.TableNames.Length);
			AssertEquals(CusHAWBSchema.Constants.TableName, LogSubscriber.TableNames[0]);
			AssertEquals(JobDeclarationSchema.Constants.TableName, LogSubscriber.TableNames[1]);
			AssertEquals(1, LogSubscriber.EventTypes.Length);
			AssertEquals(Events.AddedARecordToTheSystem.Code, LogSubscriber.EventTypes[0]);
		}

		public void TestProcess()
		{
			TempDirectory.DeleteDirectory(Env.TempPath);
			try
			{
				TestHelper.GetPopulatedCollection();
				Factory.Save();
				RunLogWalkerCycleForTest();
				Application.DoEvents();
				Thread.Sleep(0);
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
			TestHelper.SetValidRegistryAll();
		}

		TGETestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TGETestHelper(Factory));
			}
		}

		TGETestHelper testHelper;
	}
}
