using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADPreviousDocumentM2IndicatorWrapperTest : TestCase
{
	public void TestSADPreviousDocumentM2IndicatorWrapper()
	{
		var wrapper = new SADPreviousDocumentM2IndicatorWrapper();
		AssertEquals("ZZZ", wrapper.Category);
		AssertEquals("", wrapper.ComplementOfInformation);
		AssertEquals("", wrapper.CustomsOffice);
		AssertEquals(ZDate.Empty, wrapper.Date);
		AssertNull(wrapper.ItemNumber);
		AssertEquals("", wrapper.Mrn);
		AssertEquals("", wrapper.ReferenceCIN);
		AssertEquals("", wrapper.ReferenceNumber);
		AssertEquals("M2", wrapper.Register);
		AssertEquals("", wrapper.Series);
		AssertEquals("Z", wrapper.DocType);
	}
}
