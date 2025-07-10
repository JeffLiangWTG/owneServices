using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public interface IRelatedJob : IControllerIDProvider
	{
		ZString JobNumber { get; }
		ZString JobDescription { get; }
		ZString JobStatus { get; }
	}
}
