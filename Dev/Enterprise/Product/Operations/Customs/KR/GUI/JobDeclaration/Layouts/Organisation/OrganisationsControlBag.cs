using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class OrganisationsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new OrganisationsUserControl();

		[ThreadStatic]
		static OrganisationsControlBag instance;

		public static OrganisationsControlBag Instance => instance ?? (instance = new OrganisationsControlBag());

		OrganisationsControlBag()
		{
			SupplierAddressControl = RegisterControl(nameof(OrganisationsUserControl.SupplierAddressControl));
			ImporterAddressControl = RegisterControl(nameof(OrganisationsUserControl.ImporterAddressControl));
			PayerGuidFindBox = RegisterControl(nameof(OrganisationsUserControl.PayerGuidFindBox));
			IndustrialParkCodeCodeFindBox = RegisterControl(nameof(OrganisationsUserControl.IndustrialParkCodeCodeFindBox));
			ExpoterAddressControl = RegisterControl(nameof(OrganisationsUserControl.ExpoterAddressControl));
			FinalBondedWarehouseCodeFindBox = RegisterControl(nameof(OrganisationsUserControl.FinalBondedWarehouseCodeFindBox));
			ManufacturerGuidFindBox = RegisterControl(nameof(OrganisationsUserControl.ManufacturerGuidFindBox));
			ExporterGuidFindBox = RegisterControl(nameof(OrganisationsUserControl.ExporterGuidFindBox));
			StevedoreAddressControl = RegisterControl(nameof(OrganisationsUserControl.StevedoreAddressControl));
			AuthorGroupBox = RegisterControl(nameof(OrganisationsUserControl.AuthorGroupBox));
			ResponsiblePersonGroupBox = RegisterControl(nameof(OrganisationsUserControl.ResponsiblePersonGroupBox));
		}

		public ControlReference SupplierAddressControl { get; }
		public ControlReference ImporterAddressControl { get; }
		public ControlReference PayerGuidFindBox { get; }
		public ControlReference IndustrialParkCodeCodeFindBox { get; }
		public ControlReference ExpoterAddressControl { get; }
		public ControlReference FinalBondedWarehouseCodeFindBox { get; }
		public ControlReference ManufacturerGuidFindBox { get; }
		public ControlReference ExporterGuidFindBox { get; }
		public ControlReference StevedoreAddressControl { get; }
		public ControlReference AuthorGroupBox { get; }
		public ControlReference ResponsiblePersonGroupBox { get; }
	}
}
