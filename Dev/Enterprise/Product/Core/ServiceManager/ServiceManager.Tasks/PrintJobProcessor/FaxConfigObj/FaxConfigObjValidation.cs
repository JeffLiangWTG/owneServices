using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxConfigObjValidation : AutoFaxConfigObjValidation
	{
		public FaxConfigObjValidation(AutoFaxConfigObj parent)
			: base(parent) { }

		protected override void CheckLocalCountry()
		{
			base.CheckLocalCountry();

			MandatoryValidation.CheckEntered(Parent.LocalCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LocalCountryInfo);
		}

		#region Implementation

		public new FaxConfigObj Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (FaxConfigObj)base.Parent; }
		}

		#endregion
	}
}
