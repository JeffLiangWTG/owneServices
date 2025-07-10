using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiesta_lista_documenti_dichiarazione;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.tipi;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ControlChannelStrategyForCodeZeroTest : TestCase
{
	public void TestGetChannelCode_WithNoArticleInformation()
	{
		var response = new RichiestaDocumentiDichiarazione
		{
			Output = new OutputType
			{
				Esito = new Esito
				{
					CodiceErrore = ControlChannelStrategyForCodeZero.Code_Zero,
				}
			}
		};
		IControlChannelStrategy strategy = new ControlChannelStrategyForCodeZero(new ElectronicFolderResponseMessageWrapper(response));
		var controlChannel = strategy.GetControlChannel();
		AssertNullOrEmpty("ControlChannel", controlChannel);
	}

	public void TestGetChannelCode_WithMultipleArticleNodesAndSameCode()
	{
		CombineAssertions(() =>
		{
			AssertControlChannelForMultipleNodes("CD", "CD");
			AssertControlChannelForMultipleNodes("VM", "VM");
			AssertControlChannelForMultipleNodes("CS", "CS");
			AssertControlChannelForMultipleNodes("XX", null);
		});

		void AssertControlChannelForMultipleNodes(string actualCode, string expectedCode)
		{
			var response = new RichiestaDocumentiDichiarazione
			{
				Output = new OutputType
				{
					Articoli = new Collection<ArticoloRichiesta>
					{
						new ArticoloRichiesta { CodiceEsitoCdc = actualCode },
						new ArticoloRichiesta { CodiceEsitoCdc = actualCode },
						new ArticoloRichiesta { CodiceEsitoCdc = actualCode },
					},
					Esito = new Esito
					{
						CodiceErrore = ControlChannelStrategyForCodeZero.Code_Zero,
					}
				},
			};

			IControlChannelStrategy strategy = new ControlChannelStrategyForCodeZero(new ElectronicFolderResponseMessageWrapper(response));
			var controlChannel = strategy.GetControlChannel();
			AssertEquals($"ControlChannel for {actualCode}", expectedCode, controlChannel);
		}
	}

	public void TestGetChannelCode_WithMultipleArticleNodesAndDifferentCode()
	{
		CombineAssertions(() =>
		{
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "XX", "CD", "AB", "AB", "CD" }, "CD");
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "XX", "AB", "VM", "PQ", "AB", "CS" }, "VM");
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "XX", "CS", "PQ" }, "CS");
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "XX", "AB", "PQ" }, null);
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "XX", "VM", "CD" }, "VM");
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "CS", "XX", "CD" }, "CD");
			AssertControlChannelForMultipleNodesWithDifferentCodes(new[] { "CS", "VM", "CD", "CD", "CS", "VM" }, "VM");
		});

		void AssertControlChannelForMultipleNodesWithDifferentCodes(string[] codes, string expectedCode)
		{
			var response = new RichiestaDocumentiDichiarazione
			{
				Output = new OutputType
				{
					Articoli = new Collection<ArticoloRichiesta>(codes.Select(c => new ArticoloRichiesta { CodiceEsitoCdc = c }).ToList()),
					Esito = new Esito
					{
						CodiceErrore = ControlChannelStrategyForCodeZero.Code_Zero,
					}
				},
			};

			IControlChannelStrategy strategy = new ControlChannelStrategyForCodeZero(new ElectronicFolderResponseMessageWrapper(response));
			var controlChannel = strategy.GetControlChannel();
			AssertEquals($"ControlChannel for {string.Join(",", codes)}", expectedCode, controlChannel);
		}
	}

	public void TestConstructor()
	{
		var response = new RichiestaDocumentiDichiarazione
		{
			Output = new OutputType { Esito = new Esito { CodiceErrore = "D_024" } }
		};

		AssertExceptionThrown<InvalidOperationException>(() => new ControlChannelStrategyForCodeZero(new ElectronicFolderResponseMessageWrapper(response)));

		response.Output.Esito.CodiceErrore = "ABC";
		AssertExceptionThrown<InvalidOperationException>(() => new ControlChannelStrategyForCodeZero(new ElectronicFolderResponseMessageWrapper(response)));

		response.Output.Esito.CodiceErrore = null;
		AssertExceptionThrown<InvalidOperationException>(() => new ControlChannelStrategyForCodeZero(new ElectronicFolderResponseMessageWrapper(response)));
	}
}
