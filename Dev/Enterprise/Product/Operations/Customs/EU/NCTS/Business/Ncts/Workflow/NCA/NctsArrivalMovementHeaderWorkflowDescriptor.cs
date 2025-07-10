using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalMovementHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Overrides of WorkflowDescriptor

		public override string Code
		{
			get { return WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("NctsArrivalMovementHeaderWorkflowDescriptor", "NCTS Arrival"); }
		}

		public override bool SupportsBufferManagement => false;

		public override ZString MilestoneTemplateHintCaption
		{
			get { return ""; }
		}

		public override ControllerID ControllerID
		{
			get { return null; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(NctsArrivalMovementHeader); }
		}
		#endregion

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new NctsArrivalMovementHeaderFormCustomisationSettingsProvider();
		}

		#region Requires

		public override bool RequiresPort1 { get { return false; } }

		public override bool RequiresPort2 { get { return false; } }

		public override bool SupportsTasks { get { return false; } }

		public override bool SupportsEventTracking { get { return false; } }

		public override bool SupportsUniversalTemplates => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(Integration.IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy;
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("ECEEF0A6-F2D2-4A52-8966-D3A51937C2D8", "Incident Flag"), IncidentFlagList));
				result.Add(new ProcessTemplateSubType(Res.GetString("666E5A78-7E3A-478C-A30C-653F792562C6", "Simplified Type"), NctsArrivalAuthorizationCodeList));
				result.Add(new ProcessTemplateSubType(Res.GetString("1009E57B-49BE-49B7-8B08-F1A4FC43E672", "Destination Office Code"), DestinationOfficeCodeList));
				result.Add(new ProcessTemplateSubType(Res.GetString("F2CB4152-04D2-4879-8BBE-727EAC064F21", "Conforms"), Bool01List));
				result.Add(new ProcessTemplateSubType(Res.GetString("A1CC0E7F-837A-4FEC-9747-FCBAD52222E0", "State Of Seals"), YesNoList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList YesNoList
		{
			get
			{
				var yesNoList = new CodeDescriptionPairList();
				yesNoList.AddPair("", Res.GetString("2CE6B34E-E3E4-4D17-B309-5E0777971628", "All"));
				yesNoList.AddRange(new YesNoList());
				return yesNoList;
			}
		}

		public static CodeDescriptionPairList Bool01List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("5C1340FA-FC3F-4458-85E1-3DDCD5925EC6", "All"));
				result.AddRange(new Bool01List());
				return result;
			}
		}

		public  CodeDescriptionPairList DestinationOfficeCodeList
		{
			get
			{
				var factory = LastProcessTaskTemplate != null ? LastProcessTaskTemplate.Factory : CreateNewFactory();
				var customsOffices = EUCustomsOfficeCodeCollection.CustomsOfficesWithRequiredRoles(factory, [GlbCompany.CurrentCompany.GC_RN_NKCountryCode], [OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination]);
				customsOffices.Load();
				var result = new CodeDescriptionPairList();
				foreach (CargoWise.Integration.ICodeDescription item in customsOffices)
				{
					var codeWithOutCountry_Prefix = ((ZString)item.Code).Right(ProcessTaskTemplateSchema.P0_SubType3.MaxLength);
					result.AddPair(codeWithOutCountry_Prefix, item.Description);
				}
				result.SortByDescription();
				var allDesc = new CodeDescriptionPair("", Res.GetString("64B34DCA-FB20-45BA-823D-E4C43102C8ED", "All"));
				result.Insert(0, allDesc);
				return result;
			}
		}

		public static CodeDescriptionPairList NctsArrivalAuthorizationCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("0E338309-C9EA-4C6E-9BF4-1FE45936A0E9", "All"));
				result.AddRange(new NctsArrivalAuthorizationCodeList());
				return result;
			}
		}

		public static CodeDescriptionPairList IncidentFlagList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("93AB9172-6202-4893-BFD7-8748CB39A6CD", "All"));
				result.AddRange(new EventFlagList());
				result.RemoveCode(Business.EventFlagList.Codes.Cancelled);
				return result;
			}
		}

		#endregion
#if DEBUG
		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			var header = base.GetBizOForTest(factory);
			((NctsArrivalMovementHeader)header).Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			((NctsArrivalMovementHeader)header).Header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header;
		}
#endif
	}
}
