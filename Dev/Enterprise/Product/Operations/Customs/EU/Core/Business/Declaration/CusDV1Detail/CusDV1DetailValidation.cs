using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusDV1DetailValidation : AutoCusDV1DetailValidation
	{
		public CusDV1DetailValidation(AutoCusDV1Detail parent) : base(parent)
		{
		}

		public new CusDV1Detail Parent => (CusDV1Detail)base.Parent;

		protected override void CheckDV1_Relationship()
		{
			base.CheckDV1_Relationship();

			ListValidation.ErrorIfInvalidCode(Parent.DV1_RelationshipInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_RelationshipInfo);
		}

		protected override void CheckDV1_PriceInfluence()
		{
			base.CheckDV1_PriceInfluence();

			ListValidation.ErrorIfInvalidCode(Parent.DV1_PriceInfluenceInfo);
			if (!Parent.NotBuyerSellerRelationship)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_PriceInfluenceInfo);
			}
		}

		protected override void CheckDV1_Restrictions()
		{
			base.CheckDV1_Restrictions();

			ListValidation.ErrorIfInvalidCode(Parent.DV1_RestrictionsInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_RestrictionsInfo);
		}

		protected override void CheckDV1_Consideration()
		{
			base.CheckDV1_Consideration();

			ListValidation.ErrorIfInvalidCode(Parent.DV1_ConsiderationInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_ConsiderationInfo);
		}

		protected override void CheckDV1_RestrictionConsiderationDetails()
		{
			base.CheckDV1_RestrictionConsiderationDetails();

			if (Parent.DV1_Restrictions == YesNoList.Codes.Yes || Parent.DV1_Consideration == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_RestrictionConsiderationDetailsInfo, Res.GetString("BB47EC88-5BF2-47DF-BAAE-FEE7FE6D36B6", "Details for Restrictions and/or Conditions"));
			}
		}

		protected override void CheckDV1_RoyaltiesLicence()
		{
			base.CheckDV1_RoyaltiesLicence();

			ListValidation.ErrorIfInvalidCode(Parent.DV1_RoyaltiesLicenceInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_RoyaltiesLicenceInfo);
		}

		protected override void CheckDV1_RoyaltiesLicenceDetails()
		{
			base.CheckDV1_RoyaltiesLicenceDetails();

			if (!Parent.DV1_RoyaltiesLicenceDetailsReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_RoyaltiesLicenceDetailsInfo, Res.GetString("60BE908A-8799-4263-8C37-3CA51D946523", "License Details"));
			}
		}

		protected override void CheckDV1_Resale()
		{
			base.CheckDV1_Resale();

			ListValidation.ErrorIfInvalidCode(Parent.DV1_ResaleInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_ResaleInfo);
		}

		protected override void CheckDV1_ResaleDetails()
		{
			base.CheckDV1_ResaleDetails();

			if (!Parent.DV1_ResaleDetailsReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DV1_ResaleDetailsInfo, Res.GetString("CC1DEAC8-77E4-42CB-A02D-68183306B805", "Resale Details"));
			}
		}
	}
}

