using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionFinalizerLineItemGrouping))]
	internal class CommissionFinalizerLineItemGroupingTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		IEnumerable<CommissionFinalizerLineItem> CreateFinalizerLineItems(params ViewCommissionLine[] commissionLines)
		{
			return commissionLines.Select(x => new CommissionFinalizerLineItem(x));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var grouping = new CommissionFinalizerLineItemGrouping(Factory);
			grouping.Init(CreateFinalizerLineItems(Factory.New<ViewCommissionLine>()));
			return grouping;
		}

		#endregion
	}
}
