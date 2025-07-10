using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ClaimChargesController : UACreditNoteController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ClaimCharges; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Accounting.Module.Res.GetData("PlugInTabPage|ClaimCharges", "Claim Charges", "The Claim Charges tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ClaimChargesPlugin(businessEntity);
		}
	}
}
