using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public DeltaIEAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void AddMessageErrorForRequiredZG_AgreedPlaceCode()
		{
			var parent = Parent;
			if (!parent.Parent.IsImport)
			{
				base.AddMessageErrorForRequiredZG_AgreedPlaceCode();
			}
		}

		protected override void AddMessageErrorOrWarningForMismatch()
		{
			var parent = Parent;
			if (parent.Parent.JobDeclaration.IsImport)
			{
				parent.ZG_AgreedPlaceCodeInfo.AddWarning(DeclarationValidationConstants.IncotermPlaceCodeMismatch);
			}
			else
			{
				base.AddMessageErrorOrWarningForMismatch();
			}
		}

		protected override void CheckZG_AgreedPlaceCode()
		{
			base.CheckZG_AgreedPlaceCode();
			var addInfoInvoiceHeader = Parent;
			var invoiceHeader = addInfoInvoiceHeader.Parent;
			if (!invoiceHeader.IsImport && !invoiceHeader.JZ_IncoTerm.Equals(Core.Constants.IncoTerms.Other) && invoiceHeader.JZ_IncoTermPlace.IsEmpty && addInfoInvoiceHeader.ZG_IncotermCountry.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(addInfoInvoiceHeader.ZG_AgreedPlaceCodeInfo);
			}
		}
	}
}
