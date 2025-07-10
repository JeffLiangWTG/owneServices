using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CusEntryNumberForCC028CMessage : CusEntryNumber
{
	public CusEntryNumberForCC028CMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override void OnSaving()
	{
		base.OnSaving();

		if (Parent is NctsDepartureMovementHeader movementHeader)
		{
			var representativeOrganisationPK = movementHeader.Representative.OrganisationPK;
			var principalOrganisationPK = movementHeader.Header.Principal.OrganisationPK;

			CustomsRegistry customsRegistryItem = null;
			if (!representativeOrganisationPK.IsEmpty)
			{
				customsRegistryItem = RegistryHelper.GetValidCustomsRegistryForCompany(BECustomsRegistry.Instance.CustomsRegistry.Value, BERegistryDeclarationTypeList.Codes.TransitDeparture, representativeOrganisationPK);
			}
			else
			{
				customsRegistryItem = RegistryHelper.GetValidCustomsRegistryForCompany(BECustomsRegistry.Instance.CustomsRegistry.Value, BERegistryDeclarationTypeList.Codes.TransitDeparture, principalOrganisationPK);
			}

			if (!IsInDatabase && CE_EntryNum.IsEmpty && customsRegistryItem != null)
			{
				var newNumber = Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), customsRegistryItem.Organization.ToGuid(), BERegistryDeclarationTypeList.Codes.TransitDeparture, customsRegistryItem.StartingDate, customsRegistryItem.StartingNo).GetNextFormatted(Factory);
				CE_EntryNum = newNumber.TrimStart('0');
			}
		}
	}
}
