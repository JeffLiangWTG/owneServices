using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BrokerageCARSTBusinessObjectLoader : CARSTBusinessObjectLoaderOrCreator
	{
		protected internal override bool IsInterestedInCARST(CMRCARSTMessage message)
		{
			return !message.EntryNumber.IsEmpty
					&& (
						(!message.MAWB.IsEmpty) ||
						(!message.VoyageNumber.IsEmpty && !message.LloydsNumber.IsEmpty && !message.OceanBillNumber.IsEmpty)
					);
		}

		protected internal override BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
		{
			var masterBill = message.MAWB.IsEmpty ? message.OceanBillNumber : message.MAWB;
			var houseBill = message.HAWB.IsEmpty ? message.HouseBillNumber : message.HAWB;
			var containerNumber = message.ContainerNumber;

			var creator = new DeclarationRecordLoader(message.Factory);
			var reference = message.CARSTSendersReference;
			var entry = creator.LoadRecord(masterBill, houseBill, message.VoyageNumber, message.LloydsNumber, reference, message.EntryNumber, containerNumber);
			var packGroup = FindPackingGroup(entry, masterBill, houseBill, containerNumber);

			var businessObjects = new List<BusinessObject> { entry };

			if (packGroup != null)
			{
				businessObjects.Add(packGroup);
				if (reference.StartsWith("CE"))
				{
					var consolidatedDeclaration = ConsolidatedDeclaration.LoadFromRef(message.Factory, reference);
					if (consolidatedDeclaration != null)
					{
						businessObjects.Add(consolidatedDeclaration);
					}
				}
			}

			return businessObjects.ToArray();
		}

		internal PackingGroup FindPackingGroup(CusEntryHeader entry, ZString masterBill, ZString houseBill, ZString containerNumber)
		{
			PackingGroup result = null;

			var declaration = entry?.Declaration;
			if (declaration != null)
			{
				foreach (Package pack in declaration.Packages)
				{
					var packGroup = pack.PackingGroup;
					var bill = packGroup?.Bill;

					if (bill?.CU_HouseBill.EqualsIgnoringCase(houseBill) ?? false)
					{
						if (declaration.IsSea || bill.CU_MasterBill.KeepAlphanumericCharacters().EqualsIgnoringCase(masterBill.KeepAlphanumericCharacters()))
						{
							var packContainer = packGroup.Container?.CO_ContainerNumber ?? ZString.Empty;
							if (packContainer.EqualsIgnoringCase(containerNumber))
							{
								result = packGroup;
								break;
							}
						}
					}
				}
			}

			return result;
		}
	}
}
