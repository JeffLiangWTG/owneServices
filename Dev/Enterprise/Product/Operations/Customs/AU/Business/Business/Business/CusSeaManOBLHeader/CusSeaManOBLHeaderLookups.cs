using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderLookups : BaseCusSeaManOBLHeaderLookups
	{
		public CusSeaManOBLHeaderLookups(CusSeaManOBLHeader parent)
			: base(parent)
		{
		}

		#region Overrides

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewMethodsOfPayment()
		{
			return new CMRMethodsOfPayment();
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList GetNewCargoCodes()
		{
			return new CMRImportCargoCodes();
		}

		#endregion

		public CMRStatuses CargoReportStatusList
		{
			get { return Factory.GetCachedValue<CMRStatuses>(); }
		}
	}
}
