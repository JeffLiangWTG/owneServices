using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.GUI
{
	public class OrganisationsLayoutBuilder : CommonOrganisationsLayoutBuilder<JobDeclaration>
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(OrganisationsControlBag.Instance.SupplierAddressControl, t => t.IsExport);
			SetVisibility(OrganisationsControlBag.Instance.ImporterAddressControl, t => t.IsImport);
			SetVisibility(OrganisationsControlBag.Instance.PayerGuidFindBox, t => t.IsImport);
			SetVisibility(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, t => t.IsExport);
			SetVisibility(OrganisationsControlBag.Instance.IndustrialParkCodeCodeFindBox, t => t.IsExport);
			SetVisibility(OrganisationsControlBag.Instance.ExpoterAddressControl, t => t.IsExport);
			SetVisibility(OrganisationsControlBag.Instance.FinalBondedWarehouseCodeFindBox, t => t.IsExport);
			SetVisibility(OrganisationsControlBag.Instance.ExporterGuidFindBox, t => t.IsLocalExport);
			SetVisibility(OrganisationsControlBag.Instance.ManufacturerGuidFindBox, t => t.IsLocalExport);
			SetVisibility(OrganisationsControlBag.Instance.StevedoreAddressControl, t => t.IsLocalExportToSeaVessel);
			SetVisibility(OrganisationsControlBag.Instance.AuthorGroupBox, t => t.IsImport);
			SetVisibility(OrganisationsControlBag.Instance.ResponsiblePersonGroupBox, t => t.IsImport);
		}
	}
}
