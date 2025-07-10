using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ExportSuccessMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		public ExportSuccessMessagePrettyFormatter(CusEntryHeader cusEntryHeader, pucomexReturn messageObject)
		{
			this.cusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			this.messageObject = Argument.NotNull(messageObject, nameof(messageObject));
		}

		readonly CusEntryHeader cusEntryHeader;
		readonly pucomexReturn messageObject;

		public ZString GetFormattedMessageText()
		{
			return GetHtmlWithTemplateEmptyWithDynamicHtml5(
				$"Job Number: {GetJobLink(cusEntryHeader)}<br/><br/>",
				ResponseFromBrazilianCustoms,
				ReceiveFollowingUpdates,
				GetHtmlTableForContent(),
				(NoResString)"<br/>");
		}

		string GetHtmlTableForContent()
		{
			var titles = new List<string>();
			var values = new List<string>();

			foreach (var (heading, getValue) in Columns)
			{
				var fieldValue = getValue.Invoke(messageObject);

				if (!string.IsNullOrEmpty(fieldValue))
				{
					titles.Add(heading);
					values.Add(fieldValue);
				}
			}

			var tableCreator = GetNewTableCreator(titles);
			tableCreator.WriteRow(values.ToArray());

			return tableCreator.ToHtml();
		}

		static List<(string heading, Func<pucomexReturn, string> getValue)> Columns => new List<(string, Func<pucomexReturn, string>)>()
		{
			(nameof(pucomexReturn.message), p => p.message),
			(nameof(pucomexReturn.due), p => p.due),
			(nameof(pucomexReturn.ruc), p => p.ruc),
			(nameof(pucomexReturn.chaveDeAcesso), p => p.chaveDeAcesso),
			(nameof(pucomexReturn.date), p => p.date),
			(nameof(pucomexReturn.cpf), p => p.cpf),
		};
	}
}
