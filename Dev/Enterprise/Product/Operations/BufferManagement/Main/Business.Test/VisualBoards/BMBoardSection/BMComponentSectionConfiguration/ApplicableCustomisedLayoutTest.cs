using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ApplicableCustomisedLayout))]
	class ApplicableCustomisedLayoutTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var layout = BMSTestHelper.CreateControlCustomisation(Factory);
			var link = BMSTestHelper.CreateControlCustomisationLink(Factory, Factory.New<BMBoard>(), layout);

			return new ApplicableCustomisedLayout(link);
		}
	}
}
