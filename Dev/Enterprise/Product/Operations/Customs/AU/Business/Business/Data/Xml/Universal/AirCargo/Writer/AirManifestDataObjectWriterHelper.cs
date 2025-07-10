using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirManifestDataObjectWriterHelper : DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper
	{
		public AirManifestDataObjectWriterHelper(DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper list)
			: base(list)
		{
		}

		public AirManifestDataObjectWriterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AirManifestDataObjectWriterHelper(Customs.Business.CusMAWB mawb)
			: base(mawb)
		{
		}

		public AirManifestDataObjectWriterHelper(CusHAWB hawb, DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper list)
			: base(hawb, list)
		{
		}

		public AirManifestDataObjectWriterHelper(CusHAWB hawb)
			: base(hawb)
		{
		}

		protected new CusHAWB HAWB
		{
			get { return (CusHAWB)base.HAWB; }
		}

		protected AirManifestDataObjectWriterHelper AUList
		{
			get { return list as AirManifestDataObjectWriterHelper; }
		}

		#region List

		public CMRConsolidatedCargoStatuses CusHAWBConsolidatedCargoStatuses
		{
			get { return factory.GetCachedValue<CMRConsolidatedCargoStatuses>(); }
		}

		public ICodeDescriptionPairList CusHAWBPrepaidCollectList
		{
			get { return HAWB == null ? AUList == null ? null : AUList.CusHAWBPrepaidCollectList : HAWB.Lookups.PrepaidCollectList; }
		}

		public RefServiceLevelCollection CusHAWBServiceLevels
		{
			get { return HAWB == null ? AUList == null ? null : AUList.CusHAWBServiceLevels : HAWB.Lookups._ServiceLevels; }
		}

		public ICodeDescriptionPairList CusHAWBShipmentTypeList
		{
			get { return HAWB == null ? AUList == null ? null : AUList.CusHAWBShipmentTypeList : HAWB.Lookups.ShipmentTypeList; }
		}

		#endregion
	}
}
