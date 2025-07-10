using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukMasterAndHouseCombinedFilterStripControl : CcsukAirInventoryHouseFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public CcsukMasterAndHouseCombinedFilterStripControl()
		{
			InitializeComponent();
		}

		public CcsukMasterAndHouseCombinedFilterStripControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			referenceNumberColumn.IsVisible = true;
			referenceNumberColumn.IsMandatory = true;
			grid.ColumnStyles.Remove(referenceNumberColumn.ColumnName);
			grid.ColumnStyles.Add(referenceNumberColumn);

			var srfDetails = new ZMultiLineTextBoxColumnInfo();
			srfDetails.ColumnName = CusHAWB.Schema.SplitReferencesForAllAwbsModuleGrid;
			srfDetails.CaptionResourceString = Res.GetData("66130AEF-6681-4C71-ACDC-0D3F2378C473", "Split Details");
			grid.ColumnStyles.Add(srfDetails);

			shipmentLinkedCheckBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("2CE6D3C1-F0DA-4E8A-A45A-305372125E2E", "Forwarding Linked?");
			shipmentLinkedCheckBoxColumnStyleInfo3.ColumnName = CusHAWB.Schema.IsLinkedToForwardingJob;
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusHAWB)(null)).IsLinkedToForwardingJob);
		}
	}
}
