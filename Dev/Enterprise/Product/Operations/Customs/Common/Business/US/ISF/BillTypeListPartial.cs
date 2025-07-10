using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.US.ISF
{
	public partial class BillTypeList : Integration.Customs.US.ISF.IBillTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion

		public static CodeDescriptionPairList GetReferenceBillTypes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(BillTypeList.Codes.HouseBillOfLading, BillTypeList.Descriptions.HouseBillOfLading);
			result.AddPair(BillTypeList.Codes.MasterBillOfLading, BillTypeList.Descriptions.MasterBillOfLading);
			result.AddPair(BillTypeList.Codes.OceanBillOfLading, BillTypeList.Descriptions.OceanBillOfLading);
			return result;
		}
	}
}
