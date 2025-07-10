using System;
using CargoWise.Customs.IT.MessageDefinitions.SingleWindow;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowStatusCustomsResponseWrapperTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SingleWindowStatusCustomsResponseWrapper(null));
	}

	public void TestControlChannel()
	{
		AssertEquals(nameof(ISingleWindowStatusCustomsResponse.ControlChannel), "", customsResponseWrapper.ControlChannel);

		customsResponse.controllo_doganale = new controllo_doganale() { flag_ctrl_dog = flag_ctrl_dog.VM };
		AssertEquals(nameof(ISingleWindowStatusCustomsResponse.ControlChannel), "VM", customsResponseWrapper.ControlChannel);
	}

	public void TestReleaseCode()
	{
		AssertEquals(nameof(ISingleWindowStatusCustomsResponse.ReleaseCode), "", customsResponseWrapper.ReleaseCode);

		customsResponse.svincolo = new svincolo() { cod_svincolo = "XXYYZZ" };
		AssertEquals(nameof(ISingleWindowStatusCustomsResponse.ReleaseCode), "XXYYZZ", customsResponseWrapper.ReleaseCode);
	}

	public void TestReleaseDate()
	{
		AssertEquals(nameof(ISingleWindowStatusCustomsResponse.ReleaseDate), ZDateTime.Empty, customsResponseWrapper.ReleaseDate);

		customsResponse.svincolo = new svincolo() { data_codice_svincolo = new DateTime(2020, 01, 01) };
		AssertEquals(nameof(ISingleWindowStatusCustomsResponse.ReleaseDate), new DateTime(2020, 01, 01), customsResponseWrapper.ReleaseDate);
	}

	protected override void SetUp()
	{
		base.SetUp();
		customsResponse = new esito_bolletta();
		customsResponseWrapper = new SingleWindowStatusCustomsResponseWrapper(customsResponse);
	}

	esito_bolletta customsResponse;
	ISingleWindowStatusCustomsResponse customsResponseWrapper;
}
