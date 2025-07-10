using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415MessageSendingObjectLookups))]
	sealed class RF415MessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLegalBasisList()
		{
			AssertType<CodeDescriptionPairList>(messageSendingObject.Lookups.LegalBasisList);
		}

		public void TestRefundTypeList()
		{
			AssertType<RF415RefundTypes>(messageSendingObject.Lookups.RefundTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var bill = Factory.New<AsycudaBill>();
			messageSendingObject = new RF415MessageSendingObject(bill);
		}

		RF415MessageSendingObject messageSendingObject;
	}
}
