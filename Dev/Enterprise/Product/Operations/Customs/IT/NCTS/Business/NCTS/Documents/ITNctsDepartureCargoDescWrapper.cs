using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using ITNctsDepartureCargoDesc = Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc;

namespace Enterprise.Customs.IT.NCTS.Business;

public class ITNctsDepartureCargoDescWrapper : NctsDepartureCargoDescWrapper
{
	protected ITNctsDepartureCargoDescWrapper(ITNctsDepartureCargoDesc line, BusinessObjectFactory factory) : base(line, factory)
	{
	}

	public static ITNctsDepartureCargoDescWrapper New(ITNctsDepartureCargoDesc line, BusinessObjectFactory factory)
	{
		return new ITNctsDepartureCargoDescWrapper(line, factory);
	}

	protected override ZString GetBox33Commodity() => base.GetBox33Commodity().Left(8);

	protected override ZString GetPreviousDocuments()
	{
		var previousDocuments = GetAllPreviousDocuments();
		return new ITCompactPreviousDocumentsBuilder().GetPreviousDocumentsFormatted(previousDocuments, Line.IsPhase5Departure);
	}

	protected override ZString GetSupportingDocuments()
	{
		var supportingDocuments = GetAllSupportingDocuments();
		return new ITCompactProducedDocumentsCertificatesBuilder().GetProducedDocumentsCertificatesFormatted(supportingDocuments, Line.IsPhase5Departure);
	}

	protected override ZString GetBox35GrossMass() => GetFormattedWeight(DepartureGoodsItemWrapper.GrossMass);

	protected override ZString GetBox38NetMass() => GetFormattedWeight(DepartureGoodsItemWrapper.NetMass);

	ZString GetFormattedWeight(ZDecimal weightValue)
	{
		var formattedValue = new IT5DecimalsWeightFormatter(weightValue).GetFormattedValue(6);
		return Tools.ValueOrThreeDashes(formattedValue);
	}
}
