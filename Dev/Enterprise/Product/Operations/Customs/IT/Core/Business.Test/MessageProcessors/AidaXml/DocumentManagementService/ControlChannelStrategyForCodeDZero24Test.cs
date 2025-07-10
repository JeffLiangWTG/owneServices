using System;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiesta_lista_documenti_dichiarazione;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.tipi;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ControlChannelStrategyForCodeDZero24Test : TestCase
{
	public void TestGetChannelCode()
	{
		var response = new RichiestaDocumentiDichiarazione
		{
			Output = new OutputType { Esito = new Esito { CodiceErrore = "D_024" } }
		};
		IControlChannelStrategy strategy = new ControlChannelStrategyForCodeDZero24(new ElectronicFolderResponseMessageWrapper(response));
		var controlChannel = strategy.GetControlChannel();
		AssertEquals("ControlChannel", "CA", controlChannel);
	}

	public void TestConstructor()
	{
		var response = new RichiestaDocumentiDichiarazione
		{
			Output = new OutputType { Esito = new Esito { CodiceErrore = "0" } }
		};

		AssertExceptionThrown<InvalidOperationException>(() => new ControlChannelStrategyForCodeDZero24(new ElectronicFolderResponseMessageWrapper(response)));

		response.Output.Esito.CodiceErrore = "ABC";
		AssertExceptionThrown<InvalidOperationException>(() => new ControlChannelStrategyForCodeDZero24(new ElectronicFolderResponseMessageWrapper(response)));

		response.Output.Esito.CodiceErrore = null;
		AssertExceptionThrown<InvalidOperationException>(() => new ControlChannelStrategyForCodeDZero24(new ElectronicFolderResponseMessageWrapper(response)));
	}
}
