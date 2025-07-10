using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class PIDJobDeclarationValidation : AutoKRJobDeclarationValidation
	{
		public PIDJobDeclarationValidation(JobDeclaration declaration) : base(declaration)
		{ }
		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePIDFreightAmount();
			ValidateDeliveryOrPickupCartageCoPK();
			ValidatePIDWeapon();
			ValidatePIDDrug();
			ValidatePIDAnimal();
			ValidatePIDEndangeredItems();
			ValidatePIDCounterfeit();
			ValidatePIDCommercialUseItems();
			ValidatePIDExcessTimeLimitItems();
			ValidatePIDPornography();
		}

		public void ValidatePIDFreightAmount()
		{
			ValidateCalculatedProperty(Parent.PIDFreightAmountInfo);
		}
		protected void CheckPIDFreightAmount()
		{
			MandatoryValidation.MessageErrorIfIsZero(Parent.PIDFreightAmountInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.PIDFreightAmountInfo);
		}

		public void ValidateDeliveryOrPickupCartageCoPK()
		{
			ValidateCalculatedProperty(Parent.DeliveryOrPickupCartageCoPKInfo);
		}
		protected void CheckDeliveryOrPickupCartageCoPK()
		{
			TypeValidation.CheckValidGuid(Parent.DeliveryOrPickupCartageCoPKInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DeliveryOrPickupCartageCoPKInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_CustomsDivision()
		{
			base.CheckJE_CustomsDivision();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsDivisionInfo);
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_RL_NKOriginInfo);
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_MessageSubTypeInfo);
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_RL_NKPortOfLoadingInfo);
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			base.CheckJE_OH_ShippingLine();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ShippingLineInfo);
		}

		protected override void CheckJE_ContainerMode()
		{
		}

		public void ValidatePIDWeapon()
		{
			ValidateCalculatedProperty(Parent.PIDWeaponInfo);
		}
		protected void CheckPIDWeapon()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDWeaponInfo);
		}

		public void ValidatePIDDrug()
		{
			ValidateCalculatedProperty(Parent.PIDDrugInfo);
		}
		protected void CheckPIDDrug()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDDrugInfo);
		}

		public void ValidatePIDAnimal()
		{
			ValidateCalculatedProperty(Parent.PIDAnimalInfo);
		}
		protected void CheckPIDAnimal()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDAnimalInfo);
		}

		public void ValidatePIDEndangeredItems()
		{
			ValidateCalculatedProperty(Parent.PIDEndangeredItemsInfo);
		}
		protected void CheckPIDEndangeredItems()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDEndangeredItemsInfo);
		}

		public void ValidatePIDCounterfeit()
		{
			ValidateCalculatedProperty(Parent.PIDCounterfeitInfo);
		}
		protected void CheckPIDCounterfeit()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDCounterfeitInfo);
		}

		public void ValidatePIDCommercialUseItems()
		{
			ValidateCalculatedProperty(Parent.PIDCommercialUseItemsInfo);
		}
		protected void CheckPIDCommercialUseItems()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDCommercialUseItemsInfo);
		}

		public void ValidatePIDExcessTimeLimitItems()
		{
			ValidateCalculatedProperty(Parent.PIDExcessTimeLimitItemsInfo);
		}
		protected void CheckPIDExcessTimeLimitItems()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDExcessTimeLimitItemsInfo);
		}

		public void ValidatePIDPornography()
		{
			ValidateCalculatedProperty(Parent.PIDPornographyInfo);
		}
		protected void CheckPIDPornography()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PIDPornographyInfo);
		}
	}
}
