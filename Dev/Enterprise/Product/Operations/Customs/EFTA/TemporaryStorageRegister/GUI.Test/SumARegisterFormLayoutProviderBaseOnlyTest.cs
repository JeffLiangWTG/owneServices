using System;
using System.Collections;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(SumARegisterFormLayoutProvider))]
sealed class SumARegisterFormLayoutProviderBaseOnlyTest : SumARegisterFormLayoutProviderAbstractTest<SumARegisterFormLayoutProvider>
{
	public void TestGetLayoutProvider() => CombineAssertions(() =>
	{
		var provider = SumARegisterFormLayoutProvider.GetLayoutProvider();
		AssertType<SumARegisterFormLayoutProvider>("Provider for default", provider);

		var mockProvider = new Mock<ISumARegisterFormLayoutProvider>();
		var objectHandleMock = new Mock<ObjectHandle>();
		objectHandleMock.Setup(m => m.GetObject()).Returns(mockProvider.Object);
		var dict = new Hashtable { { "FR", objectHandleMock.Object } };

		using (ObjectFactory.Substitute("SumARegisterFormLayoutProviders", dict))
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry("FR");
			provider = SumARegisterFormLayoutProvider.GetLayoutProvider();
			AssertSame("Provider for FR", mockProvider.Object, provider);
		}
	});

	protected override Type ExpectedDetailsHeaderLayoutType => typeof(DetailsHeaderLayout);

	protected override Type ExpectedLinesDetailsLayoutType => typeof(LinesDetailsLayout);
}
