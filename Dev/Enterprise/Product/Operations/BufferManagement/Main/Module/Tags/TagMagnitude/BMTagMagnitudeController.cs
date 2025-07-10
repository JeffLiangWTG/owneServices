using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMTagMagnitudeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMTagMagnitude; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMTagMagnitude; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TagMagnitude); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var magnitude = businessEntity as TagMagnitude
				?? throw new ModuleGuiNotSupportedException($"Trying to open {nameof(TagDefinitionForm)} for a business object which is not a tag. Object type: {businessEntity.GetType()}.");

			var definition = magnitude.Definition
				?? throw new ModuleGuiNotSupportedException($"We failed to find the Tag Group for this Tag Magnitude. Tag group PK: {magnitude.TGM_TGD_Tag}.");

			return new TagDefinitionForm(definition);
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("2A7F9319-9C85-40EF-A238-5E6765D44DAC", "New tags cannot be created here. Please use the Tag Groups module to create new tags."),
				Res.GetString("5AAB6F54-2076-46DE-B854-54F2B008B657", "Unsupported operation"),
				MessageBoxButtons.OK, DialogResult.OK);
			return null;
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
