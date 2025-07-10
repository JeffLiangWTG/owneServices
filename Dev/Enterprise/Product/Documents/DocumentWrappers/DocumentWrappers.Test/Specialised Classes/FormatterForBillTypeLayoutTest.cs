using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.SpecialisedClasses
{
	[TestedType(typeof(FormatterForBillTypeLayout))]
	sealed class FormatterForBillTypeLayoutTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FormatterForBillTypeLayoutTestClassForShipment();
		}
	}
}
