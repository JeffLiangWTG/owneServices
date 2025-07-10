using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Customs.Universal.Module;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CACusRulingController : ZZRefCusRulingController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.CACusRuling;

		public override Type TypeOfTopLevelBusinessObject => typeof(CACusRuling);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCusRulingForm((CACusRuling)businessEntity);
		}
	}
}
