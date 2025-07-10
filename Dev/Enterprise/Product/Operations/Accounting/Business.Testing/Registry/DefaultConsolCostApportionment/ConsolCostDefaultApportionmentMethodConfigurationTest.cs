using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethodConfiguration))]
	public class ConsolCostDefaultApportionmentMethodConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetApportionmentMethodDefault()
		{
			const string allString = "ALL";
			var config = new ConsolCostDefaultApportionmentMethodConfiguration();
			AssertEquals("Default method should be present only", 1, config.ConsolCostDefaultApportionmentMethodCollection.Count);
			var firstMethod = config.ConsolCostDefaultApportionmentMethodCollection[0];

			CombineAssertions("Default method should be present", () =>
			{
				AssertEquals("Default method should be present", AllocationMethod.ChargeableUnits, firstMethod.Apportionment);
				AssertEquals("Default method should be present", allString, firstMethod.ConsolType);
				AssertEquals("Default method should be present", allString, firstMethod.ContainerMode);
				AssertEquals("Default method should be present", allString, firstMethod.Module);
				AssertEquals("Default method should be present", allString, firstMethod.TransportMode);
				AssertEquals("Default method should be present", allString, firstMethod.Direction);
				AssertEquals("Default method should have set parent", config.ConsolCostDefaultApportionmentMethodCollection, config.ConsolCostDefaultApportionmentMethodCollection[0].ParentCollection);
			});
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ConsolCostDefaultApportionmentMethodConfiguration result = new ConsolCostDefaultApportionmentMethodConfiguration();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ConsolCostDefaultApportionmentMethodConfiguration BizObj
		{
			get { return (ConsolCostDefaultApportionmentMethodConfiguration)base.BizObj; }
		}

		#endregion

	}
}
