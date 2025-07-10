using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class MultivaluedAttributesCusCodeDataCollection : CusCodeDataCollection<AttributeCusCodeData>
	{
		public MultivaluedAttributesCusCodeDataCollection(AttributeCusCodeData firstAttribute) : base(firstAttribute.Parent, firstAttribute.CY_Type)
		{
			this.firstAttribute = firstAttribute;
		}
		protected readonly AttributeCusCodeData firstAttribute;

		public void UpdateAll(IEnumerable<ZString> lines)
		{
			var attributes = lines.Select((line, index) => CreateOrUpdate(line, (ZShort)index + 1)).ToArray();
			Where(x => x.CY_Order > attributes.Length).DeleteAll();
		}

		AttributeCusCodeData CreateOrUpdate(ZString data, ZShort order)
		{
			var attribute = this.Cast<AttributeCusCodeData>().FirstOrDefault(x => x.CY_Order == order) ?? AddNew();
			attribute.CY_Data = data;
			attribute.CY_Order = order;
			return attribute;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (firstAttribute.AllowMultipleAnswersForString)
			{
				return base.CreateRelationshipFilter()
					.AddToFilter(CusCodeDataSchema.CY_Code, firstAttribute.CY_Code)
					.AddToFilter(CusCodeDataSchema.CY_Order, SQLComparisonOperator.GreaterThan, ZShort.Zero);
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			(child as AttributeCusCodeData).CY_Code = firstAttribute.CY_Code;
		}
	}
}
