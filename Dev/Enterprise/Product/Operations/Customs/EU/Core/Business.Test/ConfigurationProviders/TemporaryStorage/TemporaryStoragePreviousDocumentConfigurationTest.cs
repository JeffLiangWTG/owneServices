using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(TemporaryStoragePreviousDocumentConfiguration))]
	public abstract class TemporaryStoragePreviousDocumentConfigurationAbstractTest<T> : TestCaseWithFactory where T : TemporaryStoragePreviousDocumentConfiguration, new()
	{
		[ExpectNoExceptions]
		public virtual void TestGetCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				RefCusCodeList codeLists1, codeLists2, codeLists3, codeLists4;
				GenerateCodeList(out codeLists1, out codeLists2, out codeLists3, out codeLists4);

				var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				var previousDocument = storageHeader.PreviousDocuments.AddNew();
				var codeList1 = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
				var completeFilter1 = codeList1.CompleteFilter;
				AssertCodeListForPresentationNotification(codeLists1, codeLists2, codeLists3, codeLists4, previousDocument, codeList1, completeFilter1);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				var codeList2 = previousDocument.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
				var completeFilter2 = codeList2.CompleteFilter;
				AssertCodeListForPreLodgedTempStorage(codeLists1, codeLists2, codeLists3, codeLists4, previousDocument, codeList2, completeFilter2);
			}

			void GenerateCodeList(out RefCusCodeList codeLists1, out RefCusCodeList codeLists2, out RefCusCodeList codeLists3, out RefCusCodeList codeLists4)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
					"Previous Documents Of PNTS");
				helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment,
					"Transport Charges Method Of Payment");
				helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Purpose", "Purpose", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, Core.Constants.CountryCodes.France);

				var yesterday = ZDateTime.Today.AddDays(-1);
				var tomorrow = ZDateTime.Today.AddDays(1);
				codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "A", "Test 1",
					yesterday, tomorrow);
				codeLists1.Attributes.AddNew("Purpose", "Declaration");
				codeLists1.Attributes.AddNew("Purpose", "Presentation");

				codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "B", "Test 1",
					yesterday, tomorrow);
				codeLists2.Attributes.AddNew("Purpose", "Declaration");

				codeLists3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "C", "Test 1",
					yesterday, tomorrow);
				codeLists3.Attributes.AddNew("Purpose", "Presentation");

				codeLists4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "D", "Test 1",
					yesterday, tomorrow);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "B",
					"Test 2", yesterday, tomorrow);
				Factory.Save();
			}

			void AssertCodeListForPresentationNotification(RefCusCodeList codeLists1, RefCusCodeList codeLists2, RefCusCodeList codeLists3, RefCusCodeList codeLists4, TemporaryStoragePreviousDocument previousDocument, ZZRefCusCodeListCombinedCollection codeList1, ZQuery completeFilter1)
			{
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists1.PK).MatchesFilter(completeFilter1), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Presentation");
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists2.PK).MatchesFilter(completeFilter1), NUnit.Framework.Is.EqualTo(false), "Unmatched Purpose: Presentation");
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists3.PK).MatchesFilter(completeFilter1), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Presentation");
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists4.PK).MatchesFilter(completeFilter1), NUnit.Framework.Is.EqualTo(false), "Unmatched Purpose: Presentation");
				NUnit.Framework.Assert.That((ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList, NUnit.Framework.Is.SameAs(codeList1), "Cached");
			}

			void AssertCodeListForPreLodgedTempStorage(RefCusCodeList codeLists1, RefCusCodeList codeLists2, RefCusCodeList codeLists3, RefCusCodeList codeLists4, TemporaryStoragePreviousDocument previousDocument, ZZRefCusCodeListCombinedCollection codeList2, ZQuery completeFilter2)
			{
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists1.PK).MatchesFilter(completeFilter2), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Declaration");
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists2.PK).MatchesFilter(completeFilter2), NUnit.Framework.Is.EqualTo(true), "Matched Purpose: Declaration");
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists3.PK).MatchesFilter(completeFilter2), NUnit.Framework.Is.EqualTo(false), "Unmatched Purpose: Declaration");
				NUnit.Framework.Assert.That(Factory.Load<ZZRefCusCodeListCombined>(codeLists4.PK).MatchesFilter(completeFilter2), NUnit.Framework.Is.EqualTo(false), "Unmatched Purpose: Declaration");
				NUnit.Framework.Assert.That((ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList, NUnit.Framework.Is.SameAs(codeList2), "Cached");
			}
		}

		#region CollectionMaxCount

		[ExpectNoExceptions]
		public void TestIsCollectionMaxCountValidationEnabled()
		{
			NUnit.Framework.Assert.That(configuration.IsCollectionMaxCountValidationEnabled(), NUnit.Framework.Is.EqualTo(ExpectedIsCollectionMaxCountValidationEnabled), "IsCollectionMaxCountValidationEnabled");
		}

		[ExpectNoExceptions]
		public void TestCollectionMaxCount()
		{
			if (ExpectedIsCollectionMaxCountValidationEnabled)
			{
				NUnit.Framework.Assert.That(configuration.CollectionMaxCount, NUnit.Framework.Is.EqualTo(ExpectedCollectionMaxCount), "CollectionMaxCount");
				NUnit.Framework.Assert.That(configuration.CollectionMaxCount, NUnit.Framework.Is.GreaterThanOrEqualTo(0), "CollectionMaxCount should be greater than or equal to 0");
			}
			else
			{
				NUnit.Framework.Assert.That(configuration.CollectionMaxCount, NUnit.Framework.Is.EqualTo(-1), "CollectionMaxCount should be -1");
			}
		}

		protected virtual bool ExpectedIsCollectionMaxCountValidationEnabled => true;

		protected virtual int ExpectedCollectionMaxCount => 1;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new T();
		}
		protected T configuration;
	}

	[TestedType(typeof(TemporaryStoragePreviousDocumentConfiguration))]
	sealed class TemporaryStoragePreviousDocumentConfigurationTest : TemporaryStoragePreviousDocumentConfigurationAbstractTest<TemporaryStoragePreviousDocumentConfiguration>
	{
	}
}
