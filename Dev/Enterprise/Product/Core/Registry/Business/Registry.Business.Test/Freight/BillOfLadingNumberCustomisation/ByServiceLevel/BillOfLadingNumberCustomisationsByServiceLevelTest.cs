using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationsByServiceLevel))]
	sealed class BillOfLadingNumberCustomisationsByServiceLevelTest : RegistryBusinessObjectTemplateTestCase<BillOfLadingNumberCustomisationsByServiceLevel>
	{
		public void TestAllowNonAlphanumericCharacters()
		{
			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			var childCustomisation = customisations.BillOfLadingNumberCustomisations.AddNew();
			childCustomisation.AllowNonAlphanumericCharacters = false;
			AssertEquals(false, childCustomisation.AllowNonAlphanumericCharacters);
			customisations.AllowNonAlphanumericCharacters = true;
			AssertEquals(true, childCustomisation.AllowNonAlphanumericCharacters);
		}

		public void TestEnableMacroInsertion()
		{
			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			var childCustomisation = customisations.BillOfLadingNumberCustomisations.AddNew();
			childCustomisation.EnableMacroInsertion = false;
			AssertEquals(false, childCustomisation.EnableMacroInsertion);
			customisations.EnableMacroInsertion = true;
			AssertEquals(true, childCustomisation.EnableMacroInsertion);
		}

		public void TestPrefixLength()
		{
			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			var childCustomisation = customisations.BillOfLadingNumberCustomisations.AddNew();
			childCustomisation.PrefixLength = 3;
			AssertEquals(3, childCustomisation.PrefixLength);
			customisations.PrefixLength = 5;
			AssertEquals(5, childCustomisation.PrefixLength);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BillOfLadingNumberCustomisationsByServiceLevel GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override BillOfLadingNumberCustomisationsByServiceLevel GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		BillOfLadingNumberCustomisationsByServiceLevel NewPopulatedBusinessObject()
		{
			BillOfLadingNumberCustomisationsByServiceLevel result = new BillOfLadingNumberCustomisationsByServiceLevel();
			// TODO: populate the business object.
			return result;
		}

		#endregion
	}
}
