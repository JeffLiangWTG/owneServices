using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AddressMatchingLevelControl))]
	sealed class AddressMatchingLevelControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new AddressMatchingLevelBusinessObject();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((AddressMatchingLevelControl)control).ReadOnly;

		public void TestSetControlOrBusinessEntityReadOnly()
		{
			using (var form = new ZForm())
			using (var addressMatchingLevels = new AddressMatchingLevelControlForTest())
			{
				form.Controls.Add(addressMatchingLevels);
				form.Show();

				addressMatchingLevels.SetControlOrBusinessEntityReadOnly(true);

				AssertEquals(false, addressMatchingLevels.StrictRadioButton.ReadOnly);
				AssertEquals(false, addressMatchingLevels.BalancedRadioButton.ReadOnly);
				AssertEquals(false, addressMatchingLevels.ComprehensiveRadioButton.ReadOnly);
			}
		}
	}
}
