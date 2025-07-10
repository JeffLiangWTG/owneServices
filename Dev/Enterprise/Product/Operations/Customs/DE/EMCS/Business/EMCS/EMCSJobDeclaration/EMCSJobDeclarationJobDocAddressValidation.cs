using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSJobDeclarationJobDocAddressValidation
		: EU.EMCS.Business.EMCSJobDeclarationJobDocAddressValidation
	{
		public EMCSJobDeclarationJobDocAddressValidation(AutoJobDocAddress parent, EMCSJobDeclaration declaration)
			: base(parent, declaration)
		{
		}

		protected override ZBool IsDestinationWarehouseMandatory
		{
			get
			{
				var destinationType = Declaration.JE_MessageSubType;
				return destinationType == EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationTaxWarehouse
						|| destinationType == EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationDirectDelivery;
			}
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (!Parent.E2_AddressOverride
				&& Parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress
				&& Declaration.IsConsolidatedDocument())
			{
				if (!Parent.Organisation.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, DE.Business.CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }))
				{
					Parent.OrganisationPKInfo.AddMessageError(Res.GetString("5d19786d-1d4a-409b-af35-892873ec2894", "For a consolidated e-AD the Consignor needs an EIR Authorization with a usage (USE) rule for an accredited exporter (AEX)."));
				}
			}
		}

		new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;
	}
}
