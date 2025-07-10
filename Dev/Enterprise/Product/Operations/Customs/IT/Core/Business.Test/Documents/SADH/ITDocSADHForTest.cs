using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.EU;
using ITCusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITDocSADHForTest : ITDocSADH
{
	public ITDocSADHForTest(ITCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) : base(entryHeader, factoryToWrap)
	{
	}

	public IBox16CountryOfOriginEvaluator GetBox16CountryOfOriginEvaluatorExposed() => GetBox16CountryOfOriginEvaluator();
}
