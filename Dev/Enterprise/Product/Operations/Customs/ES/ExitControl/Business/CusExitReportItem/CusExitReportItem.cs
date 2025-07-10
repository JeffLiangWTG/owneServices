using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitReportItem : EU.ExitControl.Business.CusExitReportItem
	{
		public CusExitReportItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IDictionary<ZInt, ZInt> AdditionalInfosItemNumberDictionary => Factory.GetValue(ref additionalInfosItemNumberDictionaryCached, () => AdditionalInfos.Cast<AdditionalInfo>().Select(x => x.CSI_ItemNumber).Where(x => x > ZInt.Zero).GroupBy(x => x).ToDictionary(x => x.Key, y => (ZInt)y.Count()));
		CachedProperty<IDictionary<ZInt, ZInt>> additionalInfosItemNumberDictionaryCached;

		protected override IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		#region ICusSupportingInfoTypeSupporter

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		#endregion

		protected override bool IsUCC6Core => Report?.IsUCC6 ?? true;
	}
}
