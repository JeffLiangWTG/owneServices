using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NC084ResponseMessageProcessor))]
sealed class NC084ResponseMessageProcessorTest : CH.Business.Testing.NC084ResponseMessageProcessorTest
{
	protected override BusinessObject CreateParentCore(string mrn)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.MovementReferenceNumberSetter(mrn + ".1");
		return nctsHeader;
	}
}
