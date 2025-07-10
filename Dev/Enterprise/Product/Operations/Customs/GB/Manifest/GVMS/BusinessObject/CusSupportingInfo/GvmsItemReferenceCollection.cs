using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsItemReferenceCollection<T> : CusSupportingInfoCollection<T> where T : GvmsItemReference
	{
		public GvmsItemReferenceCollection(BusinessObject parent, IEnumerable<string> validCodeList)
			: base(parent, GvmsItemReference.GvmsItemReferenceType)
		{
			this.validCodeList = validCodeList;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(CusSupportingInfoSchema.CSI_Code, SQLComparisonOperator.Equal, validCodeList);
			return result;
		}

		readonly IEnumerable<string> validCodeList;
	}
}
