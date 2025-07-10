//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTradeChainPartnerAddInfoValidation
//
//    This class should be used for overriding validation in AutoTradeChainPartnerAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	using CargoWise.EntityFramework;

	//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
	#pragma warning disable IDE0079
	#pragma warning disable IDE0005
	using CargoWiseOne.ResourceStrings;
	#pragma warning restore IDE0005, IDE0079

	public class TradeChainPartnerAddInfoValidation : AutoTradeChainPartnerAddInfoValidation
	{
		public TradeChainPartnerAddInfoValidation(AutoTradeChainPartnerAddInfo parent) : base(parent)
		{
			this.addInfo = (TradeChainPartnerAddInfo)parent;
			this.lookups = Parent.Lookups;
		}

		readonly TradeChainPartnerAddInfo addInfo;
		readonly TradeChainPartnerAddInfoLookups lookups;

		protected override void CheckCA_CSAIDType()
		{
			base.CheckCA_CSAIDType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(addInfo.CA_CSAIDTypeInfo, lookups.CSAIDTypeList);
		}

		protected override void CheckCA_CSAStatus()
		{
			base.CheckCA_CSAStatus();
			ListValidation.MessageErrorIfInvalidCode(addInfo.CA_CSAStatusInfo, lookups.CSAStatusList);
		}

		protected override void CheckCA_Type()
		{
			base.CheckCA_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(addInfo.CA_TypeInfo, lookups.TradeChainPartnersTypeList);
		}

		protected override void CheckCA_CSAID()
		{
			base.CheckCA_CSAID();
			if (addInfo.CA_CSAIDType == CSAConsigneeIDTypeList.Codes.CSA && addInfo.CA_CSAID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(addInfo.CA_CSAIDInfo);
			}
			if (addInfo.CA_CSAID.Length > 35)
			{
				addInfo.CA_CSAIDInfo.AddMessageError(Res.GetString("8FEA32BD-23D1-41DA-BDE9-CF422CFCC1D8", "CSA ID maximum length is 35."));
			}

			if (addInfo != null)
			{
				var tcp = addInfo.Parent;
				var orgHeader = tcp?.ParentOrgHeader;
				if (orgHeader != null)
				{
					OrgImpAddInfo impAddInfo = OrgImpAddInfo.Get(orgHeader);
					var tradeChainPartners = impAddInfo.TradeChainPartners;
					foreach (TradeChainPartner entry in tradeChainPartners)
					{
						if (entry.PK != tcp.PK && entry.CA_CSAID == tcp.CA_CSAID)
						{
							addInfo.CA_CSAIDInfo.AddMessageError(Res.GetString("CD25C432-B80C-4178-A3A2-DB4BBCDD206F", "Duplicate CSAIDs"));
							break;
						}
					}
				}
			}
		}

		protected override void CheckCA_Action()
		{
			base.CheckCA_Action();
			var tcp = addInfo.Parent;
			var orgHeader = addInfo.Parent?.ParentOrgHeader;
			if (orgHeader != null)
			{
				if (!tcp.CA_Action.IsEmpty && tcp.CA_Action != CSAActionTypeList.Codes.ReqAdd && tcp.CA_Action != CSAActionTypeList.Codes.ReqDel)
				{
					addInfo.CA_ActionInfo.AddError(Res.GetString("77D0A451-17A3-41D0-BECF-594A2750805B", "Actions should be Request Add or Request Delete."));
				}
				else
				{
					if (tcp.CA_Action == CSAActionTypeList.Codes.ReqAdd
						&& (tcp.CA_CSAStatus == CSAStatusList.Codes.Added
						|| tcp.CA_CSAStatus == CSAStatusList.Codes.ErrorDeleted))
					{
						addInfo.CA_ActionInfo.AddWarning(Res.GetString("843871A5-C0D2-46FA-8F5A-C84D063156C8", "This TCP record is already added at CBSA and can not be resent"));
					}
					else if (tcp.CA_Action == CSAActionTypeList.Codes.ReqDel
						&& (tcp.CA_CSAStatus == CSAStatusList.Codes.New
						|| tcp.CA_CSAStatus == CSAStatusList.Codes.ErrorAdded
						|| tcp.CA_CSAStatus == CSAStatusList.Codes.Deleted))
					{
						addInfo.CA_ActionInfo.AddWarning(Res.GetString("C328466F-2056-4D31-A262-14DA2E5AA76F", "This TCP record has not been added by CBSA and can not be deleted"));
					}
				}
			}
		}
	}
}
