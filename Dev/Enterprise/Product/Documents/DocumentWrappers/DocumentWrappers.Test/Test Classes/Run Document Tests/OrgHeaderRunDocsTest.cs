using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class OrgHeaderRunDocsTest : BaseRunDocumentsTest
	{
		public OrgHeaderRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(OrgHeader)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Organisation; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestOrganisationNotes()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Organization Notes");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestSalesPack()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Sales Pack");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}
	}
}
