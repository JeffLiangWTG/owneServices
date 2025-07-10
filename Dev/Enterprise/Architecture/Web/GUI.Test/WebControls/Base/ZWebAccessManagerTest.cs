using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public abstract class ZWebAccessManagerTest : TestCaseWithFactory
	{
		#region Test Cases

		public void TestLicenceCheckpoints()
		{
			Dictionary<string, ILicenceCheckpoint[]> itemsToTest = new Dictionary<string, ILicenceCheckpoint[]>();
			SetupTestItemsForLicenceCheckpoints(itemsToTest);
			foreach (string pageRelativePath in itemsToTest.Keys)
			{
				AssertPageLicenceCheckpoints(pageRelativePath, itemsToTest[pageRelativePath], TestWebManager.LicenceCheckpoints(pageRelativePath));
			}
		}

		protected abstract void SetupTestItemsForLicenceCheckpoints(Dictionary<string, ILicenceCheckpoint[]> itemsToTest);

		#endregion

		#region Implementation

		protected void AssertPageLicenceCheckpoints(string pageRelativePath, ILicenceCheckpoint[] expected, ILicenceCheckpoint[] actual)
		{
			AssertEquals("Number of licence checkpoints do not match for " + pageRelativePath, expected.Length, actual.Length);
			foreach (ILicenceCheckpoint expectedCheckpoint in expected)
			{
				bool checkpointFound = false;
				foreach (ILicenceCheckpoint actualCheckpoint in actual)
				{
					if (actualCheckpoint == expectedCheckpoint)
					{
						checkpointFound = true;
						break;
					}
				}
				Assert("Expected licence checkpoint " + expectedCheckpoint.Name + " for " + pageRelativePath + " not found", checkpointFound);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestWebManager = GetNewWebAccessManager();
		}

		protected abstract IWebAccessManager GetNewWebAccessManager();

		protected IWebAccessManager TestWebManager;

		#endregion
	}
}
