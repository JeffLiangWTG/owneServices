using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusContainer))]
	public class AddInfoCusContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusContainer cusContainer = Factory.New<CusContainer>();
			return new AddInfoCusContainer(cusContainer.CO_AddInfoInfo);
		}
	}
}
