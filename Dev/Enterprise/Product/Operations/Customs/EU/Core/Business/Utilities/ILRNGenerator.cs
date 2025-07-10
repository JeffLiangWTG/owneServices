using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Business
{
	public interface ILRNGenerator
	{
		BusinessObjectFactory Factory { get; }

		INumberFountainProxy LrnNumberFountain { get; }

		GlbBranch Branch { get; }
	}
}
