using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(InvHeadFixedCollection))]
	class InvHeadFixedCollectionActiveBOTest : ActiveBusinessObjectCollectionTestCase<InvHeadFixedCollection>
	{
		protected override InvHeadFixedCollection GetCollectionToTest()
		{
			var jobDec = Factory.NewWithValidTestData<JobDeclarationWithFixedInvHeads>();
			return new InvHeadFixedCollection(jobDec);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<Customs.Business.BaseJobComInvoiceHeader>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
		}
	}
}
