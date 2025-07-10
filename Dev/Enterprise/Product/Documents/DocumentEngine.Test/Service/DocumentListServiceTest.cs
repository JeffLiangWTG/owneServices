using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Service.Testing
{
	sealed class DocumentListServiceTest : TestCaseWithFactory
	{
		public void TestGetDocumentList()
		{
			var menuItemDocContextA = Factory.New<DocumentCommand>();
			menuItemDocContextA.SU_MenuName = "Doc1";
			menuItemDocContextA.SU_MenuType = "DOC";
			menuItemDocContextA.SU_BusinessContext = "A";
			menuItemDocContextA.SU_IsPublished = true;

			var menuItemDocContextB = Factory.New<DocumentCommand>();
			menuItemDocContextB.SU_MenuName = "Doc2";
			menuItemDocContextB.SU_MenuType = "DOC";
			menuItemDocContextB.SU_BusinessContext = "B";
			menuItemDocContextB.SU_IsPublished = true;

			var menuItemDocContextB2 = Factory.New<DocumentCommand>();
			menuItemDocContextB2.SU_MenuName = "Doc3";
			menuItemDocContextB2.SU_MenuType = "DOC";
			menuItemDocContextB2.SU_BusinessContext = "B";
			menuItemDocContextB2.SU_IsPublished = true;

			var menuItemDocContextC = Factory.New<DocumentCommand>();
			menuItemDocContextC.SU_MenuName = "Doc4";
			menuItemDocContextC.SU_MenuType = "DOC";
			menuItemDocContextC.SU_FilterList = "\"<RT_Desc>\" != \"Test\"";
			menuItemDocContextC.SU_BusinessContext = "B";
			menuItemDocContextC.SU_IsPublished = true;

			var menuItemDocContextD1 = Factory.New<DocumentCommand>();
			menuItemDocContextD1.SU_MenuName = "Doc5";
			menuItemDocContextD1.SU_MenuType = "ACT";
			menuItemDocContextD1.SU_BusinessContext = "B";
			menuItemDocContextD1.SU_IsPublished = true;

			var menuItemDocContextD2 = Factory.New<DocumentCommand>();
			menuItemDocContextD2.SU_MenuName = "Doc6";
			menuItemDocContextD2.SU_MenuType = "FRM";
			menuItemDocContextD2.SU_BusinessContext = "B";
			menuItemDocContextD2.SU_IsPublished = true;

			var country1 = Factory.NewWithValidTestData<RefDocType>();
			country1.RT_Desc = "Test";

			var country2 = Factory.NewWithValidTestData<RefDocType>();
			country2.RT_Desc = "Other";

			Factory.Save();

			var documents = new DocumentListService().GetDocumentList("B", null, Guid.Empty, string.Empty);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB, documents);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB2, documents);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextC, documents);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextA, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextD1, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextD2, documents, false);

			menuItemDocContextB2.SU_FilterList = "\"<OH_Code>\" == \"XYZ\"";
			Factory.Save();

			documents = new DocumentListService().GetDocumentList("B", null, Guid.Empty, string.Empty);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB2, documents, isApplicable: false);

			documents = new DocumentListService().GetDocumentList("B", null, country1.PK, "RT");
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB, documents);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB2, documents, isApplicable: false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextA, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextC, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextD1, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextD2, documents, false);

			documents = new DocumentListService().GetDocumentList("B", null, country2.PK, "RT");
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB, documents);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextC, documents);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextA, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextB2, documents, isApplicable: false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextD1, documents, false);
			AssertContainsDocumentDetailMatchingMenuItem(menuItemDocContextD2, documents, false);
		}

		public void TestGetDocumentList_ForStaffUser()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			Factory.Save();

			var documentCommand = CreateDocument(docType, false);
			var businessObject = (BusinessObject)Factory.New<ICommonShipment>();
			Factory.Save();

			var documents = new DocumentListService().GetDocumentList(
				nameof(BusinessContext.Shipment),
				null,
				businessObject.PK,
				businessObject.TablePrefix);
			AssertContainsDocumentDetailMatchingMenuItem(documentCommand, documents);
		}

		public void TestGetDocumentList_ForContactWithAccess()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			Factory.Save();

			var documentCommand = CreateDocument(docType);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var glbGroup = CreateGroupWithDocumentAccess(contact.Header, docType);
			var glbGroupOrgContactLink = Factory.New<GlbGroupOrgContactLink>();
			glbGroupOrgContactLink.GCK_GG_Group = glbGroup.PK;
			glbGroupOrgContactLink.GCK_OC_Contact = contact.PK;

			var businessObject = (BusinessObject)Factory.New<ICommonShipment>();
			Factory.Save();

			var documents = new DocumentListService().GetDocumentList(
				nameof(BusinessContext.Shipment),
				contact,
				businessObject.PK,
				businessObject.TablePrefix);
			AssertContainsDocumentDetailMatchingMenuItem(documentCommand, documents);
		}

		public void TestGetDocumentList_ForContactWithoutAccess()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			Factory.Save();

			var documentCommand = CreateDocument(docType);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			CreateGroupWithDocumentAccess(contact.Header, docType);
			var businessObject = (BusinessObject)Factory.New<ICommonShipment>();
			Factory.Save();

			var documents = new DocumentListService().GetDocumentList(
				nameof(BusinessContext.Shipment),
				contact,
				businessObject.PK,
				businessObject.TablePrefix);
			AssertContainsDocumentDetailMatchingMenuItem(documentCommand, documents, false);
		}

		public void TestGetDocumentList_IsPublished()
		{
			var menuItem1 = Factory.New<DocumentCommand>();
			menuItem1.SU_MenuName = "Doc1";
			menuItem1.SU_MenuType = "DOC";
			menuItem1.SU_BusinessContext = "B";
			menuItem1.SU_IsPublished = true;

			var menuItem2 = Factory.New<DocumentCommand>();
			menuItem2.SU_MenuName = "Doc2";
			menuItem2.SU_MenuType = "DOC";
			menuItem2.SU_BusinessContext = "B";
			menuItem2.SU_IsVisibleOnWeb = true;
			menuItem2.SU_IsPublished = false;

			var menuItem3 = Factory.New<DocumentCommand>();
			menuItem3.SU_MenuName = "Doc3";
			menuItem3.SU_MenuType = "DOC";
			menuItem3.SU_BusinessContext = "B";
			menuItem3.SU_IsVisibleOnWeb = true;
			menuItem3.SU_IsPublished = true;

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "NOE";
			Factory.Save();
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var documents = new DocumentListService().GetDocumentList("B", null, Guid.Empty, string.Empty);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem1, documents);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem2, documents, contains: false);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem3, documents);
			}
		}

		public void TestGetDocumentList_Self_Created_Private()
		{
			const string businessContext = "B";

			var newStaffA = Factory.NewWithValidTestData<GlbStaff>();
			newStaffA.GS_Code = "TSA";

			var newStaffB = Factory.NewWithValidTestData<GlbStaff>();
			newStaffB.GS_Code = "TSB";

			Factory.Save();

			var menuItem1 = Factory.New<DocumentCommand>();
			var menuItem2 = Factory.New<DocumentCommand>();
			var menuItem3 = Factory.New<DocumentCommand>();
			var menuItem4 = Factory.New<DocumentCommand>();
			using (Env.SetTemporaryUserContext(newStaffA.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				buildCommonMenuItem(menuItem1, "Doc1");
				menuItem1.SU_IsPublished = true;

				buildCommonMenuItem(menuItem2, "Doc2");
				menuItem2.SU_IsPublished = false;
			}

			using (Env.SetTemporaryUserContext(newStaffB.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				buildCommonMenuItem(menuItem3, "Doc3");
				menuItem3.SU_IsPublished = true;

				buildCommonMenuItem(menuItem4, "Doc4");
				menuItem4.SU_IsPublished = false;
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(newStaffA.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var documents = new DocumentListService().GetDocumentList(businessContext, null, Guid.Empty, string.Empty);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem1, documents);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem2, documents);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem3, documents);
				AssertContainsDocumentDetailMatchingMenuItem(menuItem4, documents, contains: false);
			}

			void buildCommonMenuItem(DocumentCommand menuItem, string menuName)
			{
				menuItem.SU_MenuName = menuName;
				menuItem.SU_MenuType = "DOC";
				menuItem.SU_BusinessContext = businessContext;
			}
		}

		DocumentCommand CreateDocument(RefDocType docType, bool isVisibleOnWeb = true)
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_IsVisibleOnWeb = isVisibleOnWeb;
			documentCommand.SU_MenuName = "Shipment Doc";
			documentCommand.SU_MenuType = "DOC";

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			template.SO_Name = "Shipment Doc Template";
			template.SO_Template = Array.Empty<byte>();

			var templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_SO = template.PK;
			templatePivot.SI_SU = documentCommand.PK;
			templatePivot.SI_Index = 1;
			templatePivot.SI_DocumentTitle = "Shipment Document 1";
			templatePivot.SI_RT_DocType = docType.PK;
			var documentConfig = templatePivot.DocConfigs.AddNew();
			documentConfig.ConfigItems.AddNew();

			Factory.Save();

			return documentCommand;
		}

		GlbGroup CreateGroupWithDocumentAccess(OrgHeader orgHeader, RefDocType docType)
		{
			var documentRights = new DocumentWebSecurityRights(Factory);
			var webSecurityRight = documentRights.GetSecurityRight(docType);

			var orgRight = orgHeader.SecurityRights
				.OfType<OrgSecurity>()
				.First(s => s.OX_SecurityItemName == webSecurityRight.SecurityItemName);
			orgRight.OX_Granted = true;

			var glbSecurity = Factory.New<GlbSecurity>();
			var glbGroup = Factory.New<GlbGroup>();
			glbSecurity.GU_SecurityRight = webSecurityRight.SecurityItemName;
			glbSecurity.GU_GG = glbGroup.PK;
			Factory.Save();

			return glbGroup;
		}

		void AssertContainsDocumentDetailMatchingMenuItem(DocumentCommand menuItem, DocumentListItem[] documentItems, bool contains = true, bool isApplicable = true)
		{
			var result = documentItems.SingleOrDefault(x => x.Name == menuItem.SU_MenuName && x.Summary == menuItem.SU_Hint && x.Index == menuItem.SU_MenuIndex && x.Id == menuItem.PK && x.Path == menuItem.SU_MenuPath && x.DownloadOnly == menuItem.SU_DownloadOnly && x.IsApplicable == isApplicable);
			if (contains)
			{
				AssertNotNull(result);
			}
			else
			{
				AssertNull(result);
			}
		}
	}
}
