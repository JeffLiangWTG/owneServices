using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CMRCodeListsController : CMRSearchOnlyController
	{
		public CMRCodeListsController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.CMRCodeLists; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CMRCodeLists); }
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return null;
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
