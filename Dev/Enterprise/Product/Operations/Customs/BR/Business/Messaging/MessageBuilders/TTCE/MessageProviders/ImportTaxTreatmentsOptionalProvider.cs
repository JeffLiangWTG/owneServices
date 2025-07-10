using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Interfaces.TTCE.Outgoing;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;

namespace Enterprise.Customs.BR.Business
{
	public class ImportTaxTreatmentsOptionalProvider : IImportTaxTreatments
	{
		public ImportTaxTreatmentsOptionalProvider(RespostaObterTratamentosTributariosImportacaoDTO response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		readonly RespostaObterTratamentosTributariosImportacaoDTO response;

		public string Ncm => response.ncm;

		public int CountryCode => (int)response.codigoPais;

		public DateTime TaxEventDate => DateTime.TryParse(response.dataFatoGerador, out var result) ? result : new DateTime();

		public string OperationType => response.tipoOperacao;

		public IEnumerable<IOptionalLegalBasis> OptionalLegalBasisList => response.fundamentosOpcionaisDisponiveis?.Select(OptionalLegalBasisProvider.New);
	}
}
