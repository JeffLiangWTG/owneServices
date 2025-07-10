using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class CartageCoverSheetRunDocsTest : BaseRunDocumentsTest
	{
		public override BusinessObject GetBusinessObject
		{
			get
			{
				CommonCartage cartage = Factory.New<CommonCartage>();
				cartage.FillWithValidTestData();
				CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
				container.FillWithValidTestData();
				return cartage;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Cartage; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}
		ZQuery fFilterForMenuItem;

		[ExpectNoExceptions]
		public void TestCartageCoverSheetWithContainers()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageCoverSheetWithNoContainers()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();
		}
	}
}
