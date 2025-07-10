using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromQueryClaim))]
	sealed class FreightWrapperFromQueryClaimTest : FreightWrapperTest
	{
		public void TestQueryClaim()
		{
			ARAccQueryClaim queryClaim = Factory.New<ARAccQueryClaim>();
			queryClaim.AY_ShortDescriptionOfClaim = "SHORT DESCRIPTION";
			FreightWrapperFromQueryClaim wrapper = new FreightWrapperFromQueryClaim(queryClaim, Factory);
			AssertEquals("SHORT DESCRIPTION", wrapper.QueryClaim.ShortDescription);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var queryClaim = Factory.New<ARAccQueryClaim>();
			var wrapper = new FreightWrapperFromQueryClaim(queryClaim, Factory);
			AssertEquals("TrackingBusinessObjectPK", queryClaim.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
PickupAgent : 
QueryClaim : ";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromQueryClaim(queryClaim, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<ARAccQueryClaim>();
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			queryClaim = GetNewBusinessObjectToWrap() as ARAccQueryClaim;
		}

		ARAccQueryClaim queryClaim;

		#endregion
	}
}
