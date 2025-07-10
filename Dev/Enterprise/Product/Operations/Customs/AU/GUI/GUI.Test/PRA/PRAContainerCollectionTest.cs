using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class PRAContainerCollectionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAddingNonCommittedElementEmulatingBindingInTheGrid()
		{
			var consol = Factory.New<CommonConsol>();
			var containerCollection = new CommonContainerCollection(consol, Factory);
			var freightContainer = (BusinessObject)((IBindingList)containerCollection).AddNew(); // uncomitted element
			var interfaceCollection = new PRAContainerCollection(Factory);
			interfaceCollection.Add(freightContainer);
		}
	}
}
