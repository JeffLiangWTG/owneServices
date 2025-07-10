using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;

namespace Enterprise.Services.OperationalActions.Module
{
	public class RunProgramActionMethodSettingsValidation : ZValidation
	{
		public RunProgramActionMethodSettingsValidation(RunProgramActionMethodSettings parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override void ValidateAll()
		{
			ValidatePath();
			ValidateArguments();
		}

		#region ValidatePath

		public void ValidatePath()
		{
			((IValidationInternals)this).Validate(Parent.PathInfo, CheckPath);
		}

		public void CheckPath()
		{
			MandatoryValidation.CheckEntered(Parent.PathInfo);
			if (!PathValidation.IsValid(Parent.Path) || !Path.IsPathRooted(Parent.Path))
			{
				Parent.PathInfo.AddError(Res.GetString("0a9e9aaa-3109-4065-ab4f-c805e386df2d", "Please enter a valid absolute program path."));
			}
		}

		#endregion

		#region ValidateArguments

		public void ValidateArguments()
		{
			((IValidationInternals)this).Validate(Parent.ArgumentsInfo, CheckArguments);
		}

		public void CheckArguments()
		{
			//Add Arguments validation here if required.
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(RunProgramActionMethodSettingsValidation); }
		}

		public RunProgramActionMethodSettings Parent
		{
			get { return parent; }
		}
		readonly RunProgramActionMethodSettings parent;

		#endregion
	}
}
