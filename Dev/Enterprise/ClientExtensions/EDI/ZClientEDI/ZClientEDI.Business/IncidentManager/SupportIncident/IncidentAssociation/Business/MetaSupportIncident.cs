using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class MetaSupportIncident : IMetaSupportIncident
	{
		public Guid PK { get; }
		public string IncidentNumber { get; }
		public DateTime SystemCreateTimeUtc { get; }
		public DateTime SystemLastEditTimeUtc { get; }
		public string Product { get; }
		public string ProgramArea { get; }
		public string Module { get; }
		public string Priority { get; }
		public string Country { get; }
		public string Description { get; }
		public string NoteText { get; }
		public IReadOnlyCollection<MetaConversationItem> ConversationItems { get; }
		public IReadOnlyCollection<MetaWorkItem> WorkItems { get; }

		public MetaSupportIncident(Dictionary<string, object> supportIncident, IEnumerable<MetaWorkItem> workItems, IEnumerable<MetaConversationItem> conversationItems)
		{
			PK = (Guid)supportIncident[AutoIncidentMain.Schema.PK];
			IncidentNumber = supportIncident[AutoIncidentMain.Schema.IM_IncidentNumber] as string ?? string.Empty;
			SystemCreateTimeUtc = (DateTime)supportIncident[AutoIncidentMain.Schema.IM_SystemCreateTimeUtc];
			SystemLastEditTimeUtc = (DateTime)supportIncident[AutoIncidentMain.Schema.IM_SystemLastEditTimeUtc];
			Product = supportIncident[AutoIncidentMain.Schema.IM_Product]?.ToString() ?? string.Empty;
			ProgramArea = supportIncident[AutoIncidentMain.Schema.IM_ProgramArea]?.ToString() ?? string.Empty;
			Module = supportIncident[AutoIncidentMain.Schema.IM_Module]?.ToString() ?? string.Empty;
			Priority = supportIncident[AutoIncidentMain.Schema.IM_Priority]?.ToString() ?? string.Empty;
			Country = supportIncident[AutoIncidentMain.Schema.IM_RN_NKCountry]?.ToString() ?? string.Empty;
			Description = supportIncident[AutoIncidentMain.Schema.IM_Description]?.ToString() ?? string.Empty;
			NoteText = supportIncident["ST_Description"]?.ToString() ?? string.Empty;

			ConversationItems = conversationItems.ToList();
			WorkItems = workItems.ToList();
		}

		public override string ToString()
		{
			var pieces = new List<string>()
			{
				Regex.Replace($"{Product}productcode", @"[\s&]", string.Empty),
				Regex.Replace($"{ProgramArea}programarea", @"[\s&]", string.Empty),
				Regex.Replace($"{Module}module", @"[\s&]", string.Empty),
				Regex.Replace($"{Priority}criticality", @"[\s&]", string.Empty),
				Regex.Replace($"{Country}country", @"[\s&]", string.Empty),
				Description,
				NoteText
			};

			foreach (var ci in ConversationItems)
			{
				pieces.Add(ci.Body);
			}

			foreach (var wi in WorkItems)
			{
				pieces.Add(wi.Summary);
				pieces.Add(wi.Details);
			}

			return string.Join(" ", pieces);
		}
	}

	public class MetaWorkItem
	{
		public Guid PK { get; }
		public DateTime SystemCreateTimeUtc { get; }
		public string WorkItemNumber { get; }
		public string Summary { get; }
		public string Details { get; }

		public MetaWorkItem(DynamicBusinessObject workItem)
		{
			PK = (Guid)workItem[AutoWorkItem.Schema.PK];
			SystemCreateTimeUtc = (DateTime)workItem[AutoWorkItem.Schema.WKI_SystemCreateTimeUtc];
			WorkItemNumber = workItem[AutoWorkItem.Schema.WKI_WorkItemNumber]?.ToString() ?? string.Empty;
			Summary = workItem[AutoWorkItem.Schema.WKI_Summary]?.ToString() ?? string.Empty;
			Details = workItem[$"PlainText{AutoWorkItem.Schema.WKI_Details}"]?.ToString() ?? string.Empty;
		}

		public MetaWorkItem(Dictionary<string, object> workItem)
		{
			PK = (Guid)workItem[AutoWorkItem.Schema.PK];
			SystemCreateTimeUtc = (DateTime)workItem[AutoWorkItem.Schema.WKI_SystemCreateTimeUtc];
			WorkItemNumber = workItem[AutoWorkItem.Schema.WKI_WorkItemNumber]?.ToString() ?? string.Empty;
			Summary = workItem[AutoWorkItem.Schema.WKI_Summary]?.ToString() ?? string.Empty;
			Details = workItem[$"PlainText{AutoWorkItem.Schema.WKI_Details}"]?.ToString() ?? string.Empty;
		}
	}

	public class MetaConversationItem
	{
		public string Body { get; }
		public DateTime PostedTimeUtc { get; }

		public MetaConversationItem(DynamicBusinessObject conversationItem)
		{
			Body = conversationItem[AutoJobConversationMessage.Schema.JCM_Body]?.ToString() ?? string.Empty;
			PostedTimeUtc = (DateTime)conversationItem[AutoJobConversationMessage.Schema.JCM_PostedTimeUtc];
		}

		public MetaConversationItem(Dictionary<string, object> conversationItem)
		{
			Body = conversationItem[AutoJobConversationMessage.Schema.JCM_Body]?.ToString() ?? string.Empty;
			PostedTimeUtc = (DateTime)conversationItem[AutoJobConversationMessage.Schema.JCM_PostedTimeUtc];
		}
	}
}
