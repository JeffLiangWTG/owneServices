using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaActualArrivalReportInformation : IActualArrivalReportInformation
	{
		ZString BerthCode { get; }
		ZString DischargeCTOID { get; }
		ZString Voyage { get; }
		ZString LloydsNumber { get; }
		ZString StevedoreID { get; }
	}
}
