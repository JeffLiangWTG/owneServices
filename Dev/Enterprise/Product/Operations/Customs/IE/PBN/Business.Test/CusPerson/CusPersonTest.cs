using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	sealed class CusPersonTest : ASYCUDA.Business.Testing.CusPersonAbstractTest<CusPersonCountry>
	{
		public void TestPersonAddress()
		{
			glbPerson.PER_HomeAddress1 = "123 Fake St";
			glbPerson.PER_HomeAddress2 = "Apt 1";
			AssertEquals("PersonAddress value", "123 Fake St Apt 1", cusPerson.PersonAddress);
		}

		public void TestPersonEmail()
		{
			glbPerson.PER_EmailAddress = "bfu@123.com";
			AssertEquals("PersonEmail value", "bfu@123.com", cusPerson.PersonEmail);
		}

		public void TestPersonHomePhone()
		{
			glbPerson.PER_HomePhone = "123-456-7890";
			AssertEquals("PersonHomePhone value", "123-456-7890", cusPerson.PersonHomePhone);
		}

		public void TestPersonMobilePhone()
		{
			glbPerson.PER_MobilePhone = "098-765-4321";
			AssertEquals("PersonMobilePhone value", "098-765-4321", cusPerson.PersonMobilePhone);
		}

		protected override BusinessObject GetNewBusinessObject() => cusPerson;

		protected override void SetUp()
		{
			base.SetUp();
			glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			cusPerson = header.Persons.AddNew();
			cusPerson.CPN_PER_Person = glbPerson.PK;
		}
		GlbPerson glbPerson;
		CusPerson cusPerson;
	}
}
