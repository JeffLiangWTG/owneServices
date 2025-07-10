using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class OwnerOfGoodsValidation : JobDocAddressValidation
	{
		public OwnerOfGoodsValidation(OwnerOfGoods parent) : base(parent)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			var ownerOfGoods = (OwnerOfGoods)Parent;
			if (ownerOfGoods.Instruction is CusEntryInstruction entryInstruction)
			{
				var ownerOfGoodsPK = ownerOfGoods.PK;
				var ownerOfGoodsOrganisationPK = ownerOfGoods.OrganisationPK;

				if (ownerOfGoods.OrganisationPK.IsEmpty)
				{
					MandatoryValidation.AddYouHaveNotEnteredMessage(ownerOfGoods.OrganisationPKInfo);
				}
				else if (entryInstruction.CEI_OH_Owner == ownerOfGoodsOrganisationPK || entryInstruction.OwnerOfGoodsCollection.Cast<OwnerOfGoods>().Any(x => x.PK != ownerOfGoodsPK && x.OrganisationPK == ownerOfGoodsOrganisationPK))
				{
					ownerOfGoods.OrganisationPKInfo.AddMessageError(Res.GetString("B93E03EF-3CC5-426B-AC08-23EA8D0C358A", "Owner must be unique, no duplicate allowed."));
				}
			}
		}
	}
}
