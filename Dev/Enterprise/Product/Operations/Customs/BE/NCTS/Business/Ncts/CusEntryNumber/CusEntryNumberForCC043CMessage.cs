using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CusEntryNumberForCC043CMessage : CusEntryNumber
{
	public CusEntryNumberForCC043CMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override void OnSaving()
	{
		base.OnSaving();

		if (Parent != null && Parent is NctsArrivalMovementHeader movemementHeader)
		{
			var customsRegistries = BECustomsRegistry.Instance.CustomsRegistry.Value;
			var customsRegistryItem = RegistryHelper.GetValidCustomsRegistryForCompany(customsRegistries, BERegistryDeclarationTypeList.Codes.TransitArrival, movemementHeader.Header.DestinationTrader.Organisation.PK);

			if (!IsInDatabase && CE_EntryNum.IsEmpty && customsRegistryItem != null)
			{
				var newNumber = Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), customsRegistryItem.Organization.ToGuid(), BERegistryDeclarationTypeList.Codes.TransitArrival, customsRegistryItem.StartingDate, customsRegistryItem.StartingNo).GetNextFormatted(Factory);
				CE_EntryNum = newNumber.TrimStart('0');
			}
		}
	}
}
