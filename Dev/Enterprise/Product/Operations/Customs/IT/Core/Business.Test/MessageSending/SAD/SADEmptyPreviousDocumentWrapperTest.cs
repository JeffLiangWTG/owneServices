using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADEmptyPreviousDocumentWrapperTest : TestCase
{
	public void TestWrapper()
	{
		var wrapper = new SADEmptyPreviousDocumentWrapper();
		AssertEquals(ZString.Empty, wrapper.DocType);
		AssertEquals(ZString.Empty, wrapper.Category);
		AssertEquals(ZString.Empty, wrapper.Mrn);
		AssertEquals(ZString.Empty, wrapper.ComplementOfInformation);
		AssertEquals(ZString.Empty, wrapper.Register);
		AssertEquals(ZString.Empty, wrapper.ReferenceNumber);
		AssertEquals(ZString.Empty, wrapper.ReferenceCIN);
		AssertEquals(ZDate.Empty, wrapper.Date);
		AssertEquals(ZString.Empty, wrapper.Series);
		AssertEquals(ZString.Empty, wrapper.CustomsOffice);
		AssertNull(wrapper.ItemNumber);
	}
}
