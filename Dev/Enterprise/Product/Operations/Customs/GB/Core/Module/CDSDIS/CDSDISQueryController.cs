using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.GUI.CDSDIS;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CDSDISQueryController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.GB.CDSDISQueryController;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.GB.CDSDISQuery;

		public override Type TypeOfTopLevelBusinessObject => typeof(CDSDISQueryMessage);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsFiles;

		protected override IZForm GetForm(IBusiness businessEntity) => new CDSDISQueryForm((CDSDISQueryMessage)businessEntity);
	}
}
