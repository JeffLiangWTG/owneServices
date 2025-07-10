using System;
using System.Linq;
using System.Text;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryComparisonFinderTest : TransactionedTestCase
	{
		public void TestRegDiffIsNotDisposable()
		{
			var isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(RegistryComparisonBusinessObject));
			Assert("RegistyDiffBusinessObject may throw an OperationCancelledException in it's consructor, meaning it's Dispose method will not be called", !isDisposable);
		}

		public void TestPeopleArentDeletingTests()
		{
			AssertNotNull("I hope you have a good reason for deleting tests", GetType().GetMethod(Encoding.Default.GetString(Convert.FromBase64String("VGVzdFJvdWdoR3Vlc3NJc250VG9vRmFyT3V0"))));
		}

		public void TestCancelFindExports()
		{
			using (var form = new ProgressForm())
			{
				form.ShowCancelButton = true;
				var progress = new RegistryForm.RegistryDiffFinderProgress(form);
				form.Show();

				AssertNoExceptionThrown("We havent cancelled yet so no exception", () => progress.Report(1));

				form.CancelButton.PerformClick();
				AssertExceptionThrown<OperationCanceledException>("Cancelled so exception", () => progress.Report(1));
			}
		}

		[RequiresSTA]
		public void TestIsUpdated()
		{
			using (var form = new ProgressForm())
			{
				form.SetStatusAndPercentComplete("", 0);
				var progress = new RegistryForm.RegistryDiffFinderProgress(form);

				Assert("Should set the text", !string.IsNullOrEmpty(form.Status));
				AssertEquals("Nothings happened so there should be no progress", 0, form.PercentComplete);

				progress.Report(RegistryForm.RegistryDiffFinderProgress.guessAtHowManyItemsWillBeChecked / 10);

				Assert("Something has happened so should have some progress", form.PercentComplete > 0);
			}
		}

		[RequiresSTA]
		public void TestRoughGuessIsntTooFarOut()
		{
			// We keep it as a guess to avoid have to enumerate the large list twice.
			// Since it's only to display progress it's ok if it isn't spot on
			// Since the tolerance is pretty wide, it should mostly get updated from bulk changes

			using (Env.SetTemporaryUserContext("CWSupport", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var form = new RegistryForm())
			{
				var allItemsCount = form.GetApplicableItemsForComparison().Count();
				var minimumDesiredAmount = (int)(0.95 * allItemsCount);
				var maximumDesiredAmount = (int)(1.15 * allItemsCount);

				var message = string.Format("The rough guess of the number of registry items is too far out from the actual. Please update the value to be between {0} and {1}, eg {2}", minimumDesiredAmount, maximumDesiredAmount, (minimumDesiredAmount + maximumDesiredAmount) / 2);
				Assert(message, minimumDesiredAmount <= RegistryForm.RegistryDiffFinderProgress.guessAtHowManyItemsWillBeChecked && RegistryForm.RegistryDiffFinderProgress.guessAtHowManyItemsWillBeChecked <= maximumDesiredAmount);
			}
		}

		[RequiresSTA]
		public void TestGetApplicableItemsForComparison_ShouldNotContainsInvisibleRegistryItem()
		{
			var itemSet = new MockRegistryItemSetForTest();
			var mockClientHook = new Mock<ClientHook>();
			mockClientHook.Setup(m => m.AdditionalRegistryItemSet).Returns(itemSet);
			using (ClientHookLoader.Instance.OverrideClientHookForTest(mockClientHook.Object))
			{
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals("CurrentUser is controller", true, Env.CurrentUser.IsController);

				using (var form = new RegistryForm())
				{
					var allItems = form.GetApplicableItemsForComparison();
					AssertEquals("Should contains BooleanItemForTest", true, allItems.Any(x => x.Name == "BooleanItemForTest"));
				}

				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals("CurrentUser is not controller", false, Env.CurrentUser.IsController);

				using (var form = new RegistryForm())
				{
					var allItems = form.GetApplicableItemsForComparison();
					AssertEquals("Should not contains BooleanItemForTest", false, allItems.Any(x => x.Name == "BooleanItemForTest"));
				}
			}
		}
	}
}
