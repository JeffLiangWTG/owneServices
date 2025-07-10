using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNCustomsDeclarationItemCollection<T> : CusSupportingInfoCollection<T> where T : PBNReferenceItem
	{
		public PBNCustomsDeclarationItemCollection(BusinessObject parent, IEnumerable<string> validCodeList)
			: base(parent, PBNReferenceItem.PBNReferenceType)
		{
			this.validCodeList = validCodeList;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusSupportingInfoSchema.CSI_Code, SQLComparisonOperator.Equal, validCodeList);
			return result;
		}

		readonly IEnumerable<string> validCodeList;
	}
}
