using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class MessageSubType
{
	internal MessageSubType(ZString code)
	{
		fCode = code;
	}

	public ZString Code
	{
		get { return fCode; }
	}

	readonly ZString fCode;
}
