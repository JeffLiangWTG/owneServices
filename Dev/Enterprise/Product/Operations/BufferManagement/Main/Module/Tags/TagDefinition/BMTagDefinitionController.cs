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
	public class BMTagDefinitionController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMTagDefinition; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMTagDefinition; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TagDefinition); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TagDefinitionForm((TagDefinition)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TagDefinitionView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TagDefinitionEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TagDefinitionNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TagDefinitionDelete; }
		}

		#endregion
	}
}
