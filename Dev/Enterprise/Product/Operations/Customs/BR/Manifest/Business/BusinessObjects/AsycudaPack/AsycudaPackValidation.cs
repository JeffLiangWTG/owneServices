using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBulkType();
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();

			if (Parent.Bill.IsMercante)
			{
				if (Parent.APA_PackUQ.IsEmpty)
				{
					Parent.APA_PackUQInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.APA_PackUQInfo.HumanReadableName));
				}
				else
				{
					var countryCode = Parent.Bill.CountryCode;
					if (!countryCode.IsEmpty)
					{
						AsycudaUniversalReference.CusRefPackLoaderHelper.MessageErrorIfNeeded(countryCode, Parent.APA_PackUQ, Parent.APA_PackUQInfo, Parent.Factory);
					}
				}
			}
		}

		public void ValidateBulkType()
		{
			ValidateCalculatedProperty(Parent.BulkTypeInfo);
		}

		protected void CheckBulkType()
		{
			if (Parent.Bill.Header.AMA_ContainerMode == Core.Constants.ContainerModes.Bulk)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BulkTypeInfo,Res.GetString("E7E07A8C-0668-4550-BF84-8F1C1834E37F", "Bulk Type cannot be empty if Container Mode is Bulk"));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.BulkTypeInfo);
		}
	}
}
