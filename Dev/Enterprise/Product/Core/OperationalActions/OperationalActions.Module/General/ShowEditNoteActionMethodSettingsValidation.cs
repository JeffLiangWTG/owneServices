using System;

using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Module
{
	public class ShowEditNoteActionMethodSettingsValidation : ZValidation
	{
		public ShowEditNoteActionMethodSettingsValidation(ShowEditNoteActionMethodSettings parent) : base(parent)
		{
			this.parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateNoteDescription();
		}

		#region ValidateNoteDescription

		public void ValidateNoteDescription()
		{
			((IValidationInternals)this).Validate(Parent.NoteDescriptionInfo, CheckNoteDescription);
		}

		public void CheckNoteDescription()
		{
			MandatoryValidation.CheckEntered(Parent.NoteDescriptionInfo);
		}

		#endregion

		#region Implementaiont

		public override Type AutoValidationType
		{
			get { return typeof(ShowEditNoteActionMethodSettingsValidation); }
		}

		public ShowEditNoteActionMethodSettings Parent
		{
			get { return parent; }
		}
		readonly ShowEditNoteActionMethodSettings parent;

		#endregion
	}
}
