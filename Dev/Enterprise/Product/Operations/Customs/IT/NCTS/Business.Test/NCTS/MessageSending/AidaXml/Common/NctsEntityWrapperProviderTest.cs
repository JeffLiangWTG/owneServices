using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsEntityWrapperProvider))]
sealed class NctsEntityWrapperProviderTest : TestCaseWithFactory
{
	public void TestGetNctsDepartureMovementHeaderWrapper_WhenNctsMovementHeaderIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>(() => provider.GetNctsDepartureMovementHeaderWrapper(null));
	}

	public void TestGetNctsDepartureMovementHeaderWrapper_Type()
	{
		AssertType<NctsDepartureMovementHeaderWrapper>(provider.GetNctsDepartureMovementHeaderWrapper(Factory.New<NctsDepartureMovementHeader>()));
	}

	public void TestGetNctsHeaderWrapper_WhenNctsHeaderIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>(() => provider.GetNctsHeaderWrapper(null));
	}

	public void TestGetNctsHeaderWrapper_Type()
	{
		AssertType<NctsHeaderWrapper>(provider.GetNctsHeaderWrapper(Factory.New<NctsHeader>()));
	}

	protected override void SetUp()
	{
		base.SetUp();
		provider = new NctsEntityWrapperProvider();
	}

	INctsEntityWrapperProvider provider;
}
