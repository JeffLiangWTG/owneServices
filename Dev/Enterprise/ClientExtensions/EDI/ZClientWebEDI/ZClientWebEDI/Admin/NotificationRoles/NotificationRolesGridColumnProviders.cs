using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	internal class NotificationRolesContactsModuleColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			NotificationGropListInternal.Clear();

			ZBindToChecker.CheckBindTo(((OrgContact)null).OC_ContactName);
			AddToDictionaryAsRequired(new ZTextEditColumn(Res.GetString("4950fb0d-a88a-44c6-b3a9-8e72215e3bea", "Contact Name"), AutoOrgContact.Schema.OC_ContactName) { ColumnKey = 0, ReadOnly = true });

			ZBindToChecker.CheckBindTo(((OrgContact)null).OC_Email);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("65bee574-f698-4da3-8599-7370ff5c8c1c", "Email"), AutoOrgContact.Schema.OC_Email) { ColumnKey = 1, ReadOnly = true });

			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).CompanyNameForBindingOnly);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("aeeea8cf-b54f-45fb-a1cb-384e9a268fc1", "Company Name"), EDIOrgContact.Schema.CompanyNameForBindingOnly) { ColumnKey = 2, ReadOnly = true });

			ZBindToChecker.CheckBindTo(((OrgContact)null).Location);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f4e71fb2-e605-44a0-9718-d03bba8ddb06", "UNLOCO"), OrgContact.Schema.Location) { ColumnKey = 3, ReadOnly = true });

			var contactTypes = OrgCodeLists.ContactType_List;
			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).IsCustomerServiceContact);
			AddNotificationGroupColumn(contactTypes[ContactType.CustomerService.Code, StringComparison.OrdinalIgnoreCase], EDIOrgContact.Schema.IsCustomerServiceContact, 4);

			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).IsAccountsReceivableContact);
			AddNotificationGroupColumn(contactTypes[ContactType.Receivables.Code, StringComparison.OrdinalIgnoreCase], EDIOrgContact.Schema.IsAccountsReceivableContact, 5);

			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).IsBorderWiseAdministrator);
			AddNotificationGroupColumn(contactTypes[EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator, StringComparison.OrdinalIgnoreCase], EDIOrgContact.Schema.IsBorderWiseAdministrator, 6);

			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).IsERequestApprover);
			AddNotificationGroupColumn(contactTypes[EDIOrgDocumentGroupTypes.Codes.ERequestPendingApprovals, StringComparison.OrdinalIgnoreCase], EDIOrgContact.Schema.IsERequestApprover, 7);

			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).IsInformationServicesTechnicalAdministrator);
			AddNotificationGroupColumn(contactTypes[EDIOrgDocumentGroupTypes.Codes.InformationServicesTechnicalAdministrator, StringComparison.OrdinalIgnoreCase], EDIOrgContact.Schema.IsInformationServicesTechnicalAdministrator, 8);

			ZBindToChecker.CheckBindTo(((EDIOrgContact)null).IsCertificationProgramContact);
			AddNotificationGroupColumn(contactTypes[EDIOrgDocumentGroupTypes.Codes.CertificationProgramContact, StringComparison.OrdinalIgnoreCase], EDIOrgContact.Schema.IsCertificationProgramContact, 9);
		}

		void AddNotificationGroupColumn(ICodeDescription item, string bindTo, int columnKey)
		{
			AddToDictionaryAsDefault(new ZCheckBoxColumn(item.Code, bindTo) { ColumnKey = columnKey, ReadOnly = false, EditorWidth = 40, SortExpression = string.Empty });
			NotificationGropListInternal.Add(item);
		}

		public ReadOnlyCodeDescriptionPairList NotificationGroupList
		{
			get
			{
				if (NotificationGropListInternal.Count == 0)
				{
					CustomizeDictionary();
				}
				return NotificationGropListInternal;
			}
		}

		readonly CodeDescriptionPairList NotificationGropListInternal = new CodeDescriptionPairList();
	}

	internal class BulkUpdateAdminGroupGridColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("516ebdab-542c-4027-a1c8-00960eae81a7", "Administration Group"), "GroupDescription") { ColumnKey = 0, ReadOnly = true });
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("e3c34477-9fbf-48dd-a800-6574286b37f2", "Membership"), "Granted") { ColumnKey = 1, AutoPostBack = false });
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("ba0d30a3-f608-4fdd-a229-5cf563b02050", "Skip"), "Skip") { ColumnKey = 2, AutoPostBack = false });
		}
	}
}
