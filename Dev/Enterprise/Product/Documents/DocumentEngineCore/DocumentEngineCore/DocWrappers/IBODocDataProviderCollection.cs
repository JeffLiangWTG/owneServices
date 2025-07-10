using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public interface IBODocDataProviderCollection
	{
		IBODocDataProvider this[string index] { get; }
		IBODocDataProvider this[int index] { get; }
		int Count { get; }
		ZString Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByParameters, ZInt maxItems);
		object Total(ZString fieldName, ZString decimalPlaces, ZString filter);
		BusinessObject Find(ZString match);
	}
}