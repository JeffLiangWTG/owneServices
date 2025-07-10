using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using OrgSupplierPartCollection = Enterprise.Customs.Business.OrgSupplierPartCollection;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public EMCSJobComInvoiceLineLookups(EMCSJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new EMCSJobComInvoiceLine Parent => (EMCSJobComInvoiceLine)base.Parent;

		public ZZRefCusCodeListCombinedCollection CNCodeList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Parent.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, ZDateTime.Today);

		public override CodeDescriptionPairList InvoiceUQList => Factory.GetEMCSPackTypeList(Parent.GetDefaultDataGroupingCode());

		public override CodeDescriptionPairList CustomsUQList => Factory.GetCachedValue<EMCSCustomsQuantityTypeList>();

		public override CodeDescriptionPairList WeightUQList
			=> RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Parent.Declaration.DateOfValuation);

		protected override void SetOrAdjustPartsListProperties(OrgSupplierPartCollection partsList)
		{
			var declaration = Parent.Declaration;

			if (partsList != null && declaration != null)
			{
				partsList.FilterBusinessObjectDefaults.RemoveAll();

				if (!InvoiceLine.JI_PartNo.IsEmpty)
				{
					partsList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", InvoiceLine.JI_PartNo));
				}

				var ownerPk = declaration.OwnerDocumentaryAddress.OrganisationPK.IsEmpty
					? declaration.JE_OH_Importer
					: declaration.OwnerDocumentaryAddress.OrganisationPK;

				if (!ownerPk.IsEmpty)
				{
					partsList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", ownerPk));
				}

				var supplierPk = declaration.JE_OH_Supplier;

				if (!supplierPk.IsEmpty)
				{
					partsList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", supplierPk));
				}
			}
		}
	}
}
