using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks;

public class IE43SpecialMentionWrapper
{
	public ZString Code { get; set; }
	public ZString Description { get; set; }
	public ZBool NctsExportFromEC { get; set; }
	public ZString CountryCode { get; set; }
}
