using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class OrganizationControlBag : ControlBag
	{
		OrganizationControlBag()
		{
			ForeignCarrierGuidFindBox = RegisterControl(nameof(OrganizationControlBag.ForeignCarrierGuidFindBox));
			DomesticCarrierGuidFindBox = RegisterControl(nameof(OrganizationControlBag.DomesticCarrierGuidFindBox));
		}

		public static OrganizationControlBag Instance => instance ?? (instance = new OrganizationControlBag());

		[ThreadStatic]
		static OrganizationControlBag instance;

		public ControlReference ForeignCarrierGuidFindBox { get; }
		public ControlReference DomesticCarrierGuidFindBox { get; }

		protected override Control CreateTemplate() => new OrgUserControl();
	}
}
