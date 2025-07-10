using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS;

public class NctsDepartureCargoDescWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper
{
	protected NctsDepartureCargoDescWrapper(NctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
	{
	}

	public static NctsDepartureCargoDescWrapper New(NctsDepartureCargoDesc line, BusinessObjectFactory factory)
	{
		return new NctsDepartureCargoDescWrapper(line, factory);
	}

	protected override ZString GetPreviousDocuments()
	{
		var previousDocuments = GetAllPreviousDocuments();
		return new CompactPreviousDocumentsBuilder().GetPreviousDocumentsFormatted(previousDocuments, Line.IsPhase5Departure);
	}
}
