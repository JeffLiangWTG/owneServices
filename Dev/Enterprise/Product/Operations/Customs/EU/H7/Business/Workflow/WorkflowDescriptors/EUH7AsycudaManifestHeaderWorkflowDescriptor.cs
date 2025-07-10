using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.H7.Business
{
	public class EUH7AsycudaManifestHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code => WorkflowDescriptors.EUH7AsycudaManifestHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("EU|AsycudaManifestHeader|Description", "Low Value (H7)");

		#endregion

		public override ControllerID ControllerID => ControllerIDs.Customs.EU.EUH7;

		public override Type WorkflowProviderType => typeof(ASYCUDA.Business.AsycudaManifestHeader);

		#region Requirements

		public override bool RequiresClient => false;

		public override bool RequiresPort1 => false;

		public override bool RequiresPort2 => false;

		public override bool RequiresBranch => false;

		public override bool RequiresDepartment => false;

		public override bool SupportsCreateTransportBooking => false;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		#endregion

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(GetCountryCodeSubType());

				return list.ToArray();
			}
		}

		static ProcessTemplateSubType GetCountryCodeSubType() => new ProcessTemplateSubType(Res.GetString("63acb40b-22b0-4918-94b4-feee8c8ab15b", "Country Code"), GetCountryCodes(new BusinessObjectFactory()));

		static CodeDescriptionPairList GetCountryCodes(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(ZZRefCusCodeListCombined.Loader.Load(factory, RefDataGroupingCodes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today));
			result.AddPair(Core.Constants.CountryCodes.UnitedKingdom, Res.GetString("17e1844b-4883-4af0-b829-6d64f0582653", "Great Britain"));
			result.Sort();

			return result;
		}

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness bizo)
		{
			yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration, WorkflowTriggerActionTypeConstants.Descriptions.SendCustomsDeclaration);
			if (ParentSupportSendG3CustomsDeclaration(parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration, WorkflowTriggerActionTypeConstants.Descriptions.SendG3CustomsDeclaration);
			}
			return result;
		}

		bool ParentSupportSendG3CustomsDeclaration(IBusiness parent)
		{
			if (parent is AsycudaManifestHeader header && ApplicationBusinessProvider.GetApplicationBusinessProvider(header) is H7ApplicationBusinessProvider provider)
			{
				return provider.SupportsSendG3CustomsDeclaration;
			}
			else if (parent is ProcessTaskTemplate processTaskTemplate)
			{
				return processTaskTemplate.P0_SubType1 == Core.Constants.CountryCodes.Spain;
			}

			return false;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var header = (AsycudaManifestHeader)source.Job;
			var action = source.Action;

			switch (action.PQ_TriggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration:
				case WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration:
					return header.ApplicationBusinessProvider.GetMessageProcessorDependOnTriggerAction(action.PQ_TriggerType, header);
				default:
					return base.GetWorkflowTriggerActionCore(source, queuedLog);
			}
		}
	}
}
