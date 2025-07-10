using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentRelatedItemGenPivotCollection))]
	public class SupportIncidentRelatedItemGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(SupportIncidentRelatedItemGenPivotCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SupportIncidentRelatedItemGenPivotCollection(Factory.NewWithValidTestData<SupportIncident>(), RelatedLinkType.MasterAlwaysParent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<SupportIncident>();
		}
	}
}
