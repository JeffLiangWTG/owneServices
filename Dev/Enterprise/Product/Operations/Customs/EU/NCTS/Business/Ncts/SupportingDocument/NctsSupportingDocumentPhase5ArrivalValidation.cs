using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsSupportingDocumentPhase5ArrivalValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public NctsSupportingDocumentPhase5ArrivalValidation(NctsSupportingDocument parent) : base(parent)
		{
		}

		protected new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;

		protected virtual ZString ItemNumberPropertyDescription => ZString.Empty;

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();
			ListValidation.ErrorIfInvalidCode(Parent.CSI_StatusInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			PropertyIsMandatoryWhenHasAttributeWithValueY(parent.CSI_ReferenceNumberInfo, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
			UniversalValidationHelper.CheckMaxLengthForPhase5AndTransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.CSI_ReferenceNumberInfo, 70, 35);
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			PropertyIsMandatoryWhenHasAttributeWithValueY(Parent.CSI_ItemNumberInfo, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber, ItemNumberPropertyDescription);
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			var parent = Parent;
			PropertyIsMandatoryWhenHasAttributeWithValueY(parent.CSI_ReferenceNumber2Info, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Complement);
			UniversalValidationHelper.CheckMaxLengthForPhase5AndTransitionPeriod(parent.IsInPhase5TransitionPeriod, parent.CSI_ReferenceNumber2Info, 35, 26);
		}

		protected override void CheckCodeCore()
		{
			var targetInfo = Parent.CSI_CodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName) => PropertyIsMandatoryWhenHasAttributeWithValueY(propertyInfo, attributeName, ZString.Empty);

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName, ZString propertyDescription)
		{
			if (Parent.RefCusCode.HasAttributeForMandatoryValidation(attributeName))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription);
			}
		}
	}
}
