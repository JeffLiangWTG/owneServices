using System.Collections;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Forwarding.Module
{
	public class AMSBillStatusFilter : ModuleTextFilter
	{
		public AMSBillStatusFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList list, FilterStripBusinessObject filterBusinessObject)
			: base(description, queryDelegate, list)
		{
			this.FilterBusinessObject = Argument.NotNull(filterBusinessObject, nameof(filterBusinessObject));
		}

		internal new readonly FilterStripBusinessObject FilterBusinessObject;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new AMSBillStatusFilterValidation(this);
		}
	}
}
