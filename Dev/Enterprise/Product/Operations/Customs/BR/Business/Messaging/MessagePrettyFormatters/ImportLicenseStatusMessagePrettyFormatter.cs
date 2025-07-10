using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseStatusMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		public ImportLicenseStatusMessagePrettyFormatter(CusEntryHeader cusEntryHeader, licompletatype li)
		{
			this.cusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			this.li = Argument.NotNull(li, nameof(li));
		}

		readonly CusEntryHeader cusEntryHeader;
		readonly licompletatype li;

		public ZString GetFormattedMessageText()
		{
			return GetHtmlWithTemplateEmptyWithDynamicHtml5(
				$"Job Number: {GetJobLink(cusEntryHeader)}<br/>Import License Number: {li.GrupoDadosBasicos.numeroli}<br/>Import License Status: {li.GrupoLIAnuencias.InformacoesLI.nomesituacao}<br/>",
				ResponseFromBrazilianCustoms,
				ReceiveFollowingUpdates,
				GetHtmlTableForContent(li.GrupoLIAnuencias.listaanuencias),
				(NoResString)"<br/>");
		}

		string GetHtmlTableForContent(listaanuenciastypeAnuencia[] listaanuencias)
		{
			var tableCreator = GetNewTableCreator(ColumnHeadings);

			foreach (var anuencias in listaanuencias)
			{
				tableCreator.WriteRow(anuencias.orgaoanuente
					, $"{anuencias.codigosituacaoanuencia} - {anuencias.nomesituacaoanuencia}"
					, $"{BRMessageHelper.GetFormattedDate(anuencias.datadiagnosticoanuencia)} {anuencias.horadiagnosticoanuencia}"
					, anuencias.datarestricaoembarque
					, BRMessageHelper.GetFormattedDate(anuencias.datavalidadeembarque)
					, BRMessageHelper.GetFormattedDate(anuencias.datavalidadedespacho)
					, $"{anuencias.codigotratamentoadministrativo} - {anuencias.nometratamentoadministrativo}"
					, anuencias.textoanuente);
			}

			return tableCreator.ToHtml();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column headings")]
		static string[] ColumnHeadings => new string[] { "Consenting Body", "Status", "Diagnosis Date", "Shipment Restricted Until", "Shipment Valid Until", "Dispatch Valid Until", "Administrative Status", "Consenting Body Statements" };
	}
}
