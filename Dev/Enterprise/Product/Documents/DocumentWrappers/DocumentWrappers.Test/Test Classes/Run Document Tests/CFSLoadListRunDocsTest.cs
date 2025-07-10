using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class CFSLoadListRunDocsTest : BaseRunDocumentsTest
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSLoadList; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestCartageAdviceForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Arrival");
			fFilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdviceForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Departure");
			fFilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdviceWithReceiptForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice With Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Arrival");
			fFilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdviceWithReceiptForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice With Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Departure");
			fFilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCargoLoadList()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cargo Load List");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCargoManifest()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cargo Manifest");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCoverSheet()
		{
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#region Implementation

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var consol = Factory.New<CFSLoadListConsol>();
				return consol;
			}
		}

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
