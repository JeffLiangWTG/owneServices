using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("NRCanPGAHeader")]
	public partial class NRCanUserControl : ZUserControl
	{
		readonly bool isOnInvoiceLine;
		readonly PGASubTabCollection subTabCollection;

		public NRCanUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;
			subTabCollection = new PGASubTabCollection(TabControl, null, AgencyCode, ProgramCodeList, CreateSubTabUserControl);
		}

		ZUserControl CreateSubTabUserControl(string code)
		{
			switch (code)
			{
				case NRCanPGADepartmentCodes.Codes.EEF:
					return new NRCanEEFUserControl(isOnInvoiceLine);
				case NRCanPGADepartmentCodes.Codes.EXP:
					return new NRCanEXPUserControl(isOnInvoiceLine);
				case NRCanPGADepartmentCodes.Codes.RDA:
					return new NRCanRDAUserControl(isOnInvoiceLine);
				default:
					return new ZUserControl();
			}
		}

		ZArchitecture.Core.CodeDescriptionPairList ProgramCodeList => new NRCanPGADepartmentCodes();

		ZString AgencyCode => PGACodes.Codes.NRCan;

		public new NRCanPGAHeader CurrentDataItem => base.CurrentDataItem as NRCanPGAHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			subTabCollection.Update(CurrentDataItem);
		}
	}
}
