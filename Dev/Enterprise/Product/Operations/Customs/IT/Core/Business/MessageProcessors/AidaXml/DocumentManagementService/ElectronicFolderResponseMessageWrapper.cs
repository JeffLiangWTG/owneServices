using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiesta_lista_documenti_dichiarazione;
using CargoWise.Customs.Shared.MessageContracts;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.Business;

sealed class ElectronicFolderResponseMessageWrapper
{
	public ElectronicFolderResponseMessageWrapper(RichiestaDocumentiDichiarazione response)
	{
		this.response = Argument.NotNull(response, nameof(response));
	}

	public string ResponseStatusCode => response.Output?.Esito?.CodiceErrore?.Trim();

	public IReadOnlyCollection<string> ArticleCdcCodes => articleCdcCodes ?? (articleCdcCodes = GetArticleCdcCodes());
	IReadOnlyCollection<string> articleCdcCodes;

	#region Implementation

	IReadOnlyCollection<string> GetArticleCdcCodes()
	{
		var codes = response.Output
			?.Articoli
			?.Select(a => a.CodiceEsitoCdc?.Trim())
			?.Where(a => !string.IsNullOrWhiteSpace(a))
			?.ToArray();

		return (codes ?? Array.Empty<string>()).ToCollection();
	}

	#endregion

	readonly RichiestaDocumentiDichiarazione response;
}
