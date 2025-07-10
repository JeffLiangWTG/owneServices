using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class InwardProcessingProductValidation : CusSupportingInfoValidation
	{
		public InwardProcessingProductValidation(InwardProcessingProduct parent) : base(parent)
		{
		}

		protected new InwardProcessingProduct Parent => (InwardProcessingProduct)base.Parent;

		protected override void CheckCSI_Tariff()
		{
			base.CheckCSI_Tariff();
			var instruction = Parent.Parent?.EntryInstruction;
			if (instruction != null && instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
			{
				var tariff = Parent.CSI_Tariff;
				if (tariff.Length != 8 || !tariff.IsNumbersOnlyOrEmpty)
				{
					Parent.CSI_TariffInfo.AddMessageError(Res.GetString("C06BD677-E6EB-4413-A0C3-07D13393007B", "The CN Code must have 8 digits."));
				}
				else if (new TariffView.Loader(Parent.Factory).LoadMostRecentCachedTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariff, ZDateTime.Today) == null)
				{
					Parent.CSI_TariffInfo.AddMessageError(Res.GetString("3BAB517A-A498-4F08-A93D-7B9AEF324D81", "The entered CN Code is not valid."));
				}
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (!Parent.FormattedTariff.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			if (!Parent.FormattedTariff.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
			}
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			base.CheckCSI_AdditionalDescription();
			if (!Parent.FormattedTariff.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_AdditionalDescriptionInfo);
			}
		}
	}
}
