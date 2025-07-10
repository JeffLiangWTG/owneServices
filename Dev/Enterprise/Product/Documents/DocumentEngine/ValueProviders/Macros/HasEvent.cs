using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class HasEvent : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<HasEvent(EventCode)>, <HasEvent(BusinessObject,EventCode)>",
				ResString.GetMultilingualString("0dc51e2b-11f3-4d4f-bab6-7515d86b97e5",
				@"Checks if a business object has an event of specified event code occurred. (Neither Canceled nor estimated). It will return ""Y"" if event log exists, otherwise ""N"".
If only event code is provided, it will do the checking on the primary data provider of the document."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<HasEvent (EMS)>", "N"),
					((NoResString)"<HasEvent (Consignor, EMS)>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var macroMatch = Regex.Match(macro).Groups[1].Value;
			var parameters = macroMatch.Split(new char[] { ',' });
			var bizOString = parameters.Length > 1 ? parameters[0] : string.Empty;
			var destBizO = ValueProviderHelper.GetBusinessObjectFromDataProvider(bizOString, report) as BusinessObject;

			if (destBizO == null)
			{
				ReportMacroError(report, Res.GetString("FFA4DDD0-5517-4447-9401-C9A13DC3E8CC", "Couldn't find business object for checking Has Event."));
				return null;
			}
			else
			{
				var eventCode = parameters.Length > 1 ? parameters[1] : parameters[0];

				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Parent, destBizO.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
				query.AddToFilter(StmALogSchema.SL_IsEstimate, false);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

				return destBizO.Factory.Exists(typeof(StmALog), query) ? "Y" : "N";
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)HasEvent(?:[\s]*)\((?:[\s]*)(\s*([\w|\.]+)\s*(,(?:[\s]*)[\w|\.]+)?\s*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
