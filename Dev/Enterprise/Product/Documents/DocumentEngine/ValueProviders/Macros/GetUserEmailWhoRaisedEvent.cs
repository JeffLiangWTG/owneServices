using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	public class GetUserEmailWhoRaisedEvent : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetUserEmailWhoRaisedEvent(\"{eventCode}\",\"{eventReference}\"[,\"{businessObjectPk}\"])>",
				ResString.GetMultilingualString("507428e4-25ac-4476-a4ac-1026a18b7d52", @"Returns the email address of the user who created the most recent actual event that is not canceled. It also meets the following criteria.
- {0}: The code of the event.
- {1}: The reference text of the event. ""*"" and ""?"" wildcards are supported.
- {2} (optional): The primary key of the business object whose events should be searched. If it is empty or not provided, The primary key of the primary data provider of the document will be used.",
"eventCode", "eventReference", "businessObjectPk"),
				new List<(string example, object expectedResult)> {
					($"<GetUserEmailWhoRaisedEvent(\"{AutoEvents.AddedARecordToTheSystemCode}\",\"Confirmed*\")>", "test1@test1.com"),
					($"<GetUserEmailWhoRaisedEvent(\"{AutoEvents.IncidentClosedCode}\",\"Confirmed*\", \"<JS_PK>\")>", "test2@test2.com")
				});
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var match = Regex.Match(macro);

			if (match.Success)
			{
				var eventCode = match.Groups["EventCode"].Value;
				var eventReference = match.Groups["EventReference"].Value;
				var businessObjectPkString = match.Groups["BusinessObjectPk"].Value;
				var businessObject = report.BODocDataProvider != null ? report.BODocDataProvider.ParentBusinessObject : null;
				ZGuid businessObjectPK = ZGuid.Empty;
				if (ZGuid.TryParse(businessObjectPkString, out businessObjectPK) || businessObject != null)
				{
					var query = new ZQuery { OrderBy = AutoStmALog.Schema.SL_EventTime + OrderByClause.Descending };
					query.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode));
					query.AddToFilter(new ZQuery(StmALogSchema.SL_IsEstimate, "N"));
					query.AddToFilter(new ZQuery(StmALogSchema.SL_IsCancelled, "N"));
					query.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, businessObjectPK.IsEmpty && businessObject != null ? businessObject.PK : businessObjectPK));
					var factory = businessObject?.Factory ?? new BusinessObjectFactory();
					var log = factory.Load<StmALog>(query).FirstOrDefault(GetQuery(eventReference));
					if (log != null)
					{
						var user = log.User;
						if (user != null)
						{
							result = user.GS_EmailAddress;
						}
					}
				}
			}

			return result;
		}

		Func<StmALog, bool> GetQuery(ZString eventReference)
		{
			return log =>
			{
				if (eventReference.ContainsAnyChar("*?"))
				{
					var regex = TriggerConditionRegexProvider.GetEventReferenceWithWildcardsRegex(eventReference);
					return regex.Match(log.SL_ReferenceForBinding).Success;
				}
				else
				{
					return log.SL_ReferenceForBinding == eventReference;
				}
			};
		}

		static readonly Regex regex = new Regex(@"^<\s*GetUserEmailWhoRaisedEvent\s*\(\s*""(?<EventCode>\w*?)"",\s*""(?<EventReference>.*?)""(?:,\s*""(?<BusinessObjectPk>.*?)"")?\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
