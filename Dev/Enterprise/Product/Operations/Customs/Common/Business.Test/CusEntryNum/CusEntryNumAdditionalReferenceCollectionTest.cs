using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.Freight;
using Enterprise.Registry.Business;
using NUnit.Framework;
using WTG.NUnit;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;

namespace Enterprise.Customs.Common.Testing
{
	[TestedType(typeof(CusEntryNumAdditionalReferenceCollection))]
	class CusEntryNumAdditionalReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestPackLineSynchroniseOnRemoved()
		{
			var testPackLineSynchroniseProvider = Factory.New<TestPackLineSynchroniseProvider>();
			var entryNumColl = new CusEntryNumAdditionalReferenceCollection(testPackLineSynchroniseProvider);
			var provider = testPackLineSynchroniseProvider as IPackLineSynchroniseProvider;
			var entryNum = entryNumColl.AddNew();
			NUnit.Framework.Assert.That(((TestPackLineSynchronise)provider.PackLineSynchronise).index, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Before remove event");
			entryNumColl.Remove(entryNum);
			NUnit.Framework.Assert.That(((TestPackLineSynchronise)provider.PackLineSynchronise).index, Is.EqualTo(1).Using(CustomComparers.TypeComparison), "After remove event");
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);
			CusEntryNumber number = numbers.AddNew();
			NUnit.Framework.Assert.That(number.CE_Category, Is.EqualTo(CusEntryNumber.Categories.AdditionalReferenceNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(number.CE_ParentID, Is.EqualTo(bizObj.PK));
			NUnit.Framework.Assert.That(number.CE_ParentTable, Is.EqualTo(AutoDummyBizo.Schema.TableName).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(number.Parent, Is.EqualTo(bizObj).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAllNumbersAsString()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);

			var cusNumber1 = numbers.AddNew();
			cusNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "XXXABILL1";
			NUnit.Framework.Assert.That(numbers.AllNumbersAsString, Is.EqualTo("AMS: XXXABILL1").Using(CustomComparers.TypeComparison));

			cusNumber1.CE_RN_NKCountryCode = "AU";
			NUnit.Framework.Assert.That(numbers.AllNumbersAsString, Is.EqualTo("AMS: XXXABILL1/AU").Using(CustomComparers.TypeComparison));

			var cusNumber2 = numbers.AddNew();
			cusNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusNumber2.CE_EntryNum = "CV00001";
			cusNumber2.CE_RN_NKCountryCode = "CN";
			NUnit.Framework.Assert.That(numbers.AllNumbersAsString, Is.EqualTo("AMS: XXXABILL1/AU, BKG: CV00001/CN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAllNumbersAsString_Empty()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);
			NUnit.Framework.Assert.That(numbers.AllNumbersAsString, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIBODocDataProviderCollectionGetRow()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);

			CusEntryNumber number1 = numbers.AddNew();
			number1.CE_EntryType = "AAA";

			CusEntryNumber number2 = numbers.AddNew();
			number2.CE_EntryType = "BBB";
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			IBODocDataProviderCollection collection = numbers;
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["AAA"]), Is.EqualTo(number1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["AU:AAA"]), Is.EqualTo(number1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["OTH:AU:AAA"]), Is.EqualTo(number1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["ZW:AAA"]), Is.EqualTo(default(BusinessObject)));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["CUS:AU:AAA"]), Is.EqualTo(default(BusinessObject)));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["BBB"]), Is.EqualTo(number2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["GB:BBB"]), Is.EqualTo(number2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(BODocDataProvider.GetBusinessObject(collection["AU:BBB"]), Is.EqualTo(default(BusinessObject)));
			NUnit.Framework.Assert.That(collection["ZZZ"], Is.EqualTo(default(IBODocDataProvider)));
			NUnit.Framework.Assert.That(collection[null], Is.EqualTo(default(IBODocDataProvider)));
			NUnit.Framework.Assert.That(collection[string.Empty], Is.EqualTo(default(IBODocDataProvider)));
		}

		[ExpectNoExceptions]
		public void TestLoadAfterSave()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);
			CusEntryNumber number = numbers.AddNew();
			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			number.CE_EntryNum = "111";
			number.CE_ParentTable = "JobDeclaration";
			Factory.Save();

			var loadedBizObj = NewFactory().Load<DummyBusinessObject>(bizObj.PK);
			var loadedNumbers = new CusEntryNumAdditionalReferenceCollection(loadedBizObj);
			loadedNumbers.Load();
			NUnit.Framework.Assert.That(loadedNumbers.Count, Is.GreaterThan(0), "Numbers must be not empty");
			NUnit.Framework.Assert.That(loadedNumbers.Contains(number.PK), Is.True, "Numbers contains previously saved number");
			NUnit.Framework.Assert.That(loadedNumbers[0].Parent, Is.EqualTo(loadedBizObj).Using(CustomComparers.TypeComparison), "Parent retained");
		}

		[ExpectNoExceptions]
		public void TestAddNewIfNotExist()
		{
			var numbers = new CusEntryNumAdditionalReferenceCollection(Factory.New<DummyBusinessObject>());
			numbers.AddNewIfNotExist("AAA", "number1");
			numbers.AddNewIfNotExist("AAA", "number2");

			CusEntryNumber entryNumber = numbers.AddNewIfNotExist("BBB", "number1");
			NUnit.Framework.Assert.That(entryNumber.CE_EntryType, Is.EqualTo("BBB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryNumber.CE_EntryNum, Is.EqualTo("number1").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(3));

			NUnit.Framework.Assert.That(numbers.AddNewIfNotExist("", "some number"), Is.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(numbers.AddNewIfNotExist("CCC", ""), Is.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(3), "Not adding when type or number are empty");

			NUnit.Framework.Assert.That(numbers.AddNewIfNotExist("AAA", "number1"), Is.EqualTo(default(CusEntryNumber)));
			numbers.AddNewIfNotExist("BBB", "number1");
			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(3), "Not adding if type/number pair already exist");

			numbers.AddNewIfNotExist("AAA", "number3");
			numbers.AddNewIfNotExist("BBB", "number3");
			numbers.AddNewIfNotExist("CCC", "number1");
			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(6));

			var entryNumber_AddeddOffInterface = ((ICusEntryNumAdditionalReferenceCollection)numbers).AddNewIfNotExist("DDD", "number4");
			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(7));
			NUnit.Framework.Assert.That(entryNumber_AddeddOffInterface.CE_EntryType, Is.EqualTo("DDD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryNumber_AddeddOffInterface.CE_EntryNum, Is.EqualTo("number4").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddNewIfNotExistLongNumber()
		{
			var numbers = new CusEntryNumAdditionalReferenceCollection(Factory.New<DummyBusinessObject>());
			var number = new ZString('1', 40);
			var type = new ZString("AAA");
			numbers.AddNewIfNotExist(type, number);
			numbers.AddNewIfNotExist(type, number);

			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestGetAllReferenceNumbersByType()
		{
			var numbers = new CusEntryNumAdditionalReferenceCollection(Factory.New<DummyBusinessObject>());
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByType("").Length, Is.EqualTo(0));
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByType("AAA").Length, Is.EqualTo(0));

			Action<string, string> addNewNumber = (type, number) =>
				{
					CusEntryNumber number1 = numbers.AddNew();
					number1.CE_EntryType = type;
					number1.CE_EntryNum = number;
				};

			addNewNumber("AAA", "number1");
			addNewNumber("BBB", "number2");
			addNewNumber("AAA", "number3");
			addNewNumber("AAA", "number4");

			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByType("AAA"), Is.EquivalentTo(new ZString[] { "number1", "number3", "number4" }));
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByType("BBB"), Is.EquivalentTo(new ZString[] { "number2" }));
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByType("XXX"), Is.EquivalentTo(Array.Empty<ZString>()));
		}

		[ExpectNoExceptions]
		public void TestGetAllReferenceNumbersByTypeAndCountry()
		{
			var numbers = new CusEntryNumAdditionalReferenceCollection(Factory.New<DummyBusinessObject>());
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByTypeAndCountry("", "").Length, Is.EqualTo(0));
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByTypeAndCountry("AAA", "AU").Length, Is.EqualTo(0));

			Action<string, string, string> addNewNumber = (type, number, countryCode) =>
			{
				CusEntryNumber number1 = numbers.AddNew();
				number1.CE_EntryType = type;
				number1.CE_EntryNum = number;
				number1.CE_RN_NKCountryCode = countryCode;
			};

			addNewNumber("AAA", "number1", "AU");
			addNewNumber("BBB", "number2", "AU");
			addNewNumber("AAA", "number3", "AU");
			addNewNumber("AAA", "number4", "AU");

			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByTypeAndCountry("AAA", "AU"), Is.EquivalentTo(new ZString[] { "number1", "number3", "number4" }));
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByTypeAndCountry("BBB", "AU"), Is.EquivalentTo(new ZString[] { "number2" }));
			NUnit.Framework.Assert.That(numbers.GetAllReferenceNumbersByTypeAndCountry("XXX", "XX"), Is.EquivalentTo(Array.Empty<ZString>()));
		}

		[ExpectNoExceptions]
		public void TestGetNumberByType()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);
			CusEntryNumber number = numbers.AddNew();
			number.CE_EntryType = "XXX";
			NUnit.Framework.Assert.That(numbers.GetFirstReferenceNumberByType("XXX"), Is.EqualTo(number));
		}

		[ExpectNoExceptions]
		public void TestGetNumberByTypeAndCountry()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);

			CusEntryNumber number = numbers.AddNew();
			number.CE_EntryType = "XXX";
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Algeria;

			CusEntryNumber number2 = numbers.AddNew();
			number2.CE_EntryType = "XX1";
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Algeria;

			CusEntryNumber number3 = numbers.AddNew();
			number3.CE_EntryType = "XX2";
			number3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Romania;

			NUnit.Framework.Assert.That(numbers.GetFirstReferenceNumberByTypeAndCountry("XX1", Core.Constants.CountryCodes.Algeria), Is.EqualTo(number2));
			NUnit.Framework.Assert.That(numbers.GetFirstReferenceNumberByTypeAndCountry("XX2", Core.Constants.CountryCodes.Romania), Is.EqualTo(number3));
		}

		[ExpectNoExceptions]
		public void TestICusEntryNumAdditionalReferenceCollectionMembers()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = (ICusEntryNumAdditionalReferenceCollection)new CusEntryNumAdditionalReferenceCollection(bizObj);

			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(0), "Precondition");
			var number = numbers.AddNew();
			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(1), "AddNew() created a new number");
			NUnit.Framework.Assert.That(numbers[0], Is.EqualTo(number), "Test indexer");

			numbers.RemoveAndDelete(number);
			NUnit.Framework.Assert.That(numbers.Count, Is.EqualTo(0), "Test RemoveAndDelete deleted the numer");
		}

		[ExpectNoExceptions]
		public void TestCusEntryNumAdditionalReferenceCollectionProvider()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var provider = new CusEntryNumAdditionalReferenceCollectionProvider();
			var collection = provider.GetCollection(parent);
			NUnit.Framework.Assert.That(collection is CusEntryNumAdditionalReferenceCollection, Is.True);

			var number = (CusEntryNumber)collection.AddNew();
			NUnit.Framework.Assert.That(number.CE_Category, Is.EqualTo(CusEntryNumber.Categories.AdditionalReferenceNumber).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(number.CE_ParentID, Is.EqualTo(parent.PK));
			NUnit.Framework.Assert.That(number.CE_ParentTable, Is.EqualTo(AutoDummyBizo.Schema.TableName).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(number.Parent, Is.EqualTo(parent).Using(CustomComparers.TypeComparison));
		}

		#region RemoveErrorReporter tests

		public override void TestRemoveFromRelationship()
		{
			base.TestRemoveFromRelationship();
			CargoWise.Common.ErrorReporter.Clear();
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			base.TestAddAndCancelOfElementAsThoughBinding();
			CargoWise.Common.ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestRemoveDeveloperNotification()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);
			CusEntryNumber num1 = numbers.AddNew();
			num1.CE_ParentTable = "JobDeclaration";

			Factory.Save();
			numbers.Remove(num1);

			NUnit.Framework.Assert.That(CargoWise.Common.ErrorReporter.LastKeyReported, Is.EqualTo("MEP:CusEntryNumAdditionalReferenceCollection:Remove"));
			CargoWise.Common.ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestSuspendRemoveDeveloperNotification()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var numbers = new CusEntryNumAdditionalReferenceCollection(bizObj);

			CusEntryNumber num1 = numbers.AddNew();
			CusEntryNumber num2 = numbers.AddNew();

			numbers.RemoveAndDelete(num1);
			numbers.RemoveAndDeleteAll();

			NUnit.Framework.Assert.That(CargoWise.Common.ErrorReporter.LastKeyReported, Is.EqualTo(string.Empty));
		}

		#endregion

		#region Implementation

		class TestPackLineSynchroniseProvider : DummyBusinessObject, IPackLineSynchroniseProvider
		{
			public TestPackLineSynchroniseProvider(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}
			IPackLineSynchronise packLineSynchronise;
			IPackLineSynchronise IPackLineSynchroniseProvider.PackLineSynchronise
			{
				get { return packLineSynchronise ?? (packLineSynchronise = new TestPackLineSynchronise()); }
			}
		}

		class TestPackLineSynchronise : IPackLineSynchronise
		{
			public ZInt index = 0;

			void IPackLineSynchronise.MarkSyncDirty()
			{
				index++;
			}

			void IPackLineSynchronise.CleanForConcurrency()
			{
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			return new CusEntryNumAdditionalReferenceCollection(bizObj);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusEntryNumber>();
		}

		#endregion
	}
}
