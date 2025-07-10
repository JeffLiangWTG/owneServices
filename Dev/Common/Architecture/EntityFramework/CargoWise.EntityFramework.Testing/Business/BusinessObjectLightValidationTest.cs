using System;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectLightValidationTest : TestCaseWithFactory
	{
		public void TestSuspendMarkingAsNeedingValidation()
		{
			MutantChild kid = Factory.New<MutantChild>();
			kid.IsTestingSuspendMarkingAsNeedingValidation = true;
			AssertEquals("LightValidationIsValid", false, kid.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", false, ((ILightValidationInternals)kid).IsValid);

			kid.RunPreSaveValidation();
			kid.OnFactorySavingInternal();
			AssertEquals("LightValidationIsValid", true, kid.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", true, ((ILightValidationInternals)kid).IsValid);

			using (kid.SuspendMarkingAsNeedingValidation())
			{
				kid.Z0_Code = "ABC";
			}

			AssertEquals("LightValidationIsValid", true, kid.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", true, ((ILightValidationInternals)kid).IsValid);
		}

		public void TestSuspensionOfMarkingAsInvalidDuringHasChanges()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			MutantChild kid = Factory.New<MutantChild>();
			kid.Parent = dummy;
			AssertEquals("LightValidationIsValid", false, dummy.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", false, ((ILightValidationInternals)dummy).IsValid);

			dummy.RunPreSaveValidation();
			kid.RunPreSaveValidation();
			kid.OnFactorySavingInternal();
			dummy.OnFactorySavingInternal();
			AssertEquals("LightValidationIsValid", true, dummy.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", true, ((ILightValidationInternals)dummy).IsValid);
			AssertEquals(1, dummy.RunPreSaveValidationCount);
		}

		public void TestValidationWhenSavingTwiceWithAutoLoggedBO()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("LightValidationIsValid", false, dummy.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", false, ((ILightValidationInternals)dummy).IsValid);
			dummy.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("LightValidationIsValid", true, dummy.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", true, ((ILightValidationInternals)dummy).IsValid);
			AssertEquals(1, dummy.RunPreSaveValidationCount);
			dummy.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("LightValidationIsValid", true, dummy.LightValidationIsValid);
			AssertEquals("DataLayerIsValid", true, ((ILightValidationInternals)dummy).IsValid);
			AssertEquals(1, dummy.RunPreSaveValidationCount);
		}

		public void TestValidationAfterReload()
		{
			DummyBusinessObject dummyNoErr = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummyWithErr = Factory.New<DummyBusinessObject>();
			dummyWithErr.Z0_Description = "Bad";

			dummyNoErr.RunPreSaveValidation();
			dummyWithErr.RunPreSaveValidation();
			AssertEquals(1, dummyNoErr.RunPreSaveValidationCount);
			AssertEquals(1, dummyWithErr.RunPreSaveValidationCount);
			Assert("No error", !dummyNoErr.HasErrors);
			Assert("Has error", dummyWithErr.HasErrors);

			dummyNoErr.RunPreSaveValidation();
			dummyWithErr.RunPreSaveValidation();
			AssertEquals(1, dummyNoErr.RunPreSaveValidationCount);
			AssertEquals(2, dummyWithErr.RunPreSaveValidationCount);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummyNoErrReloaded = factory2.Load<DummyBusinessObject>(dummyNoErr.PK);
			DummyBusinessObject dummyWithErrReloaded = factory2.Load<DummyBusinessObject>(dummyWithErr.PK);

			dummyNoErrReloaded.RunPreSaveValidation();
			dummyWithErrReloaded.RunPreSaveValidation();
			AssertEquals(0, dummyNoErrReloaded.RunPreSaveValidationCount);
			AssertEquals(1, dummyWithErrReloaded.RunPreSaveValidationCount);
		}

		public void TestLightValidationEnabled()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("LightValidationEnabled", true, dummy.LightValidationEnabled);

			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals("LightValidationEnabled", false, dummy2.LightValidationEnabled);

			frameworkSettings.LightValidationEnabled = false;
			AssertEquals("LightValidationEnabled", false, dummy.LightValidationEnabled);
			AssertEquals("LightValidationEnabled", false, dummy2.LightValidationEnabled);
		}

		public void TestShouldValidateOnSave()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);
			AssertEquals("IsValid", false, dummy.LightValidationIsValid);

			dummy.RunPreSaveValidation();
			AssertEquals("IsValid", true, dummy.LightValidationIsValid);
			AssertEquals("Should validate on save", false, dummy.ShouldValidateOnSave);

			dummy.Z0_Code = "EDI";
			AssertEquals("IsValid", true, dummy.LightValidationIsValid);
			AssertEquals("Should validate on save", false, dummy.ShouldValidateOnSave);

			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			dummy2.RunPreSaveValidation();
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			dummy2.ZD1_Code = "EDI";
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
		}

		public void TestMarkAsNeedingValidation()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.RunPreSaveValidation();
			AssertEquals("IsValid", true, dummy.LightValidationIsValid);
			AssertEquals("Should validate on save", false, dummy.ShouldValidateOnSave);

			dummy.MarkAsNeedingValidation();
			AssertEquals("IsValid", false, dummy.LightValidationIsValid);
			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);

			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			dummy.MarkAsNeedingValidation();
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
		}

		[NUnit.Framework.ExpectException(typeof(NotSupportedException))]
		public void TestLightValidationIsValidReportsDeveloperErrorWhenNoLightValidation()
		{
			DummyDependantBusinessObject dependant = Factory.New<DummyDependantBusinessObject>();
			object x = dependant.LightValidationIsValid;
		}

		public void TestLightValidationIsValidGetsPersisted()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals(false, dummy.LightValidationIsValid);
			Factory.Save();
			AssertEquals(false, ((ILightValidationInternals)dummy).IsValid);

			BusinessObjectFactory reloadFactory = new BusinessObjectFactory();
			DummyBusinessObject dummyReloaded = reloadFactory.Load<DummyBusinessObject>(dummy.PK);
			AssertEquals(false, dummyReloaded.LightValidationIsValid);
			AssertEquals(false, ((ILightValidationInternals)dummyReloaded).IsValid);

			dummy.RunPreSaveValidation();
			AssertEquals(true, dummy.LightValidationIsValid);
			Factory.Save();
			AssertEquals(true, ((ILightValidationInternals)dummy).IsValid);

			reloadFactory = new BusinessObjectFactory();
			dummyReloaded = reloadFactory.Load<DummyBusinessObject>(dummy.PK);
			AssertEquals(true, dummyReloaded.LightValidationIsValid);
			AssertEquals(true, ((ILightValidationInternals)dummy).IsValid);
		}

		public void TestPreSaveValidationWithLightValidation()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals(0, dummy.RunPreSaveValidationCount);
			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);

			dummy.RunPreSaveValidation();
			AssertEquals(1, dummy.RunPreSaveValidationCount);

			dummy.RunPreSaveValidation();
			AssertEquals(1, dummy.RunPreSaveValidationCount);

			dummy.MarkAsNeedingValidation();
			dummy.RunPreSaveValidation();
			AssertEquals(2, dummy.RunPreSaveValidationCount);

			dummy.RunPreSaveValidation();
			AssertEquals("Dummy has light validation so should only be validated when dirty", 2, dummy.RunPreSaveValidationCount);

			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(0, dummy2.RunPreSaveValidationCount);
			dummy2.RunPreSaveValidation();
			AssertEquals(1, dummy2.RunPreSaveValidationCount);
			dummy2.RunPreSaveValidation();
			AssertEquals("DummyDependent has normal validation so should be validated every time", 2, dummy2.RunPreSaveValidationCount);
		}

		public void TestMarkAsNeedingValidationIncludingChildren()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			dummy.RegisterEditableChildObject(dummy2);
			dummy.RegisterEditableChildObject(dummy3);
			dummy2.RegisterEditableChildObject(dummy4);
			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy3.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy4.ShouldValidateOnSave);

			dummy.RunPreSaveValidation();
			AssertEquals("Should validate on save", false, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			AssertEquals("Should validate on save", false, dummy3.ShouldValidateOnSave);
			AssertEquals("Should validate on save", false, dummy4.ShouldValidateOnSave);

			dummy.MarkAsNeedingValidationIncludingChildren();
			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy3.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy4.ShouldValidateOnSave);
		}

		public void TestValidateShallowIfImprovesPreSaveValidationPerformance()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy5 = Factory.New<DummyBusinessObject>();
			dummy.RegisterEditableChildObject(dummy2);
			dummy2.RegisterEditableChildObject(dummy5);
			dummy.RegisterEditableChildObject(dummy3);
			dummy.Collection.Add(dummy4);

			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy3.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy4.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy5.ShouldValidateOnSave);

			dummy.ValidateShallowIfImprovesPreSaveValidationPerformance();

			AssertEquals("Should validate on save", false, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
			AssertEquals("Should validate on save", false, dummy3.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy4.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy5.ShouldValidateOnSave);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestValidateShallowIfImprovesPreSaveValidationPerformance_WhenValidationSuspended()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			using (dummy.GetValidationSuspender())
			{
				dummy.Z0_VarCharMax = "Value";
				dummy.ValidateShallowIfImprovesPreSaveValidationPerformance();
			}
		}

		public void TestIBusinessValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy.RegisterEditableChildObject(dummy2);

			AssertEquals("Should validate on save", true, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);

			((IBusiness)dummy).ValidateIfQuickAndImprovesPreSaveValidationPerformance();

			AssertEquals("Should validate on save", false, dummy.ShouldValidateOnSave);
			AssertEquals("Should validate on save", true, dummy2.ShouldValidateOnSave);
		}

		public void TestRunPreSaveValidationFetchWithLightValidation()
		{
			DummyBusinessObject bizO1 = DummyBusinessObject.New(Factory);
			DummyBusinessObject bizO2 = DummyBusinessObject.New(Factory);
			BusinessObject dep1 = Factory.New(typeof(DummyDependantBusinessObject));
			bizO1.RegisterEditableChildObject(dep1);
			bizO1.RegisterEditableChildObject(bizO2);
			bizO1.LightValidationIsValid = true;

			BusinessObjectFetchStrategyForTest dep1Fetch = (BusinessObjectFetchStrategyForTest)dep1.FetchStrategy;
			BusinessObjectFetchStrategyForTest bizO1Fetch = (BusinessObjectFetchStrategyForTest)bizO1.FetchStrategy;
			BusinessObjectFetchStrategyForTest bizO2Fetch = (BusinessObjectFetchStrategyForTest)bizO2.FetchStrategy;
			AssertEquals(0, dep1Fetch.FetchForValidateCoreCount);
			AssertEquals(0, bizO1Fetch.FetchForValidateCoreCount);
			AssertEquals(0, bizO2Fetch.FetchForValidateCoreCount);

			bizO1.RunPreSaveValidationInternal(false);
			((IBusiness)bizO1).RunPreSaveValidationFetch(false);
			AssertEquals(0, bizO1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, dep1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, bizO2Fetch.FetchForValidateCoreCount);
		}

		public void TestIsValidChanges()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			Factory.Save();
			AssertEquals("bizO.HasChanges", false, bizO.HasChanges);
			AssertEquals("bizO.HasChanges", false, ((ILightValidationInternals)bizO).IsValidHasChanges);
			((ILightValidationInternals)bizO).IsValid = true;
			AssertEquals("bizO.HasChanges", false, bizO.HasChanges);
			AssertEquals("bizO.HasChanges", true, ((ILightValidationInternals)bizO).IsValidHasChanges);
			Factory.Save();
			AssertEquals("bizO.HasChanges", false, bizO.HasChanges);
			AssertEquals("bizO.HasChanges", false, ((ILightValidationInternals)bizO).IsValidHasChanges);
			((ILightValidationInternals)bizO).IsValid = false;
			AssertEquals("bizO.HasChanges", false, bizO.HasChanges);
			AssertEquals("bizO.HasChanges", true, ((ILightValidationInternals)bizO).IsValidHasChanges);
			bizO.Z0_Code = "ZZZ";
			AssertEquals("bizO.HasChanges", true, bizO.HasChanges);
			AssertEquals("bizO.HasChanges", true, ((ILightValidationInternals)bizO).IsValidHasChanges);
			Factory.Save();
			AssertEquals("bizO.HasChanges", false, bizO.HasChanges);
			AssertEquals("bizO.HasChanges", false, ((ILightValidationInternals)bizO).IsValidHasChanges);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestIsValidConcurrencyPolicy()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			Factory.Save();
			TestConnection.ExecuteNonQuery(
				"update dbo.DummyBizo set Z0_IsValid = @bool where Z0_PK = @pk",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@bool", "Y", DummyBizoSchema.Z0_IsValid);
					cmd.AddParameterBasedOnDbColumn("@pk", bizO.PK.ToGuid(), DummyBizoSchema.PK);
				});
			bizO.Z0_Short = 10;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			frameworkSettings = TestEntityFrameworkSettings.Get();
			frameworkSettings.LightValidationEnabled = true;
		}

		protected override void TearDown()
		{
			base.TearDown();

			frameworkSettings.Dispose();
		}

		TestEntityFrameworkSettings frameworkSettings;
	}
}
