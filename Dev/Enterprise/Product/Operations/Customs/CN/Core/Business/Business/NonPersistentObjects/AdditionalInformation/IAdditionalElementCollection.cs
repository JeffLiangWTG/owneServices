using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

public interface IAdditionalElementCollection
{
	ZString GetValue(ZString code);
	void SetValue(ZString code, ZString value);
}
