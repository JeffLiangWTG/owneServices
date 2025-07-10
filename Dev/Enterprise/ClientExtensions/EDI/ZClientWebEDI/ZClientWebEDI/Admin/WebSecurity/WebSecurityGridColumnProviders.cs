using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	internal class WebSecurityGridColumnProviders
	{
		enum ColumnKeys : int
		{
			SecurityItemName = 0,
			OX_GrantedForWeb,
			Select,

			OZ_Granted,
			OC_ContactName,

			OC_Email,
			CompanyName,
			Branch,
			UNLOCO,

			Granted,
			Skip,
		}

		internal class SecurityGridColumnProvider : GridColumnProvider
		{
			protected override void CustomizeDictionaryCore()
			{
				base.CustomizeDictionaryCore();
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5ffc323a-c12a-40bc-a7f0-8bff87ac338f", "Security Item"), OrgSecurity.Schema.SecurityItemNameForDisplay) { ColumnKey = ColumnKeys.SecurityItemName });
				AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("06f83eb7-45a3-48be-9a70-5a2e84382309", "Granted"), OrgSecurity.Schema.OX_GrantedForWeb) { ColumnKey = ColumnKeys.OX_GrantedForWeb, AutoPostBack = false });
			}
		}

		internal class ContactGridColumnProvider : GridColumnProvider
		{
			protected override void CustomizeDictionaryCore()
			{
				base.CustomizeDictionaryCore();
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5ffc323a-c12a-40bc-a7f0-8bff87ac338f", "Security Item"), OrgSecurityContacts.Schema.SecurityItemName) { ColumnKey = ColumnKeys.SecurityItemName });
				AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("06f83eb7-45a3-48be-9a70-5a2e84382309", "Granted"), OrgSecurityContacts.Schema.OZ_Granted)
				{ ColumnKey = ColumnKeys.OZ_Granted, AutoPostBack = false });
			}
		}

		internal class BulkUpdateSecurityGridColumnProvider : GridColumnProvider
		{
			protected override void CustomizeDictionaryCore()
			{
				base.CustomizeDictionaryCore();
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5ffc323a-c12a-40bc-a7f0-8bff87ac338f", "Security Item"), "SecurityItemName") { ColumnKey = ColumnKeys.SecurityItemName, ReadOnly = true });
				AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("06f83eb7-45a3-48be-9a70-5a2e84382309", "Granted"), "Granted") { ColumnKey = ColumnKeys.Granted, AutoPostBack = false });
				AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("ba0d30a3-f608-4fdd-a229-5cf563b02050", "Skip"), "Skip") { ColumnKey = ColumnKeys.Skip, AutoPostBack = false });
			}
		}

		internal class WebSecurityContactsModuleColumnProvider : GridColumnProvider
		{
			protected override void CustomizeDictionaryCore()
			{
				base.CustomizeDictionaryCore();

				ZBindToChecker.CheckBindTo(((OrgContact)null).OC_ContactName);
				AddToDictionaryAsRequired(new ZTextEditColumn(Res.GetString("4950fb0d-a88a-44c6-b3a9-8e72215e3bea", "Contact Name"), OrgContact.Schema.OC_ContactName)
				{ ColumnKey = ColumnKeys.OC_ContactName });

				ZBindToChecker.CheckBindTo(((OrgContact)null).OC_Email);
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("65bee574-f698-4da3-8599-7370ff5c8c1c", "Email"), OrgContact.Schema.OC_Email)
				{ ColumnKey = ColumnKeys.OC_Email });

				ZBindToChecker.CheckBindTo(((EDIOrgContact)null).CompanyNameForBindingOnly);
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("aeeea8cf-b54f-45fb-a1cb-384e9a268fc1", "Company Name"), EDIOrgContact.Schema.CompanyNameForBindingOnly)
				{ ColumnKey = ColumnKeys.CompanyName });

				ZBindToChecker.CheckBindTo(((OrgContact)null).Location);
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f4e71fb2-e605-44a0-9718-d03bba8ddb06", "UNLOCO"), OrgContact.Schema.Location)
				{ ColumnKey = ColumnKeys.UNLOCO });
			}

			protected override List<int> GetOldColumnsOrder()
			{
				var result = new List<int>();
				result.Add((int)ColumnKeys.OC_ContactName);
				result.Add((int)ColumnKeys.OC_Email);
				result.Add((int)ColumnKeys.CompanyName);
				result.Add((int)ColumnKeys.UNLOCO);
				return result;
			}
		}
	}
}
