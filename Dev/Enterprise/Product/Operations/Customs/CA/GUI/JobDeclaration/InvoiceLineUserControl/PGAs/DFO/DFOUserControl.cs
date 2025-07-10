using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("DFOPGAHeader")]
	public partial class DFOUserControl : ZUserControl
	{
		readonly bool isOnInvoiceLine;
		readonly PGASubTabCollection subTabCollection;

		public DFOUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();

			this.isOnInvoiceLine = isOnInvoiceLine;
			subTabCollection = new PGASubTabCollection(TabControl, null, AgencyCode, ProgramCodeList, CreateSubTabUserControl);
		}

		ZUserControl CreateSubTabUserControl(string code)
		{
			switch (code)
			{
				case DFOPGADepartmentCodes.Codes.ABI:
					return new ABIProgramUserControl(isOnInvoiceLine);
				case DFOPGADepartmentCodes.Codes.AIS:
					return new AISProgramUserControl(isOnInvoiceLine);
				case DFOPGADepartmentCodes.Codes.TTP:
					return new TTPProgramUserControl(isOnInvoiceLine);

				default:
					return new ZUserControl();
			}
		}

		ZArchitecture.Core.CodeDescriptionPairList ProgramCodeList => new DFOPGADepartmentCodes();

		ZString AgencyCode => PGACodes.Codes.DFO;

		public new DFOPGAHeader CurrentDataItem => base.CurrentDataItem as DFOPGAHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			subTabCollection.Update(CurrentDataItem);
		}
	}
}
