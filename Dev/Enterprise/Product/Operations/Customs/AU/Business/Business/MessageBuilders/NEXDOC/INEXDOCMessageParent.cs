using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface INEXDOCMessageParent
	{
		ZString RexNumber { get; }
		BusinessObjectFactory Factory { get; }
	}
}
