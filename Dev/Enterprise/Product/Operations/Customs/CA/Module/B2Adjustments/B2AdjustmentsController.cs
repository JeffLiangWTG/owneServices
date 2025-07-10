using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.B2Adjustments; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.B2Adjustments; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CAB2AdjustmentsView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CAB2AdjustmentsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CAB2AdjustmentsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CAB2AdjustmentsEdit; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobDeclarationForm((JobDeclaration)businessEntity);
		}
	}
}
