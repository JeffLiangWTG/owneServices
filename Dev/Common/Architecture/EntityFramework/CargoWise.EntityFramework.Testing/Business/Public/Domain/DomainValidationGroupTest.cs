using System;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DomainValidationGroupTest : TestCaseWithDummy
	{
		public void TestRegisterValidationType()
		{
			Assert("Pre-condition", !Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			Assert("Should contain the domain validation now", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest2>();
			Assert("Should still contain the previous domain validation", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
			Assert("Should contain the new domain validation", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest2)));
		}

		public void TestRegisterValidationType_MultipleSubTypes()
		{
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<DummyBaseBusinessObject, DummyBizOValidationForTest2>();
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest3>();
			Assert("All three should be registered", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
			Assert("All three should be registered", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest2)));
			Assert("All three should be registered", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest3)));

			DummyChildBusinessObject dummyChild = Factory.New<DummyChildBusinessObject>();
			AssertEquals("Sanity check", typeof(DummyBaseBusinessObject), typeof(DummyChildBusinessObject).BaseType);
			Assert("Only two should be registered", dummyChild.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
			Assert("Only two should be registered", dummyChild.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest2)));
			Assert("Only two should be registered", !dummyChild.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest3)));

			ValidationGroup.UnregisterValidationType<DummyBaseBusinessObject, DummyBizOValidationForTest2>();
			Assert("Only two should be registered now", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
			Assert("Only two should be registered now", !Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest2)));
			Assert("Only two should be registered now", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest3)));
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "ValidationType is abstract")]
		public void TestRegisterValidationType_AbstractValidationType()
		{
			ValidationGroup.RegisterValidationType<DummyBusinessObject, ZValidation>();
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Cannot register domain validation to base BusinessObject class")]
		public void TestRegisterValidationType_RegisteringBusinessObject()
		{
			ValidationGroup.RegisterValidationType<BusinessObject, DummyBizOValidationForTest>();
		}

		public void TestRegisterValidationType_NoValidConstructor()
		{
			try
			{
				ValidationGroup.RegisterValidationType<DummyBusinessObject, InvalidDummyBizOValidationForTest>();
				Fail("Exception should have been thrown");
			}
			catch (InvalidOperationException ex)
			{
				AssertEquals("Expected a constructor with a single parameter of type or subclass of type " + typeof(DummyBusinessObject).FullName + " for validation type " + typeof(InvalidDummyBizOValidationForTest).FullName, ex.Message);
			}
		}

		public void TestRegisterValidationType_SameTypeDontGetAddedMoreThanOnce()
		{
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			AssertEquals("Should only create 1", 1, ValidationGroup.CreateDomainValidation(Dummy).Length);
		}

		public void TestUnregisterValidationType()
		{
			AssertEquals("Pre-condition", 0, ValidationGroup.BusinessObjectDomainValidations.Count);
			ValidationGroup.UnregisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			Assert(!ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(DummyBusinessObject)));
			Assert(!Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			Assert(ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(DummyBusinessObject)));
			Assert(Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.UnregisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			Assert("Should be removed as there are no other validation types attached to DummyBusinessObject", !ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(DummyBusinessObject)));
			Assert(!Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest2>();
			Assert(ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(DummyBusinessObject)));
			Assert(!ValidationGroup.BusinessObjectDomainValidations[typeof(DummyBusinessObject)].IsEmpty);
			Assert(Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.UnregisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			Assert("BusinessObjectDomainValidation object should not be removed as the base type still has domain validation", ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(DummyBusinessObject)));
			Assert(!Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
		}

		public void TestUnregisterValidationType_EmptyElementShouldBeRemovedFromDictionary()
		{
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			Assert("Pre-condition", ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(AutoDummyBizo)));
			Assert("Pre-condition", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.UnregisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			Assert("Should be removed from the dictionary", !ValidationGroup.BusinessObjectDomainValidations.ContainsKey(typeof(AutoDummyBizo)));
			Assert("Should not exist", !Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
		}

		public void TestUnregisteringBaseValidationTypeShouldNotScrewUpValidationType()
		{
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			Assert("Should contain domain validation", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest)));

			ValidationGroup.UnregisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			Assert("Should contain domain validation", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest)));
		}

		public void TestRegisterAndUnregisterValidationTypes_NonGeneric()
		{
			Assert("Pre-condition", !Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.RegisterValidationType(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest));
			Assert("Should contain the domain validation now", Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));

			ValidationGroup.UnregisterValidationType(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest));
			Assert("Should be unregistered", !Dummy.Validation.ContainsDomainValidation(typeof(DummyBizOValidationForTest)));
		}

		public void TestContainsDomainValidation()
		{
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<DummyBaseBusinessObject, DummyBizOValidationForTest2>();
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest3>();
			Assert("All three should be registered", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest)));
			Assert("All three should be registered", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest2)));
			Assert("All three should be registered", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest3)));
			Assert(ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject)));

			AssertEquals("Sanity check", typeof(DummyBaseBusinessObject), typeof(DummyChildBusinessObject).BaseType);
			Assert("Only two should be registered", ValidationGroup.ContainsDomainValidation(typeof(DummyChildBusinessObject), typeof(DummyBizOValidationForTest)));
			Assert("Only two should be registered", ValidationGroup.ContainsDomainValidation(typeof(DummyChildBusinessObject), typeof(DummyBizOValidationForTest2)));
			Assert("Only two should be registered", !ValidationGroup.ContainsDomainValidation(typeof(DummyChildBusinessObject), typeof(DummyBizOValidationForTest3)));
			Assert(ValidationGroup.ContainsDomainValidation(typeof(DummyChildBusinessObject)));

			ValidationGroup.UnregisterValidationType<DummyBaseBusinessObject, DummyBizOValidationForTest2>();
			Assert("Only two should be registered now", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest)));
			Assert("Only two should be registered now", !ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest2)));
			Assert("Only two should be registered now", ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject), typeof(DummyBizOValidationForTest3)));
			Assert(ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject)));

			ValidationGroup.UnregisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			Assert(ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject)));
			Assert(!ValidationGroup.ContainsDomainValidation(typeof(DummyChildBusinessObject)));
			Assert(!ValidationGroup.ContainsDomainValidation(typeof(AutoDummyBizo)));

			ValidationGroup.UnregisterValidationType<DummyBusinessObject, DummyBizOValidationForTest3>();
			Assert(!ValidationGroup.ContainsDomainValidation(typeof(DummyBusinessObject)));
			Assert(!ValidationGroup.ContainsDomainValidation(typeof(DummyChildBusinessObject)));
			Assert(!ValidationGroup.ContainsDomainValidation(typeof(AutoDummyBizo)));
		}

		[ExpectNoExceptions]
		public void TestContainsDomainValidation_NullBusinessObjectType()
		{
			ValidationGroup.ContainsDomainValidation(null);
		}

		public void TestCreateDomainValidation()
		{
			AssertEquals("Pre-condition", 0, ValidationGroup.CreateDomainValidation(Dummy).Length);

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			ZValidation[] additionalValidations = ValidationGroup.CreateDomainValidation(Dummy);
			string failureMessage = "AdditionalValidation is now registered in the DomainValidationTypes, should be creating a new instance of the registered type";
			AssertEquals(failureMessage, 1, additionalValidations.Length);
			AssertEquals(failureMessage, typeof(DummyBizOValidationForTest), additionalValidations[0].GetType());

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest2>();
			additionalValidations = ValidationGroup.CreateDomainValidation(Dummy);
			AssertEquals(failureMessage, 2, additionalValidations.Length);
			AssertContainValidationOfType<DummyBizOValidationForTest>(additionalValidations, true);
			AssertContainValidationOfType<DummyBizOValidationForTest2>(additionalValidations, true);

			AssertEquals("AdditionalValidation not registered, should return empty array", 0, ValidationGroup.CreateDomainValidation(Factory.New<DummyDependantBusinessObject>()).Length);
		}

		public void TestCreateDomainValidation_MultipleSubTypes()
		{
			ValidationGroup.RegisterValidationType<AutoDummyBizo, DummyBizOValidationForTest>();
			ValidationGroup.RegisterValidationType<DummyBaseBusinessObject, DummyBizOValidationForTest2>();
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest3>();
			ZValidation[] additionalValidations = ValidationGroup.CreateDomainValidation(Dummy);
			AssertEquals("Three Validation objects should be created", 3, additionalValidations.Length);
			AssertContainValidationOfType<DummyBizOValidationForTest>(additionalValidations, true);
			AssertContainValidationOfType<DummyBizOValidationForTest2>(additionalValidations, true);
			AssertContainValidationOfType<DummyBizOValidationForTest3>(additionalValidations, true);

			DummyChildBusinessObject dummyChild = Factory.New<DummyChildBusinessObject>();
			AssertEquals("Sanity check", typeof(DummyBaseBusinessObject), typeof(DummyChildBusinessObject).BaseType);
			additionalValidations = ValidationGroup.CreateDomainValidation(dummyChild);
			AssertEquals("Two Validation objects should be created", 2, additionalValidations.Length);
			AssertContainValidationOfType<DummyBizOValidationForTest>(additionalValidations, true);
			AssertContainValidationOfType<DummyBizOValidationForTest2>(additionalValidations, true);
			AssertContainValidationOfType<DummyBizOValidationForTest3>(additionalValidations, false);
		}

		[ExpectNoExceptions]
		public void TestCreateDomainValidation_NullBusinessObjectType()
		{
			ValidationGroup.CreateDomainValidation(null);
		}

		public void TestValidateAllWithDomainValidations()
		{
			Dummy.FillWithValidTestData();
			Dummy.Validation.ValidateAll();
			Assert("Pre-condition", !Dummy.HasErrors);

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest>();
			Dummy.Validation.ValidateAll();
			AssertEquals("Should be added from DummyBizOValidationForTest", 1, Dummy.Z0_AnotherDateInfo.GetErrors().Count());
			AssertHasErrors("FROM TEST1", Dummy.Z0_AnotherDateInfo);

			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest2>();
			ValidationGroup.RegisterValidationType<DummyBusinessObject, DummyBizOValidationForTest3>();
			Dummy.Validation.ValidateAll();
			AssertEquals("Should be added from all three domain validations", 3, Dummy.Z0_AnotherDateInfo.GetErrors().Count());
			AssertHasErrors("FROM TEST1", Dummy.Z0_AnotherDateInfo);
			AssertHasErrors("FROM TEST2", Dummy.Z0_AnotherDateInfo);
			AssertHasErrors("FROM TEST3", Dummy.Z0_AnotherDateInfo);
		}

		void AssertContainValidationOfType<ValidationTypeT>(ZValidation[] validations, bool expectToFind) where ValidationTypeT : AutoDummyBizoValidation
		{
			foreach (ZValidation validation in validations)
			{
				if (validation.GetType() == typeof(ValidationTypeT))
				{
					Assert(typeof(ValidationTypeT).Name + " should not exist in the list", expectToFind);
					return;
				}
			}

			Assert(typeof(ValidationTypeT).Name + " does not exist in the list", !expectToFind);
		}

		DomainValidationGroup ValidationGroup
		{
			get { return Factory.Validation.MainGroup; }
		}

		#region class InvalidDummyBizOValidationForTest

		class InvalidDummyBizOValidationForTest : ZValidation
		{
			public InvalidDummyBizOValidationForTest(BusinessObject parent, BusinessObject parent2)
				: base(parent)
			{
			}

			public InvalidDummyBizOValidationForTest(DummyBusinessObject parent, int something)
				: base(parent)
			{
			}

			public override Type AutoValidationType
			{
				get { return typeof(InvalidDummyBizOValidationForTest); }
			}

			public override void ValidateAll()
			{
			}
		}

		#endregion
	}
}
