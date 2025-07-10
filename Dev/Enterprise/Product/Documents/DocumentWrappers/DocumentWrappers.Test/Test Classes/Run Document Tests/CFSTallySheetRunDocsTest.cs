using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class CFSTallySheetRunDocsTest : BaseRunDocumentsTest
	{
		public CFSTallySheetRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(TallyContainer)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSTallySheet; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestCFSOutturnReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "CFS Outturn Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTallySheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Tally Sheet");
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
