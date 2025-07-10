using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSAddInfoCusContainer))]
	class EMCSAddInfoCusContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var container = declaration.CusContainers.AddNew();

			return new EMCSAddInfoCusContainer(container.CO_AddInfoInfo);
		}
	}
}
