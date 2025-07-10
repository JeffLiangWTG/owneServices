using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEADPScoring.Loader))]
	public class UPEADPScoringLoaderTest : LoaderTestCase
	{
		public void TestLoadOrCreate()
		{
			UPEADPScoring createdRefundForUser1Day1 = LoadOrCreate(User1, Date1);
			UPEADPScoring createdRefundForUser2Day1 = LoadOrCreate(User2, Date1);
			UPEADPScoring createdRefundForUser1Day2 = LoadOrCreate(User1, Date2);
			UPEADPScoring createdRefundForUser2Day2 = LoadOrCreate(User2, Date2);
			UPEADPScoring loadedRefundForUser1Day1 = LoadOrCreate(User1, Date1);
			UPEADPScoring loadedRefundForUser2Day1 = LoadOrCreate(User2, Date1);
			UPEADPScoring loadedRefundForUser1Day2 = LoadOrCreate(User1, Date2);
			UPEADPScoring loadedRefundForUser2Day2 = LoadOrCreate(User2, Date2);
			AssertEquals("LoadOrCreate should load the existing record", loadedRefundForUser1Day1.PK, createdRefundForUser1Day1.PK);
			AssertEquals("LoadOrCreate should load the existing record", loadedRefundForUser1Day1.PK, createdRefundForUser1Day1.PK);
			AssertEquals("LoadOrCreate should load the existing record", loadedRefundForUser1Day1.PK, createdRefundForUser1Day1.PK);
			AssertEquals("LoadOrCreate should load the existing record", loadedRefundForUser1Day1.PK, createdRefundForUser1Day1.PK);
		}

		UPEADPScoring LoadOrCreate(GlbStaff user, ZDateTime date)
		{
			UPEADPScoring result = Loader.LoadOrCreate(user, date);
			AssertEquals("User", user.PK, result.User.PK);
			AssertEquals("Date", date.Date, result.T4_Date);
			return result;
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new UPEADPScoring.Loader(Factory);
		}

		UPEADPScoring.Loader Loader
		{
			get
			{
				if (fLoader == null)
				{
					fLoader = new UPEADPScoring.Loader(Factory);
				}

				return fLoader;
			}
		}

		UPEADPScoring.Loader fLoader;
		GlbStaff User1
		{
			get
			{
				if (fUser1 == null)
				{
					fUser1 = Factory.NewWithValidTestData<GlbStaff>();
				}

				return fUser1;
			}
		}

		GlbStaff fUser1;
		GlbStaff User2
		{
			get
			{
				if (fUser2 == null)
				{
					fUser2 = Factory.NewWithValidTestData<GlbStaff>();
				}

				return fUser2;
			}
		}

		GlbStaff fUser2;
		readonly ZDateTime Date1 = ZDateTime.Now;
		readonly ZDateTime Date2 = new ZDateTime(2002, 2, 2);
		#endregion
	}
}
