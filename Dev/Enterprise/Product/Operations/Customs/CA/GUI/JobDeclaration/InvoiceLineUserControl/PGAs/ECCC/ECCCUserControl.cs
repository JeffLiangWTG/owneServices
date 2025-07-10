using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("ECCCPGAHeader")]
	public partial class ECCCUserControl : ZUserControl
	{
		readonly bool isOnInvoiceLine;
		readonly PGASubTabCollection subTabCollection;

		public ECCCUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;
			subTabCollection = new PGASubTabCollection(TabControl, null, AgencyCode, ProgramCodeList, CreateSubTabUserControl);
		}

		ZUserControl CreateSubTabUserControl(string code)
		{
			switch (code)
			{
				case (ECCCPGADepartmentCodes.Codes.ODS):
					return new ECCCOzoneUserControl(isOnInvoiceLine);
				case (ECCCPGADepartmentCodes.Codes.WEN):
					return new ECCCWildlifeUserControl(isOnInvoiceLine);
				case (ECCCPGADepartmentCodes.Codes.WRM):
					return new ECCCWasteUserControl(isOnInvoiceLine);
				case (ECCCPGADepartmentCodes.Codes.VEE):
					return new ECCCVehicleUserControl(isOnInvoiceLine);
				default:
					return new ZUserControl();
			}
		}

		ZArchitecture.Core.CodeDescriptionPairList ProgramCodeList => new ECCCPGADepartmentCodes();

		ZString AgencyCode => PGACodes.Codes.ECCC;

		public new ECCCPGAHeader CurrentDataItem => base.CurrentDataItem as ECCCPGAHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			subTabCollection.Update(CurrentDataItem);
		}
	}
}
