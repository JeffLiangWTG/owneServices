
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CodeDescriptionPairListCustomsWETExemption : CodeDescriptionPairList
	{
		public CodeDescriptionPairListCustomsWETExemption()
		{
			AddPair("404", "Item 4 Schedule 4 - Goods owned & for official use of the Governments");
			AddPair("408", "Item 8 Schedule 4 - Goods for use by or sale to SOFA personnel");
			AddPair("415", "Item 15 Schedule 4 - Goods imported by passengers or crew of ships or aircraft etc");
			AddPair("417", "Item 17 Schedule 4 - Goods exported and returned unaltered");
			AddPair("418A", "Item 18A Schedule 4 - Imported goods returned after repair");
			AddPair("418B", "Item 18B Schedule 4 - Free of charge warrenty goods");
			AddPair("418C", "Item 18C Schedule 4 - Global product safety recall goods");
			AddPair("421", "Item 21 Schedule 4 - TEXCO goods");
			AddPair("421A", "Item 21A Schedule 4 - TRADEX goods");
			AddPair("424", "Item 24 Schedule 4 - Goods under a will or intestacy");
			AddPair("433B", "Item 33B Schedule 4 - Samples of negligible value");
			AddPair("4101", "Item 101 Schedule 4 - Goods for official use of Diplomatic Missions");
			AddPair("4102", "Item 102 Schedule 4 - Goods for use of staff of Diplomatic Missions");
			AddPair("4103", "Item 103 Schedule 4 - Goods for use in Colsular Posts");
			AddPair("4104", "Item 104 Schedule 4 - Goods for personal use of Consulate employees");
			AddPair("4105", "Item 105 Schedule 4 - Goods for official use of other Consular Posts");
			AddPair("4106", "Item 106 Schedule 4 - International Organisation Privileges & Immunities Act goods");
			AddPair("W75A", "Exemptions for dealings that are GST-free supplies");
			AddPair("W75B", "Exemption for a local entry relating to an importation that is a non-taxable importations");
			AddPair("W720", "Sales in bond");
		}
	}
}
