using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.WCB.Testing
{
	public class InvLineFixedCollectionTest : TestCaseWithFactory
	{
		public void TestAllowNew()
		{
			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertEquals(false, InvLineFixedCollection.AllowNew);
		}

		InvLineFixedCollection InvLineFixedCollection
		{
			get
			{
				if (invLineFixedCollection == null)
				{
					TestHelper.InvHeadWithFixedInvLines.JZ_JE = Factory.NewWithValidTestData<JobDeclaration>().PK;
					invLineFixedCollection = (InvLineFixedCollection)TestHelper.InvHeadWithFixedInvLines.CreateNewJobComInvoiceLineCollection();
				}

				return invLineFixedCollection;
			}
		}

		InvLineFixedCollection invLineFixedCollection;
		WCBTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new WCBTestHelper(Factory));
			}
		}

		WCBTestHelper testHelper;
	}
}
