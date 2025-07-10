using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	class CDSTaxValidationHelper : TaxValidationHelper
	{
		public CDSTaxValidationHelper(EU.Business.Declaration.IEuTax parent, IEnumerable<EU.Business.Declaration.IEuTax> allTaxes) : base(parent, allTaxes)
		{
		}

		public override void CheckTaxTypeAndMop(ZPropertyInfo zPropertyInfoForError)
		{
			if (parent != null)
			{
				if (HasRowsWithMopIn(CombinationDelayed) >= 1 && ((from EU.Business.Declaration.IEuTax t in allBrotherTaxes where t.G4_Type == parent.G4_Type select t).Take(2).Count() > 1))
				{
					zPropertyInfoForError.AddMessageError("Postponed MoP (G) must be the only row per tax type");
				}
				else if (!(AllRowsMopAreInThisList(CombinationOne) || AllRowsMopAreInThisList(CombinationDelayed) || AllRowsMopAreInThisList(CombinationExport)))
				{
					zPropertyInfoForError.AddMessageError("Invalid combination of MoP, see Tariff Volume 3, section 17");
				}
			}
		}

		protected override List<string> CombinationOne => new List<string> { "A", "B", "C", "D", "E", "H", "M", "N", "P", "R", "S", "T", "U", "V", "X", "Z", string.Empty };
		protected override List<string> CombinationExport => new List<string> { "O", string.Empty };
	}
}
