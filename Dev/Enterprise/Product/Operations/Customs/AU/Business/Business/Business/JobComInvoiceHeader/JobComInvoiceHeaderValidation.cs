using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceHeaderValidation : Customs.Business.InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		#region Added Validations

		protected virtual void CheckJZ_ITOTIncoTerm()
		{
			if (Parent.JobDeclaration.IsImport)
			{
				if (Parent.JZ_ITOTIncoTerm == EdificeIncoTermAndCustomsChargeFactory.ErrorIncoTermCode)
				{
					Parent.JZ_ITOTIncoTermInfo.AddMessageError("System cannot calculate ITOT incoterm with current charge information.\r\nThis usually happens when you have ONS included in lines while OFT excluded in lines. There is no agreed incoterm for the situation.");
				}
			}
		}

		#endregion

		#region ValidateCalculatedProperties

		public void ValidateJZ_ITOTIncoTerm()
		{
			ValidateCalculatedProperty(Parent.JZ_ITOTIncoTermInfo);
		}

		public void ValidateJZ_Nature10PackCount()
		{
			ValidateCalculatedProperty(Parent.JZ_Nature10PackCountInfo);
		}

		#endregion

		#region Overrides

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JZ_WeightUQInfo, Parent.Lookups.JZ_WeightUQ_List);
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			ValidateJZ_WeightUQ();
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			ValidateJZ_InvoiceNumber();
		}

		protected override void CheckJZ_OA_SupplierAddress()
		{
			base.CheckJZ_OA_SupplierAddress();
			var supplier = Parent.Supplier;
			var declaration = Parent.JobDeclaration;
			var isSupplierAddressAvailableOnGrid = declaration != null && (!declaration.IsExWarehouse && (declaration.IsQuarantine || !declaration.IsExport));
			if (isSupplierAddressAvailableOnGrid && supplier != null && supplier.LocalBusinessRegNo.IsEmpty && !supplier.HasCCIDWithMatchedAddress(Parent.JZ_OA_SupplierAddress))
			{
				Parent.JZ_OA_SupplierAddressInfo.AddMessageError("There is no CID code that matches this address or any ABN code in the Supplier. Please update the Supplier Organisation");
			}
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			if (Parent.IsAttachedToPersistentDeclaration && !Parent.JobDeclaration.IsExWarehouse)
			{
				MessageValidation.CheckEntered(Parent.JZ_InvoiceNumberInfo);
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			if (!Parent.JZ_InvoiceNumber.IsEmpty && (Parent.JZ_InvoiceAmount.IsEmpty || Parent.JZ_InvoiceAmount <= 0.0m))
			{
				Parent.JZ_InvoiceAmountInfo.AddMessageError("Invoice Total must be greater than zero");
			}

			ValidateJZ_RX_NKInvoice_Currency();
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override void CheckJZ_ValuationDateOverride()
		{
			base.CheckJZ_ValuationDateOverride();
			if (Parent.JZ_ValuationDateOverride.IsValid && Parent.JobDeclaration != null && Parent.JobDeclaration.JE_DateOfFirstArrival.IsValid &&
				Parent.JZ_ValuationDateOverride.Date > Parent.JobDeclaration.JE_DateOfFirstArrival.Date)
			{
				Parent.JZ_ValuationDateOverrideInfo.AddMessageError("Valuation Date must not be after the first Arrival Date");
			}
		}

		#endregion
	}
}
