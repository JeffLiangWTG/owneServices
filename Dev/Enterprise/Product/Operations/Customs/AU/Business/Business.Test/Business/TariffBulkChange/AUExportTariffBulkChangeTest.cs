using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TBCClassification = Enterprise.Customs.Business.TariffBulkChange.TBCClassification;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUExportTariffBulkChange))]
	sealed class AUExportTariffBulkChangeTest : Customs.Business.Testing.BaseTariffBulkChangeTest
	{
		public void TestAUExportLoadConcordanceWithNoAuto()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.EXP, ZString.Empty);

			var topLevelObject = new AUExportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.AU.Declaration.Business.Testing.Business.TariffBulkChange.TestFiles.AUTestExportTariffConcordance.csv");
			topLevelObject.LoadConcordance(str, false);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2OneOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			string tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2OneLookupCode));
			AssertEquals(true, tempstring.Contains(One2OneFreeStandingLookupCode));
			AssertEquals(One2OnePartNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));

			AssertEquals(One2ManyOldValidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots[0].Part.OP_PartNum);

			TariffBulkChange.TariffBulkChangeOldTariff selectedOldTariff = topLevelObject.TariffBulkChangeOldTariffs[1];
			var lookupCode1TariffNum = selectedOldTariff.OriginalClassifications[0].CC_TariffNum;
			TariffBulkChange.TariffBulkChangeNewTariff selectedNewTariff = selectedOldTariff.TariffBulkChangeNewTariffs[1];
			var newTariffNum = selectedNewTariff.NewTariffNum;
			var selectedOriginalClassifications = new TBCClassification[2];
			selectedOriginalClassifications[0] = selectedOldTariff.OriginalClassifications[1];
			selectedOriginalClassifications[1] = selectedOldTariff.OriginalClassifications[2];
			topLevelObject.UpdateOriginalLookups(selectedOldTariff, selectedNewTariff, selectedOriginalClassifications);
			AssertEquals("Class 1 does not change", lookupCode1TariffNum, selectedOldTariff.OriginalClassifications[0].CC_TariffNum);
			AssertEquals("Class 1 newtariff is empy", "", selectedOldTariff.OriginalClassifications[0].NewTariffNum);
			AssertEquals("Now only 1 original class left", 1, selectedOldTariff.OriginalClassifications.Count);
			AssertEquals("Now 2 New Classifications", 2, selectedOldTariff.NewClassifications.Count);
			AssertEquals("Existing Tariff Num does not change", lookupCode1TariffNum, selectedOldTariff.NewClassifications[0].CC_TariffNum);
			AssertEquals("Existing Tariff Num does not change", lookupCode1TariffNum, selectedOldTariff.NewClassifications[1].CC_TariffNum);
			AssertEquals("New Classes now have new code", newTariffNum, selectedOldTariff.NewClassifications[0].NewTariffNum);
			AssertEquals("New Classes now have new code", newTariffNum, selectedOldTariff.NewClassifications[1].NewTariffNum);

			var saveClass1 = selectedOldTariff.NewClassifications[0];
			var saveClass2 = selectedOldTariff.NewClassifications[1];

			TariffBulkChange.TariffBulkChangeNewTariff[] selectedNewTariffs;
			selectedNewTariffs = new TariffBulkChange.TariffBulkChangeNewTariff[2];
			selectedNewTariffs[0] = selectedOldTariff.TariffBulkChangeNewTariffs[0];
			selectedNewTariffs[1] = selectedOldTariff.TariffBulkChangeNewTariffs[1];
			topLevelObject.MakeNewClassifications(selectedOldTariff, selectedNewTariffs);
			AssertEquals("Now another 2 New Classifications should exist", 4, selectedOldTariff.NewClassifications.Count);
			tempstring = "";
			ZString uniqueCode = "A";
			foreach (BaseCusClassification classification in selectedOldTariff.NewClassifications)
			{
				if (classification.CC_LookupCode.IsEmpty)
				{
					classification.CC_LookupCode = "NEW" + uniqueCode;
					uniqueCode = "B";
					tempstring += classification.CC_TariffNum + "*";
					AssertEquals(BaseCusClassification.ClassificationType.EXP, classification.CC_ClassificationType);
					AssertEquals(classification.CC_Description, classification.CC_TariffNum);
				}
			}
			AssertEquals(true, tempstring.Contains(selectedNewTariffs[0].NewTariffNum));
			AssertEquals(true, tempstring.Contains(selectedNewTariffs[1].NewTariffNum));

			var savePivot1 = selectedOldTariff.TariffItemPivots[0];
			var savePivot2 = selectedOldTariff.TariffItemPivots[1];

			var selectedNewClassification = selectedOldTariff.NewClassifications[2];
			selectedNewClassification.NewLookupCode = NewLookupCode;
			var selectedParts = new BaseCusClassPartPivot[2];
			selectedParts[0] = savePivot2;
			selectedParts[1] = savePivot1;
			topLevelObject.UpdateProducts(selectedOldTariff, selectedNewClassification, null, selectedParts);
			AssertEquals("Parts now have new lookup code", NewLookupCode, savePivot1.NewLookUpCode);
			AssertEquals("Parts now have new lookup code", NewLookupCode, savePivot2.NewLookUpCode);

			selectedNewClassification = selectedOldTariff.NewClassifications[0];
			selectedNewClassification.NewLookupCode = AnotherNewLookupCode;
			selectedParts = new BaseCusClassPartPivot[1];
			selectedParts[0] = savePivot2;
			topLevelObject.UpdateProducts(selectedOldTariff, selectedNewClassification, null, selectedParts);
			AssertEquals("This part should not change", NewLookupCode, savePivot1.NewLookUpCode);
			AssertEquals("This part should have another new code", AnotherNewLookupCode, savePivot2.NewLookUpCode);

			Factory.Save();

			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass1.CC_TariffChangePending);
			var fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass1.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass2.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, AUExportTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
		}

		public void TestAUExportLoadConcordanceWithAutoOne2One()
		{
			base.CreateSomePartsAndClassifications(BaseCusClassification.ClassificationType.EXP, ZString.Empty);

			var topLevelObject = new AUExportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	"Enterprise.Customs.AU.Declaration.Business.Testing.Business.TariffBulkChange.TestFiles.AUTestExportTariffConcordance.csv");
			topLevelObject.LoadConcordance(str, true);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			string tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));

			AssertEquals(One2ManyOldValidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.OP_PartNum);

			Factory.Save();

			var one2OnePart = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, one2OnePart);
			AssertEquals(One2OneNewTariffNum, one2OnePart.PivotsForBinding[0].NewTariffNum);
		}

		public void TestCountrySpecificOverrides()
		{
			var topLevelObject = new AUExportTariffBulkChange(Factory);
			AssertEquals("ReferenceKey", "HS2022 AHECC EXP", topLevelObject.ReferenceKey);
			AssertEquals("CountryPK", Core.Constants.CountryGuids.Australia, topLevelObject.CountryPK);
			AssertEquals("CountryCode", Enterprise.Core.Constants.CountryCodes.Australia, topLevelObject.CountryCode);
		}

		protected override BusinessObject GetNewBusinessObject() => new AUExportTariffBulkChange(Factory);

		[TestedType(typeof(TariffBulkChange.TariffBulkChangeOldTariff))]
		sealed class TariffBulkChangeOldTariffTest : NonPersistentBusinessObjectTestCase
		{
		}

		[TestedType(typeof(TariffBulkChange.TariffBulkChangeNewTariff))]
		sealed class TariffBulkChangeNewTariffTest : NonPersistentBusinessObjectTestCase
		{
		}

		[TestedType(typeof(TariffBulkChange.TariffBulkChangeOldTariffCollection))]
		sealed class TariffBulkChangeOldTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffBulkChange.TariffBulkChangeOldTariffCollection>
		{
			protected override TariffBulkChange.TariffBulkChangeOldTariffCollection GetCollectionToTest() => new TariffBulkChange.TariffBulkChangeOldTariffCollection(TBC);

			protected override BusinessObject GetNewElementToAddToTheCollection() => new TariffBulkChange.TariffBulkChangeOldTariff(TBC);

			AUExportTariffBulkChange TBC => tbc ?? (tbc = new AUExportTariffBulkChange(Factory));
			AUExportTariffBulkChange tbc;
		}

		[TestedType(typeof(TariffBulkChange.TariffBulkChangeNewTariffCollection))]
		sealed class TariffBulkChangeNewTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffBulkChange.TariffBulkChangeNewTariffCollection>
		{
			protected override TariffBulkChange.TariffBulkChangeNewTariffCollection GetCollectionToTest() => new TariffBulkChange.TariffBulkChangeNewTariffCollection(TBC, OldTariffNum);

			protected override BusinessObject GetNewElementToAddToTheCollection() => new TariffBulkChange.TariffBulkChangeNewTariff(TBC, OldTariffNum);

			AUExportTariffBulkChange TBC => tbc ?? (tbc = new AUExportTariffBulkChange(Factory));
			AUExportTariffBulkChange tbc;

			TariffBulkChange.TariffBulkChangeOldTariff OldTariffNum => oldtariffnum ?? (oldtariffnum = new TariffBulkChange.TariffBulkChangeOldTariff());
			TariffBulkChange.TariffBulkChangeOldTariff oldtariffnum;
		}

		[TestedType(typeof(TariffBulkChange.TBCClassificationCollection<TBCClassification>))]
		sealed class TBCClassificationCollectionTest : BusinessObjectCollectionTestCase
		{
			protected override BusinessObjectCollection GetCollectionToTest() => new TariffBulkChange.TBCClassificationCollection<TBCClassification>(Factory);
		}

		[TestedType(typeof(TBCClassification))]
		sealed class TBCClassificationBusinessObjectTest : EnterpriseBusinessObjectTestCase
		{
			public void TestTBCClassificationNewProperties()
			{
				var classification = Factory.New<TBCClassification>();
				classification.NewLookupCode = "ABC";
				AssertEquals("ABC", classification.NewLookupCode);
				classification.NewTariffNum = "12345678";
				AssertEquals("1234.56.78", classification.NewTariffNum);
			}
		}
	}
}
