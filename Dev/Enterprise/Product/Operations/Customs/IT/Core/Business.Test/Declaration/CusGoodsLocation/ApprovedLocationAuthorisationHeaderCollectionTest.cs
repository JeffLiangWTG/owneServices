using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(ApprovedLocationAuthorisationHeaderCollection))]
sealed class ApprovedLocationAuthorisationHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<ApprovedLocationAuthorisationHeaderCollection>
{
	protected override Type GetExpectedCollectionType() => typeof(ApprovedLocationAuthorisationHeaderCollection);

	protected override ApprovedLocationAuthorisationHeaderCollection GetCollectionToTest()
	{
		var owner = Factory.New<OrgHeader>();
		return new ApprovedLocationAuthorisationHeaderCollection(Factory, owner.PK);
	}
}
