using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class OrganisationsControlBag : ControlBag
	{
		OrganisationsControlBag()
		{
			DefermentPartyDocAddressControl = RegisterControl(nameof(DefermentPartyDocAddressControl));
			ExporterDocAddressControl = RegisterControl(nameof(ExporterDocAddressControl));
			ContractualPartnerDocAddressControl = RegisterControl(nameof(ContractualPartnerDocAddressControl));
			CarrierEUBorderDocAddressControl = RegisterControl(nameof(CarrierEUBorderDocAddressControl));
			DutyPayerGuidFindBox = RegisterControl(nameof(DutyPayerGuidFindBox));
		}

		public static OrganisationsControlBag Instance => instance ?? (instance = new OrganisationsControlBag());

		[ThreadStatic]
		static OrganisationsControlBag instance;

		protected override Control CreateTemplate() => new OrganisationsUserControl();

		public ControlReference DefermentPartyDocAddressControl { get; }
		public ControlReference ExporterDocAddressControl { get; }
		public ControlReference ContractualPartnerDocAddressControl { get; }
		public ControlReference CarrierEUBorderDocAddressControl { get; }
		public ControlReference DutyPayerGuidFindBox { get; }
	}
}
