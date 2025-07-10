using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface ILine
	{
		ZString DeclarationType { get; } // DECLN-TYPE
		ZString DescriptionOfGoods { get; } // GDS-DESC

		IEnumerable<IPackage> Packages { get; }

		IEnumerable<IContainer> Containers { get; }

		ZString CommodityCode { get; }  // BASE-CMDTY-CODE
		ZString SupplementaryCode1 { get; } // EC-SUPPLEMENT
		ZString SupplementaryCode2 { get; } // EC-SUPPLEMENT2
		ZString CountryOriginCode { get; }  // ITEM-ORIG-CNTRY
		ZDecimal GrossMassInKilograms { get; }  // ITEM-GROSS-MASS
		ZString Procedure { get; }  // CPC
		ZDecimal NetMassInKilograms { get; }    // ITEM-NET-MASS

		IEnumerable<IPreviousDocument> PreviousDocuments { get; }

		ZDecimal SupplementaryUnits { get; }    // ITEM-SUPP-UNITS
		ZString SupplementaryUnitsUQ { get; }

		IOrganisation SupervisingOffice { get; } // I-SPOFF...
		IOrganisation Consignee { get; } // I-CNSGE...
		IOrganisation Shipper { get; } // I-CNSGR...

		ZDecimal ThirdQuantity { get; } // ITEM-THRD-QTY

		IEnumerable<IStatement> Statements { get; }

		IEnumerable<ISupportingDocument> SupportingDocuments { get; }

		ZDecimal StatisticalValue { get; }  // ITEM-STAT-VAL-DC

		IEnumerable<ITax> Taxes { get; }

		ZString PrincipalsRepresentativeName { get; } // REPR-NAME
		ZString PrincipalsRepresentativeCity { get; } // REPR-CITY

		ZString UNDGCode { get; } // UNDG-Code

		ZBool FECNetMassInKilograms { get; } // ITEM-PROC-INST "QV1"
		ZBool FECSupplementaryUnits { get; } // ITEM-PROC-INST "QV2"

		ZBool IsProcedureThatAllowsZeroSupplementaryQty { get; }
	}
}
