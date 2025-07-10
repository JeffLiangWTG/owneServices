using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class NationalAdditionalCodeCollection : CusCodeDataWithOrderCollection<NationalAdditionalCode>
	{
		public NationalAdditionalCodeCollection(ZPropertyInfo info, short size, short startOrder = 1)
			: base(info, CusCodeDataTypeList.Codes.NationalAdditionalCode, size, startOrder)
		{
		}

		public static NationalAdditionalCodeCollection New(ZPropertyInfo info)
		{
			var master = info.BizObj as INationalAdditionalCodeSupporter;
			var provider = NationalAdditionalCodeProvider.GetByNationalAdditionalCodeSupporter(master);
			var nationalAdditionalCodeCollection = new NationalAdditionalCodeCollection(info, provider.NumberOfCodes, provider.CodesStartingOrder);
			nationalAdditionalCodeCollection.Load();
			return nationalAdditionalCodeCollection;
		}
	}
}
