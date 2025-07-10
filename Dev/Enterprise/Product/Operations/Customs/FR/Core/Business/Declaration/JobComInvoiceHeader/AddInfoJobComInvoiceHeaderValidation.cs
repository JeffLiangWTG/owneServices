using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

		protected new AddInfoJobComInvoiceHeaderLookups Lookups => Parent.Lookups;

		protected override void CheckZG_ValuationMethod()
		{
			base.CheckZG_ValuationMethod();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ValuationMethodInfo);
			var parent = Parent;
			if (!(parent.Parent.JobDeclaration?.IsUCC6AndIsImport ?? false) || parent.Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !x.IsSimplifiedOrPreliminaryUnderCodeC))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_ValuationMethodInfo);
			}
		}

		protected override bool IsMandatoryZG_AgreedPlaceCodeCore => !Parent.Parent.ZG_AgreedPlaceCode_ReadOnly;
	}
}
