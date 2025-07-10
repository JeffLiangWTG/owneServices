using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IImportLine : ILine
	{
		ZString ValueAdjustmentCode { get; } // ITEM-ADJT-CODE
		ZDecimal ValueAdjustmentAmount { get; } // ITEM-VAL-ADJT
		ZString ValuationMethod { get; } // VAL-MTHD-CODE
		ZString PreferenceCode { get; } // PREFERENCE
		ZString QuotaOrderNumber { get; } // QTA-NO
		ZDecimal ItemPrice { get; } // ITEM-PRC-AC

		ZBool FECCountryOfOrigin { get; } // ITEM-PROC-INST "ORG"

		/// <summary>
		/// The code for the country from which the goods originated.
		/// ITEM-ORIG-CNTRY
		/// </summary>
		ZString CountryOfExport { get; }
	}
}
