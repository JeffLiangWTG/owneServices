using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethod))]
	public class ConsolCostDefaultApportionmentMethodObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolCostDefaultApportionmentMethod();
		}
	}
}
