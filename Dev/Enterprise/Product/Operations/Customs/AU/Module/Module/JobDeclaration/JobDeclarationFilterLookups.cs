using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageSubTypeList()
		{
			return Factory.GetCachedValue("AUJobDeclarationFilterLookupsMessageSubTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("NCF", "Non-Confirming");
				result.AddPair("CFM", "Confirming");
				result.AddPair(JobDeclaration.MessageSubType.FormalEntry, "Formal Entry");
				result.AddPair(JobDeclaration.MessageSubType.SimplifiedEntry, "Simplified Entry");
				result.AddPair(JobDeclaration.MessageSubType.SelfAssessedClearance, "Self Assessed Clearance");
				result.AddPair(JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines, "Self Assessed Clearance (With Lines)");
				result.AddPair(JobDeclaration.MessageSubType.RequestForCargoRelease, "Request for Cargo Release");
				result.AddPair(JobDeclaration.MessageSubType.PeriodDeclarationType1, "Period Declaration Type 1 (Detailed)");
				result.AddPair(JobDeclaration.MessageSubType.PeriodDeclarationType2, "Period Declaration Type 2 (Compressed)");
				return result;
			});
		}

		public CodeDescriptionPairList COLSEntryNumberStatusList
		{
			get
			{
				return Factory.GetCachedValue("AUJobDeclarationFilterLookups|B74CE7FF-FBC5-4036-BE62-700DB49C1A38", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(COLSEntryStatusList.Codes.LrnActive, "Open");
					list.AddPair(COLSEntryStatusList.Codes.LrnInactive, "Closed");
					return list;
				});
			}
		}

		public CodeDescriptionPairList COLSLodgementStatusList => Factory.GetCachedValue<COLSLodgementStatusList>();

		public CodeDescriptionPairList COLSMessageStatusList
		{
			get
			{
				return Factory.GetCachedValue("AUJobDeclarationFilterLookups|E4DBF52D-540A-4CF9-A506-06600CEC01E8", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddRange(Factory.GetCachedValue<COLSHeaderStatusList>());
					list.AddPair(DeclarationFilterConstants.COLSExtraMessageStatus.Failed, "Any failed status");
					list.AddPair(DeclarationFilterConstants.COLSExtraMessageStatus.Success, "Any sucessful status");
					return list;
				});
			}
		}

		public override CodeDescriptionPairList ContainerModeList
		{
			get
			{
				return Factory.GetCachedValue("AUJobDeclarationFilterLookupsContainerModeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(Core.Constants.ContainerModes.Combination, Core.Constants.ContainerModeDescriptions.Combination);
					result.AddPair(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModeDescriptions.FCLMixedShipper);
					result.AddPair(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModeDescriptions.LCL);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList MessageTypeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("AUJobDeclarationFilterLookupsMessageTypeList", () =>
				{
					return new AUJobMessageTypeList();
				});
			}
		}

		public override CodeDescriptionPairList PaymentPartyList()
		{
			return Factory.GetCachedValue("AUJobDeclarationFilterLookupsPaymentPartyList", () =>
			{
				var result = new CodeDescriptionPairList(base.PaymentPartyList());
				result.AddPair(JobDeclaration.PaymentMethods.SecondBroker, "Second Broker Payment Account");
				result.AddPair(JobDeclaration.PaymentMethods.Cash, "Cash Payment over Customs Counter");
				return result;
			});
		}

		public virtual CodeDescriptionPairList CMREntryStatusList
		{
			get
			{
				var statusList = new CodeDescriptionPairList();
				statusList.AddRange(Factory.GetCachedValue<CMRImportEntryAdviceList>());

				var consolidatedEntriesEnabled = RawDataRegistry.Instance.EnableConsolidatedEntries.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				if (consolidatedEntriesEnabled)
				{
					statusList.AddRange(Factory.GetCachedValue<ConsolidatedEntryStatusList>());
				}
				return statusList;
			}
		}

		public override CodeDescriptionPairList TransportTypeList
		{
			get
			{
				return Factory.GetCachedValue("AUJobDeclarationFilterLookupsTransportTypeList", () =>
				{
					var result = new CodeDescriptionPairList(new TransportTypeList());
					result.AddPair(Core.Constants.TransportModes.Other, Core.Constants.TransportModeDescriptions.Other);
					return result;
				});
			}
		}
		public CodeDescriptionPairList CMRMessageStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("AUJobDeclarationFilterLookupsCMRMessageStatusList", () =>
				{
					var result = new CMRImportMessageStatusList();
					result.RemoveCode("");
					result.Insert(0, new CodeDescriptionPair(Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter, "Not Sent"));
					return result;
				});
			}
		}

		public virtual CodeDescriptionPairList CMRConsolidatedCargoStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("AUJobDeclarationFilterLookupsCMRConsolidatedCargoStatusList", () =>
				{
					var result = new CMRConsolidatedCargoStatuses();
					result.Insert(0, new CodeDescriptionPair(DeclarationFilterConstants.ConsolidatedCargoStatus.NotClearForFilter, "Not Clear"));
					return result;
				});
			}
		}

		public CodeDescriptionPairList PaymentStatusList
		{
			get
			{
				return Factory.GetCachedValue("AUJobDeclarationFilterLookupsPaymentStatusList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(DeclarationFilterConstants.PaymentStatus.Paid, "Show only paid");
					result.AddPair(DeclarationFilterConstants.PaymentStatus.NotPaid, "Show only unpaid");
					return result;
				});
			}
		}

		public CodeDescriptionPairList NatureTypeList
		{
			get
			{
				return Factory.GetCachedValue("AUJobDeclarationFilterLookupsNatureTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(DeclarationFilterConstants.NatureTypes.Nature10, "Nature 10");
					result.AddPair(DeclarationFilterConstants.NatureTypes.Nature20, "Nature 20");
					result.AddPair(DeclarationFilterConstants.NatureTypes.Nature30, "Nature 30");
					return result;
				});
			}
		}

		public CodeDescriptionPairList ProduceTypeList
		{
			get { return Factory.GetCachedValue<EXDOCCommodityCodes>(); }
		}

		public CodeDescriptionPairList RFPStatusList
		{
			get { return Factory.GetCachedValue<EXDOCComplianceStatusCodes>(); }
		}

		public override CodeDescriptionPairList ApplicationCodeList()
		{
			return Factory.GetCachedValue("AUDeclarationApplicationCodeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, "Send CMR Message");
				result.AddPair(Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced, Customs.Business.DeclarationApplicationCodeList.Descriptions.Interfaced);
				result.AddPair(Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages, "Send Legacy Message");
				return result;
			});
		}
	}
}
