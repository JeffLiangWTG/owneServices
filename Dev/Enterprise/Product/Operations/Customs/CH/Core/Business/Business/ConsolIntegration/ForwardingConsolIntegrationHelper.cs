using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.CH;

namespace Enterprise.Customs.CH.Business;

public class ForwardingConsolIntegrationHelper : IForwardingConsolIntegrationHelper
{
	public ForwardingConsolIntegrationHelper()
	{
	}

	public bool CanPrintGroupDeliveryNoteForConsol(Forwarding.IForwardingConsol consol, IDocumentSupporterQueryProvider queryProvider)
	{
		var canPrint = true;

		var declarations = from s in consol.Shipments.Cast<ForwardingShipment>()
						   from d in s.Declarations.OfType<JobDeclaration>()
						   select d;

		if (!declarations.Any())
		{
			queryProvider.ShowMessage(Res.GetString("1C16F8F8-70A2-4712-9A71-B399200484B2", "There are no Swiss declarations."), MessageCaption);
			canPrint = false;
		}
		else
		{
			if (!CheckAllDeclarationsHaveSameCustomsOffice(declarations))
			{
				queryProvider.ShowMessage(Res.GetString("84613B57-4558-4E48-B0B3-CFF4387BF9D8", "Not all declarations have the same customs office."), MessageCaption);
				canPrint = false;
			}
			else if (!CheckAtLeastOneDeclarationHaveEntries(declarations))
			{
				queryProvider.ShowMessage(Res.GetString("9C165D94-2BF6-48C5-9957-A2415725EDCF", "No declaration has entries."), MessageCaption);
				canPrint = false;
			}
			else if (!CheckAllDeclarationEntriesCleared(declarations))
			{
				canPrint = queryProvider.ShowConfirmation(Res.GetString("6E8E8E97-87DC-4225-BF9C-F1002E990605", "Some declarations have not been cleared yet. Continue?"), MessageCaption);
			}
		}

		return canPrint;
	}

	bool CheckAllDeclarationsHaveSameCustomsOffice(IEnumerable<JobDeclaration> declarations)
	{
		return declarations.AllSame(d => d.JE_CustomsOffice);
	}

	bool CheckAtLeastOneDeclarationHaveEntries(IEnumerable<JobDeclaration> declarations)
	{
		return declarations.Any(d => d.ActiveEntryHeaders.Count > 0);
	}

	bool CheckAllDeclarationEntriesCleared(IEnumerable<JobDeclaration> declarations)
	{
		var entries = from d in declarations
					  from e in d.ActiveEntryHeaders.Cast<CusEntryHeader>()
					  select e;
		return entries.All(e => e.CH_Status == CHLogicalStatusList.Codes.Accepted);
	}

	string MessageCaption => Res.GetString("1733593B-CAC1-4C22-BD2D-CF330019D6F9", "Import Group Delivery Note");
}
