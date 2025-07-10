using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterJobDocAddressValidation : JobDocAddressValidation
	{
		public CusCAeMHMasterJobDocAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.Consolidator:
						if (!Parent.OrganisationPK.IsEmpty && Parent.Organisation != null)
						{
							CAAddressValidator.ValidateMandatoryCityNameOnly(Parent, Parent.E2_OA_AddressInfo, Res.GetString("e5147ca8-64d6-4ead-a486-5185e014d081", "Consolidator"));
						}
						break;
					case DocAddressTypes.Codes.PlaceOfConsolidation:
						if (!Parent.OrganisationPK.IsEmpty && Parent.Organisation != null)
						{
							CAAddressValidator.ValidateMandatoryCityNameOnly(Parent, Parent.E2_OA_AddressInfo, Res.GetString("125359ee-7e57-4bea-86cc-839747c50d15", "Place of Consolidation"));
						}
						break;
				}
			}
		}
	}
}
