using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class AllocationFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			using (AllocationForm form = new AllocationForm(Allocation))
			{
				AssertEquals("Classifier Allocation", form.FormHeading);
			}
		}

		public void TestProcess()
		{
			AllocationForTest allocationForTest = new AllocationForTest(Factory);
			using (AllocationForm form = new AllocationForm(allocationForTest))
			{
				form.Show();
				form.ReAllocateButton.PerformClick();
				AssertEquals("Error Please fix the errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.ToString());
				allocationForTest.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
				form.ReAllocateButton.PerformClick();
				AssertEquals(true, allocationForTest.ReAllocated);
				AssertEquals(true, allocationForTest.ReLoadedClassifierAllocation);
			}
		}

		public void TestRefreshButton_Click()
		{
			AllocationForTest allocationForTest = new AllocationForTest(Factory);
			using (AllocationForm form = new AllocationForm(allocationForTest))
			{
				form.Show();
				form.RefreshButton.PerformClick();
				AssertEquals("Error Please fix the errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.ToString());
				allocationForTest.Queue = DeclarationQueueCodeDescriptionPairList.Codes.Classification;
				form.RefreshButton.PerformClick();
				AssertEquals(true, allocationForTest.ReLoadedClassifierAllocation);
			}
		}

		Allocation Allocation
		{
			get
			{
				if (fAllocation == null)
				{
					fAllocation = new Allocation(Factory);
				}

				return fAllocation;
			}
		}

		Allocation fAllocation;
		#region Allocation For Test
		class AllocationForTest : Allocation
		{
			public AllocationForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override void ReAllocate()
			{
				if (!ReLoadedClassifierAllocation) //this is to ensure that the order of calling is correct
				{
					ReAllocated = true;
				}
			}

			public bool ReAllocated;
			public override void ReLoadClassifierAllocation()
			{
				ReLoadedClassifierAllocation = true;
			}

			public bool ReLoadedClassifierAllocation;
		}
		#endregion
	}
}
