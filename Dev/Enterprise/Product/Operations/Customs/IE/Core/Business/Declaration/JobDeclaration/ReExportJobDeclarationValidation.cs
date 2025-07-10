using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ReExportJobDeclarationValidation : CommonExportJobDeclarationValidation
	{
		public ReExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_ContainerMode_Mandatory()
		{
			if (Parent.JE_ContainerMode.IsEmpty)
			{
				Parent.JE_ContainerModeInfo.AddMessageError(Res.GetString("{4A7509F3-70B4-4EBA-9A42-1F749FDC0327}", "Container Indicator field is mandatory"));
			}
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_LocationOtherInformationInfo);
		}

		protected override void CheckJE_LocationQualifier()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_LocationQualifierInfo);
		}

		protected override void CheckJE_LocationOfGoods()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_LocationOfGoodsInfo);
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			if (Parent.JE_OH_ShippingLine.IsEmpty)
			{
				MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.JE_OH_ShippingLineInfo);
			}
			else
			{
				CheckCarrierHasEORI();
			}
		}

		protected override void CheckJE_TransportMeans()
		{
			var info = Parent.JE_TransportMeansInfo;
			MandatoryValidation.MessageErrorIfIsEntered(info);
		}
	}
}
