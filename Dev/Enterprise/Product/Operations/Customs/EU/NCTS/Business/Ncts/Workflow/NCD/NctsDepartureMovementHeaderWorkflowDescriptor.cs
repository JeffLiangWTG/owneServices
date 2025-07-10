using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("NctsDepartureMovementHeaderWorkflowDescriptor", "NCTS Departure"); }
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
			get { return typeof(NctsDepartureMovementHeader); }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new NctsDepartureMovementHeaderFormCustomisationSettingsProvider();
		}

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("856E52AD-9C2E-4020-8766-B548AA600651", "Declaration Type"), DeclarationTypeList));
				result.Add(new ProcessTemplateSubType(Res.GetString("0B58F46E-1A56-426A-A2B4-540A9D66C879", "Additional Declaration type"), NctsTypeOfAdditionalDeclarationList));
				result.Add(new ProcessTemplateSubType(Res.GetString("5F15255E-72D0-4823-8DF5-556B3539D2A4", "Inland Transport Mode At Departure"), ModeOfTransportList));
				result.Add(new ProcessTemplateSubType(Res.GetString("F54013B7-6777-4BC2-84D1-C0F00639BF95", "Is Simplified NCTS Procedure"), NctsControlResult));
				result.Add(new ProcessTemplateSubType(Res.GetString("C41BC3E1-8335-40BC-94D0-57B3FBD055A6", "Destination Ctry./Rgn."), Countries));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList Countries
		{
			get
			{
				var factory = LastProcessTaskTemplate != null ? LastProcessTaskTemplate.Factory : CreateNewFactory();
				var countriesList = new CodeDescriptionPairList();
				countriesList.AddRange(factory.GetCachedCountryNC008List(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				return countriesList;
			}
		}

		public static CodeDescriptionPairList NctsControlResult
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("BE997EE2-4152-44FD-8399-E72F97088C91", "All"));
				result.AddRange(new NctsControlResult());
				return result;
			}
		}

		public static CodeDescriptionPairList ModeOfTransportList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("D92494AB-E73F-4712-BCCC-BFB72C0777B5", "All"));
				result.AddRange(new ModeOfTransportList());
				return result;
			}
		}

		public static CodeDescriptionPairList NctsTypeOfAdditionalDeclarationList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("0E338309-C9EA-4C6E-9BF4-1FE45936A0E9", "All"));
				result.AddRange(new NctsTypeOfAdditionalDeclarationList());
				return result;
			}
		}

		public static CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("93AB9172-6202-4893-BFD7-8748CB39A6CD", "All"));
				result.AddRange(new NctsPhase5DeclarationTypeList());
				return result;
			}
		}

		#endregion

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
#if DEBUG
		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			var header = base.GetBizOForTest(factory);
			((NctsDepartureMovementHeader)header).Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			((NctsDepartureMovementHeader)header).Header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}
#endif
	}
}
