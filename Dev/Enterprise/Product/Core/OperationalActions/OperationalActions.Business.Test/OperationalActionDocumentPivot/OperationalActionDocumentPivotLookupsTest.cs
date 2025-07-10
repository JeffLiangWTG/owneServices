using System.Reflection;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionDocumentPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentsContactGroups()
		{
			DocumentCommand noContact = Factory.New<DocumentCommand>();
			noContact.SU_MenuName = "noContact";
			noContact.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			noContact.SU_GS_NKStaffCode = "";
			noContact.SU_ContactType = ContactType.NoContactType.Code;
			DocumentCommand anyContact = Factory.New<DocumentCommand>();
			anyContact.SU_MenuName = "anyContact";
			anyContact.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			anyContact.SU_GS_NKStaffCode = "";
			anyContact.SU_ContactType = ContactType.Consignor.Code;
			ZQuery scope = new ZQuery(StmMenuItemSchema.PK, new ZGuid[] { noContact.PK, anyContact.PK });
			OperationalActionContext context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			OperationalAction action = Factory.New<OperationalAction>();
			action.Context = context;
			OperationalActionDocumentPivot pivot = action.DocumentPivots.AddNew();
			AssertLoadedDocuments("", scope, pivot.Lookups.Documents, noContact, anyContact);
		}

		public void TestDocuments()
		{
			DocumentCommand document1 = Factory.New<DocumentCommand>();
			DocumentCommand document2 = Factory.New<DocumentCommand>();
			DocumentCommand document3 = Factory.New<DocumentCommand>();
			DocumentCommand document4 = Factory.New<DocumentCommand>();
			DocumentCommand document5 = Factory.New<DocumentCommand>();
			document1.SU_MenuName = "document1";
			document2.SU_MenuName = "document2";
			document3.SU_MenuName = "document3";
			document4.SU_MenuName = "document4";
			document5.SU_MenuName = "document5";
			document1.SU_ContactType = ContactType.Consignor.Code;
			document2.SU_ContactType = ContactType.Consignor.Code;
			document3.SU_ContactType = ContactType.Consignor.Code;
			document4.SU_ContactType = ContactType.Consignor.Code;
			document5.SU_ContactType = ContactType.Consignor.Code;
			document1.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			document2.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			document3.SU_BusinessContext = nameof(BusinessContext.Order);
			document4.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			document5.SU_BusinessContext = MockOperationalActionSupportable.BusinessContext.ToString();
			document1.SU_GS_NKStaffCode = "";
			document2.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			document3.SU_GS_NKStaffCode = "";
			document4.SU_GS_NKStaffCode = "";
			document5.SU_GS_NKStaffCode = "";
			document4.SU_IsSystemDefined = true;
			document5.SU_IsSystemDefined = true;
			document5.SU_IsClientSpecific = true;
			ZQuery scope = new ZQuery(StmMenuItemSchema.PK, new ZGuid[] { document1.PK, document2.PK, document3.PK, document4.PK, document5.PK });
			OperationalAction action = Factory.New<OperationalAction>();
			OperationalActionContext context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			OperationalActionDocumentPivot pivot = Factory.New<OperationalActionDocumentPivot>();
			action.Context = context;
			pivot.SF_SU_Inward = action.PK;
			action.SU_Calc_IsPublished = true;
			AssertLoadedDocuments("published", scope, pivot.Lookups.Documents, document1, document4, document5);
			action.SU_Calc_IsPublished = false;
			AssertLoadedDocuments("un-published", scope, pivot.Lookups.Documents, document1, document2, document4, document5);
			pivot.SF_IsSystemDefined = true;
			AssertLoadedDocuments("system", scope, pivot.Lookups.Documents, document4);
			pivot.SF_SU_Inward = ZGuid.Empty;
			AssertLoadedDocuments("detatched", scope, pivot.Lookups.Documents, document1, document2, document3, document4, document5);
		}

		#region Implementation
		static void AssertLoadedDocuments(string message, ZQuery scope, StmMenuItemBaseCollection collection, params DocumentCommand[] documents)
		{
			ZQuery additionalFilter = (ZQuery)typeof(StmMenuItemBaseCollection).InvokeMember("AdditionalFilter", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, collection, null);
			ZQuery combined = new ZQuery(additionalFilter, scope);
			AssertContainsExactElementsInAnyOrder<DocumentCommand>(message, (d) => d.SU_MenuName, documents, collection.Factory.Load<DocumentCommand>(combined));
		}
		#endregion
	}
}
