using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(AutoINCusSupportingInfo))]
sealed class AutoINCusSupportingInfoTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		var supportingInfo = Factory.New<DummyCusSupportingInfo>();
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(supportingInfo, "INCusSupportingInfo", schemaTypeName: nameof(AutoCusSupportingInfo.Schema));
	}
}

class DummyCusSupportingInfo : AutoINCusSupportingInfo
{
	public DummyCusSupportingInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}
}
