using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroupRelatedItemGenPivotCollection))]
	public class IncidentManagementGroupRelatedItemGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(IncidentManagementGroupRelatedItemGenPivotCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IncidentManagementGroupRelatedItemGenPivotCollection(Factory.NewWithValidTestData<IncidentManagementGroup>(), RelatedLinkType.MasterAlwaysParent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<IncidentManagementGroup>();
		}
	}
}
