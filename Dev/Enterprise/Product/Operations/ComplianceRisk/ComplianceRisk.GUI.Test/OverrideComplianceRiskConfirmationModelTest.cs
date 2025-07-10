using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(OverrideComplianceRiskConfirmationModel))]
	public class OverrideComplianceRiskConfirmationModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRegistryOverride()
		{
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title1", IsMandatory = false }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.ComplianceRiskOverrideDecisionReason.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new OverrideComplianceRiskConfirmationModel();
				model.Code = "CCC";
				AssertNoErrors(model.CodeInfo);

				model.Code = "AAA";
				AssertHasErrors("The Code is Invalid.", model.CodeInfo);
			}
		}
	}
}
