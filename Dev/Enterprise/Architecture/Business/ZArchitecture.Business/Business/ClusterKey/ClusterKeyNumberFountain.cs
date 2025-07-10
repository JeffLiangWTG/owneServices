using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Environment;
using NumberFountains = Enterprise.NumberFountain.NumberFountains;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	static class ClusterKeyNumberFountain
	{
		public static INumberFountainProxy FountainProxy => Fountain.Wrap();

		static INumberFountain Fountain => new NumberFountains().ClusterKeyNumber;
	}
}
