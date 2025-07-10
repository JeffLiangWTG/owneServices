using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineCatchZoneCollection : Customs.Business.CusCodeDataCollection<QuarantineCatchZone>
	{
		public QuarantineCatchZoneCollection(QuarantineExDocHeader quarantineExDocHeader)
			: base(quarantineExDocHeader, CusCodeDataTypeList.Codes.NEXDOCSCatchZone)
		{
		}
	}
}
