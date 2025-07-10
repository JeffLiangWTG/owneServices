using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public interface IMessageInterpreter<in TDataProvider> where TDataProvider : class
{
	ZString GetMessageInterpretation(TDataProvider dataProvider);
}
