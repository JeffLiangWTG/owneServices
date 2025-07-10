using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(PaymentAuthorisationSettingsControl))]
	public class PaymentAuthorisationSettingsControlWithPaymentAuthorisationTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new PaymentAuthorisationSettingsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PaymentAuthorisationSettingsControl)control).PaymentAuthorisationSettingsGrid.ReadOnly;
		}

		[RequiresSTA]
		public void TestSortCollectionOnBinding()
		{
			using (ZForm testForm = new ZForm())
			{
				testForm.MinimumSize = new System.Drawing.Size(1024, 600);
				testForm.Size = new System.Drawing.Size(1024, 600);
				testForm.CaptionRenderingEnabled = true;

				RegistryZUserControl testControl = GetNewControl();
				testForm.Controls.Add(testControl);

				IBusiness testBusinessEntity = GetNewBusinessEntity();

				AmountBasedAuthorisationRequirementCollection testCollection = (AmountBasedAuthorisationRequirementCollection)testBusinessEntity;
				AmountBasedMultiLevelAuthorisationRequirement settings = testCollection.AddNew();
				settings.Amount = 100M;
				settings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				settings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
				settings = testCollection.AddNew();
				settings.Amount = 50M;
				settings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				settings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
				settings = testCollection.AddNew();
				settings.Amount = 100M;
				settings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
				settings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

				testControl.SetDataBinding(testBusinessEntity, "");

				AssertEquals("The collection should be properly sorted.", 50M, testCollection[0].Amount);
				AssertEquals("The collection should be properly sorted.",
					AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, testCollection[0].AuthorisationRequirement);
				AssertEquals("The collection should be properly sorted.", 100M, testCollection[1].Amount);
				AssertEquals("The collection should be properly sorted.",
					AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, testCollection[1].AuthorisationRequirement);
				AssertEquals("The collection should be properly sorted.", 100M, testCollection[2].Amount);
				AssertEquals("The collection should be properly sorted.",
					AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly, testCollection[2].AuthorisationRequirement);
			}
		}
	}
}
