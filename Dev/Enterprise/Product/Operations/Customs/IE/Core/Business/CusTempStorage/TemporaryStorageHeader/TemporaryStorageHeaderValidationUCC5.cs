
namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageHeaderValidationUCC5 : EU.Business.CusTempStorage.TemporaryStorageHeaderValidation
	{
		public TemporaryStorageHeaderValidationUCC5(EU.Business.CusTempStorage.TemporaryStorageHeader parent) : base(parent)
		{
		}

		protected new void CheckPresentationCustomsOffice()
		{
		}

		protected override void CheckAMA_DateAtCustomsOffice()
		{
			var presentationDate = Parent.AMA_DateAtCustomsOffice;
			var presentationDateInfo = Parent.AMA_DateAtCustomsOfficeInfo;

			if (!presentationDate.IsEmpty && presentationDate.IsInTheFutureDatePartOnly)
			{
				presentationDateInfo.AddMessageError(Res.GetString("EE964BD8-3124-457B-B6D3-78EBB87374D3", "Goods Presentation Date can't be a future date."));
			}
		}

		protected override void CheckAMA_OA_Carrier()
		{
		}

		protected override void CheckDeclarantAndRepresentativeAreDifferent()
		{
		}

		protected override void CheckRepresentativeAndDeclarantAreDifferent()
		{
		}
	}
}
