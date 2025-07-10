using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.WCB.Testing
{
	public class InvHeadFixedCollectionTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			InvHeadWithFixedInvLines line1 = Factory.New<InvHeadWithFixedInvLines>();
			InvHeadWithFixedInvLines line2 = Factory.New<InvHeadWithFixedInvLines>();
			InvHeadFixedCollection.Add(line1);
			InvHeadFixedCollection.Add(line2);
			AssertEquals(line1, InvHeadFixedCollection[0]);
			AssertEquals(line2, InvHeadFixedCollection[1]);
		}

		InvHeadFixedCollection InvHeadFixedCollection
		{
			get
			{
				return invHeadFixedCollection ?? (invHeadFixedCollection = new InvHeadFixedCollection(Factory.New<JobDeclarationWithFixedInvHeads>()));
			}
		}

		InvHeadFixedCollection invHeadFixedCollection;
	}
}
