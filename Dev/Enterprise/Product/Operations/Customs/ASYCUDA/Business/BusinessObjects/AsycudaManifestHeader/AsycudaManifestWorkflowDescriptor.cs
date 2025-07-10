using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestWorkflowDescriptor : WorkflowDescriptor
	{
		public static class Constants
		{
			public const string Code = "AMW";
			public static IMultilingualString Description { get { return ResString.GetMultilingualString("6D69ACD0-78A7-4CCE-A165-153717C1D0EB", "Manifest"); } }
		}

		public override string Code
		{
			get { return Constants.Code; }
		}

		public override IMultilingualString Description
		{
			get { return Constants.Description; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AsycudaManifestHeader); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		public override MessageRecipientPartyType SupportedManifestMessageRecipientParties(IBaseTrigger trigger)
		{
			return MessageRecipientPartyType.USAirAMS;
		}

		#region Criteria Requirements

		public override bool RequiresBranch
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override ZString ClientName
		{
			get { return "Carrier"; }
		}
		#endregion

		#region Sub Types
		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("DC627150-DDA2-49EE-88E3-526F779F7528", "Transport Mode"), AsycudaManifestHeaderLookups.TransportModeForWorkFlow));
				list.Add(new ProcessTemplateSubType(Res.GetString("1AAA93A6-92B8-43C3-A710-7BA53EEDA5A3", "Container Mode"), AsycudaManifestHeaderLookups.ContainerModeList));
				list.Add(GetManifestTypes());
				list.Add(GetCountries());
				return list.ToArray();
			}
		}
		#endregion

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalShipmentManifestXML);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalManifestEventXML);
			if (ParentSupportsAutoSendGlobalManifest(parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest, WorkflowTriggerActionTypeConstants.Descriptions.SendGlobalManifest);
			}
			return result;
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			if (ParentSupportsAutoSendGlobalManifest(parent))
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			var parent = source.Job;
			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest)
			{
				return new CustomsStmProcessQueueCreatorProcessor(parent, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest);
			}
			else
			{
				return base.GetWorkflowTriggerActionCore(source);
			}
		}

		bool ParentSupportsAutoSendGlobalManifest(IBusiness parent)
		{
			if (parent is AsycudaManifestHeader header)
			{
				var provider = header.ApplicationBusinessProvider;
				return (provider?.SupportsAutoSendGlobalManifest(header.AMA_ManifestType) ?? false) && provider?.GetSendGlobalManifestProcessor(header) != null;
			}
			else if (parent is ProcessTaskTemplate template)
			{
				var type = template.P0_SubType3;
				var country = template.P0_SubType4;
				return ApplicationBusinessProvider.GetApplicationBusinessProviders(parent.Factory, country, type).Any(p => p.SupportsAutoSendGlobalManifest(type));
			}
			else
			{
				return false;
			}
		}

		ProcessTemplateSubType GetManifestTypes()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			foreach (var manifestType in ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(LastProcessTaskTemplate?.Factory ?? new BusinessObjectFactory()).Values.SelectMany(p => p.ManifestTypes).Distinct(new CodeEqualityComparer()))
			{
				codeDescriptionPairList.Add(manifestType);
			}
			return new ProcessTemplateSubType(Res.GetString("C200A713-DC67-4927-BB8E-1A8F45E2CE2C", "Manifest Type"), codeDescriptionPairList);
		}

		ProcessTemplateSubType GetCountries()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			if (!string.IsNullOrEmpty(LastProcessTaskTemplate?.P0_SubType3))
			{
				foreach (var countryCode in ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(LastProcessTaskTemplate.Factory).Keys.Where(k => k.ManifestTypeCode == LastProcessTaskTemplate.P0_SubType3).Select(k => k.CountryOrGrouping).Distinct())
				{
					var country = RefCountry.LoadFromCountryCode(LastProcessTaskTemplate.Factory, countryCode);
					if (country != null)
					{
						codeDescriptionPairList.Add(country);
					}
				}
			}
			return new ProcessTemplateSubType(Res.GetString("DF14F195-F688-43F9-B398-620B0C3C68D3", "Country/Region"), codeDescriptionPairList);
		}

		class CodeEqualityComparer : IEqualityComparer<ICodeDescription>
		{
			public bool Equals(ICodeDescription x, ICodeDescription y)
			{
				return StringComparer.OrdinalIgnoreCase.Equals(x.Code, y.Code);
			}

			public int GetHashCode(ICodeDescription obj)
			{
				return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Code);
			}
		}
	}
}
