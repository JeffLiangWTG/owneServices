using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementHeaderValidation : Customs.Business.CusStatementHeaderValidation
	{
		public CusStatementHeaderValidation(CusStatementHeader parent) : base(parent)
		{
		}

		protected new CusStatementHeader Parent => (CusStatementHeader)base.Parent;

		protected override void CheckB2_BranchDesignation()
		{
			base.CheckB2_BranchDesignation();

			MandatoryIfNotReadOnly(Parent.B2_BranchDesignationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.B2_BranchDesignationInfo);
		}

		protected override void CheckB2_OH_Importer()
		{
			base.CheckB2_OH_Importer();

			MandatoryIfNotReadOnly(Parent.B2_OH_ImporterInfo);
			ListValidation.ErrorIfInvalidPK(Parent.B2_OH_ImporterInfo);
		}

		protected override void CheckB2_StatementType()
		{
			base.CheckB2_StatementType();

			MandatoryIfNotReadOnly(Parent.B2_StatementTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.B2_StatementTypeInfo);
		}

		protected override void CheckB2_PeriodStartDate()
		{
			base.CheckB2_PeriodStartDate();

			MandatoryIfNotReadOnly(Parent.B2_PeriodStartDateInfo);
		}

		protected override void CheckB2_PeriodEndDate()
		{
			base.CheckB2_PeriodEndDate();

			MandatoryIfNotReadOnly(Parent.B2_PeriodEndDateInfo);
		}

		protected override void CheckB2_EntryFilerCode()
		{
			base.CheckB2_EntryFilerCode();

			MandatoryIfNotReadOnly(Parent.B2_EntryFilerCodeInfo);

			if (!Parent.B2_EntryFilerCodeInfo.ReadOnly && Parent.Lookups.Profiles.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B2_EntryFilerCodeInfo);
			}
		}

		protected override void CheckB2_PaymentType()
		{
			base.CheckB2_PaymentType();

			MandatoryIfNotReadOnly(Parent.B2_PaymentTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.B2_PaymentTypeInfo);
		}

		protected override void CheckB2_ImporterCustomsID()
		{
			base.CheckB2_ImporterCustomsID();

			MandatoryIfNotReadOnly(Parent.B2_ImporterCustomsIDInfo);
		}

		protected override void CheckB2_CheckNo()
		{
			base.CheckB2_CheckNo();

			if (!Parent.B2_CheckNoInfo.ReadOnly && Parent.B2_PaymentType == MethodOfPaymentList.Codes.R)
			{
				MandatoryValidation.CheckEntered(Parent.B2_CheckNoInfo);
			}
		}

		void MandatoryIfNotReadOnly(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
			}
		}
	}
}
