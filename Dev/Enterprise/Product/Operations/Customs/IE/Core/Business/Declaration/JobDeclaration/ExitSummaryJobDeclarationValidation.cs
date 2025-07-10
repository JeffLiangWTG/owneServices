using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryJobDeclarationValidation : CommonExportJobDeclarationValidation
	{
		public ExitSummaryJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
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

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo);
			CheckNoAmendingOnEntryStatus(Parent.OriginalCustomsOffice, Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_EntryStyle()
		{
			base.CheckJE_EntryStyle();
			CheckNoAmendingOnEntryStatus(Parent.OriginalDeclarationType, Parent.JE_EntryStyleInfo);
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

		protected override void CheckJE_ContainerMode_Mandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ContainerModeInfo);
		}

		protected override void CheckJE_TransportMeans()
		{
			var info = Parent.JE_TransportMeansInfo;
			MandatoryValidation.MessageErrorIfIsEntered(info);
		}
	}
}
