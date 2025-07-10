using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class JobComInvoiceHeaderValidation : AutoILJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_InvoiceType()
		{
			base.CheckJZ_InvoiceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_InvoiceTypeInfo);
		}

		protected override void CheckJZ_PreferenceDocumentType()
		{
			base.CheckJZ_PreferenceDocumentType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_PreferenceDocumentTypeInfo);
		}

		protected override void CheckJZ_IncoTermPlace()
		{
			base.CheckJZ_IncoTermPlace();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_IncoTermPlaceInfo);
		}

		protected override void CheckJZ_PaymentTerms()
		{
			base.CheckJZ_PaymentTerms();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_PaymentTermsInfo);
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			MandatoryValidation.MessageErrorIfIsZero(Parent.JZ_InvoiceAmountInfo);
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			var parent = Parent;
			if ((parent.JobDeclaration?.IsImport ?? false)
				&& (parent.JZ_InvoiceType == InvoiceTypeList.Codes._380 || parent.JZ_InvoiceType == InvoiceTypeList.Codes._325)
				&& (parent.Supplier?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, CountryCodes.Israel)?.OK_CustomsRegNo.IsEmpty ?? true))
			{
				parent.JZ_OH_SupplierInfo.AddMessageError(ValidationCaptions.JobComInvoiceHeader.SupplierCustomsNumberIsMissing);
			}
		}
	}
}
