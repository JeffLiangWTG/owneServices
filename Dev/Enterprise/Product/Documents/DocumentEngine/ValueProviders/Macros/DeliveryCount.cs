using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class DeliveryCount : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DeliveryCount>", ResString.GetMultilingualString("2db52d97-ec22-4eef-aca3-260534601b1f", "Returns the number of times this document has been delivered."),
				new List<(string example, object expectedResult)> { ("<DeliveryCount>", 1) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			int result = 0;

			BusinessObject bizo = report.Parent.BusinessObjectToLogAgainst;
			if (bizo != null)
			{
				ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, report.DocumentDeliveredEventCode);
				ZString reference = ((ZString)(report.Parent.StmMenuCommand.SU_MenuName + "/" + report.Name)).Left(StmALog.Schema.SL_ReferenceMaxLength);
				query.AddToFilter(StmALogSchema.SL_Reference, reference);

				StmALog[] logs = bizo.GetLogs().Find(query);
				result = logs.Length;
			}

			return result + 1;
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)DeliveryCount(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
