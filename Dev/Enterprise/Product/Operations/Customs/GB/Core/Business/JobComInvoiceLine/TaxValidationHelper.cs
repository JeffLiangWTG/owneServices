using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business
{
	public class TaxValidationHelper
	{
		public TaxValidationHelper(EU.Business.Declaration.IEuTax parent, IEnumerable<EU.Business.Declaration.IEuTax> allTaxes)
		{
			allBrotherTaxes = allTaxes;
			this.parent = parent;
		}

		public virtual void CheckTaxTypeAndMop(ZPropertyInfo zPropertyInfoForError)
		{
			if (parent != null)
			{
				if (HasRowsWithMopIn(CombinationDelayed) >= 1 && ((from EU.Business.Declaration.IEuTax t in allBrotherTaxes where t.G4_Type == parent.G4_Type select t).Take(2).Count() > 1))
				{
					zPropertyInfoForError.AddMessageError("Postponed MoP (G) must be the only row per tax type");
				}
				else if (!(AllRowsMopAreInThisList(CombinationOne) || AllRowsMopAreInThisList(CombinationTwo) || AllRowsMopAreInThisList(CombinationDelayed) || AllRowsMopAreInThisList(CombinationExport)))
				{
					zPropertyInfoForError.AddMessageError("Invalid combination of MoP, see Tariff Volume 3, section 17");
				}
			}
		}

		protected bool AllRowsMopAreInThisList(List<string> codes)
		{
			return (from EU.Business.Declaration.IEuTax t in allBrotherTaxes where t.G4_Type == parent.G4_Type select t).All(tax => codes.Contains(tax.G4_MethodOfPayment));
		}

		protected int HasRowsWithMopIn(List<string> codes)
		{
			return (from EU.Business.Declaration.IEuTax t in allBrotherTaxes where t.G4_Type == parent.G4_Type && codes.Contains(t.G4_MethodOfPayment) select t).Count();
		}

		protected IEnumerable<EU.Business.Declaration.IEuTax> allBrotherTaxes;
		protected EU.Business.Declaration.IEuTax parent;

		protected virtual List<string> CombinationOne => new List<string> { "A", "F", "N", "Q", "S", "T", "U", "V", "W", "X", "Y", "Z", string.Empty };
		protected virtual List<string> CombinationTwo => new List<string> { "D", "F", "P", "Q", "S", "T", "U", "V", "W", "X", "Y", "Z", string.Empty };
		protected virtual List<string> CombinationDelayed => new List<string> { "G" };
		protected virtual List<string> CombinationExport => new List<string> { "L" };

		public static TaxValidationHelper GetTaxValidationHelper(EU.Business.Declaration.IEuTax parent, IEnumerable<EU.Business.Declaration.IEuTax> allTaxes)
		{
			var invLineTax = parent as JobComInvoiceLineTax;  // Remember, taxes can also live under CusClassPartPivot

			if (invLineTax != null)
			{
				var invoiceLine = invLineTax.InvoiceLine;
				var declaration = (Declaration.JobDeclaration)invoiceLine?.Declaration;
				return declaration?.ApplicationExtender?.GetTaxValidationHelper(parent, allTaxes) ?? new TaxValidationHelper(parent, allTaxes);
			}

			return new TaxValidationHelper(parent, allTaxes);
		}
	}
}
