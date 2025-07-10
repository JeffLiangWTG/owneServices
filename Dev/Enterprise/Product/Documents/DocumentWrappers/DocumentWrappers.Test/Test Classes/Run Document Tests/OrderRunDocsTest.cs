using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class OrderRunDocsTest : BaseRunDocumentsTest
	{
		public OrderRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(Order)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Order; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestPreAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Order Pre-Advice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestOrderNotification()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Order Notification");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestOrderAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Order Advice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForMissingDocuments()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request For Missing Documents");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
