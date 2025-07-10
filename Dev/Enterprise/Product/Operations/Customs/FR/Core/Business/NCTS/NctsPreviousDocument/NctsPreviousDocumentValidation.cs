using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPreviousDocumentValidation : EU.NCTS.Business.NctsPreviousDocumentPhase4Validation
	{
		public NctsPreviousDocumentValidation(NctsPreviousDocument parent)
			: base(parent)
		{ }

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			if (Parent.CSI_Code == PreviousDocumentCodeList.Codes._337 && !Parent.CSI_ReferenceNumber.IsEmpty)
			{
				var header = ComplementaryJobISTFinder.FindFromReferenceNumber(Parent.Factory, Parent.CSI_ReferenceNumber, Parent.Declaration?.CountryCode ?? Core.Constants.CountryCodes.France);
				if (header is null)
				{
					Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("152B39FE-173B-4911-A9FF-391CF417AA18", "Could not find matching temporary storage register using reference {0}.", Parent.CSI_ReferenceNumber));
				}
			}
		}

		protected override void CheckCSI_SubType()
		{
		}
	}
}
