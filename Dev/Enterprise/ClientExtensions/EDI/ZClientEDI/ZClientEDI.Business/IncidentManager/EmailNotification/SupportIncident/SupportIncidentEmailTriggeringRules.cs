using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public class SupportIncidentEmailTriggeringRules : IEDIEmailTriggeringRules
	{
		public SupportIncidentEmailTriggeringRules(SupportIncident incident)
		{
			this.DataSource = incident;
		}

		public EnterpriseBusinessObject DataSource { get; }

		public bool Allow(IEDIEmailTemplateBuilder emailTemplateBuilder)
		{
			if (emailTemplateBuilder != null)
			{
				var templateCode = emailTemplateBuilder.GetIEDIEmailTemplate()?.TemplateCode ?? ZString.Empty;
				return AllowGeneratingByCode(DataSource, templateCode);
			}
			return false;
		}

		#region Rules

		public static bool AllowGeneratingByCode(BusinessObject businessObject, ZString code)
		{
			return !GetBLNTags(businessObject, false, out _).Any(x => x.TagMagnitude.TGM_Code == code || x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
		}

		public static bool IsAllSuppressed(BusinessObject businessObject)
		{
			return GetBLNTags(businessObject, false, out _).Any(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
		}

		public static IncidentEmailTagRuleStatus GetTagRuleStatus(BusinessObject businessObject, ZString code)
		{
			var tag = GetBLNTags(businessObject, false, out var jobHeader).FirstOrDefault(x => x.TagMagnitude.TGM_Code == code);
			if (jobHeader == null)
			{
				return IncidentEmailTagRuleStatus.NotExists;
			}

			if (tag == null)
			{
				if (jobHeader.Logs.LogsNotInDB.Any(x =>
					x.SL_SE_NKEvent == Enterprise.ZArchitecture.Business.AutoEvents.TagWasAddedOrRemoved.Code &&
					x.Parameters.TryGetValue(BMConstants.TagEventParameters.Action, out var tagAction) && tagAction == TagActionType.RemoveTag.ToCode() &&
					x.Parameters.TryGetValue(BMConstants.TagEventParameters.Tag, out var tagCode) && tagCode == code &&
					x.Parameters.TryGetValue(BMConstants.TagEventParameters.TagGroup, out var tagGroup) && tagGroup == TagGroupCode))
				{
					return IncidentEmailTagRuleStatus.Deleted;
				}
				else
				{
					return IncidentEmailTagRuleStatus.NotExists;
				}
			}
			else
			{
				if (tag.IsDeleted)
				{
					return IncidentEmailTagRuleStatus.Deleted;
				}
				else
				{
					return tag.IsInDatabase ? IncidentEmailTagRuleStatus.Existing : IncidentEmailTagRuleStatus.New;
				}
			}
		}

		public static bool SuppressAll(BusinessObject businessObject)
		{
			if (GetBLNTags(businessObject, true, out var jobHeader).Any(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification))
			{
				return true;
			}
			else if (jobHeader != null)
			{
				var tagDefinition = jobHeader.Factory.LoadTop1<TagDefinition>(new ZQuery(TagDefinitionSchema.TGD_Code, TagGroupCode));
				var magnitude = tagDefinition?.Magnitudes.FirstOrDefault(x => x.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification && x.TGM_IsActive);
				if (magnitude != null)
				{
					jobHeader.AddTag(magnitude);
					return true;
				}
			}

			return false;
		}

		public static void UnsuppressAll(BusinessObject businessObject)
		{
			if (GetBLNTags(businessObject, true, out var jobHeader).Any(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification))
			{
				foreach (var tag in jobHeader.TagLinks)
				{
					if (tag.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification)
					{
						tag.Delete();
					}
				}
			}
		}

		static IEnumerable<TagLink> GetBLNTags(BusinessObject businessObject, bool createJobHeaderIfNull, out ProcessJobHeader jobHeader)
		{
			jobHeader = null;
			if (businessObject is IWorkflowProvider provider)
			{
				var header = createJobHeaderIfNull ?
					ProcessJobHeader.GetForParent(provider, businessObject.Factory) :
					ProcessJobHeader.GetForParentWithoutCreation(provider, businessObject.Factory);
				var result = header?.Tags.Where(x => x.Definition.TGD_Code.Equals(TagGroupCode));

				jobHeader = header;
				return result ?? [];
			}

			return [];
		}

		public static string TagGroupCode => "BLN";

		#endregion
	}

	public enum IncidentEmailTagRuleStatus
	{
		New,
		Existing,
		NotExists,
		Deleted,
	}
}
