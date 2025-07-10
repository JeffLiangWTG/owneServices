using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ReleaseAdditionalDocumentWrapper))]
sealed class ReleaseAdditionalDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => new ReleaseAdditionalDocumentWrapper(Factory.New<CusSupportingInfo>());

	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new ReleaseAdditionalDocumentWrapper(null));

	public void TestType() => AssertEquals("100", provider.Type);

	public void TestReference() => AssertEquals("REF123456", provider.Reference);

	public void TestQuantity() => AssertEquals(5m, provider.Quantity);

	public void TestValue() => AssertEquals(25.3m, provider.Value);

	protected override void SetUp()
	{
		base.SetUp();
		var cusSupportingInfo = Factory.New<CusSupportingInfo>();
		cusSupportingInfo.CSI_Code = "100";
		cusSupportingInfo.CSI_ReferenceNumber = "REF123456";
		cusSupportingInfo.CSI_Quantity = 5;
		cusSupportingInfo.CSI_Value = 25.3;
		provider = new ReleaseAdditionalDocumentWrapper(cusSupportingInfo);
	}
	ReleaseAdditionalDocumentWrapper provider;
}
