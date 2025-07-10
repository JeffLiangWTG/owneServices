using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOpportunityValueAnalysisController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ClientIntelligenceModifyTradeProfile; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ClientIntelligenceViewTradeProfile; }
		}

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new EDIOpportunityValueAnalysisPlugIn((EDIOrgOpportunity)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption { get { return ZClientEDI.Res.GetData("PlugInTabPage|EDIOpportunityValueAnalysis", "Value Analysis"); } }

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.EDIOpportunityValueAnalysis; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIOrgOpportunity); }
		}
	}
}
