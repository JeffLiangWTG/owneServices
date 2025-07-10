using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementLineChargeLookups : Customs.Business.CusStatementLineChargeLookups
	{
		public CusStatementLineChargeLookups(CusStatementLineCharge parent) : base(parent)
		{
		}

		public new CusStatementLineCharge Parent => (CusStatementLineCharge)base.Parent;

		public CodeDescriptionPairList ChargeTypeList => new NationalFeeTypeCodeList(Factory);

		public CodeDescriptionPairList MethodOfPaymentList
		{
			get
			{
				var direction = Parent.ChargesDetail?.Statement?.B2_BranchDesignation ?? ZString.Empty;
				return Factory.GetCachedValue("FR.CusStatementLineChargeLookups." + direction, () =>
				{
					var isImport = direction == StatementEntryTypeImpExpList.Codes.Import;
					var isExport = direction == StatementEntryTypeImpExpList.Codes.Export;
					return TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.France, isImport, isExport);
				});
			}
		}

		public CodeDescriptionPairList ChargeGroupList => Factory.GetCachedValue<CommunautaryChargeCodeList>();
	}
}
