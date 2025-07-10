using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ExportIncoTermAndCustomsChargeFactory))]
sealed class ExportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
{
	public void TestParentType()
	{
		var exportCharges = ExportIncoterAndCustomsCharge.GetAllCharges().ToArray();
		var expectedParentType = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice;
		foreach (var parentType in exportCharges.Select(c => c.ParentTypes))
		{
			AssertEquals(expectedParentType, parentType);
		}
	}

	ExportIncoTermAndCustomsChargeFactory ExportIncoterAndCustomsCharge => exportIncoTermAndCustomsChargeFactory ??= new ExportIncoTermAndCustomsChargeFactory();
	ExportIncoTermAndCustomsChargeFactory exportIncoTermAndCustomsChargeFactory;
}
