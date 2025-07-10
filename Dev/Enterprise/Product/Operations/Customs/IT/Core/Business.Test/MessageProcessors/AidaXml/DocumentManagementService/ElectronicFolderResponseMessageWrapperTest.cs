using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiesta_lista_documenti_dichiarazione;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.tipi;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ElectronicFolderResponseMessageWrapperTest : TestCase
{
	public void TestResponseStatusCode()
	{
		var response = new RichiestaDocumentiDichiarazione();
		var wrapper = new ElectronicFolderResponseMessageWrapper(response);
		AssertEquals("ResponseStatusCode", null, wrapper.ResponseStatusCode);

		response.Output = new OutputType { Esito = new Esito { } };
		wrapper = new ElectronicFolderResponseMessageWrapper(response);
		AssertEquals("ResponseStatusCode", null, wrapper.ResponseStatusCode);

		response.Output.Esito.CodiceErrore = "0";
		wrapper = new ElectronicFolderResponseMessageWrapper(response);
		AssertEquals("ResponseStatusCode", "0", wrapper.ResponseStatusCode);

		response.Output.Esito.CodiceErrore = " D_024   ";
		wrapper = new ElectronicFolderResponseMessageWrapper(response);
		AssertEquals("ResponseStatusCode", "D_024", wrapper.ResponseStatusCode);
	}

	public void TestArticleCdcCodes()
	{
		var response = new RichiestaDocumentiDichiarazione();
		var wrapper = new ElectronicFolderResponseMessageWrapper(response);
		var articleCdcCodes = wrapper.ArticleCdcCodes;
		AssertNotNull(nameof(wrapper.ArticleCdcCodes), articleCdcCodes);
		AssertEquals($"{nameof(wrapper.ArticleCdcCodes)} Count", 0, articleCdcCodes.Count);

		response.Output = new OutputType { Articoli = new Collection<ArticoloRichiesta>() };
		response.Output.Articoli.Add(new ArticoloRichiesta { CodiceEsitoCdc = "AB" });
		wrapper = new ElectronicFolderResponseMessageWrapper(response);
		articleCdcCodes = wrapper.ArticleCdcCodes;
		AssertNotNull(nameof(wrapper.ArticleCdcCodes), articleCdcCodes);
		AssertEquals($"{nameof(wrapper.ArticleCdcCodes)} Count", 1, articleCdcCodes.Count);
		AssertArrayEqualsByElements("Codes", new[] { "AB" }, articleCdcCodes.ToArray());

		response.Output.Articoli.Add(new ArticoloRichiesta { CodiceEsitoCdc = " PQ " });
		response.Output.Articoli.Add(new ArticoloRichiesta { CodiceEsitoCdc = "" });
		response.Output.Articoli.Add(new ArticoloRichiesta { CodiceEsitoCdc = "vm" });
		response.Output.Articoli.Add(new ArticoloRichiesta { CodiceEsitoCdc	 = "cd" });
		response.Output.Articoli.Add(new ArticoloRichiesta { CodiceEsitoCdc = null });

		wrapper = new ElectronicFolderResponseMessageWrapper(response);
		articleCdcCodes = wrapper.ArticleCdcCodes;
		AssertNotNull(nameof(wrapper.ArticleCdcCodes), articleCdcCodes);
		AssertEquals($"{nameof(wrapper.ArticleCdcCodes)} Count", 4, articleCdcCodes.Count);
		AssertArrayEqualsByElements("Codes", new[] { "AB", "PQ", "vm", "cd" }, articleCdcCodes.ToArray());
	}
}
