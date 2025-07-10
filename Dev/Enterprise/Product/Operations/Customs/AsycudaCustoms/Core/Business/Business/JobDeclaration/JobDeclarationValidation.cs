namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobDeclarationValidation : Customs.Business.BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => Parent;

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJE_DeclarationType();
		}

		public void ValidateJE_DeclarationType()
		{
			ValidateCalculatedProperty(Parent.JE_DeclarationTypeInfo);
		}

		protected void CheckJE_DeclarationType()
		{
			var entryInstruction = Parent.CusEntryInstruction;
			if (entryInstruction != null)
			{
				entryInstruction.Validation.ValidateCEI_Style();
				Parent.JE_DeclarationTypeInfo.AddAllNotificationsFrom(entryInstruction.CEI_StyleInfo);
				Parent.MarkAsNeedingValidation();
			}
		}

		protected override string CannotChangeMessageTypeErrorText => Res.GetString("94457AD0-E9F3-42D5-BC30-A34B93AF3115", "Once an entry has been merged, Shipment Type should not change.");

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			if (Parent.IsExport && Parent.IsBondedWarehouseAutomationOn && (!(Parent.Supplier?.OH_IsWarehouseClient ?? false)) && Parent.HasOutwardInvoiceLine)
			{
				Parent.JE_OH_SupplierInfo.AddMessageError(JobDeclaration.SupplierMustBeMarkedAsWarehouseClient(Parent.TermNameForBondedWarehouse));
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			if (!Parent.IsExport && Parent.IsBondedWarehouseAutomationOn && (!(Parent.Importer?.OH_IsWarehouseClient ?? false)) && (Parent.HasOutwardInvoiceLine || Parent.HasInwardInvoiceLine))
			{
				Parent.JE_OH_ImporterInfo.AddMessageError(JobDeclaration.ImporterMustBeMarkedAsWarehouseClient(Parent.TermNameForBondedWarehouse));
			}
		}

		protected override void CheckJE_ManifestNumber()
		{
			base.CheckJE_ManifestNumber();
			var manifestNumber = Parent.JE_ManifestNumber;
			if (!manifestNumber.IsEmpty && !manifestNumber.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.JE_ManifestNumberInfo.AddMessageError(Res.GetString("e7ac4bb7-67bf-413d-b49d-3326d75a966f", "Manifest Number should be alphanumeric."));
			}
		}
	}
}
