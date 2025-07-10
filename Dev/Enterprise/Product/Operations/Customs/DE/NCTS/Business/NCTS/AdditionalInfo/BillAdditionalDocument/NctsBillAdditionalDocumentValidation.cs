using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsBillAdditionalDocumentValidation : EU.NCTS.Business.NctsBillAdditionalDocumentValidation
	{
		public NctsBillAdditionalDocumentValidation(NctsBillAdditionalDocument parent) : base(parent)
		{
		}

		new NctsBillAdditionalDocument Parent => (NctsBillAdditionalDocument)base.Parent;

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			if (Parent.IsNotificationToCustomsOffice())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}
	}
}
