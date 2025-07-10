using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IStmDocumentDelivery
	{
		ZGuid PK { get; }

		ZGuid SDL_GB { get; set; }

		ZGuid SDL_GE { get; set; }

		ZGuid SDL_GS { get; set; }

		ZString SDL_Instructions { get; set; }
		ZBool SDL_IsProcessed { get; set; }

		ZGuid SDL_ParentId { get; set; }

		ZString SDL_ParentControllerIdOrTableCode { get; set; }

		ZByte SDL_RetryAttempts { get; set; }

		ZGuid SDL_SU { get; set; }
	}
}
