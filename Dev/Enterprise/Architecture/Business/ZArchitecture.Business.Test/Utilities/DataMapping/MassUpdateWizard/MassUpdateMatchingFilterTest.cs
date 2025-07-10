using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(MassUpdateMatchingFilter))]
	sealed class MassUpdateMatchingFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateField()
		{
			MassUpdateMatchingFilter filter = new MassUpdateMatchingFilter(Helper.Wizard);
			filter.Field = ZString.Empty;
			AssertHasErrorContaining(filter.FieldInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(filter.FieldInfo, ListValidation.InvalidCodeError);

			filter.Field = "BSZSD";
			AssertNoErrorContaining(filter.FieldInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(filter.FieldInfo, ListValidation.InvalidCodeError);

			foreach (ICodeDescription pair in filter.FieldList)
			{
				filter.Field = pair.Code;
				AssertNoErrorContaining(filter.FieldInfo, MandatoryValidation.MustBeEntered);
				AssertNoErrorContaining(filter.FieldInfo, ListValidation.InvalidCodeError);
			}
		}

		public void TestFieldValueType()
		{
			IBusinessObjectCollection currencyList = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { Factory });
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC Noodle");
			list.AddPair("DEF", "DEF Spaghetti");
			list.AddPair("GHI", "GHI Rice");
			Helper.Z0_GuidBindToListForTesting = currencyList;
			Helper.Z0_FK_CodeBindToListForTesting = list;
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			MassUpdateMatchingFilter filter = new MassUpdateMatchingFilter(wizard);
			Dictionary<string, FieldType> expectedResult = new Dictionary<string, FieldType>();
			expectedResult.Add("", FieldType.Text);
			expectedResult.Add("Number (" + DummyBizoSchema.Constants.Z0_Number + ")", FieldType.Integer);
			expectedResult.Add("Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")", FieldType.TextMultiLine);
			expectedResult.Add("Bool (" + DummyBizoSchema.Constants.Z0_Bool + ")", FieldType.Boolean);
			expectedResult.Add("Short (" + DummyBizoSchema.Constants.Z0_Short + ")", FieldType.Integer);
			expectedResult.Add("Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")", FieldType.Decimal);
			expectedResult.Add("Date (" + DummyBizoSchema.Constants.Z0_Date + ")", FieldType.DateTime);
			expectedResult.Add("Date (With Offset) (" + DummyBizoSchema.Constants.Z0_DateTimeOffset + ")", FieldType.DateTimeOffset);
			expectedResult.Add("Geography (" + DummyBizoSchema.Constants.Z0_Geography + ")", FieldType.Geography);
			expectedResult.Add("Guid (" + DummyBizoSchema.Constants.Z0_Guid + ")", FieldType.GuidDropEdit);
			expectedResult.Add("FK_Code (" + DummyBizoSchema.Constants.Z0_FK_Code + ")", FieldType.TextDropEdit);
			expectedResult.Add("Byte (" + DummyBizoSchema.Constants.Z0_Byte + ")", FieldType.Byte);
			expectedResult.Add("Time (" + DummyBizoSchema.Constants.Z0_Time + ")", FieldType.Time);

			foreach (KeyValuePair<string, FieldType> pair in expectedResult)
			{
				filter.Field = pair.Key;
				AssertEquals(pair.Key, pair.Value.ToString(), filter.FieldValueType);
			}

			Helper.Z0_GuidBindToListForTesting = new DummyBusinessObjectCollection(Factory);
			Helper.Z0_FK_CodeBindToListForTesting = currencyList;
			Helper.Z0_GuidModuleIDForTesting = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			wizard = new MassUpdateWizard(Helper.CollectionInfo);
			filter = new MassUpdateMatchingFilter(wizard);

			expectedResult.Clear();
			expectedResult.Add("Guid (" + DummyBizoSchema.Constants.Z0_Guid + ")", FieldType.Guid);
			expectedResult.Add("FK_Code (" + DummyBizoSchema.Constants.Z0_FK_Code + ")", FieldType.TextCodeFindBox);

			foreach (KeyValuePair<string, FieldType> pair in expectedResult)
			{
				filter.Field = pair.Key;
				AssertEquals(pair.Key, pair.Value.ToString(), filter.FieldValueType);
			}
		}

		public void TestModuleID()
		{
			Helper.Z0_GuidModuleIDForTesting = ModuleIDs.RefCurrency;
			var wizard = new MassUpdateWizard(Helper.CollectionInfo);
			var filter = new MassUpdateMatchingFilter(wizard);

			AssertEquals(ModuleIDs.NotAssigned, filter.ModuleID);

			filter.Field = "Guid (" + DummyBizoSchema.Constants.Z0_Guid + ")";
			AssertEquals(ModuleIDs.RefCurrency, filter.ModuleID);

			filter.Field = "FK_Code (" + DummyBizoSchema.Constants.Z0_FK_Code + ")";
			AssertEquals(ModuleIDs.NotAssigned, filter.ModuleID);
		}

		public void TestValidateOperator()
		{
			MassUpdateMatchingFilter filter = new MassUpdateMatchingFilter(Helper.Wizard);
			filter.Operator = ZString.Empty;
			AssertHasErrorContaining(filter.OperatorInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(filter.OperatorInfo, ListValidation.InvalidCodeError);

			filter.Operator = "BSZ";
			AssertNoErrorContaining(filter.OperatorInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(filter.OperatorInfo, ListValidation.InvalidCodeError);

			foreach (ICodeDescription pair in filter.OperatorList)
			{
				filter.Operator = pair.Code;
				AssertNoErrorContaining(filter.OperatorInfo, MandatoryValidation.MustBeEntered);
				AssertNoErrorContaining(filter.OperatorInfo, ListValidation.InvalidCodeError);
			}
		}

		public void TestFieldValueInIZType()
		{
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			MassUpdateMatchingFilter filter = new MassUpdateMatchingFilter(wizard);
			Dictionary<string, IZType> expectedResult = new Dictionary<string, IZType>();
			expectedResult.Add("", ZString.Empty);
			expectedResult.Add("Number (" + DummyBizoSchema.Constants.Z0_Number + ")", ZInt.Zero);
			expectedResult.Add("Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")", ZString.Empty);
			expectedResult.Add("Bool (" + DummyBizoSchema.Constants.Z0_Bool + ")", ZBool.False);
			expectedResult.Add("Short (" + DummyBizoSchema.Constants.Z0_Short + ")", ZShort.Zero);
			expectedResult.Add("Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")", ZDecimal.Zero);
			expectedResult.Add("Date (" + DummyBizoSchema.Constants.Z0_Date + ")", ZDateTime.Empty);
			expectedResult.Add("Date (With Offset) (" + DummyBizoSchema.Constants.Z0_DateTimeOffset + ")", ZDateTimeOffset.Empty);
			expectedResult.Add("Geography (" + DummyBizoSchema.Constants.Z0_Geography + ")", ZGeography.Empty);
			expectedResult.Add("Guid (" + DummyBizoSchema.Constants.Z0_Guid + ")", ZGuid.Empty);
			expectedResult.Add("Byte (" + DummyBizoSchema.Constants.Z0_Byte + ")", ZByte.Zero);
			expectedResult.Add("Time (" + DummyBizoSchema.Constants.Z0_Time + ")", ZTime.Empty);

			foreach (KeyValuePair<string, IZType> pair in expectedResult)
			{
				filter.Field = pair.Key;
				AssertEquals(pair.Key, pair.Value, filter.FieldValueInIZType);
			}
		}

		public void TestNoFactoryInWizard()
		{
			MassUpdateWizard wizard = new MassUpdateWizard(HelperNoFactory.CollectionInfo);
			MassUpdateMatchingFilter filter = new MassUpdateMatchingFilter(wizard);
			Assert("No exception should be thrown when no factory is provided", filter.OperatorList != null);
		}

		public void TestReadOnlyFieldShouldNotCheckMaxLength()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_FK_Code = ZString.Empty;
			bizObj1.Z0_FK_Code_MaxLength = 0;
			collection.Add(bizObj1);

			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBusinessObject.Schema.Z0_FK_Code)
			};

			var wizard = new MassUpdateWizard(collectionInfo);
			var filter = new MassUpdateMatchingFilter(wizard);
			filter.Field = string.Format("{0} ({0})", DummyBusinessObject.Schema.Z0_FK_Code);
			AssertNoExceptionThrown(() => filter.FieldValue = string.Format("Text ({0})", MassUpdateMatchingFilter.Schema.FieldValue));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MassUpdateMatchingFilter(Helper.Wizard);
		}

		MassUpdateWizardTestHelper Helper
		{
			get { return helper ?? (helper = new MassUpdateWizardTestHelper(Factory)); }
		}
		MassUpdateWizardTestHelper helper;

		MassUpdateWizardTestHelperNoFactory HelperNoFactory
		{
			get { return helperNoFactory ?? (helperNoFactory = new MassUpdateWizardTestHelperNoFactory()); }
		}
		MassUpdateWizardTestHelperNoFactory helperNoFactory;

		class MassUpdateWizardTestHelperNoFactory
		{
			public MassUpdateWizard Wizard
			{
				get
				{
					if (wizard == null)
					{
						wizard = new MassUpdateWizard(null);
					}

					return wizard;
				}
			}

			MassUpdateWizard wizard;

			public IImportCollectionInfo CollectionInfo
			{
				get
				{
					if (collectionInfo == null)
					{
						collectionInfo = new ImportCollectionInfoImpl(new DummyBusinessObjectCollection(null))
						{
							new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Number" },
						};
					}

					return collectionInfo;
				}
			}
			IImportCollectionInfo collectionInfo;
		}
	}
}
