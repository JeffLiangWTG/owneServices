using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class SailingRunDocsTest : BaseRunDocumentsTest
	{
		public SailingRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(JobSailing)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.BookingLoadList; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestCargoLoadList()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cargo Load List");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}
	}
}
