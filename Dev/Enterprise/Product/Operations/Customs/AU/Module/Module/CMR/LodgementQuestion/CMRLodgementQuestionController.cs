using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CMRLodgementQuestionController : CMRSearchOnlyController
	{
		public CMRLodgementQuestionController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.CMRLodgementQuestion; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CMRLodgementQuestion); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|CMRLodgementQuestion", "Default CP Questions and Answers", "The Default CP Questions and Answers tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CPQAPlugIn((OrgHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
