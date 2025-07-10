using System;
using System.Linq;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsGuaranteeRefresher : NctsGuaranteeRefresher
	{
		public FRNctsGuaranteeRefresher(NctsHeader nctsHeader) : base(nctsHeader)
		{
			this.nctsHeader = nctsHeader;
		}

		readonly NctsHeader nctsHeader;

		protected override Func<EU.Business.CusGuaranteeHeader, bool> GuaranteeFilter => (x) => x.CPH_Type == GuaranteeTypeList.Codes.COD && !x.CPH_SubType.IsEmpty;

		protected override void CreateNctsGuaranteeFromPrincipalGuarantee(EU.Business.CusGuaranteeHeader guarantee)
		{
			var guarantees = nctsHeader.GetEffectiveGuarantees();
			var nctsGuarantee = guarantees.FirstOrDefault(x => x.PW_BondNumber == guarantee.CPH_Number) ?? guarantees.AddNew();
			nctsGuarantee.PW_BondType = guarantee.CPH_SubType;
			if (guarantee.CPH_SubType == EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee)
			{
				nctsGuarantee.PW_BondNumber2 = guarantee.CPH_Number;
			}
			else
			{
				nctsGuarantee.PW_BondNumber = guarantee.CPH_Number;
			}
			nctsGuarantee.PW_Password = guarantee.MainAccessCode.Left(CusBondDetail.Schema.PW_PasswordMaxLength);
		}
	}
}
