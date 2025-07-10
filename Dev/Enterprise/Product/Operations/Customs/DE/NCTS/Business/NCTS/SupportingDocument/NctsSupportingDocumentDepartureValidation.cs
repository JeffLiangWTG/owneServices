using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsSupportingDocumentDepartureValidation : NctsSupportingDocumentPhase5DepartureValidation
	{
		public NctsSupportingDocumentDepartureValidation(NctsSupportingDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			PropertyIsMandatoryWhenHasAttributeWithValueY(Parent.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			PropertyIsMandatoryWhenHasAttributeWithValueY(Parent.CSI_ReferenceNumber2Info, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			UniversalValidationHelper.CheckMaxLength(Parent.CSI_DescriptionInfo, 26);
		}

		protected override ZString ItemNumberPropertyDescription => Res.GetString("4E3B6D60-6480-40E2-A2D1-511A9168884C", "Item Number (1-99999)");

		void PropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			if (DE.Business.CusSupportingInfoHelper.HasAttributeForMandatoryValidation(Parent.RefCusCode, attributeName))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;
	}
}
