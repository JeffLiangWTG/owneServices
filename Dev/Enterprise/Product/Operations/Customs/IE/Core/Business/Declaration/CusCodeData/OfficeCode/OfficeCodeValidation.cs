using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class OfficeCodeValidation : EuOfficeCodeValidation
	{
		public OfficeCodeValidation(OfficeCode parent) : base(parent)
		{
		}

		#region CY_Code
		protected override void CheckIfOfficeTypeEmptyOrInvalid()
		{
			var targetInfo = Parent.CY_CodeInfo;
			if (Parent.CY_Code.IsEmpty)
			{
				targetInfo.AddError(MandatoryValidation.MustBeEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(targetInfo)));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.CY_CodeInfo);
			}
		}

		protected override INotificationType OfficeTypeRepeatedMoreThanMaxNotificationType => NotificationType.Error;

		#endregion

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			if (Parent.Parent is JobDeclaration declaration && declaration.IsAmendmentValidationMode && WasDataChanged(declaration, Parent.CY_Code.ToUpperInvariant()))
			{
				Parent.CY_DataInfo.AddMessageError(CommonResStrings.ShouldNotAmendThisValue);
			}
		}

		protected bool IsOfficeCodeChanged(ZString officeCode) => !officeCode.IsEmpty && !Parent.CY_Data.EqualsIgnoringCase(officeCode);
		protected virtual bool WasDataChanged(JobDeclaration declaration, ZString code) => false;

		protected new OfficeCode Parent => (OfficeCode)base.Parent;
	}
}
