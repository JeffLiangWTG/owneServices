using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Meursing
{
	public interface IMeursingTarget
	{
		BusinessObjectFactory Factory { get; }

		void SetMeursingResult(ZString meursingResult);
	}
}
