using System;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace ZClientEDI.Test.MasterFiles.OpportunityManager.GUI
{
	public class OpportunityValueEstimationUserControlTest : TestCaseWithFactory
	{
		public void TestContractedLabelCaption()
		{
			using (var userControl = new OpportunityValueEstimationUserControl())
			{
				var contractedLabel = userControl.FindSingleOrDefault<ZLabel>("ContractedLabel");
				AssertEquals("Contracted", contractedLabel.Text);
				AssertEquals("Caption Resource String is not assigned", ResourceStringData.Empty, contractedLabel.CaptionResourceString);
			}

			var replacementValue = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
			AssertEquals("Precondition: Temporary Registry value differs from the current one", false, replacementValue.Equals("Contracted"));

			OrganisationsDataRegistry.Instance.CurrentLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, replacementValue);

			using (var userControl = new OpportunityValueEstimationUserControl())
			{
				var contractedLabel = userControl.FindSingleOrDefault<ZLabel>("ContractedLabel");
				AssertEquals("Verify the caption is being read from the registry", replacementValue.ToString(), contractedLabel.Text);
			}
		}

		public void TestLocalPotentialLabel()
		{
			using (var userControl = new OpportunityValueEstimationUserControl())
			{
				var localReachLabel = userControl.FindSingleOrDefault<ZLabel>("LocalPotentialLabel");
				AssertEquals("Local Reach", localReachLabel.Text);
				AssertEquals("Caption Resource String is not assigned", ResourceStringData.Empty, localReachLabel.CaptionResourceString);
			}

			var replacementValue = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
			AssertEquals("Precondition: Temporary Registry value differs from the current one", false, replacementValue.Equals("Local Reach"));

			OrganisationsDataRegistry.Instance.PotentialLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, replacementValue);

			using (var userControl = new OpportunityValueEstimationUserControl())
			{
				var localReachLabel = userControl.FindSingleOrDefault<ZLabel>("LocalPotentialLabel");
				AssertEquals("Verify the caption is being read from the registry", replacementValue.ToString(), localReachLabel.Text);
			}
		}
	}
}
