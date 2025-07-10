using CargoWise.EntityFramework;
using CargoWise.Types;
using RepresentationTypeList = Enterprise.Customs.EU.Business.RepresentationTypeList;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconDeclarationValidation : Customs.Business.CusReconDeclarationValidation
	{
		public CusReconDeclarationValidation(CusReconDeclaration parent) : base(parent)
		{
		}

		protected override void CheckCRD_CustomsOffice()
		{
			base.CheckCRD_CustomsOffice();
			var parent = Parent;
			if (parent.CRD_CustomsOffice.IsEmpty)
			{
				parent.CRD_CustomsOfficeInfo.AddMessageError(MandatoryValidation.MustBeEnteredMessage(parent.CRD_CPH_ReconClearanceAuthorisationInfo.HumanReadableName));
			}
		}

		protected override void CheckCRD_PeriodFrom()
		{
			base.CheckCRD_PeriodFrom();
			var parent = Parent;
			var targetInfo = parent.CRD_PeriodFromInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (parent.CRD_PeriodFrom > ZDate.Today)
			{
				targetInfo.AddMessageError(Res.GetString("1953BE59-6C87-4428-A068-070DD088EFCA", "The Period can't be in the future."));
			}
		}

		protected override void CheckCRD_PeriodTo()
		{
			base.CheckCRD_PeriodTo();
			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.CRD_PeriodToInfo);

			if (parent.CRD_PeriodTo < parent.CRD_PeriodFrom)
			{
				parent.CRD_PeriodToInfo.AddMessageError(Res.GetString("38BB987E-C452-49BA-812E-DA1C7AF44E6E", "Period To cannot be before Period From."));
			}
		}

		protected override void CheckCRD_CPH_ReconClearanceAuthorisation()
		{
			base.CheckCRD_CPH_ReconClearanceAuthorisation();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_CPH_ReconClearanceAuthorisationInfo);
		}

		protected override void CheckCRD_DeclarationType()
		{
			base.CheckCRD_DeclarationType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_DeclarationTypeInfo);
		}

		protected override void CheckCRD_DeclarantType()
		{
			base.CheckCRD_DeclarantType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_DeclarantTypeInfo);
		}

		protected override void CheckCRD_OA_DeclarantAddress()
		{
			base.CheckCRD_OA_DeclarantAddress();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CRD_OA_DeclarantAddressInfo);
		}

		protected override void CheckCRD_OA_RepresentativeAddress()
		{
			base.CheckCRD_OA_RepresentativeAddress();
			var parent = Parent;
			if (parent.CRD_DeclarantType == RepresentationTypeList.Codes._2Direct)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CRD_OA_RepresentativeAddressInfo);
			}
		}

		protected override void CheckCRD_OA_BuyingAgentAddress()
		{
			base.CheckCRD_OA_BuyingAgentAddress();
			var parent = Parent;
			if (parent.CRD_DeclarantType == RepresentationTypeList.Codes._3Indirect)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CRD_OA_BuyingAgentAddressInfo);
			}
		}
	}
}
