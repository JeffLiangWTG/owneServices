using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IEXDOCRefCodeTypeProvider
	{
		ZString Type { get; }

		BusinessObjectFactory Factory { get; }
	}
}
