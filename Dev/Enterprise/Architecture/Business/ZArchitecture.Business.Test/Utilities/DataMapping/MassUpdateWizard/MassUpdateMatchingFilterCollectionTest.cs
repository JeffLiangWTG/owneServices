using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(MassUpdateMatchingFilterCollection))]
	sealed class MassUpdateMatchingFilterCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MassUpdateMatchingFilterCollection>
	{
		protected override MassUpdateMatchingFilterCollection GetCollectionToTest()
		{
			return new MassUpdateMatchingFilterCollection(Helper.Wizard);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MassUpdateMatchingFilter(helper.Wizard);
		}

		MassUpdateWizardTestHelper Helper
		{
			get { return helper ?? (helper = new MassUpdateWizardTestHelper(Factory)); }
		}
		MassUpdateWizardTestHelper helper;
	}
}
