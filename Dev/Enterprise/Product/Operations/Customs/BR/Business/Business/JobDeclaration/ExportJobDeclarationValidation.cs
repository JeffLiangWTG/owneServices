using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ExportJobDeclarationValidation : JobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckBoardingOfficeCode()
		{
			base.CheckBoardingOfficeCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BoardingOfficeCodeInfo);
		}

		protected override void CheckBoardingEnclosureCode()
		{
			base.CheckBoardingEnclosureCode();
			if (Parent.BoardingOfficeIsCustomsEnclosure)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BoardingEnclosureCodeInfo);
			}
		}

		protected override void CheckJE_DeclarantType()
		{
			base.CheckJE_DeclarantType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_DeclarantTypeInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			if (Parent.ClearanceOfficeIsCustomsEnclosure)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
			}
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			if (Parent.JE_DeclarantType != TypeOfOperationExportList.Codes._1001)
			{
				var targetInfo = Parent.JE_OA_DeclarantAddressInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

				var registrationNumber = Parent.DeclarantAddress?.Header?.PrimaryRegistrationNumber;
				CheckRegistrationNumberEntered(targetInfo, registrationNumber);
				CheckRegistrationNumberIsCJN(targetInfo, registrationNumber);
			}
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			if (Parent.JE_DeclarantType == TypeOfOperationExportList.Codes._1001)
			{
				var targetInfo = Parent.JE_OH_SupplierInfo;
				var registrationNumber = Parent.Supplier?.PrimaryRegistrationNumber;
				var orgName = Res.GetString("7F2C0463-25C2-447A-9454-92888B87B88F", "Main Supplier");

				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

				CheckRegistrationNumberEntered(targetInfo, registrationNumber, orgName);
				CheckRegistrationNumberIsCJN(targetInfo, registrationNumber, orgName);
			}
		}
	}
}
