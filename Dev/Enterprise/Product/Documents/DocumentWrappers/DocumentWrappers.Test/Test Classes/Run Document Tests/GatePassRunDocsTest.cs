using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class GatePassRunDocsTest : BaseRunDocumentsTest
	{
		public GatePassRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(GatePassShipment)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.GatePass; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestGatePass()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Gate Pass");
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
