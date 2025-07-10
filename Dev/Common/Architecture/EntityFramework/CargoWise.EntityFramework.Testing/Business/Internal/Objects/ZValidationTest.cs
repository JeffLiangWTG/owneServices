using System.Collections.ObjectModel;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZValidationTest : TestCaseWithDummy
	{
		public void TestContainsPiggybackedValidationFindsMatchingType()
		{
			BusinessObject bizO = Factory.New<DummyBusinessObject>();
			ZValidation validation = new ConcreteZValidation(bizO);
			validation.GetIValidationInternals().Add(new ConcreteZChildValidation(bizO));
			AssertEquals(false, validation.ContainsPiggybackedValidation(typeof(ConcreteZValidation)));
			AssertEquals(true, validation.ContainsPiggybackedValidation(typeof(ConcreteZChildValidation)));
		}

		public void TestCreateDomainValidations()
		{
			Dummy.Factory.Validation.MainGroup.RegisterValidationType<DummyBusinessObject, ZValidationDummyBizOValidationForTest>();
			ReadOnlyCollection<ZValidation> domainValidations = Dummy.Validation.CreateDomainValidations();
			AssertEquals(1, domainValidations.Count);
			AssertEquals(typeof(ZValidationDummyBizOValidationForTest), domainValidations[0].GetType());

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.Validation.MainGroup.RegisterValidationType<AutoDummyBizo, ZValidationDummyBizOValidationForTest>();
			newFactory.Validation.MainGroup.RegisterValidationType<AutoDummyBizo, ZValidationDummyBizOValidationForTest2>();
			newFactory.Validation.MainGroup.RegisterValidationType<DummyBaseBusinessObject, ZValidationDummyBizOValidationForTest3>();
			Dummy.Factory.Validation.AddAllFrom(newFactory.Validation);
			domainValidations = Dummy.Validation.CreateDomainValidations();
			AssertEquals(3, domainValidations.Count);
			AssertEquals(typeof(ZValidationDummyBizOValidationForTest), domainValidations[0].GetType());
			AssertEquals(typeof(ZValidationDummyBizOValidationForTest2), domainValidations[1].GetType());
			AssertEquals(typeof(ZValidationDummyBizOValidationForTest3), domainValidations[2].GetType());
		}

		public void TestCreateDomainValidation_ParentDoesNotHaveDomain()
		{
			ConcreteZValidation validationWithoutDomain = new ConcreteZValidation();
			AssertEquals("Should return an empty array when domain is null", 0, validationWithoutDomain.CreateDomainValidations().Count);
			Assert("Should return false when domain is null", !validationWithoutDomain.ContainsDomainValidation(null));
		}

		public void TestContainsDomainValidation_ParentDoesNotHaveDomain()
		{
			ConcreteZValidation validationWithoutDomain = new ConcreteZValidation();
			Assert("Always return false when domain is null", !validationWithoutDomain.ContainsDomainValidation(typeof(ZValidationDummyBizOValidationForTest)));
		}

		public void TestDomainValidations()
		{
			AssertEquals("Should not be ZValidation[] array, the performance is too slow", typeof(ReadOnlyCollection<ZValidation>), Dummy.Validation.DomainValidations.GetType());
		}
	}
}
