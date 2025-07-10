using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethodCollection))]
	public class ConsolCostDefaultApportionmentMethodCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ConsolCostDefaultApportionmentMethodCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ConsolCostDefaultApportionmentMethodCollection GetCollectionToTest()
		{
			return new ConsolCostDefaultApportionmentMethodCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ConsolCostDefaultApportionmentMethod();
		}

		#endregion
	}
}
