using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class SealNumberCollection : CusCodeDataCollection<SealNumber>
	{
		public SealNumberCollection(BusinessObject master) : base(master, CusCodeDataTypeList.Codes.Seal)
		{
		}

		public new SealNumber AddNew(ZString sealNumber) => AddNew(CusCodeDataTypeList.Codes.Seal, sealNumber);
	}
}
