using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class CFSContainerRegoRunDocTest : BaseRunDocumentsTest
	{
		public CFSContainerRegoRunDocTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return Factory.New(typeof(CFSContainer)); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSContainerRego; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestCartersNoteAKLNZ()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Carters Note AKL NZ");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCFSOutturnReport()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "CFS Outturn Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCoverSheet()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportReceivalAdvice()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Receival Advice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNZDangerousGoodsPackingCertificate()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "NZ Dangerous Goods Packing Certificate");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTallySheet()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Tally Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForService()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Service");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAuthorisationForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Authorization for Service");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
