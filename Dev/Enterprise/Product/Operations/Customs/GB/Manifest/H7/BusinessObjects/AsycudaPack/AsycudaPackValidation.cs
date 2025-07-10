using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaPackValidation : EU.H7.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(ASYCUDA.Business.AsycudaPack parent) : base(parent)
		{
		}

		new AsycudaPack Parent => base.Parent as AsycudaPack;

		AsycudaManifestHeader Header => Parent.Bill?.Header as AsycudaManifestHeader;

		bool IsForBIRDS => Header?.IsForBIRDS ?? false;

		protected override void CheckAPA_GoodsDescription()
		{
			if (Parent.APA_GoodsDescription.Trim().Length > 512)
			{
				Parent.APA_GoodsDescriptionInfo.AddMessageError(Res.GetString("E31093E1-1876-4299-895E-83D28041C457", "Goods Description has exceeded the max length of 512."));
			}
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			if (IsForBIRDS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackUQInfo);
			}
		}

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();
			if (IsForBIRDS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_MarksAndNumbersInfo);
			}
		}
	}
}
