using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class SimplifiedLVSJobDocAddressValidation : JobDocAddressValidation
	{
		public SimplifiedLVSJobDocAddressValidation(SimplifiedLVS simplifiedLVS, AutoJobDocAddress parent) : base(parent)
		{
			this.simplifiedLVS = simplifiedLVS;
		}
		readonly SimplifiedLVS simplifiedLVS;

		protected override void CheckOrganisationPK()
		{
			if (simplifiedLVS != null && simplifiedLVS.Invoices.Count == 0 && !simplifiedLVS.OnlyValidateCargoListHeaderProperty)
			{
				base.CheckOrganisationPK();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, Res.GetString("e956d8e5-9df1-4018-87fb-87be3c39504c", "Vendor"));
			}
		}
	}
}
