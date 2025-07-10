using CargoWise.Types;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	sealed class StmSystemDefinedFieldCountryValidationTest : StmSystemDefinedFieldValidationTestCase
	{
		public void TestCheckS1_IsSuppressed()
		{
			AssertNoErrors("Precondition: S1_IsSuppressed should not have errors.", Parent.S1_IsSuppressedInfo);

			Parent.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.Australia;
			Parent.S1_IsSuppressed = true;
			AssertNoErrors(Parent.S1_IsSuppressedInfo);

			Parent.S1_IsSuppressed = false;
			AssertNoErrors(Parent.S1_IsSuppressedInfo);

			StmSystemDefinedFieldCountry fieldCountry1 = ParentCollection.AddNew();
			fieldCountry1.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.NewZealand;
			fieldCountry1.S1_IsSuppressed = true;
			AssertHasError(fieldCountry1.S1_IsSuppressedInfo, "This field is already country/region specific for AU. There is no need to specify a suppressed country/region.");

			fieldCountry1.S1_IsSuppressed = false;
			AssertNoErrors(fieldCountry1.S1_IsSuppressedInfo);

			StmSystemDefinedFieldCountry fieldCountry2 = ParentCollection.AddNew();
			fieldCountry2.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.Singapore;
			fieldCountry2.S1_IsSuppressed = true;
			AssertHasError(fieldCountry2.S1_IsSuppressedInfo, "This field is already country/region specific for AU and NZ. There is no need to specify a suppressed country/region.");

			fieldCountry2.S1_IsSuppressed = false;
			AssertNoErrors(fieldCountry2.S1_IsSuppressedInfo);

			StmSystemDefinedFieldCountry fieldCountry3 = ParentCollection.AddNew();
			fieldCountry3.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.China;
			fieldCountry3.S1_IsSuppressed = true;
			AssertHasError(fieldCountry3.S1_IsSuppressedInfo, "This field is already country/region specific for AU, NZ and SG. There is no need to specify a suppressed country/region.");

			fieldCountry3.S1_IsSuppressed = false;
			AssertNoErrors(fieldCountry3.S1_IsSuppressedInfo);
		}

		public void TestCheckS1_RN_NKCntrySpecific()
		{
			AssertNoErrors("Precondition: S1_RN_NKCntrySpecific should not have errors.", Parent.S1_RN_NKCntrySpecificInfo);

			Parent.S1_RN_NKCntrySpecific = ZString.Empty;
			AssertHasError(Parent.S1_RN_NKCntrySpecificInfo, "Please enter a Country/Region.");

			StmSystemDefinedFieldCountry fieldCountry = ParentCollection.AddNew();
			fieldCountry.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.Australia;
			AssertNoErrors(fieldCountry.S1_RN_NKCntrySpecificInfo);

			Parent.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.Australia;
			AssertHasError(Parent.S1_RN_NKCntrySpecificInfo, "The Country/Region has been duplicated and must be unique.");

			Parent.S1_RN_NKCntrySpecific = Core.Constants.CountryCodes.NewZealand;
			AssertNoErrors(Parent.S1_RN_NKCntrySpecificInfo);
		}

		#region Implementation

		protected override AutoStmSystemDefinedField GetNewParent()
		{
			return ParentCollection.AddNew();
		}

		StmSystemDefinedFieldCountryCollection ParentCollection
		{
			get
			{
				if (fParentCollection == null)
				{
					StmSystemDefinedField parentField = Factory.New<StmSystemDefinedField>();
					fParentCollection = new StmSystemDefinedFieldCountryCollection(parentField, Factory);
				}
				return fParentCollection;
			}
		}

		StmSystemDefinedFieldCountryCollection fParentCollection;

		#endregion
	}
}
