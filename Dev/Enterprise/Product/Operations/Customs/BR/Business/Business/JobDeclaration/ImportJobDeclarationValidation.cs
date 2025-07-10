using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override bool ShouldValidatePackagesActualPackageCount => false;

		protected override void CheckEntranceOfficeCode()
		{
			base.CheckEntranceOfficeCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.EntranceOfficeCodeInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();
			ValidationHelper.CheckPaymentMethod(Parent);
		}

		protected override void CheckBRTransportModeIsMandatory()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.BRTransportModeInfo);
		}

		protected override void CheckOperationType()
		{
			base.CheckOperationType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.OperationTypeInfo);
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			var targetInfo = Parent.JE_OH_ImporterInfo;
			var registrationNumber = Parent.Importer?.PrimaryRegistrationNumber;

			CheckRegistrationNumberEntered(targetInfo, registrationNumber);
			CheckRegistrationNumberIsCJN(targetInfo, registrationNumber);
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			if (Parent.OperationType == TypeOfOperationImportList.Codes.AccountAndOrder)
			{
				var targetInfo = Parent.JE_OH_ConsigneeInfo;
				var registrationNumber = Parent.IntermConsignee?.PrimaryRegistrationNumber;
				var orgName = Parent.ConsigneeCaption.Caption;

				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

				CheckRegistrationNumberEntered(targetInfo, registrationNumber, orgName);
				CheckRegistrationNumberIsCJN(targetInfo, registrationNumber, orgName);
			}
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			if (Parent.IsCargoProvenanceAvailable)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsOriginInfo);
			}
		}

		protected override void CheckJE_DispatchModality()
		{
			base.CheckJE_DispatchModality();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_DispatchModalityInfo);
		}
	}
}
