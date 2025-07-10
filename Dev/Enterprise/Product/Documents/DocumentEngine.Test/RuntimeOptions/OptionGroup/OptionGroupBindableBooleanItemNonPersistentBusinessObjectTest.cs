using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(OptionGroupBindableBooleanItem))]
	sealed class OptionGroupBindableBooleanItemNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OptionGroupBindableBooleanItem(new ZBoolDescriptionPair("test", true));
		}
	}
}
