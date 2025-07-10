using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.IT.Business;

public class EmptyBox16CountryOfOriginEvaluator : IBox16CountryOfOriginEvaluator
{
	ZString IBox16CountryOfOriginEvaluator.Evaluate() => ZString.Empty;
}
