using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromGlbPerson))]
	sealed class FreightWrapperFromGlbPersonTest : FreightWrapperTest
	{
		public void TestQueryClaim()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "Full Name";
			var wrapper = new FreightWrapperFromGlbPerson(person, Factory);
			AssertEquals("Full Name", wrapper.Person.FullName);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var person = Factory.New<GlbPerson>();
			var wrapper = new FreightWrapperFromGlbPerson(person, Factory);
			AssertEquals("TrackingBusinessObjectPK", person.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"Person : ";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromGlbPerson(person, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<GlbPerson>();
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

			person = GetNewBusinessObjectToWrap() as GlbPerson;
		}

		GlbPerson person;

		#endregion Implementation
	}
}
