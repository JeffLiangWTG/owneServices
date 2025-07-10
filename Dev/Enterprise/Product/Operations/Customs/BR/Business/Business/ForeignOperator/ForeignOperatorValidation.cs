using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ForeignOperatorValidation : Customs.Business.CusGoodsCatalogProductionInfoValidation
	{
		public ForeignOperatorValidation(ForeignOperator parent)
			: base(parent)
		{
		}

		public new ForeignOperator Parent => (ForeignOperator)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAuthorityCode();
			ValidateCountryCode();
		}

		protected override void CheckCGI_Reference()
		{
		}

		public void ValidateCountryCode()
		{
			ValidateCalculatedProperty(Parent.CountryCodeInfo);
		}

		protected void CheckCountryCode()
		{
			var targetInfo = Parent.CountryCodeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (!Parent.IsForeignOperatorModuleEnabled)
			{
				MandatoryValidation.CheckEntered(targetInfo);
				CheckDuplicate(targetInfo);
			}
		}

		public void ValidateAuthorityCode()
		{
			ValidateCalculatedProperty(Parent.AuthorityCodeInfo);
		}

		protected void CheckAuthorityCode()
		{
			if (!Parent.IsForeignOperatorModuleEnabled)
			{
				CheckDuplicate(Parent.AuthorityCodeInfo);
			}
		}

		void CheckDuplicate(ZPropertyInfo info)
		{
			if (Parent.GoodsCatalog is CusGoodsCatalog goodsCatalog)
			{
				var countryCode = Parent.CountryCode;
				var autorityCode = Parent.AuthorityCode;

				if (goodsCatalog.ForeignOperators.Any(x => x.CountryCode == countryCode && x.AuthorityCode == autorityCode && x.PK != Parent.PK))
				{
					info.AddError(Res.GetString("ed6fb82c-e62c-4735-ab39-3aebf7d628b3", "This Country of Origin and Authority Code already exists in this Goods Catalog"));
				}
			}
		}

		protected override void CheckCGI_BFR_ForeignOperator()
		{
			base.CheckCGI_BFR_ForeignOperator();

			if (Parent.IsForeignOperatorModuleEnabled)
			{
				var targetInfo = Parent.CGI_BFR_ForeignOperatorInfo;
				if (Parent.CountryCode.IsEmpty)
				{
					MandatoryValidation.CheckEntered(targetInfo);
				}

				var foreignOperatorPK = Parent.CGI_BFR_ForeignOperator;
				if (foreignOperatorPK.IsValid && Parent.GoodsCatalog is CusGoodsCatalog goodsCatalog)
				{
					if (goodsCatalog.CGC_OH_Owner != Parent.BRForeignOperator?.BFR_OH_Owner)
					{
						targetInfo.AddError(Res.GetString("03413457-34EB-4078-8600-17866729C7F0", "There is no correspondence between manufacturer and Catalog Owner on Foreign Operator module."));
					}
					else if (goodsCatalog.ForeignOperators.Any(x => x.CGI_BFR_ForeignOperator == foreignOperatorPK && x.PK != Parent.PK))
					{
						targetInfo.AddError(Res.GetString("63745A0C-0EE1-403A-AEF3-21EA8F212D19", "This manufacturer already exists in this Goods Catalog"));
					}
					else if (Parent.BRForeignOperator.BFR_CustomsStatus != ForeignOperatorCustomsStatusTypeList.Codes.Active)
					{
						targetInfo.AddMessageError(Res.GetString("ADFC08F9-D67B-432C-8D83-B9AE001F8FC4", "The Foreign Operator is not Active."));
					}
					else if (Parent.BRForeignOperator.BFR_AuthorityIdentifier.IsEmpty)
					{
						targetInfo.AddMessageError(Res.GetString("B6C59F86-B918-405A-87A0-AAEC830BA210", "The Foreign Operator does not have an Authority Identifier."));
					}
					else if (Parent.BRForeignOperator.BFR_MessageStatus == BRMessageStatusList.Codes.NotSent)
					{
						targetInfo.AddMessageError(Res.GetString("BE4884E8-7B83-4A8D-99FF-A04D57047E0D", "This Foreign Operator should not be used because there might be messages that need to be sent (Message Status: NOT - NOT Sent)."));
					}
					else if (Parent.BRForeignOperator.BFR_MessageStatus == BRMessageStatusList.Codes.AwaitingResponse)
					{
						targetInfo.AddMessageError(Res.GetString("C174419C-9FD0-447E-94A5-2A6D473E894F", "This Foreign Operator should not be used as there is a message awaiting a response (Message Status: 'AWA - Awaiting Response)'."));
					}
				}
			}
		}
	}
}
