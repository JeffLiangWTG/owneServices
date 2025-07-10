using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("TCPGAHeader")]
	public partial class TCUserControl : ZUserControl
	{
		readonly bool isOnInvoiceLine;
		readonly PGASubTabCollection subTabCollection;

		public TCUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;
			subTabCollection = new PGASubTabCollection(TabControl, null, AgencyCode, ProgramCodeList, CreateSubTabUserControl);
		}

		ZUserControl CreateSubTabUserControl(string code)
		{
			switch (code)
			{
				case TCPGADepartmentCodes.Codes.TPR:
					return new TCTPRUserControl(isOnInvoiceLine);
				case TCPGADepartmentCodes.Codes.VPR:
					return new TCVPRUserControl(isOnInvoiceLine);
				default:
					return new ZUserControl();
			}
		}

		ZArchitecture.Core.CodeDescriptionPairList ProgramCodeList => new TCPGADepartmentCodes();

		ZString AgencyCode => PGACodes.Codes.TC;

		public new TCPGAHeader CurrentDataItem => base.CurrentDataItem as TCPGAHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			subTabCollection.Update(CurrentDataItem);
		}
	}
}
