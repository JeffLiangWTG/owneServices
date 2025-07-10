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
	public class BMTagRuleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMTagRule; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMTagRule; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TagRule); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TagRuleForm((TagRule)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TagRuleView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TagRuleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TagRuleNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TagRuleDelete; }
		}

		#endregion
	}
}
