using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class DeliveryDocketRunDocsTest : BaseRunDocumentsTest
	{
		public DeliveryDocketRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get
			{
				CommonCartage cartage = Factory.New<CommonCartage>();
				cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
				return cartage;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Cartage; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestDeliveryDocket()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delivery Docket with Receipt");
			RunDocument();
		}
	}
}
