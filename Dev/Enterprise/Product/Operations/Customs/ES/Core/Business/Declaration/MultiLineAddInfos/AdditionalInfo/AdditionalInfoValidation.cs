using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected override bool IsCodeMandatory => !HasEXS();

		protected override bool IsOtherFieldsEnabled => !(HasEXS() || (Parent.Declaration?.IsExport ?? false));

		ZBool HasEXS()
		{
			var result = false;
			if (Parent.ParentAsInvoiceHeader != null)
			{
				result = Parent.ParentAsInvoiceHeader.InvoiceLines.Any(x => ((JobComInvoiceLine)x).IsEXS);
			}
			else if (Parent.ParentAsInvoiceLine != null)
			{
				result = Parent.ParentAsInvoiceLine.IsEXS;
			}

			return result;
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;
	}
}
