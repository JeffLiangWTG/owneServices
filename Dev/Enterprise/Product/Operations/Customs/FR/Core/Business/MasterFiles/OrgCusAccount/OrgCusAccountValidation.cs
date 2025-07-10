using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class OrgCusAccountValidation : Enterprise.MasterFiles.Business.OrgCusAccountValidation
	{
		const int AccountLength = 8;
		const int DEC_AccountLength = 35;

		public OrgCusAccountValidation(OrgCusAccount parent)
			: base(parent)
		{
		}

		protected new OrgCusAccount Parent => (OrgCusAccount)base.Parent;

		protected override void CheckCZ_Account()
		{
			base.CheckCZ_Account();
			var orgHeader = Parent.Header;

			if (orgHeader != null && Parent.CZ_Code == OrgCusAccountCodeList.Codes.DEC)
			{
				if (Parent.CZ_Account.Length > DEC_AccountLength || !Parent.CZ_Account.IsLettersAndNumbersOnlyOrEmpty)
				{
					Parent.CZ_AccountInfo.AddMessageError(Res.GetString("3338AD38-3942-4EA9-B27E-623C104E5939", "Entered DECO for Delta IE Account cannot have more than {0} alphanumeric characters", DEC_AccountLength));
				}

				if (IsCombinationOfCodeDECAndAccountNumberNotUnique(orgHeader))
				{
					Parent.CZ_AccountInfo.AddError(Res.GetString("B23F8CDC-6F07-49A9-9275-1DEE5D6BB6A7", "Entered DECO for Delta IE Account already exists for this organization"));
				}
			}
			else
			{
				if (Parent.CZ_Account.Length != AccountLength)
				{
					Parent.CZ_AccountInfo.AddMessageError(Res.GetString("A05C8780-C00A-4AC2-8DD0-010A7117E6A1", "Entered Account must have {0} numeric digits", AccountLength));
				}
			}
		}

		protected override void CheckCZ_Code()
		{
			base.CheckCZ_Code();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CZ_CodeInfo);

			if (Parent.CZ_Code != OrgCusAccountCodeList.Codes.DEC)
			{
				var orgHeader = Parent.Header;
				if (orgHeader != null)
				{
					if (!Parent.CZ_Type.IsEmpty && TheCoupleTypeAndCodeAlreadyExist(orgHeader))
					{
						Parent.CZ_CodeInfo.AddMessageError(Res.GetString("8FB6DB58-B595-470F-8BC6-26B435277D50", "This combination already exists for this organization"));
					}
				}
			}
		}

		protected override void CheckCZ_Issuer()
		{
			base.CheckCZ_Issuer();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CZ_IssuerInfo);
		}

		protected override void CheckCZ_Type()
		{
			base.CheckCZ_Type();

			if (Parent.CZ_Code == OrgCusAccountCodeList.Codes.DEC)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CZ_TypeInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CZ_TypeInfo);
			}
			else
			{
				if (Parent.CZ_Code == OrgCusAccountCodeList.Codes.DGI || Parent.CZ_Code == OrgCusAccountCodeList.Codes.DGE || Parent.CZ_Code == OrgCusAccountCodeList.Codes.DTA)
				{
					ListValidation.ErrorIfInvalidCode(Parent.CZ_TypeInfo);
					MandatoryValidation.CheckEntered(Parent.CZ_TypeInfo);
				}

				var orgHeader = Parent.Header;
				if (orgHeader != null)
				{
					var orgImpAddInfo = FROrgImpAddInfo.Get(orgHeader);

					if (Parent.CZ_Type == OrgCusAccountDeltaGTypeList.Codes.G1 && orgImpAddInfo.ZO_DeltaG1SubProcedure == ZString.Empty)
					{
						Parent.CZ_TypeInfo.AddMessageError(Res.GetString("2FAA85FC-435C-4860-9E36-826BA4ECE6AB", "You're setting a Delta G1 type of account but Delta G1 sub procedure type in consignee tab of current organization is not set."));
					}

					if (!Parent.CZ_Code.IsEmpty && TheCoupleTypeAndCodeAlreadyExist(orgHeader))
					{
						Parent.CZ_TypeInfo.AddMessageError(Res.GetString("8FB6DB58-B595-470F-8BC6-26B435277D50", "This combination already exists for this organization"));
					}
				}
			}
		}

		bool IsCombinationOfCodeDECAndAccountNumberNotUnique(OrgHeader orgHeader)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgCusAccountSchema.CZ_OH, orgHeader.PK);
			query.AddToFilter(OrgCusAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(OrgCusAccountSchema.CZ_Code, OrgCusAccountCodeList.Codes.DEC);
			query.AddToFilter(OrgCusAccountSchema.CZ_Account, Parent.CZ_Account);
			return orgHeader.Factory.Exists(typeof(OrgCusAccount), query);
		}

		bool TheCoupleTypeAndCodeAlreadyExist(OrgHeader orgHeader)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgCusAccountSchema.CZ_OH, orgHeader.PK);
			query.AddToFilter(OrgCusAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(OrgCusAccountSchema.CZ_Code, Parent.CZ_Code);
			query.AddToFilter(OrgCusAccountSchema.CZ_Type, Parent.CZ_Type);
			return orgHeader.Factory.Exists(typeof(OrgCusAccount), query);
		}

		protected override void CheckCZ_RepresentativeID()
		{
			base.CheckCZ_RepresentativeID();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CZ_RepresentativeIDInfo);
		}
	}
}
