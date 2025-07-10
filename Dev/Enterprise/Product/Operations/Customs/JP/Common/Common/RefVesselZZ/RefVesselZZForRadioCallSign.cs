using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common
{
	[CodeProperty(RefVesselZZ.Schema.ZZO_RadioCallSign)]
	public class RefVesselZZForRadioCallSign : RefVesselZZ
	{
		public RefVesselZZForRadioCallSign(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZO_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Japan;
		}
		#endif
	}
}
