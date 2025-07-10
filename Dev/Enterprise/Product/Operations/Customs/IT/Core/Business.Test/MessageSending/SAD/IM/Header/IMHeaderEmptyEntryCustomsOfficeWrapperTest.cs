using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMHeaderEmptyEntryCustomsOfficeWrapperTest : TestCase
{
	public void TestWrapper()
	{
		var wrapper = new IMHeaderEmptyEntryCustomsOfficeWrapper();
		AssertEquals(ZString.Empty, wrapper.Name);
		AssertEquals(ZString.Empty, wrapper.Nationality);
		AssertEquals(ZString.Empty, wrapper.ReferenceNumber);
	}
}
