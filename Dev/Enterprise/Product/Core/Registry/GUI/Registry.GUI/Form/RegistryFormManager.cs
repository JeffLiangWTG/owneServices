using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.GUI
{
	class RegistryFormManager : NonPersistentBusinessObject
	{
		#region Validation Error Message

		public void RemoveValidationErrorMessage()
		{
			SetValidationErrorMessage(null);
		}

		public void SetValidationErrorMessage(string errorMessage)
		{
			fValidationErrorMessage = errorMessage;
			Validation.ValidateOverrideDefault();
		}

		public string ValidationErrorMessage
		{
			get { return fValidationErrorMessage; }
			set { fValidationErrorMessage = value; }
		}

		string fValidationErrorMessage;

		#endregion

		#region Bound Properties

		#region Override Default

		public ZBool OverrideDefault
		{
			get { return overrideDefault; }
			set { SetNonPersistentPropertyValue<ZBool>(OverrideDefaultInfo, ref overrideDefault, value); }
		}

		public ZPropertyInfo OverrideDefaultInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideDefault)); }
		}

		ZBool overrideDefault;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public RegistryFormManagerValidation Validation
		{
			get { return GetNewValidation(); }
		}

		RegistryFormManagerValidation GetNewValidation()
		{
			return new RegistryFormManagerValidation(this);
		}

		#endregion
	}
}
