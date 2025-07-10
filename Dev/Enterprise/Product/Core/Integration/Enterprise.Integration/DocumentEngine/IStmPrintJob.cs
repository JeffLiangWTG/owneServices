using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IStmPrintJob
	{
		ZGuid PK { get; }

		ZShort SP_Copies { get; set; }
		ZBlob SP_CustomProperties { get; set; }
		ZString SP_EmailAttachments { get; set; }
		ZString SP_EmailAttachmentFormat { get; set; }
		ZString SP_Destination { get; set; }
		ZString SP_JobType { get; set; }
		ZDateTime SP_RunDateTime { get; set; }
		ZGuid SP_SB_DeliveryGroup { get; set; }
		ZGuid SP_SQ { get; set; }
	}
}
