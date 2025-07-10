using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public interface IUpdateContext : IEntityContext
	{
		EntityInfo EntityInfo { get; }
	}
}