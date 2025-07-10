using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.SG
{
	public static class DataExtensions
	{
		public static CodeDescriptionPairList GetCycleNumbers(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ASYCUDA.CycleNumbers", () =>
			{
				var result =
					new UntranslatableCodeDescriptionPairList(
						(NoResString)"Cycle Numbers are date strings and are not translatable");
				var cycleNumbers = (ICodeDescriptionPairList)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().CycleNumbers.Value;
				result.AddRange(cycleNumbers);

				return result;
			});
		}
	}
}
