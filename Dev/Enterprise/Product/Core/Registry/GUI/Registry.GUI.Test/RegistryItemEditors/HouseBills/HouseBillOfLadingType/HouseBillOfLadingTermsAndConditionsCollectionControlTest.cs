using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(HouseBillOfLadingTermsAndConditionsCollectionControl))]
	sealed class HouseBillOfLadingTermsAndConditionsCollectionControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new HouseBillOfLadingTermsAndConditionsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			HouseBillOfLadingTermsAndConditionsCollectionControl houseBillOfLadingTermsAndConditionsCollectionControl =
				(HouseBillOfLadingTermsAndConditionsCollectionControl)control;

			return houseBillOfLadingTermsAndConditionsCollectionControl.TermsAndConditionsGrid.ReadOnly &&
				houseBillOfLadingTermsAndConditionsCollectionControl.TermsAndConditionsImageSelectionControl.ReadOnly;
		}
	}
}
