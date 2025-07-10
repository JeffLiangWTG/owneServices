using CargoWise.EntityFramework;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaTransferHeaderValidation : AutoAsycudaTransferHeaderValidation
	{
		public AsycudaTransferHeaderValidation(AutoAsycudaTransferHeader parent) : base(parent)
		{
		}

		protected new AsycudaTransferHeader Parent => (AsycudaTransferHeader)base.Parent;

		protected override void CheckATF_TransferType()
		{
			base.CheckATF_TransferType();
			MandatoryValidation.CheckEntered(Parent.ATF_TransferTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ATF_TransferTypeInfo);
		}
	}
}
