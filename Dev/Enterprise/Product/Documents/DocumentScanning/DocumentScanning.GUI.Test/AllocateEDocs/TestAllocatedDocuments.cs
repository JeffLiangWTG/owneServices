using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class TestAllocatedDocuments : TestCaseWithDocumentFactory
	{
#if !WINZOR
		const int AllocatedDocumentsTabPageIndex = 2;
#else
		const int AllocatedDocumentsTabPageIndex = 1;
#endif

		public void TestAllocatedDocumentsViewCount_WhenNotSetToAllCompanies_DoesNotIncludeOtherCompanyDocuments()
		{
			AssertEquals("Precondition: Not showing document even though company has not been set", 6, DocManager.AllocatedDocumentsView.Count);

			var companyKey = ZGuid.NewZGuid();
			DocManager.AllocatedDocumentsView[0].SC_GC_Company = companyKey;

			AssertEquals("Should not show document belonging to other company", 5, DocManager.AllocatedDocumentsView.Count);
		}

		[RequiresSTA]
		public void TestAllocatedDocumentsViewCount_WhenSetToAllCompanies_IncludesOtherCompanyDocuments()
		{
			var companyKey = ZGuid.NewZGuid();
			DocManager.AllocatedDocumentsView[0].SC_GC_Company = companyKey;

			using (var form = new ZAllocateDocumentsForm(DocManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = AllocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Shows document belonging to other company, before allowing all companies", 5, DocManager.AllocatedDocumentsView.Count);

				form.AllocatedDocumentsCompanySpecificCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				AssertEquals("Should show document belonging to other company when requested to do so", 6, DocManager.AllocatedDocumentsView.Count);
			}
		}

		public void TestAllocatedDocumentsViewCount_ItemRefColumnsReadOnly()
		{
			using (var form = new ZAllocateDocumentsForm(DocManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = AllocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				for (var i = 0; i < DocManager.AllocatedDocumentsView.Count; i++)
				{
					AssertEquals(true, DocManager.AllocatedDocumentsView[i].ParentMain.SM_ParentFKInfo.ReadOnly);
				}

				AssertEquals("Item Refs column should be present", true, form.AllocatedGridControl.Grid.Columns.Any(s => s.ColumnName == "ParentMain+SM_ParentFK"));
				AssertEquals("Item Refs column should be readonly", true, form.AllocatedGridControl.Grid.Columns.First(s => s.ColumnName == "ParentMain+SM_ParentFK").ColumnStyle.ReadOnly);
			}
		}

		public void TestAllocatedDocumentsViewCount_WhenNotSetToAllBranches_DoesNotIncludeOtherBranchDocuments()
		{
			AssertEquals("Precondition: Not showing document even though branch has not been set", 6, DocManager.AllocatedDocumentsView.Count);

			var branchKey = ZGuid.NewZGuid();
			DocManager.AllocatedDocumentsView[0].SC_GB_Branch = branchKey;

			AssertEquals("Should not show document belonging to other branch", 5, DocManager.AllocatedDocumentsView.Count);
		}

		public void TestAllocatedDocumentsViewCount_WhenSetToAllBranches_IncludesOtherBranchDocuments()
		{
			var branchKey = ZGuid.NewZGuid();
			DocManager.AllocatedDocumentsView[0].SC_GB_Branch = branchKey;

			using (var form = new ZAllocateDocumentsForm(DocManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = AllocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Shows document belonging to other branch, before allowing all branches", 5, DocManager.AllocatedDocumentsView.Count);

				form.AllocatedDocumentsBranchSpecificCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				AssertEquals("Should show document belonging to other branch when requested to do so", 6, DocManager.AllocatedDocumentsView.Count);
			}
		}

		public void TestAllocatedDocumentsViewCount_WhenNotSetToAllDepartments_DoesNotIncludeOtherDepartmentDocuments()
		{
			AssertEquals("Precondition: Not showing document even though department has not been set", 6, DocManager.AllocatedDocumentsView.Count);

			var departmentKey = ZGuid.NewZGuid();
			DocManager.AllocatedDocumentsView[0].SC_GE_Department = departmentKey;

			AssertEquals("Should not show document belonging to other department", 5, DocManager.AllocatedDocumentsView.Count);
		}

		[RequiresSTA]
		public void TestAllocatedDocumentsViewCount_WhenSetToAllDepartment_IncludesOtherDepartmentDocuments()
		{
			var departmentKey = ZGuid.NewZGuid();
			DocManager.AllocatedDocumentsView[0].SC_GE_Department = departmentKey;

			using (var form = new ZAllocateDocumentsForm(DocManager))
			{
				form.DocumentAllocationTabControl.SelectedIndex = AllocatedDocumentsTabPageIndex;
				form.Show();

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				AssertEquals("Precondition: Shows document belonging to other department, before allowing all departments", 5, DocManager.AllocatedDocumentsView.Count);

				form.AllocatedDocumentsDepartmentSpecificCheckBox.Checked = true;

				AssertEquals("Precondition: Size of grid not reflecting the view being used", DocManager.AllocatedDocumentsView.Count, form.AllocatedGridControl.Grid.List.Count);
				AssertEquals("Should show document belonging to other department when requested to do so", 6, DocManager.AllocatedDocumentsView.Count);
			}
		}

		#region Implementation

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DocManagerDBHelperTestClass dBHelper = new DocManagerDBHelperTestClass();
			if (!dBHelper.DatabaseExists(1))
			{
				dBHelper.CreateDatabase(1);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable((new DocManagerDBHelper()).GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			Org1 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;
			filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			Org2 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			SetUpStorageMainObjects();

			DocManager = new AllocateDocumentsManagerExposed(MasterFactory);

			SetUpDocumentObjects();

			DocManager.AllocateDocuments();
		}

		void SetUpStorageMainObjects()
		{
			ParentFirstSet = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			ParentFirstSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			MasterFactory.AllocateToDB(ParentFirstSet);
			ParentFirstSet.SM_ParentFK = Org1.PK;

			ParentSecondSet = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			ParentSecondSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			MasterFactory.AllocateToDB(ParentSecondSet);
			ParentSecondSet.SM_ParentFK = Org2.PK;
		}

		void SetUpDocumentObjects()
		{
			var testImage = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
			Doc1FirstSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc1FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1FirstSetUnallocated.SC_ParentID = Org1.PK;
			Doc1FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1FirstSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1FirstSetUnallocated.SC_Desc = "AAA";
			Doc1FirstSetUnallocated.SC_ImageData = testImage;

			Doc2FirstSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc2FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2FirstSetUnallocated.SC_ParentID = Org1.PK;
			Doc2FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2FirstSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc2FirstSetUnallocated.SC_Desc = "BBB";
			Doc2FirstSetUnallocated.SC_ImageData = testImage;

			Doc3FirstSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc3FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3FirstSetUnallocated.SC_ParentID = Org1.PK;
			Doc3FirstSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3FirstSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc3FirstSetUnallocated.SC_Desc = "CCC";
			Doc3FirstSetUnallocated.SC_ImageData = testImage;

			Doc1SecondSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc1SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1SecondSetUnallocated.SC_ParentID = Org2.PK;
			Doc1SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1SecondSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1SecondSetUnallocated.SC_Desc = "DDD";
			Doc1SecondSetUnallocated.SC_ImageData = testImage;

			Doc2SecondSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc2SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2SecondSetUnallocated.SC_ParentID = Org2.PK;
			Doc2SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc2SecondSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc2SecondSetUnallocated.SC_Desc = "EEE";
			Doc2SecondSetUnallocated.SC_ImageData = testImage;

			Doc3SecondSetUnallocated = DocManager.UnallocatedDocuments.AddNew();
			Doc3SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3SecondSetUnallocated.SC_ParentID = Org2.PK;
			Doc3SecondSetUnallocated.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc3SecondSetUnallocated.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc3SecondSetUnallocated.SC_Desc = "FFF";
			Doc3SecondSetUnallocated.SC_ImageData = testImage;
		}

		AllocateDocumentsManagerExposed DocManager;

		OrgHeader Org1;
		OrgHeader Org2;
		StorageMain ParentFirstSet;
		StorageDocsUnallocated Doc1FirstSetUnallocated;
		StorageDocsUnallocated Doc2FirstSetUnallocated;
		StorageDocsUnallocated Doc3FirstSetUnallocated;
		StorageMain ParentSecondSet;
		StorageDocsUnallocated Doc1SecondSetUnallocated;
		StorageDocsUnallocated Doc2SecondSetUnallocated;
		StorageDocsUnallocated Doc3SecondSetUnallocated;
		EmbeddedResourceRetriever resourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			foreach (var file in SerializableEDoc.fileNames)
			{
				if (File.Exists(file))
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception) // file in use, etc. don't bother trying to handle.
					{
					}
				}
			}
			SerializableEDoc.fileNames.Clear();
			resourceRetriever.Dispose();
		}

		#endregion
	}
}
