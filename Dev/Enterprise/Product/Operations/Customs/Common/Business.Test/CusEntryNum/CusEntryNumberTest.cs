using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	[TestedType(typeof(CusEntryNumber))]
	class CusEntryNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var cusEntryNum = (CusEntryNumber)base.GetBusinessObjectForFetchForLoad();
			cusEntryNum.CE_ParentTable = "CusEntryHeader";

			return cusEntryNum;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusEntryNum = (CusEntryNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			cusEntryNum.CE_ParentTable = "CusEntryHeader";

			return cusEntryNum;
		}

		[ExpectNoExceptions]
		public void TestADDLogHasReference()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "ABC";
			entryNumber.CE_EntryNum = "12345";
			entryNumber.CE_ParentTable = "CusEntryHeader";
			Factory.Save();

			NUnit.Framework.Assert.That(entryNumber.Logs.MostRecentLog.SL_Reference, Is.EqualTo("ABC: <12345>").Using(CustomComparers.TypeComparison));
		}

		public void TestNoDBHitOnStmDocDataOverrideWhenDelete()
		{
			var factory = Factory.CreateNewFactory();
			var entryNumber = factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "ABC";
			entryNumber.CE_EntryNum = "12345";
			entryNumber.CE_ParentTable = "CusEntryHeader";
			factory.Save();
			entryNumber.Delete();
			var expectedDBHits = new Dictionary<string, int> { { "StmDocDataOverride", 0 } };
			AssertDbHits(expectedDBHits, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		[ExpectNoExceptions]
		public void TestGetNewValidation()
		{
			var fakeParent = Factory.New<ParentOfCenForTest>();
			var fakeCen = Factory.New<FakeCenForTest>();
			fakeCen.Parent = fakeParent;
			fakeParent.TestType = 1;
			NUnit.Framework.Assert.That(fakeCen.GetNewValidationExposed(), Is.TypeOf(typeof(ParentOfCenForTest.FakeCenValidationType1)));

			fakeParent = Factory.New<ParentOfCenForTest>();
			fakeCen = Factory.New<FakeCenForTest>();
			fakeCen.Parent = fakeParent;
			fakeParent.TestType = 2;
			NUnit.Framework.Assert.That(fakeCen.GetNewValidationExposed(), Is.TypeOf(typeof(ParentOfCenForTest.FakeCenValidationType2)));

			fakeParent = Factory.New<ParentOfCenForTest>();
			fakeCen = Factory.New<FakeCenForTest>();
			fakeParent.TestType = 3;
			NUnit.Framework.Assert.That(fakeCen.GetNewValidationExposed(), Is.TypeOf(typeof(CusEntryNumValidation))); // it's abstract

			fakeParent = Factory.New<ParentOfCenForTest>();
			fakeCen = Factory.New<FakeCenForTest>();
			fakeParent.TestType = 4;
			NUnit.Framework.Assert.That(fakeCen.GetNewValidationExposed(), Is.TypeOf(typeof(CusEntryNumValidation))); // wrogn base type
		}

		[ExpectNoExceptions]
		public void TestErrorReportIfEntryNumberChangedAfterFillMessagePlaceHolder()
		{
			var dummyObject = Factory.New<DummyBusinessObject>();
			var entryColl = new CusEntryNumAdditionalReferenceCollection(dummyObject);
			var entryNum = entryColl.AddNew();
			entryNum.CE_EntryType = "XYZ";
			entryNum.CE_EntryNum = "1234567";
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "~111~";
			entryNum.FillMessagePlaceHolder(message, "111", "222");
			entryNum.CE_EntryNum = "987541";
			NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, Does.Contain("was used to fill placeholder in a message and now is changed to"));
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestDeleteWhenSaveFails()
		{
			AssertDeleteWhenSaveFails(CusEntryNumberTypes.UnitedStates.EntrySummary, Core.Constants.CountryCodes.UnitedStates, true);
			AssertDeleteWhenSaveFails(CusEntryNumberTypes.UnitedStates.InBond, Core.Constants.CountryCodes.UnitedStates, true);
		}

		[ExpectNoExceptions]
		void AssertDeleteWhenSaveFails(ZString entryType, ZString countryCode, bool shouldBeDeleted)
		{
			TestCusEntryNumber entryNumber = Factory.New<TestCusEntryNumber>();
			entryNumber.CE_EntryIsSystemGenerated = true;
			entryNumber.CE_EntryNum = "1";
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = countryCode;
			entryNumber.CE_ParentTable = "CusEntryHeader";

			try
			{
				entryNumber.ThrowExceptionWhenSaving = true;
				Factory.Save();
			}
			catch { }

			NUnit.Framework.Assert.That(entryNumber.IsDeleted, Is.EqualTo(shouldBeDeleted));

			if (shouldBeDeleted)
			{
				entryNumber = Factory.New<TestCusEntryNumber>();
				entryNumber.CE_EntryIsSystemGenerated = true;
				entryNumber.CE_EntryNum = "1";
				entryNumber.CE_EntryType = entryType;
				entryNumber.CE_RN_NKCountryCode = countryCode;
				entryNumber.CE_ParentTable = "CusEntryHeader";
				Factory.Save();

				NUnit.Framework.Assert.That(entryNumber.IsInDatabase, Is.EqualTo(true), "PreCondition:Saved");

				try
				{
					entryNumber.ThrowExceptionWhenSaving = true;
					Factory.Save();
				}
				catch { }

				//"Should not delete this time as number fountain already consumed and this number is already committed to db", 
				NUnit.Framework.Assert.That(entryNumber.IsDeleted, Is.EqualTo(false));
			}
		}

		[ExpectNoExceptions]
		public void TestPackLineSynchroniserWorks()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var syncher = new TestPackLineSynchroniser();
			parent.PackLineSynchroniser = syncher;

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.Parent = parent;

			entryNumber.CE_EntryType = "ABC";
			NUnit.Framework.Assert.That(syncher.index, Is.EqualTo(1), "Synch attempted");

			entryNumber.CE_EntryNum = "123454";
			NUnit.Framework.Assert.That(syncher.index, Is.EqualTo(2), "Synch attempted");
		}

		[ExpectNoExceptions]
		public void TestEventAfterEntryNumChanged()
		{
			var dummyObject = Factory.New<DummyBusinessObject>();
			var entryColl = new CusEntryNumAdditionalReferenceCollection(dummyObject);
			var entryNum = entryColl.AddNew();
			NUnit.Framework.Assert.That(dummyObject.index, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Before change");
			entryNum.CE_EntryNum = "112";
			NUnit.Framework.Assert.That(dummyObject.index, Is.EqualTo(1).Using(CustomComparers.TypeComparison), "After change");
		}

		public void TestAdditionalEntryNumberValidation()
		{
			var dummyObject = Factory.New<DummyBusinessObject>();
			var entryColl = new CusEntryNumAdditionalReferenceCollection(dummyObject);
			var entryNum = entryColl.AddNew();
			entryNum.CE_EntryType = "XYZ";
			entryNum.CE_EntryNum = "1234567";
			AssertHasError(entryNum.CE_EntryNumInfo, "XYZ ERROR ON 1234567");
			entryNum.CE_EntryType = "ABC";
			entryNum.Validation.ValidateCE_EntryNum();
			AssertNoErrors(entryNum.CE_EntryNumInfo);
		}

		[ExpectNoExceptions]
		public void TestEntryStatusDescription()
		{
			var dummyObject = Factory.New<DummyBusinessObject>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.Parent = dummyObject;
			entryNumber.CE_EntryStatus = "AAA";
			NUnit.Framework.Assert.That(entryNumber.EntryStatusDescription, Is.EqualTo("AAA Description").Using(CustomComparers.TypeComparison));
			entryNumber.CE_EntryStatus = "CCC";
			NUnit.Framework.Assert.That(entryNumber.EntryStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		class DummyBusinessObject : DummyBizOWithAutoLogs, IPackLineSynchroniseProvider, IAdditionalReferenceNumberSupporter, ICusEntryNumEntryStatusListProvider
		{
			public ZInt index = 0;

			public DummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IPackLineSynchronise IPackLineSynchroniseProvider.PackLineSynchronise
			{
				get { return PackLineSynchroniser; }
			}

			public TestPackLineSynchroniser PackLineSynchroniser;

			void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
			{
			}

			bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
			{
				get { return false; }
			}

			void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
			{
				index++;
			}

			CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
			{
				get { throw new NotImplementedException(); }
			}

			void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
			{
				if (type == "XYZ")
				{
					info.AddError(type + " ERROR ON " + number);
				}
			}

			CodeDescriptionPairList ICusEntryNumEntryStatusListProvider.EntryStatusList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("AAA", "AAA Description");
					result.AddPair("BBB", "BBB Description");
					return result;
				}
			}

			void ICusEntryNumEntryStatusListProvider.OnEntryStatusSet(ZString entryType, ZString newValue) { }

			public override string TableName => "CusEntryHeader";
		}

		class TestPackLineSynchroniser : IPackLineSynchronise
		{
			public int index;
			void IPackLineSynchronise.MarkSyncDirty()
			{
				index++;
			}

			void IPackLineSynchronise.CleanForConcurrency()
			{
			}
		}

		class TestCusEntryNumber : CusEntryNumber
		{
			public TestCusEntryNumber(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ThrowExceptionWhenSaving;

			public override void OnSaving()
			{
				base.OnSaving();

				if (ThrowExceptionWhenSaving)
				{
					throw new Exception();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLogReference()
		{
			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "ABC";
			newEntryNumber.CE_EntryNum = "12345";
			newEntryNumber.CE_ParentTable = "CusEntryHeader";
			newEntryNumber.AdditionalCustomLogReferenceSuffix = "asDaSD";
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber.Logs.MostRecentLog.SL_Reference, Is.EqualTo("ABC: <12345> asDaSD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(newEntryNumber.AdditionalCustomLogReferenceSuffix, Is.EqualTo("").Using(CustomComparers.TypeComparison));

			System.Threading.Thread.Sleep(100); // Force log entries to have different SL_EventTime values

			newEntryNumber.CE_EntryType = "DEF";
			newEntryNumber.CE_EntryNum = "9876";
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber.Logs.MostRecentLog.SL_Reference, Is.EqualTo("Type: From <ABC> to <DEF>. DEF: From <12345> to <9876>").Using(CustomComparers.TypeComparison));

			System.Threading.Thread.Sleep(100); // Force log entries to have different SL_EventTime values

			newEntryNumber.CE_EntryNum = "7777";
			newEntryNumber.AdditionalCustomLogReferenceSuffix = "asDaSD";
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber.Logs.MostRecentLog.SL_Reference, Is.EqualTo("DEF: From <9876> to <7777> asDaSD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(newEntryNumber.AdditionalCustomLogReferenceSuffix, Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCE_RNSetForNewObject()
		{
			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			NUnit.Framework.Assert.That(newEntryNumber.CE_RN_NKCountryCode, Is.EqualTo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "CE_RN_NKCountryCode is populated");
		}

		[ExpectNoExceptions]
		public void TestDeleteCANEntryNumberOnSaving()
		{
			var newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			newEntryNumber.CE_EntryNum = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber.IsDeleted, Is.EqualTo(true), "Deleted");

			var newEntryNumber2 = Factory.New<CusEntryNumber>();
			newEntryNumber2.CE_EntryType = CusEntryNumberTypes.UnitedStates.FTZ;
			newEntryNumber2.CE_EntryNum = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber2.IsDeleted, Is.EqualTo(true), "Deleted");
		}

		[ExpectNoExceptions]
		public void TestDeleteSavedCANEntryNumberOnSaving()
		{
			var newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "XXX";
			newEntryNumber.CE_ParentTable = "CusEntryHeader";
			Factory.Save();
			newEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			newEntryNumber.CE_EntryNum = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber.IsDeleted, Is.EqualTo(true), "Deleted");

			var newEntryNumber2 = Factory.New<CusEntryNumber>();
			newEntryNumber2.CE_EntryType = "YXZ";
			newEntryNumber2.CE_ParentTable = "CusEntryHeader";
			Factory.Save();
			newEntryNumber2.CE_EntryType = CusEntryNumberTypes.UnitedStates.FTZ;
			newEntryNumber2.CE_EntryNum = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(newEntryNumber2.IsDeleted, Is.EqualTo(true), "Deleted");
		}

		[ExpectNoExceptions]
		public void TestNew()
		{
			DummyBusinessObject bO = Factory.New<DummyBusinessObject>();
			CusEntryNumber number = CusEntryNumber.New(bO, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			NUnit.Framework.Assert.That(number, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(number.CE_EntryType, Is.EqualTo("~~~").Using(CustomComparers.TypeComparison), "EntryType should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentID, Is.EqualTo(bO.PK), "PK should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentTable, Is.EqualTo(bO.TableName).Using(CustomComparers.TypeComparison), "TableName should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_RN_NKCountryCode, Is.EqualTo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "Country PK should match current company's country PK");
			NUnit.Framework.Assert.That(number.Parent, Is.EqualTo(bO).Using(CustomComparers.TypeComparison));

			number = CusEntryNumber.New(bO, "AAA", Core.Constants.CountryCodes.Brazil);
			NUnit.Framework.Assert.That(number, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(number.CE_EntryType, Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "EntryType should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentID, Is.EqualTo(bO.PK), "PK should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_ParentTable, Is.EqualTo(bO.TableName).Using(CustomComparers.TypeComparison), "TableName should match given BizObj");
			NUnit.Framework.Assert.That(number.CE_RN_NKCountryCode, Is.EqualTo(Core.Constants.CountryCodes.Brazil).Using(CustomComparers.TypeComparison), "Country PK should match current company's country PK");
			NUnit.Framework.Assert.That(number.Parent, Is.EqualTo(bO).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLoadFromBusinessObject()
		{
			DummyBusinessObject bizObj1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bizObj2 = Factory.New<DummyBusinessObject>();

			CusEntryNumber number1 = CusEntryNumber.New(bizObj1, "111", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CusEntryNumber number2 = CusEntryNumber.New(bizObj1, "222", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CusEntryNumber number3 = CusEntryNumber.New(bizObj2, "999", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			CusEntryNumber[] loadedNumbers = CusEntryNumber.Load(bizObj1);

			NUnit.Framework.Assert.That(loadedNumbers.Length, Is.EqualTo(2));
			NUnit.Framework.Assert.That(loadedNumbers, Has.Some.EqualTo(number1));
			NUnit.Framework.Assert.That(loadedNumbers, Has.Some.EqualTo(number2));
		}

		[ExpectNoExceptions]
		public void TestSettingParentBusinessObject()
		{
			DummyBusinessObject bizObj1 = Factory.New<DummyBusinessObject>();
			var entryNumber1 = CusEntryNumber.New<FakeCenForTest>(bizObj1, "111", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			FakeCenForTest[] loadedNumbers = CusEntryNumber.Load<FakeCenForTest>(bizObj1);
			var theLoadedNumber = loadedNumbers.Single();
			NUnit.Framework.Assert.That(theLoadedNumber, Is.EqualTo(entryNumber1));
			NUnit.Framework.Assert.That(theLoadedNumber.Parent, Is.EqualTo(bizObj1).Using(CustomComparers.TypeComparison));

			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_ParentID = bizObj1.PK;
			entryNumber2.CE_ParentTable = "DummyBizo";
			NUnit.Framework.Assert.That(entryNumber2.Parent, Is.Not.EqualTo(default(BusinessObject)));
			NUnit.Framework.Assert.That(entryNumber2.Parent.PK, Is.EqualTo(bizObj1.PK));

			DummyBusinessObject bizObj2 = Factory.New<DummyBusinessObject>();
			entryNumber2.CE_ParentID = bizObj2.PK;
			NUnit.Framework.Assert.That(entryNumber2.Parent, Is.Not.EqualTo(default(BusinessObject)));
			NUnit.Framework.Assert.That(entryNumber2.Parent.PK, Is.EqualTo(bizObj2.PK));

			var entryNumber3 = Factory.New<CusEntryNumber>();
			entryNumber3.Parent = bizObj2;
			NUnit.Framework.Assert.That(entryNumber3.CE_ParentTable, Is.EqualTo(bizObj2.TableName).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryNumber3.CE_ParentID, Is.EqualTo(bizObj2.PK));

			entryNumber3.Parent = null;
			NUnit.Framework.Assert.That(entryNumber3.CE_ParentTable, Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryNumber3.CE_ParentID, Is.EqualTo(ZGuid.Empty));
		}

		[ExpectNoExceptions]
		public void TestLoad()
		{
			DummyBusinessObject bizObj = Factory.New<DummyBusinessObject>();
			CusEntryNumber number = CusEntryNumber.New(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CusEntryNumber loadedNumber = CusEntryNumber.Load(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			NUnit.Framework.Assert.That(number, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(loadedNumber, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(loadedNumber.PK, Is.EqualTo(number.PK), "PK should match for both CusEntryNumbers");
			NUnit.Framework.Assert.That(loadedNumber, Is.EqualTo(number), "Both CusEntryNumbers should match");
			NUnit.Framework.Assert.That(number.Parent, Is.EqualTo(bizObj).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(loadedNumber.Parent, Is.EqualTo(bizObj).Using(CustomComparers.TypeComparison));

			number = CusEntryNumber.New(bizObj, "AAA", Core.Constants.CountryCodes.Brazil);
			loadedNumber = CusEntryNumber.Load(bizObj, "AAA", Core.Constants.CountryCodes.Brazil);
			NUnit.Framework.Assert.That(number, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(loadedNumber, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(loadedNumber, Is.EqualTo(number), "Both CusEntryNumbers should match");

			number.CE_ParentTable = "";
			loadedNumber = CusEntryNumber.Load(bizObj, "AAA", Core.Constants.CountryCodes.Brazil);
			NUnit.Framework.Assert.That(loadedNumber, Is.EqualTo(number), "Loading should not be restricted to parentTable");
		}

		[ExpectNoExceptions]
		public void TestLoad_MultipleNumbers()
		{
			CombineAssertions(() =>
			{
				DummyBusinessObject bizObj = Factory.New<DummyBusinessObject>();

				CusEntryNumber number1 = CusEntryNumber.New(bizObj, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Brazil);
				number1.CE_EntryNum = "AAA";
				number1.CE_ParentTable = "CusEntryHeader";

				CusEntryNumber number2 = CusEntryNumber.New(bizObj, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Brazil);
				number2.CE_EntryNum = "BBB";
				number2.CE_ParentTable = "CusEntryHeader";
				Factory.Save();

				var loadedNumbers = CusEntryNumber.Load<CusEntryNumber>(Factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, new ZString[] { "AAA", "BBB" }, Core.Constants.CountryCodes.Brazil);
				NUnit.Framework.Assert.That(loadedNumbers.Length, Is.EqualTo(2), "2 numbers should be loaded");
				NUnit.Framework.Assert.That(new CusEntryNumber[] { number1, number2 }, Is.EquivalentTo(loadedNumbers), "same numbers");
			});
		}

		[ExpectNoExceptions]
		public void TestLoadWillPickEarliestCreated()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var number1 = CusEntryNumber.New(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			number1.CE_EntryNum = "ENT1";
			number1.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			var number2 = CusEntryNumber.New(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			number2.CE_EntryNum = "ENT2";
			number2.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			var loadedNumber = CusEntryNumber.Load(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			NUnit.Framework.Assert.That(loadedNumber, Is.EqualTo(number2));
			Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				var newFactory = new BusinessObjectFactory();
				var bizObj2 = newFactory.Load<DummyBusinessObject>(bizObj.PK);
				loadedNumber = CusEntryNumber.Load(bizObj2, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				NUnit.Framework.Assert.That(loadedNumber.CE_EntryNum, Is.EqualTo("ENT2").Using(CustomComparers.TypeComparison), i.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreate()
		{
			DummyBusinessObject bO = Factory.New<DummyBusinessObject>();
			CusEntryNumber number = CusEntryNumber.New(bO, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CusEntryNumber loadedNumber = CusEntryNumber.LoadOrCreate(bO, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CusEntryNumber createdNumber = CusEntryNumber.LoadOrCreate(bO, "~~1", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			NUnit.Framework.Assert.That(number, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(loadedNumber, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(createdNumber, Is.Not.EqualTo(default(CusEntryNumber)));
			NUnit.Framework.Assert.That(loadedNumber.PK, Is.EqualTo(number.PK), "PK should match for both Number and LoadedNumber CusEntryNumbers");
			NUnit.Framework.Assert.That(loadedNumber, Is.EqualTo(number), "Both Number and LoadedNumber CusEntryNumbers should match");
		}

		[ExpectNoExceptions]
		public void TestLoadWithEntryTypeEntryNumber()
		{
			DummyBusinessObject bO = Factory.New<DummyBusinessObject>();
			CusEntryNumber number = CusEntryNumber.New(bO, "~~1", Core.Constants.CountryCodes.Australia);
			number.CE_EntryNum = "1";

			CusEntryNumber number2 = CusEntryNumber.New(bO, "~~2", Core.Constants.CountryCodes.Australia);
			number2.CE_EntryNum = "2";

			CusEntryNumber number3 = CusEntryNumber.New(bO, "~~2", Core.Constants.CountryCodes.Brazil);
			number3.CE_EntryNum = "2";

			CusEntryNumber[] entryNumbers = CusEntryNumber.Load(Factory, "~~2", "2", Core.Constants.CountryCodes.Brazil);
			NUnit.Framework.Assert.That(entryNumbers.Length, Is.EqualTo(1));
			NUnit.Framework.Assert.That(entryNumbers[0], Is.EqualTo(number3));
		}

		[ExpectNoExceptions]
		public void TestLookupsOnICusEntryNumber()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var number = CusEntryNumber.New(parent, "~~1", Core.Constants.CountryCodes.Australia);
			NUnit.Framework.Assert.That((number as Integration.Customs.ICusEntryNumber).Lookups, Is.EqualTo(number.Lookups).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCanDelete()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			CusEntryNumber number = CusEntryNumber.New(bo, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			NUnit.Framework.Assert.That(((ICanDelete)number).CanDelete, Is.EqualTo(true));
			NUnit.Framework.Assert.That(string.IsNullOrEmpty(((ICanDelete)number).ReasonForNotAbleToDelete), Is.True);

			number.CanDeleteHandler += delegate(object sender, CanDeleteCusEntryNumberEventArgs e)
			{
				e.CanDelete = false;
				e.ReasonForNotAbleToDelete = (NoResString)"AAA";
			};
			NUnit.Framework.Assert.That(((ICanDelete)number).CanDelete, Is.EqualTo(false));
			NUnit.Framework.Assert.That(((ICanDelete)number).ReasonForNotAbleToDelete, Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOnStatusChanged()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var number = CusEntryNumber.New(bizo, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			number.CE_EntryStatus = "AAA"; // no exception occured
			number.StatusChanged += StatusChangedMethod;
			statusCangedFired = false;
			number.CE_EntryStatus = "BBB";
			NUnit.Framework.Assert.That(statusCangedFired, Is.True);
			NUnit.Framework.Assert.That(oldStatusValue, Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(newStatusValue, Is.EqualTo("BBB").Using(CustomComparers.TypeComparison));
			statusCangedFired = false;
			number.CE_EntryStatus = "BBB";
			NUnit.Framework.Assert.That(!statusCangedFired, Is.True);
		}
		bool statusCangedFired;
		ZString oldStatusValue;
		ZString newStatusValue;

		void StatusChangedMethod(object sender, StatusChangedEventArgs e)
		{
			statusCangedFired = true;
			oldStatusValue = e.OldStatus;
			newStatusValue = e.NewStatus;
		}

		[ExpectNoExceptions]
		public void TestGetEntryNumberTypeForDisplay()
		{
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeForDisplay("XLV", "AU", false), Is.EqualTo("EXLV").Using(CustomComparers.TypeComparison), "AU Export: XLV -> EXLV");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeForDisplay("XLV", "AU", true), Is.EqualTo("XLV").Using(CustomComparers.TypeComparison), "AU Import: XLV -> XLV");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeForDisplay("XLV", "GB", false), Is.EqualTo("XLV").Using(CustomComparers.TypeComparison), "GB Export: XLV -> XLV");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeForDisplay("XLV", "GB", true), Is.EqualTo("XLV").Using(CustomComparers.TypeComparison), "GB Import: XLV -> XLV");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeForDisplay("XLV", "DE", false), Is.EqualTo("XLV").Using(CustomComparers.TypeComparison), "DE Export: XLV -> XLV");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeForDisplay("XLV", "DE", true), Is.EqualTo("XLV").Using(CustomComparers.TypeComparison), "DE Import: XLV -> XLV");
		}

		[ExpectNoExceptions]
		public void TestGetEntryNumberTypeFromDisplay()
		{
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeFromDisplay("EXTI", "AU", false), Is.EqualTo("XTI").Using(CustomComparers.TypeComparison), "AU Export: EXTI -> XTI");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeFromDisplay("EXTI", "AU", true), Is.EqualTo("EXT").Using(CustomComparers.TypeComparison), "AU Import: EXTI -> EXT");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeFromDisplay("EXTI", "GB", false), Is.EqualTo("EXT").Using(CustomComparers.TypeComparison), "GB Export: EXTI -> EXT");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeFromDisplay("EXTI", "GB", true), Is.EqualTo("EXT").Using(CustomComparers.TypeComparison), "GB Import: EXTI -> EXT");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeFromDisplay("EXTI", "DE", false), Is.EqualTo("EXT").Using(CustomComparers.TypeComparison), "DE Export: EXTI -> EXT");
			NUnit.Framework.Assert.That(CusEntryNumber.GetEntryNumberTypeFromDisplay("EXTI", "DE", true), Is.EqualTo("EXT").Using(CustomComparers.TypeComparison), "DE Import: EXTI -> EXT");
		}

		[ExpectNoExceptions]
		public void TestGenericLoadOrCreate()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var bizo = newFactory.New<DummyBusinessObject>();
			var num1 = CusEntryNumber.New<FakeCenForTest>(bizo, "KS1", "K$");
			NUnit.Framework.Assert.That(num1.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(num1, bizo.PK, bizo.TableName, "KS1", "K$", ZString.Empty);
			var num2 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(bizo, "!@");
			NUnit.Framework.Assert.That(num2.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(num2, bizo.PK, bizo.TableName, ZString.Empty, "!@", ZString.Empty);
			var num3 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(bizo, "ID3", "G%");
			NUnit.Framework.Assert.That(num3.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(num3, bizo.PK, bizo.TableName, "ID3", "G%", ZString.Empty);
			var num4 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(bizo, "ID4", "ES", "G3");
			NUnit.Framework.Assert.That(num4.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(num4, bizo.PK, bizo.TableName, "ID4", "ES", ZString.Empty, null, "G3");
			num1.CE_EntryNum = "N1";
			num1.CE_EntryStatus = "S1";
			num1.CE_ParentTable = "CusEntryHeader";
			num2.CE_EntryNum = "N2";
			num2.CE_EntryStatus = "S2";
			num2.CE_ParentTable = "CusEntryHeader";
			num3.CE_EntryNum = "N3";
			num3.CE_EntryStatus = "S3";
			num3.CE_ParentTable = "CusEntryHeader";
			num4.CE_EntryNum = "N4";
			num4.CE_EntryStatus = "S4";
			num4.CE_ParentTable = "CusEntryHeader";
			newFactory.Save();

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadNums = CusEntryNumber.Load<FakeCenForTest>(newFactory2, "KS1", "N1", "K$");
			NUnit.Framework.Assert.That(reloadNums.Length, Is.EqualTo(1));
			var reloadNum1 = reloadNums[0];
			NUnit.Framework.Assert.That(reloadNum1.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			NUnit.Framework.Assert.That(reloadNum1.PK, Is.EqualTo(num1.PK));
			AssertCusEntryNumber(reloadNum1, bizo.PK, bizo.TableName, "KS1", "K$", "N1", "S1");
			var reloadBizo = newFactory2.Load<DummyBusinessObject>(bizo.PK);
			NUnit.Framework.Assert.That(reloadBizo.PK, Is.EqualTo(bizo.PK));
			var reloadNum2 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "!@");
			NUnit.Framework.Assert.That(reloadNum2.PK, Is.EqualTo(num2.PK));
			NUnit.Framework.Assert.That(reloadNum2.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(reloadNum2, reloadBizo.PK, reloadBizo.TableName, ZString.Empty, "!@", "N2", "S2");
			var reloadNum3 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "ID3", "G%");
			NUnit.Framework.Assert.That(reloadNum3.PK, Is.EqualTo(num3.PK));
			NUnit.Framework.Assert.That(reloadNum3.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(reloadNum3, reloadBizo.PK, reloadBizo.TableName, "ID3", "G%", "N3", "S3");
			var reloadNum4 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "ID4", "ES", "G3");
			NUnit.Framework.Assert.That(reloadNum4.PK, Is.EqualTo(num4.PK));
			NUnit.Framework.Assert.That(reloadNum4.GetType(), Is.EqualTo(typeof(FakeCenForTest)));
			AssertCusEntryNumber(reloadNum4, reloadBizo.PK, reloadBizo.TableName, "ID4", "ES", "N4", "S4", "G3");
			NUnit.Framework.Assert.That(CusEntryNumber.Load<FakeCenForTest>(reloadBizo), Is.EquivalentTo(new[] { reloadNum1, reloadNum2, reloadNum3, reloadNum4 }));

			num1.CE_EntryStatus = "AS1";
			num2.CE_EntryStatus = "AS2";
			num3.CE_EntryStatus = "AS3";
			newFactory.Save();
			AssertCusEntryNumber(reloadNum1, bizo.PK, bizo.TableName, "KS1", "K$", "N1", "S1");
			reloadNums = CusEntryNumber.Load<FakeCenForTest>(newFactory2, "KS1", "N1", "K$");
			NUnit.Framework.Assert.That(reloadNums.Length, Is.EqualTo(1));
			reloadNum1 = reloadNums[0];
			AssertCusEntryNumber(reloadNum1, bizo.PK, bizo.TableName, "KS1", "K$", "N1", "S1");
			reloadNums = CusEntryNumber.Load<FakeCenForTest>(newFactory2, "KS1", "N1", "K$", true);
			NUnit.Framework.Assert.That(reloadNums.Length, Is.EqualTo(1));
			reloadNum1 = reloadNums[0];
			AssertCusEntryNumber(reloadNum1, bizo.PK, bizo.TableName, "KS1", "K$", "N1", "AS1");

			AssertCusEntryNumber(reloadNum2, reloadBizo.PK, reloadBizo.TableName, ZString.Empty, "!@", "N2", "S2");
			reloadNum2 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "!@");
			AssertCusEntryNumber(reloadNum2, reloadBizo.PK, reloadBizo.TableName, ZString.Empty, "!@", "N2", "S2");
			reloadNum2 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "!@", true);
			AssertCusEntryNumber(reloadNum2, reloadBizo.PK, reloadBizo.TableName, ZString.Empty, "!@", "N2", "AS2");

			AssertCusEntryNumber(reloadNum3, reloadBizo.PK, reloadBizo.TableName, "ID3", "G%", "N3", "S3");
			reloadNum3 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "ID3", "G%");
			AssertCusEntryNumber(reloadNum3, reloadBizo.PK, reloadBizo.TableName, "ID3", "G%", "N3", "S3");
			reloadNum3 = CusEntryNumber.LoadOrCreate<FakeCenForTest>(reloadBizo, "ID3", "G%", true);
			AssertCusEntryNumber(reloadNum3, reloadBizo.PK, reloadBizo.TableName, "ID3", "G%", "N3", "AS3");

			num1.CE_EntryStatus = "BS1";
			num2.CE_EntryStatus = "BS2";
			num3.CE_EntryStatus = "BS3";
			newFactory.Save();
			NUnit.Framework.Assert.That(CusEntryNumber.Load<FakeCenForTest>(reloadBizo, "K$"), Is.EqualTo(reloadNum1));
			AssertCusEntryNumber(reloadNum1, bizo.PK, bizo.TableName, "KS1", "K$", "N1", "AS1");
			NUnit.Framework.Assert.That(CusEntryNumber.Load<FakeCenForTest>(reloadBizo, "K$", true), Is.EqualTo(reloadNum1));
			AssertCusEntryNumber(reloadNum1, bizo.PK, bizo.TableName, "KS1", "K$", "N1", "BS1");

			NUnit.Framework.Assert.That(CusEntryNumber.Load<FakeCenForTest>(reloadBizo, "ID3", "G%"), Is.EqualTo(reloadNum3));
			AssertCusEntryNumber(reloadNum3, reloadBizo.PK, reloadBizo.TableName, "ID3", "G%", "N3", "AS3");
			NUnit.Framework.Assert.That(CusEntryNumber.Load<FakeCenForTest>(reloadBizo, "ID3", "G%", true), Is.EqualTo(reloadNum3));
			AssertCusEntryNumber(reloadNum3, reloadBizo.PK, reloadBizo.TableName, "ID3", "G%", "N3", "BS3");

			num1.CE_EntryNum = "TTT";
			num2.CE_EntryNum = "TTT";
			num3.CE_EntryNum = "TTT";
			num1.CE_RN_NKCountryCode = "G%";
			num2.CE_RN_NKCountryCode = "G%";
			num3.CE_RN_NKCountryCode = "G%";
			num1.CE_EntryType = "ID3";
			num2.CE_EntryType = "ID3";
			num3.CE_EntryType = "ID3";
			num1.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			num2.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			num3.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			newFactory.Save();
			var reloadNum = CusEntryNumber.LoadMostRecentByCreateTime<FakeCenForTest>(newFactory, "ID3", "TTT", "G%");
			NUnit.Framework.Assert.That(reloadNum.PK, Is.EqualTo(num2.PK));
		}

		[ExpectNoExceptions]
		public void TestSupportsClone_NotSystemGenerated()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var number = CusEntryNumber.New(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			number.CE_EntryIsSystemGenerated = false;

			NUnit.Framework.Assert.That(!number.CE_EntryIsSystemGenerated, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Precondition: The number is not system-generated.");

			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			NUnit.Framework.Assert.That(number.SupportsClone(), Is.True, "The entry supports clone when CE_EntryIsSystemGenerated = false and CE_Categoray = OTH.");

			number.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			NUnit.Framework.Assert.That(number.SupportsClone(), Is.True, "The entry supports clone when CE_EntryIsSystemGenerated = false and CE_Categoray = CUS.");

			number.CE_Category = CusEntryNumber.Categories.InspectionStatus;
			NUnit.Framework.Assert.That(number.SupportsClone(), Is.True, "The entry supports clone when CE_EntryIsSystemGenerated = false and CE_Categoray = INS.");

			number.CE_Category = CusEntryNumber.Categories.DocumentsRelated;
			NUnit.Framework.Assert.That(number.SupportsClone(), Is.True, "The entry supports clone when CE_EntryIsSystemGenerated = false and CE_Categoray = DRE.");
		}

		[ExpectNoExceptions]
		public void TestSupportsClone_SystemGenerated()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var number = CusEntryNumber.New(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			number.CE_EntryIsSystemGenerated = true;

			NUnit.Framework.Assert.That(number.CE_EntryIsSystemGenerated, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Precondition: The number is system-generated.");

			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			NUnit.Framework.Assert.That(number.SupportsClone(), Is.True, "The entry supports clone when CE_EntryIsSystemGenerated = true and CE_Categoray = OTH.");

			number.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			NUnit.Framework.Assert.That(!number.SupportsClone(), Is.True, "The entry does not support clone when CE_EntryIsSystemGenerated = true and CE_Categoray = CUS.");

			number.CE_Category = CusEntryNumber.Categories.InspectionStatus;
			NUnit.Framework.Assert.That(!number.SupportsClone(), Is.True, "The entry does not support clone when CE_EntryIsSystemGenerated = true and CE_Categoray = INS.");

			number.CE_Category = CusEntryNumber.Categories.DocumentsRelated;
			NUnit.Framework.Assert.That(!number.SupportsClone(), Is.True, "The entry does not support clone when CE_EntryIsSystemGenerated = true and CE_Categoray = DRE.");
		}

		public void TestBuildRowDeletedReport()
		{
			ErrorReporter.Clear();
			var newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.Delete();
			AssertNoExceptionThrown(() => { _ = newEntryNumber.CE_RN_NKCountryCode; });
			NUnit.Framework.Assert.That(ErrorReporter.TotalErrorCount, Is.EqualTo(1));

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, Does.Contain("Developer Error: Should not be accessing a property on a deleted business object"));
				NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, Does.Contain("Deletion StackTrace"));
				NUnit.Framework.Assert.That(ErrorReporter.LastMessageReported, Does.Contain("TestBuildRowDeletedReport"));
			});

			ErrorReporter.Clear();
		}

		#region Implementation

		[ExpectNoExceptions]
		void AssertCusEntryNumber(CusEntryNumber entryNumber, ZGuid parentPK, ZString parentTable, ZString entryType, ZString countryCode, ZString number, ZString? status = null, ZString? entryLineReference = null)
		{
			NUnit.Framework.Assert.That(entryNumber.CE_ParentID, Is.EqualTo(parentPK), "entryNumber.CE_ParentID");
			NUnit.Framework.Assert.That(entryNumber.CE_ParentTable, Is.EqualTo(parentTable), "entryNumber.CE_ParentTable");
			NUnit.Framework.Assert.That(entryNumber.CE_EntryType, Is.EqualTo(entryType), "entryNumber.CE_EntryType");
			NUnit.Framework.Assert.That(entryNumber.CE_RN_NKCountryCode, Is.EqualTo(countryCode), "entryNumber.CE_RN_NKCountryCode");
			NUnit.Framework.Assert.That(entryNumber.CE_EntryNum, Is.EqualTo(number), "entryNumber.CE_EntryNum");
			if (status.HasValue)
			{
				NUnit.Framework.Assert.That(entryNumber.CE_EntryStatus, Is.EqualTo(status.Value), "entryNumber.CE_EntryStatus");
			}

			if (entryLineReference.HasValue)
			{
				NUnit.Framework.Assert.That(entryNumber.CE_EntryLineReference, Is.EqualTo(entryLineReference.Value), "entryNumber.CE_EntryLineReference");
			}
		}

		#region Test helper classes

		class FakeCenForTest : CusEntryNumber
		{
			public FakeCenForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public CusEntryNumValidation GetNewValidationExposed()
			{
				return GetNewValidation();
			}
		}

		class ParentOfCenForTest : DummyBusinessObject, ICusEntryNumberValidationDeciderOfType
		{
			public ParentOfCenForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public int TestType { get; set; }

			public Type GetCusEntryNumberValidationType()
			{
				switch (TestType)
				{
					case 1:
						return typeof(FakeCenValidationType1);
					case 2:
						return typeof(FakeCenValidationType2);
					case 3:
						return typeof(FakeCenValidationType3);
					case 4:
						return typeof(FakeCenValidationType4);
					default:
						return typeof(CusEntryNumValidation);
				}
			}

			public class FakeCenValidationType1 : CusEntryNumValidation
			{
				public FakeCenValidationType1(AutoCusEntryNum parent) : base(parent)
				{ }
			}
			public class FakeCenValidationType2 : CusEntryNumValidation
			{
				public FakeCenValidationType2(AutoCusEntryNum parent) : base(parent)
				{ }
			}

			public abstract class FakeCenValidationType3 : CusEntryNumValidation
			{
				public FakeCenValidationType3(AutoCusEntryNum parent) : base(parent)
				{ }
			}

			public class FakeCenValidationType4 : ZValidation
			{
				public FakeCenValidationType4(AutoCusEntryNum parent) : base(parent)
				{ }

				public override Type AutoValidationType => throw new NotImplementedException();

				public override void ValidateAll()
				{
					throw new NotImplementedException();
				}
			}
		}

		#endregion

		#endregion
	}
}
