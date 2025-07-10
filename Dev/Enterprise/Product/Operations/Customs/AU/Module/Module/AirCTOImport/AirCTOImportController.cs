using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOImportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AirCTOImportController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCTOImport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CTOCusHAWB); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			CTOCusMAWB parent = Factory.New<CTOCusMAWB>();
			return parent.ChildBills.AddNew();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)businessEntity;
			CTOCusMAWB mAWB = hAWB.MAWB;

			ZForm result = new AirCTOForm(mAWB);
			return result;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsAirCTOImport; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsAirCTOImportEdit; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsAirCTOImportNew; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsAirCTOImportDelete; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
