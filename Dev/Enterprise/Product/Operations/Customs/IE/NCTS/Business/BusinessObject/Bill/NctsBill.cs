using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsBill : EU.NCTS.Business.NctsBill
	{
		public NctsBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments => (EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;
		protected override EU.NCTS.Business.ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

		[ChildEditable(true)]
		public new EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;
		protected override EU.NCTS.Business.INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems() => new EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
			return result;
		}
	}
}
