using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public class ProductSupportingInfoValidation : CusSupportingInfoValidation
	{
		public ProductSupportingInfoValidation(ProductSupportingInfo parent)
			: base(parent)
		{
		}

		protected new ProductSupportingInfo Parent => (ProductSupportingInfo)base.Parent;

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			var entryInstruction = Parent.Parent as CusEntryInstruction;

			if (entryInstruction != null && entryInstruction.EnabledOutwardProcessing)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}

		protected override void CheckCSI_Tariff()
		{
			base.CheckCSI_Tariff();

			var entryInstruction = Parent.Parent as CusEntryInstruction;

			if (entryInstruction != null && entryInstruction.EnabledOutwardProcessing)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_TariffInfo);
			}
		}
	}
}
