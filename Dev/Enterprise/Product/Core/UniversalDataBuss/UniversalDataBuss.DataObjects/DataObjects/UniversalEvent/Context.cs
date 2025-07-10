using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class Context : IDataObject
	{
		public static Context GetContextFromLogs(ContextType type, IEnumerable<ISimpleLog> logs)
		{
			var context = new Context()
			{
				Type = type,
				Value = GetFormattedStringFromLogs(logs)
			};
			context.logs = logs;
			return context;
		}

		[Mandatory]
		public ContextType Type { get; set; }
		[Mandatory, MaxLength(ValueMaxLength), AllowLineControlWhiteSpace, TrimWhiteSpace]
		public ZString? Value { get; set; }

		public List<Context> SubContextCollection { get; set; }

		public const int ValueMaxLength = UniversalXmlInfo.MaxStringLength;

		public ZString GetErrorLog() => logs == null ? ZString.Empty : GetFormattedStringFromLogs(logs.Where(l => l.Type == LogType.Error));
		IEnumerable<ISimpleLog> logs;

		static ZString GetFormattedStringFromLogs(IEnumerable<ISimpleLog> logs)
		{
			return string.Join("\r\n", logs.Select(log => log.Type == LogType.Information ?
				log.Message :
				string.Join("\r\n", log.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
			)));
		}
	}
}
