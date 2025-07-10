using System;
using CargoWise.EntityFramework;
using Enterprise.Client.AUS.Business;
using Enterprise.Client.AUS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.AUS.Modules
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("It is used by reflection in ClientOverride.cs")]
	public class OriginPreferenceMappingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.OriginPreferenceMapping; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ClientAUSOriginPreferenceMapping); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ClientAUSOriginPreferenceMappingForm((ClientAUSOriginPreferenceMapping)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
