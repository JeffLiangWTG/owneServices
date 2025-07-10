using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class ExportCustomsManifestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.ExportCustomsManifest; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.Customs.AU.ExportCustomsManifest;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ExportCustomsManifestHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ExportCustomsManifestHeader header = businessEntity as ExportCustomsManifestHeader;
			ZForm result = header == null ? null : new ExportManifestForm(header);
			return result;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (businessObjectToReturn != null)
			{
				IBusiness result = businessObjectToReturn;
				businessObjectToReturn = null;
				return result;
			}
			else
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}

		public void SetNewBusinessObjectToReturn(IBusiness businessObject)
		{
			businessObjectToReturn = businessObject;
		}

		protected IBusiness businessObjectToReturn;

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ExportManifestOnShipping; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ExportManifestOnShippingEdit; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ExportManifestOnShippingNew; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ExportManifestOnShippingDelete; }
		}
	}
}
