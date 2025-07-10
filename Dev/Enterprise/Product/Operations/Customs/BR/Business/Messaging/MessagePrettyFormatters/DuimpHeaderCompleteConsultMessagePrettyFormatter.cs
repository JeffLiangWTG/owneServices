using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Duimp;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpHeaderCompleteConsultMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		public DuimpHeaderCompleteConsultMessagePrettyFormatter(CusEntryHeader cusEntryHeader, DuimpConsultaCover messageObject)
		{
			this.cusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			this.messageObject = Argument.NotNull(messageObject, nameof(messageObject));
		}

		readonly CusEntryHeader cusEntryHeader;
		readonly DuimpConsultaCover messageObject;

		public ZString GetFormattedMessageText()
		{
			return GetHtmlWithTemplateEmptyWithDynamicHtml5(
				$"Job Number: {GetJobLink(cusEntryHeader)}",
				ResponseFromBrazilianCustoms,
				$"{ReceiveFollowingUpdates}",
				GetContent(),
				"");
		}

		string GetContent()
		{
			var content = new ZStringBuilder();
			content.Append((NoResString)"<br/>");
			foreach (var (tableSection, headings, getValue) in Columns)
			{
				var fieldValue = getValue.Invoke(messageObject);
				if (fieldValue?.Any() ?? false)
				{
					content.Append($"{tableSection}:<br/><br/>");
					var table = GetNewTableCreator(headings);
					foreach (var values in fieldValue)
					{
						table.WriteRow(cellAttributes, values);
					}
					content.Append(table.ToHtml());
					content.Append((NoResString)"<br/>");
				}
			}

			return content.ToString();
		}

		static List<(string tableSection, string[] headings, Func<DuimpConsultaCover, IEnumerable<string[]>> getValue)> Columns => new List<(string, string[], Func<DuimpConsultaCover, IEnumerable<string[]>>)>()
		{
			(nameof(DuimpConsultaCover.identificacao), new[] { nameof(DuimpConsultaCover.identificacao.numero), nameof(DuimpConsultaCover.identificacao.versao) },
				p => new[] { new[] { p.identificacao?.numero, p.identificacao?.versao } }),
			(nameof(DuimpConsultaCover.situacao), new[] { nameof(DuimpConsultaCover.situacao.situacaoDuimp), nameof(DuimpConsultaCover.situacao.situacaoAnaliseRetificacao), nameof(DuimpConsultaCover.situacao.situacaoLicenciamento), nameof(DuimpConsultaCover.situacao.controleCarga) },
				p => new[] { new[] { p.situacao?.situacaoDuimp, p.situacao?.situacaoAnaliseRetificacao, p.situacao?.situacaoLicenciamento, p.situacao?.controleCarga } }),
			(nameof(DuimpConsultaCover.situacao.situacaoConferenciaAduaneira), new[] { nameof(SituacaoConferenciaAduaneiraCover.siglaOrgao), nameof(SituacaoConferenciaAduaneiraCover.situacao), nameof(SituacaoConferenciaAduaneiraCover.indicadorAutorizacaoEntrega), nameof(SituacaoConferenciaAduaneiraCover.indicadorDesembaracoDecisaoJudicial) },
				p => p.situacao?.situacaoConferenciaAduaneira?.Select(s => new[] { s.siglaOrgao, s.situacao, s.indicadorAutorizacaoEntrega, s.indicadorDesembaracoDecisaoJudicial }).ToArray()),
			(nameof(DuimpConsultaCover.situacao.situacaoConferenciaAnuente), new[] { nameof(SituacaoConferenciaAnuenteCover.siglaOrgao), nameof(SituacaoConferenciaAnuenteCover.situacao), nameof(SituacaoConferenciaAnuenteCover.indicadorAutorizacaoProsseguimentoConferenciaAnuente), nameof(SituacaoConferenciaAnuenteCover.indicadorConclusaoDecisaoJudicial) },
				p => p.situacao?.situacaoConferenciaAnuente?.Select(s => new[] { s.siglaOrgao, s.situacao, s.indicadorAutorizacaoProsseguimentoConferenciaAnuente, s.indicadorConclusaoDecisaoJudicial }).ToArray()),
			(nameof(DuimpConsultaCover.resultadoAnaliseRisco), new[] { nameof(DuimpConsultaCover.resultadoAnaliseRisco.canalConsolidado) },
				p => new[] { new[] { p.resultadoAnaliseRisco?.canalConsolidado } }),
			(nameof(DuimpConsultaCover.resultadoAnaliseRisco.resultadoRFB), new[] { nameof(ResultadoAnaliseRiscoRfbCover.orgao), nameof(ResultadoAnaliseRiscoRfbCover.resultado) },
				p => p.resultadoAnaliseRisco?.resultadoRFB?.Select(s => new[] { s.orgao, s.resultado }).ToArray()),
			(nameof(DuimpConsultaCover.resultadoAnaliseRisco.resultadoAnuente), new[] { nameof(ResultadoAnaliseRiscoAnuenteCover.orgao), nameof(ResultadoAnaliseRiscoAnuenteCover.resultado) },
				p => p.resultadoAnaliseRisco?.resultadoAnuente?.Select(s => new[] { s.orgao, s.resultado }).ToArray()),
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Cell attributes")]
		readonly NameValueCollection cellAttributes = new NameValueCollection {
			{ "class", "table" },
			TableInterpretation.Attributes.AlignCenter,
		};
	}
}
