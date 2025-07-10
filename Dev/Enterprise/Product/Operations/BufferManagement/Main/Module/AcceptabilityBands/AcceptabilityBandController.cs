using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class AcceptabilityBandController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AcceptabilityBand; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AcceptabilityBand; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMComponentAcceptabilityBand); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AcceptabilityBandForm((BMComponentAcceptabilityBand)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AcceptabilityBand; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AcceptabilityBandEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AcceptabilityBandNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AcceptabilityBandDelete; }
		}

		#endregion
	}
}
