using CargoWise.Types;

namespace Enterprise.Integration.LandTransport
{
	public interface IDtbConsignmentAction
	{
		ZGuid PK { get; }

		ZGuid LTA_LTS_ConsignmentAddress { get; set; }

		ZGuid LTA_K1_RunSheetInstruction { get; }
	}
}
