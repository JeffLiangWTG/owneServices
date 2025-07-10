using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("HCPGAHeader")]
	public partial class HCUserControl : ZUserControl
	{
		readonly bool isOnInvoiceLine;
		readonly PGASubTabCollection subTabCollection;

		public HCUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;
			subTabCollection = new PGASubTabCollection(TabControl, null, AgencyCode, ProgramCodeList, CreateSubTabUserControl);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ZUserControl CreateSubTabUserControl(string code)
		{
			switch (code)
			{
				case HCPGADepartmentCodes.Codes.API:
					return new ActivePHIngredientsUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.BBC:
					return new BloodComponentUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.CPR:
					return new ConsumerProductUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.CTO:
					return new CellsTissuesAndOrgansUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.DSE:
					return new DonorSemenUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.HDR:
					return new HumanDrugsUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.MDE:
					return new MedicalDevicesUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.NHP:
					return new NaturalHealthProductsUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.OCS:
					return new OfficeOfControlledSubstancesUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.PES:
					return new PesticideUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.RED:
					return new RadiationEmittingDevicesUserControl(isOnInvoiceLine);
				case HCPGADepartmentCodes.Codes.VET:
					return new VetDrugUserControl(isOnInvoiceLine);
				default:
					return new ZUserControl();
			}
		}

		ZArchitecture.Core.CodeDescriptionPairList ProgramCodeList => new HCPGADepartmentCodes();

		ZString AgencyCode => PGACodes.Codes.HC;

		public new HCPGAHeader CurrentDataItem => base.CurrentDataItem as HCPGAHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			subTabCollection.Update(CurrentDataItem);
		}
	}
}
