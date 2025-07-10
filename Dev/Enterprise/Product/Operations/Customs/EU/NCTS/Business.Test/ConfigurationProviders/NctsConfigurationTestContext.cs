using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public class NctsConfigurationTestContext(BusinessObjectFactory factory) : ConfigurationTestContext<NctsConfiguration>(factory)
{
	protected override IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<NctsConfiguration> nctsConfigurationMock)
	{
		var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		factory.ClearCachedValue<NctsConfiguration>($"NctsConfiguration_{currentCountryCode}");

		var objectHandleMock = new Mock<ObjectHandle>();
		objectHandleMock.Setup(m => m.GetObject()).Returns(nctsConfigurationMock.Object);

		var nctsConfiguration = new KeyObjectHandleDictionaryObject
		{
			{ currentCountryCode, objectHandleMock.Object },
		};

		return ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration);
	}
}
