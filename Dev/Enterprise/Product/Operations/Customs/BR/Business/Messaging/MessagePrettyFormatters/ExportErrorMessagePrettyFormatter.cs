using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ExportErrorMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		public ExportErrorMessagePrettyFormatter(CusEntryHeader cusEntryHeader, error dueErrorXML)
		{
			this.cusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			this.dueErrorXML = Argument.NotNull(dueErrorXML, nameof(dueErrorXML));
		}

		readonly CusEntryHeader cusEntryHeader;
		readonly error dueErrorXML;

		public ZString GetFormattedMessageText()
		{
			return GetHtmlWithTemplateEmptyWithDynamicHtml5(
				$"<br/>Job Number: {GetJobLink(cusEntryHeader)}",
				ResponseFromBrazilianCustoms,
				ReceiveFollowingUpdates,
				GetHtmlTableForErrorContent(dueErrorXML) + (NoResString)"<br/>",
				GetHtmlTableForErrorInfoContent(dueErrorXML.info),
				(NoResString)"<br/><hr/><br/>");
		}

		string GetHtmlTableForErrorContent(error fullError)
		{
			var tableCreator = GetNewTableCreator(ColumnHeadingsError);
			var errors = fullError.detail;

			if (errors.Length > 0)
			{
				foreach (var error in errors)
				{
					tableCreator.WriteRow(error.code
						, error.message
						, error.tag
						, error.date
						, error.status
						, error.severity);
				}
			}
			else
			{
				tableCreator.WriteRow(""
					, fullError.message
					, fullError.tag
					, fullError.date
					, fullError.status
					, fullError.severity);
			}

			return tableCreator.ToHtml();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column headings")]
		static string[] ColumnHeadingsError => new string[] { "code", "message", "tag", "date", "status", "severity" };

		string GetHtmlTableForErrorInfoContent(errorInfo errorInfo)
		{
			var tableCreator = GetNewTableCreator(ColumnHeadingsInfo);

			tableCreator.WriteRow(errorInfo.ambiente
				, errorInfo.mnemonico
				, errorInfo.sistema
				, errorInfo.trackerId
				, errorInfo.url
				, errorInfo.visao);

			return tableCreator.ToHtml();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column headings")]
		static string[] ColumnHeadingsInfo => new string[] { "ambiente", "mnemonico", "sistema", "trackerId", "url", "visao" };
	}
}
