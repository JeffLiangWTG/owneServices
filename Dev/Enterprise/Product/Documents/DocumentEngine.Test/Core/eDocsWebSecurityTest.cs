using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class eDocsWebSecurityTest : TestCaseWithFactory
	{
		public void TestCanViewDocumentWhenNoContactPk()
		{
			var security = new eDocsWebSecurity(Factory, (ZGuid?)null);
			var docType = GetDocType();

			AssertEquals(true, security.CanViewDocument(docType));
		}

		public void TestCanViewDocumentWhenNoContact()
		{
			var security = new eDocsWebSecurity(Factory, (OrgContact)null);
			var docType = GetDocType();

			AssertEquals(true, security.CanViewDocument(docType));
		}

		public void TestCanViewDocumentWhenInvalidContact()
		{
			var security = new eDocsWebSecurity(Factory, ZGuid.NewZGuid());
			var docType = GetDocType();

			AssertEquals(false, security.CanViewDocument(docType));
		}

		public void TestCanViewDocumentWhenNoDocType()
		{
			var contact = GetContact();
			var security = new eDocsWebSecurity(Factory, contact.PK);

			AssertEquals(false, security.CanViewDocument(null));
		}

		public void TestCanViewDocument()
		{
			var contact = GetContact();
			var docType = GetDocType();

			GrantSecurityRightToContact(contact, docType, true);
			Factory.Save();

			var security = new eDocsWebSecurity(Factory, contact.PK);
			AssertEquals(true, security.CanViewDocument(docType));

			GrantSecurityRightToContact(contact, docType, false);
			Factory.Save();
			AssertEquals("using cached value", true, security.CanViewDocument(docType));

			security = new eDocsWebSecurity(Factory, contact.PK);
			AssertEquals(false, security.CanViewDocument(docType));
		}

		public void TestCanViewDocument_EnableSecurityGroupsForContactsInGLOW()
		{
			var contact = GetContact();
			var docType = GetDocType();
			var documentRights = new DocumentWebSecurityRights(Factory);
			var webSecurityRight = documentRights.GetSecurityRight(docType);

			var orgRight = contact.Header.SecurityRights
				.OfType<OrgSecurity>()
				.FirstOrDefault(s => s.OX_SecurityItemName == webSecurityRight.SecurityItemName);
			orgRight.OX_Granted = true;
			Factory.Save();

			using (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var security = new eDocsWebSecurity(Factory, contact.PK);
				AssertEquals("After enable EnableSecurityGroupsForContactsInGLOW, orgSecurity should have no effect", false, security.CanViewDocument(docType));

				// Add doctype to security group
				var glbSecurity = Factory.New<GlbSecurity>();
				var glbGroup = Factory.New<GlbGroup>();
				glbSecurity.GU_SecurityRight = webSecurityRight.SecurityItemName;
				glbSecurity.GU_GG = glbGroup.PK;
				Factory.Save();
				security = new eDocsWebSecurity(Factory, contact.PK);
				AssertEquals("Not Granted before adding contact into security group", false, security.CanViewDocument(docType));

				// Add contact to security group
				var glbGroupOrgContactLink = Factory.New<GlbGroupOrgContactLink>();
				glbGroupOrgContactLink.GCK_GG_Group = glbGroup.PK;
				glbGroupOrgContactLink.GCK_OC_Contact = contact.PK;
				Factory.Save();
				security = new eDocsWebSecurity(Factory, contact.PK);
				AssertEquals("Granted after adding contact into security group", true, security.CanViewDocument(docType));

				orgRight.OX_Granted = false;
				AssertEquals("Granted - orgSecurity's grant removed but Group security is still granted", true, security.CanViewDocument(docType));

				// Remove doctype from security group
				glbSecurity.GU_SecurityRight = "";
				Factory.Save();
				security = new eDocsWebSecurity(Factory, contact.PK);
				AssertEquals("Not Granted", false, security.CanViewDocument(docType));
			}
		}

		void GrantSecurityRightToContact(OrgContact contact, RefDocType docType, bool granted)
		{
			if (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value)
			{
				var webSecurity = new DocumentWebSecurityRights(Factory);
				var docSecurityRight = webSecurity.GetSecurityRight(docType);
				var groupCode = $"DOC_{docType.RT_ReferenceType}_{docType.RT_DocType}";
				var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, groupCode));

				if (group == null)
				{
					group = Factory.New<GlbGroup>();
					group.GG_Type = "ORG";
					group.GG_Code = groupCode;
					group.GG_Desc = group.GG_Code;

					var security = Factory.New<GlbSecurity>();
					security.GU_GG = group.PK;
					security.GU_SecurityRight = docSecurityRight.SecurityItemName;
					security.GU_SecurityItemIsAllowed = true;
				}

				var contactQuery = new ZQuery(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, contact.PK);
				contactQuery.AddToFilter(GlbGroupOrgContactLinkSchema.GCK_GG_Group, group.PK);
				var contactLink = Factory.LoadTop1<GlbGroupOrgContactLink>(contactQuery);

				if (granted && contactLink == null)
				{
					var newContactLink = Factory.New<GlbGroupOrgContactLink>();
					newContactLink.GCK_GG_Group = group.PK;
					newContactLink.GCK_OC_Contact = contact.PK;
				}
				else if (!granted && contactLink != null)
				{
					contactLink.Delete();
				}
			}
			else
			{
				var webSecurity = new DocumentWebSecurityRights(Factory);
				var docSecurityRight = webSecurity.GetSecurityRight(docType);
				var orgSecurityRight = contact.Header.SecurityRights.AddNew();
				orgSecurityRight.OX_SecurityItemName = docSecurityRight.SecurityItemName;
				orgSecurityRight.OX_Granted = granted;
			}
		}

		OrgContact GetContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			return contact;
		}

		RefDocType GetDocType()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			Factory.Save();

			return docType;
		}
	}
}
