using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaImpendingArrivalReportInformation : IImpendingArrivalReportInformation
	{
		ZString Voyage { get; }
		ZString LloydsNumber { get; }
		ZString[] SlotChartererIDs { get; }
	}
}
