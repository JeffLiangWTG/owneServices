using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryInstructionValidation : AutoKRCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		protected override void CheckCEI_Style()
		{
		}

		protected override void CheckCEI_AgreedRateApp()
		{
			base.CheckCEI_AgreedRateApp();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_AgreedRateAppInfo, Parent.Lookups.AgreedRateList);
		}

		protected override void CheckCEI_PackQty()
		{
			base.CheckCEI_PackQty();

			if (Parent.CEI_PackQty < 0)
			{
				Parent.CEI_PackQtyInfo.AddMessageError(Res.GetString("C09BAACF-9F70-4863-BCEA-78E4184059B7", "Total Pack Qty cannot be negative."));
			}

			if (!Parent.JobDeclaration?.JE_TotalNoOfPacksPackType.IsEmpty ?? false)
			{
				if (!PackageKindCodeList.IsBulk(Parent.JobDeclaration.JE_TotalNoOfPacksPackType) && Parent.CEI_PackQty.IsEmpty)
				{
					Parent.CEI_PackQtyInfo.AddMessageError(Res.GetString("CA151B2C-4013-4822-9846-4B27B3E6F6A6", "Total Pack Qty must be greater than zero."));
				}
			}
			else
			{
				if (!Parent.CEI_PackQty.IsEmpty)
				{
					Parent.CEI_PackQtyInfo.AddMessageError(Res.GetString("F68FCB80-0870-410B-948E-1DEA2D6B6FB5", "Since the pack type is empty, Total Pack Qty should be 0."));
				}
			}
		}

		protected override void CheckCEI_RefundType()
		{
			base.CheckCEI_RefundType();
			if (IsRefundRequestValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_RefundTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_RefundTypeInfo, Parent.Lookups.RefundTypeList);
		}

		protected override void CheckCEI_RefundCauseCode()
		{
			base.CheckCEI_RefundCauseCode();
			if (IsRefundRequestValidationOn && Parent.CEI_RefundType == RefundTypeList.Codes.A)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_RefundCauseCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_RefundCauseCodeInfo, Parent.Lookups.RefundCauseCodeList);
		}

		protected override void CheckCEI_RefundReasonCode()
		{
			base.CheckCEI_RefundReasonCode();
			if (IsRefundRequestValidationOn && RefundCauseCodeList.IsRefundReasonCodeMandatory(Parent.CEI_RefundCauseCode))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_RefundReasonCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_RefundReasonCodeInfo, Parent.Lookups.RefundReasonCodeList);
		}

		protected override void CheckCEI_BondedFactoryUseCode()
		{
			base.CheckCEI_BondedFactoryUseCode();
			if (IsImport)
			{
				CheckBondedFactoryUsageFields(Parent.CEI_BondedFactoryUseCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CEI_BondedFactoryUseCodeInfo);
			}
		}

		protected override void CheckCEI_BondedFactoryArrivalDate()
		{
			base.CheckCEI_BondedFactoryArrivalDate();
			if (IsImport)
			{
				CheckBondedFactoryUsageFields(Parent.CEI_BondedFactoryArrivalDateInfo);
			}
		}

		void CheckBondedFactoryUsageFields(ZPropertyInfo propertyInfo)
		{
			if (DeclarationProcedureTypeCodeList.IsBondedFactoryProcedure(Declaration?.JE_ProcedureType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		protected override void CheckCEI_FTARelationArticleCode()
		{
			if (IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CEI_FTARelationArticleCodeInfo);
			}
			else
			{
				base.CheckCEI_FTARelationArticleCode();
			}
		}

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		JobDeclaration Declaration => (JobDeclaration)Parent.JobDeclaration;

		bool IsRefundRequestValidationOn => Declaration?.IsRefundRequestValidationOn ?? false;

		bool IsImport => Declaration?.IsImport ?? false;
	}
}
