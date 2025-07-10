using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class RefDocTypeRunDocsTest : BaseRunDocumentsTest
	{
		public RefDocTypeRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(RefDocType)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.RefDocType; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestDocTypeCoverSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Doc Type Cover Sheet");
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
