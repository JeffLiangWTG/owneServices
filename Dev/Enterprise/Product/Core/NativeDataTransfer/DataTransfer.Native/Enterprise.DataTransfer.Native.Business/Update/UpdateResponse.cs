using CargoWise.Common;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common.Logging;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class UpdateResponse
	{
		internal UpdateResponse(EntityInfo entityInfo, ILogBuffer logBuffer)
		{
			this.EntityInfo = Argument.NotNull(entityInfo, "EntityInfo entityInfo");
			this.LogBuffer = Argument.NotNull(logBuffer, " ILogBuffer logBuffer");
		}

		public readonly EntityInfo EntityInfo;
		public readonly ILogBuffer LogBuffer;
	}
}
