using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Client.WCB.Testing
{
	public class JobDeclarationFixedCollectionTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			JobDeclarationWithFixedInvHeads line1 = Factory.New<JobDeclarationWithFixedInvHeads>();
			JobDeclarationWithFixedInvHeads line2 = Factory.New<JobDeclarationWithFixedInvHeads>();
			JobDeclarationFixedCollection.Add(line1);
			JobDeclarationFixedCollection.Add(line2);
			AssertEquals(line1, JobDeclarationFixedCollection[0]);
			AssertEquals(line2, JobDeclarationFixedCollection[1]);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, JobDeclarationFixedCollection.AllowNew);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
		}

		JobDeclarationFixedCollection JobDeclarationFixedCollection
		{
			get
			{
				return jobDeclarationFixedCollection ?? (jobDeclarationFixedCollection = new JobDeclarationFixedCollection(new BaseJobDeclarationCollection(Factory)));
			}
		}

		JobDeclarationFixedCollection jobDeclarationFixedCollection;
	}
}
