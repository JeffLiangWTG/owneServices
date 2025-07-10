using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICIQDataLine
	{
		ZString CIQTariffCode { get; }
		ZString CIQTariffDescription { get; }

		IEnumerable<ZString> CargoAttributes { get; }
		ZString EndUse { get; }
		ZString Brand { get; }
		ZString Model { get; }
		ZString Specification { get; }
		ZInt QGPByDays { get; }
		ZDateTime ExpiryDate { get; }
		ZString Ingredient { get; }
		ZString BatchNumber { get; }
		ZDateTime ManufactureDate { get; }
		ZString ManufacturerCIQ { get; }
		ZString ManufacturerName { get; }

		ZBool NonDangerousChemical { get; }
		ZString UNDGNumber { get; }
		ZString UNDGClass { get; }
		ZString UNDGPackageType { get; }
		ZString UNDGPackingGroup { get; }

		IEnumerable<ICIQProductQualification> ProductQualifications { get; }
	}

	public interface ICIQProductQualification
	{
		ZInt Sequence { get; }
		ZString DocumentType { get; }
		ZString DocumentNumber { get; }
		ZInt LineNumber { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfQuantity { get; }
	}
}
