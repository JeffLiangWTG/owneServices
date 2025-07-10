using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsAdditionalInfoValidation : EU.NCTS.Business.NctsAdditionalInfoPhase5Validation
	{
		public NctsAdditionalInfoValidation(NctsAdditionalInfo parent)
			: base(parent)
		{
		}

		new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_SubTypeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			PropertyIsMandatoryWhenHasAttributeWithValueY(Parent.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			var targetInfo = Parent.CSI_DescriptionInfo;
			if (Parent.IsNotificationToCustomsOffice())
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			else
			{
				PropertyIsMandatoryWhenHasAttributeWithValueY(targetInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
			}
		}

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			if (Parent.RefCusCode.HasAttributeForMandatoryValidation(attributeName))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}
	}
}
