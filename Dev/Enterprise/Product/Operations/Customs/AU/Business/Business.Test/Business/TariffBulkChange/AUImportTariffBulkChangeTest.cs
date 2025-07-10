using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.TariffBulkChange;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUImportTariffBulkChange))]
	sealed class AUImportTariffBulkChangeTest : Customs.Business.Testing.BaseTariffBulkChangeTest
	{
		public void TestAULoadConcordanceWithNoAuto()
		{
			zeroZero = "";
			CreateSomePartsAndClassifications(Common.ClassificationType.IMP, Common.ClassificationType.EXP);

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
	GetTestResourcePath("AUTestImportTariffConcordance.csv"));
			topLevelObject.LoadConcordance(str, false);
			AssertEquals(6, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2OneOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			var tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2OneLookupCode));
			AssertEquals(true, tempstring.Contains(One2OneFreeStandingLookupCode));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2OnePartNum));
			AssertEquals(true, tempstring.Contains(One2OneWithTwoClassesPartNum));
			AssertEquals(true, tempstring.Contains(AnotherOne2OneWithTwoClassesPartNum));

			AssertEquals(One2OneOldTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots.Count);
			AssertEquals(One2OneLookupCodeWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications[0].CC_LookupCode);
			AssertEquals(One2OnePartNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications.Count);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));
			AssertEquals(true, tempstring.Contains(One2ManyWithTwoClassesPartNum));
			AssertEquals(true, tempstring.Contains(AnotherOne2ManyWithTwoClassesPartNum));

			AssertEquals(One2ManyOldValidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[3].OriginalClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2ManyOldValidTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2ManyOldValidNew3TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[4].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[4].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[4].TariffItemPivots.Count);
			AssertEquals(One2ManyOldValidLookupCodeWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyOldValidPartNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2MannyNoDotsOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2MannyNoDotsNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew3TariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[5].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[5].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[5].TariffItemPivots.Count);
			AssertEquals(One2ManynoDotsLookupCode, topLevelObject.TariffBulkChangeOldTariffs[5].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyNoDotsPartNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffItemPivots[0].Part.OP_PartNum);
		}

		public void TestAULoadConcordanceWithAutoOne2One()
		{
			zeroZero = "";
			CreateSomePartsAndClassifications(Common.ClassificationType.IMP, Common.ClassificationType.EXP);

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestImportTariffConcordance.csv"));
			topLevelObject.LoadConcordance(str, true);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			var tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot1 in topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots)
			{
				tempstring += pivot1.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));
			AssertEquals(true, tempstring.Contains(One2ManyWithTwoClassesPartNum));
			AssertEquals(true, tempstring.Contains(AnotherOne2ManyWithTwoClassesPartNum));

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

			AssertEquals(One2ManyOldValidTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2ManyOldValidNew3TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots.Count);
			AssertEquals(One2ManyOldValidLookupCodeWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyOldValidPartNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2MannyNoDotsOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2MannyNoDotsNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew3TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[3].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots.Count);
			AssertEquals(One2ManynoDotsLookupCode, topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyNoDotsPartNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots[0].Part.OP_PartNum);

			Factory.Save();

			var part1 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, part1);
			AssertEquals(One2OneNewTariffNum, part1.PivotsForBinding[0].NewTariffNum);

			var part2 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OneWithTwoClassesPartNum));
			AssertNotEquals(null, part2);
			AssertEquals(One2OneNewTariffNum, part2.PivotsForBinding[0].NewTariffNum);

			var part3 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, AnotherOne2OneWithTwoClassesPartNum));
			AssertNotEquals(null, part3);
			var pivot = part3.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault(x => x.Classification.CC_ClassificationType == "IMP");
			AssertEquals(One2OneNewTariffNum, pivot.NewTariffNum);

			var part4 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNumWithDifferentSuffix));
			AssertNotEquals(null, part4);
			AssertEquals(One2OneNewTariffNumWithDifferentSuffix, part4.PivotsForBinding[0].NewTariffNum);
		}

		public void TestAULoadConcordanceWithNoAutoAnd4DigitStats()
		{
			zeroZero = "00";
			CreateSomePartsAndClassifications(Common.ClassificationType.IMP, Common.ClassificationType.EXP);

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestImportTariffConcordance.csv"));
			topLevelObject.LoadConcordance(str, false);
			AssertEquals(6, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2OneOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			var tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2OneLookupCode));
			AssertEquals(true, tempstring.Contains(One2OneFreeStandingLookupCode));
			tempstring = "";
			foreach (CusClassPartPivot pivot1 in topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots)
			{
				tempstring += pivot1.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2OnePartNum));
			AssertEquals(true, tempstring.Contains(One2OneWithTwoClassesPartNum));
			AssertEquals(true, tempstring.Contains(AnotherOne2OneWithTwoClassesPartNum));

			AssertEquals(One2OneOldTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].OldTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2OneNewTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[1].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots.Count);
			AssertEquals(One2OneLookupCodeWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].OriginalClassifications[0].CC_LookupCode);
			AssertEquals(One2OnePartNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[1].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications.Count);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidFreeStandingLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidLookupCode2));
			tempstring = "";
			foreach (CusClassPartPivot pivot in topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots)
			{
				tempstring += pivot.Part.OP_PartNum + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum));
			AssertEquals(true, tempstring.Contains(One2ManyOldInvalidPartNum2));
			AssertEquals(true, tempstring.Contains(One2ManyWithTwoClassesPartNum));
			AssertEquals(true, tempstring.Contains(AnotherOne2ManyWithTwoClassesPartNum));

			AssertEquals(One2ManyOldValidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[3].OriginalClassifications.Count);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots.Count);
			tempstring = "";
			foreach (BaseCusClassification classification in topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications)
			{
				tempstring += classification.CC_LookupCode + "*";
			}
			AssertEquals(true, tempstring.Contains(One2ManyOldValidLookupCode));
			AssertEquals(true, tempstring.Contains(One2ManyOldValidFreeStandingLookupCode));
			AssertEquals(One2ManyOldValidPartNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2ManyOldValidTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2ManyOldValidNew3TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[4].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[4].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[4].TariffItemPivots.Count);
			AssertEquals(One2ManyOldValidLookupCodeWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyOldValidPartNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[4].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2MannyNoDotsOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2MannyNoDotsNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew3TariffNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[5].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[5].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[5].TariffItemPivots.Count);
			AssertEquals(One2ManynoDotsLookupCode, topLevelObject.TariffBulkChangeOldTariffs[5].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyNoDotsPartNum, topLevelObject.TariffBulkChangeOldTariffs[5].TariffItemPivots[0].Part.OP_PartNum);

#pragma warning disable IDE0001 // suppress simplification to base type higher in inheritance hierachy.
			var selectedOldTariff = topLevelObject.TariffBulkChangeOldTariffs[2];
			var lookupCode1TariffNum = selectedOldTariff.OriginalClassifications[0].CC_TariffNum;
			var selectedNewTariff = selectedOldTariff.TariffBulkChangeNewTariffs[1];
#pragma warning restore IDE0001 // suppress simplification to base type higher in inheritance hierachy.
			var newTariffNum = selectedNewTariff.NewTariffNum;
			TBCClassification[] selectedOriginalClassifications;
			selectedOriginalClassifications = new TBCClassification[2];
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

			TariffBulkChangeNewTariff[] selectedNewTariffs;
			selectedNewTariffs = new TariffBulkChangeNewTariff[2];
			selectedNewTariffs[0] = selectedOldTariff.TariffBulkChangeNewTariffs[0];
			selectedNewTariffs[1] = selectedOldTariff.TariffBulkChangeNewTariffs[1];
			topLevelObject.MakeNewClassifications(selectedOldTariff, selectedNewTariffs);
			AssertEquals("Now another 2 New Classifications should exist", 4, selectedOldTariff.NewClassifications.Count);
			tempstring = "";
			ZString uniqueCode = "A";
			foreach (var classification in selectedOldTariff.NewClassifications)
			{
				if (classification.CC_LookupCode.IsEmpty)
				{
					classification.CC_LookupCode = "NEW" + uniqueCode;
					uniqueCode = "B";
					tempstring += classification.CC_TariffNum + "*";
					AssertEquals(Common.ClassificationType.IMP, classification.CC_ClassificationType);
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
			selectedParts = new CusClassPartPivot[1];
			selectedParts[0] = savePivot2;
			topLevelObject.UpdateProducts(selectedOldTariff, selectedNewClassification, null, selectedParts);
			AssertEquals("This part should not change", NewLookupCode, savePivot1.NewLookUpCode);
			AssertEquals("This part should have another new code", AnotherNewLookupCode, savePivot2.NewLookUpCode);

			Factory.Save();

			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass1.CC_TariffChangePending);
			var fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Orig Traiff Num should not have changed yet", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should have pending changes", saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			AssertNotEquals("Pre-condition: Parts lookup code should not have changed yet", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should have pending changes", savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should exist", fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0);
			topLevelObject.ApplyPendingTariffChanges();
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass1.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass1.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass1.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Orig Traiff Num should now have changed", newTariffNum, saveClass2.CC_TariffNum);
			Assert("Classification should not have pending changes", !saveClass2.CC_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(saveClass2.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", NewLookupCode, savePivot1.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot1.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot1.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
			AssertEquals("Parts lookup code should now have changed", AnotherNewLookupCode, savePivot2.Classification.CC_LookupCode);
			Assert("Part should not have pending changes", !savePivot2.CI_TariffChangePending);
			fTBCChangeDataLogs = new LogsForNominatedEvent(savePivot2.Logs, BaseTariffBulkChange.ChangeDataEvent);
			Assert("Active event should not exist", fTBCChangeDataLogs == null || fTBCChangeDataLogs.Count == 0);
		}

		public void TestAULoadConcordanceWithAutoOne2OneAnd4DigitStats()
		{
			zeroZero = "00";
			CreateSomePartsAndClassifications(Common.ClassificationType.IMP, Common.ClassificationType.EXP);

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestImportTariffConcordance.csv"));
			topLevelObject.LoadConcordance(str, true);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs.Count);

			AssertEquals(One2ManyOldInvalidTariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].OldTariffNum);
			AssertEquals(2, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldInvalidNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldInvalidNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[0].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[0].OriginalClassifications.Count);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[0].NewClassifications.Count);
			AssertEquals(4, topLevelObject.TariffBulkChangeOldTariffs[0].TariffItemPivots.Count);
			var tempstring = "";
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
			AssertEquals(true, tempstring.Contains(One2ManyWithTwoClassesPartNum));
			AssertEquals(true, tempstring.Contains(AnotherOne2ManyWithTwoClassesPartNum));

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

			AssertEquals(One2ManyOldValidTariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2ManyOldValidNew1TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2ManyOldValidNew2TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2ManyOldValidNew3TariffNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[2].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots.Count);
			AssertEquals(One2ManyOldValidLookupCodeWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyOldValidPartNumWithDifferentSuffix, topLevelObject.TariffBulkChangeOldTariffs[2].TariffItemPivots[0].Part.OP_PartNum);

			AssertEquals(One2MannyNoDotsOldTariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].OldTariffNum);
			AssertEquals(3, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs.Count);
			AssertEquals(One2MannyNoDotsNew1TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[0].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew2TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[1].NewTariffNum);
			AssertEquals(One2MannyNoDotsNew3TariffNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffBulkChangeNewTariffs[2].NewTariffNum);
			AssertEquals(0, topLevelObject.TariffBulkChangeOldTariffs[3].OriginalClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications.Count);
			AssertEquals(1, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots.Count);
			AssertEquals(One2ManynoDotsLookupCode, topLevelObject.TariffBulkChangeOldTariffs[3].NewClassifications[0].CC_LookupCode);
			AssertEquals(One2ManyNoDotsPartNum, topLevelObject.TariffBulkChangeOldTariffs[3].TariffItemPivots[0].Part.OP_PartNum);

			Factory.Save();

			var part1 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, part1);
			AssertEquals(One2OneNewTariffNum, part1.PivotsForBinding[0].NewTariffNum);

			var part2 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OneWithTwoClassesPartNum));
			AssertNotEquals(null, part2);
			AssertEquals(One2OneNewTariffNum, part2.PivotsForBinding[0].NewTariffNum);

			var part3 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, AnotherOne2OneWithTwoClassesPartNum));
			AssertNotEquals(null, part3);
			var part3pivot = part3.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault(x => x.Classification.CC_ClassificationType == "IMP");
			AssertEquals(One2OneNewTariffNum, part3pivot.NewTariffNum);

			var part4 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNumWithDifferentSuffix));
			AssertNotEquals(null, part4);
			AssertEquals(One2OneNewTariffNumWithDifferentSuffix, part4.PivotsForBinding[0].NewTariffNum);
		}

		public void TestChangeTCO()
		{
			var class1 = createClass("C1", "0104.00.00 99", "TC1", "1111111");
			var class2 = createClass("C2", "0104.00.00 99", "TC2", "1111111");
			var class3 = createClass("C3", "0104.00.00 99", "TC1", "2222222");
			var class4 = createClass("C4", "0104.00.00 99", "TC1", "3333333");
			var class5 = createClass("C5", "0104.00.00 99", "TC1", "4444444");
			var class6 = createClass("C6", "0104.00.01 99", "TC1", "1111111");
			var class7 = createClass("C7", "0104.00.00 99", "BL", "1111111");

			var part1 = createPart("P1", "0104.00.00 99", "TC1", "1111111");
			var part2 = createPart("P2", "0104.00.00 99", "TC2", "1111111");
			var part3 = createPart("P3", "0104.00.00 99", "TC1", "2222222");
			var part4 = createPart("P4", "0104.00.00 99", "TC1", "3333333");
			var part5 = createPart("P5", "0104.00.00 99", "TC1", "4444444");
			var part6 = createPart("P6", "0104.00.01 99", "TC1", "1111111");
			var part7 = createPart("P7", "0104.00.00 99", "BL", "1111111");

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestTCOChange.csv"));
			topLevelObject.ChangeTCO(str);

			AssertEquals("Class 1", "1111119", class1.InstrumentCode);
			AssertEquals("Class 2", "1111119", class2.InstrumentCode);
			AssertEquals("Class 3", "2222222", class3.InstrumentCode);
			AssertEquals("Class 4", "12345678", class4.InstrumentCode);
			AssertEquals("Class 5", "", class5.InstrumentCode);
			AssertEquals("Class 6", "1111111", class6.InstrumentCode);
			AssertEquals("Class 7", "1111111", class7.InstrumentCode);

			AssertEquals("Part 1", "1111119", part1.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Part 2", "1111119", part2.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Part 3", "2222222", part3.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Part 4", "12345678", part4.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Part 5", "", part5.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Part 6", "1111111", part6.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
			AssertEquals("Part 7", "1111111", part7.PivotsForBinding[0].AddInfo.ZA_InstrumentCode_Hidden);
		}

		public void TestTCOIsSaveAllowed()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestTCOChange.csv"));
			topLevelObject.ChangeTCO(stream, isSaveAllowed: false);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Save not allowed", ContinueWithSave.No, topLevelObject.TCOAdditionalContinueWithSave());
			AssertEquals("Was correct message", topLevelObject.SaveNotAllowedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestTCOChange.csv"));
			topLevelObject.ChangeTCO(stream, isSaveAllowed: true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			AssertEquals("Save is allowed", ContinueWithSave.Yes, topLevelObject.ApplyAdditionalContinueWithSave());
		}

		public void TestCountrySpecificOverrides()
		{
			var topLevelObject = new AUImportTariffBulkChange(Factory);
			AssertEquals("ReferenceKey", "HS2022 TARIFF IMP", topLevelObject.ReferenceKey);
			AssertEquals("CountryPK", Core.Constants.CountryGuids.Australia, topLevelObject.CountryPK);
			AssertEquals("CountryCode", Core.Constants.CountryCodes.Australia, topLevelObject.CountryCode);
		}

		public void TestExistingPendingChangesRemoved()
		{
			CreateSomePartsAndClassifications(Common.ClassificationType.IMP, Common.ClassificationType.EXP);

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestImportTariffConcordance.csv"));
			topLevelObject.LoadConcordance(stream, true);

			var part1 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, part1);
			var importClass1 = Factory.Load<TBCClassification>(part1.PivotsForBinding[0].CI_CC);
			AssertNotEquals(null, importClass1);

			var part2 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2ManyWithTwoClassesPartNum));
			AssertNotEquals(null, part2);
			var importClass2 = Factory.Load<TBCClassification>(part2.PivotsForBinding[0].CI_CC);
			AssertNotEquals(null, importClass2);
			importClass2.NewLookupCode = "XYZ";
			importClass2.NewTariffNum = One2OneClass2NewTariffNum;
			part2.PivotsForBinding[0].NewLookUpPK = importClass1.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("A-1", One2OneNewTariffNum, part1.PivotsForBinding[0].NewTariffNum);
				Assert("A-2", !part1.PivotsForBinding[0].CI_TariffChangePending);
				Assert("A-3", importClass1.CC_TariffChangePending);
				Assert("A-4", importClass1.NewLookupCode.IsEmpty);
				AssertEquals("A-5", One2OneNewTariffNum, importClass1.NewTariffNum);
				Assert("A-6", part2.PivotsForBinding[0].CI_TariffChangePending);
				Assert("A-7", !part2.PivotsForBinding[0].NewLookUpPK.IsEmpty);
				Assert("A-8", !part2.PivotsForBinding[0].NewLookUpCode.IsEmpty);
				Assert("A-9", importClass2.CC_TariffChangePending);
				Assert("A-10", !importClass2.NewLookupCode.IsEmpty);
				Assert("A-11", !importClass2.NewTariffNum.IsEmpty);

				topLevelObject = new AUImportTariffBulkChange(Factory);
				foreach (BaseCusClassPartPivot item in part1.PivotsForBinding)
				{
					item.ResetTBCCacheForTesting();
				}
				foreach (BaseCusClassPartPivot item in part2.PivotsForBinding)
				{
					item.ResetTBCCacheForTesting();
				}
				topLevelObject.CheckAndRemoveAnyExistingPendingChanges();
				Assert("B-1", !part1.PivotsForBinding[0].CI_TariffChangePending);
				Assert("B-2", !importClass1.CC_TariffChangePending);
				Assert("B-3", importClass1.NewLookupCode.IsEmpty);
				Assert("B-4", importClass1.NewTariffNum.IsEmpty);
				Assert("B-5", !part2.PivotsForBinding[0].CI_TariffChangePending);
				Assert("B-6", part2.PivotsForBinding[0].NewLookUpPK.IsEmpty);
				Assert("B-7", part2.PivotsForBinding[0].NewLookUpCode.IsEmpty);
				Assert("B-8", !importClass2.CC_TariffChangePending);
				Assert("B-9", importClass2.NewLookupCode.IsEmpty);
				Assert("B-10", importClass2.NewTariffNum.IsEmpty);
			});
		}

		public void TestExistingPendingChangesNotRemovedAfterHS2012DataEntryStarted()
		{
			CreateSomePartsAndClassifications(Common.ClassificationType.IMP, Common.ClassificationType.EXP);

			var topLevelObject = new AUImportTariffBulkChange(Factory);
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
				GetTestResourcePath("AUTestImportTariffConcordance.csv"));
			topLevelObject.LoadConcordance(stream, true, true, true);

			var part1 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2OnePartNum));
			AssertNotEquals(null, part1);
			var importClass1 = Factory.Load<TBCClassification>(part1.PivotsForBinding[0].CI_CC);
			AssertNotEquals(null, importClass1);

			var part2 = Factory.LoadTop1<AUOrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, One2ManyWithTwoClassesPartNum));
			AssertNotEquals(null, part2);
			var importClass2 = Factory.Load<TBCClassification>(part2.PivotsForBinding[0].CI_CC);
			AssertNotEquals(null, importClass2);
			importClass2.NewLookupCode = "XYZ";
			importClass2.NewTariffNum = One2OneClass2NewTariffNum;
			part2.PivotsForBinding[0].NewLookUpPK = importClass1.PK;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			AssertEquals(ContinueWithSave.Yes, topLevelObject.ApplyAdditionalContinueWithSave());
			Factory.Save();

			AssertEquals(One2OneNewTariffNum, part1.PivotsForBinding[0].NewTariffNum);
			Assert(!part1.PivotsForBinding[0].CI_TariffChangePending);
			Assert(importClass1.CC_TariffChangePending);
			Assert(importClass1.NewLookupCode.IsEmpty);
			AssertEquals(One2OneNewTariffNum, importClass1.NewTariffNum);
			Assert(part2.PivotsForBinding[0].CI_TariffChangePending);
			Assert(!part2.PivotsForBinding[0].NewLookUpPK.IsEmpty);
			Assert(!part2.PivotsForBinding[0].NewLookUpCode.IsEmpty);
			Assert(importClass2.CC_TariffChangePending);
			Assert(!importClass2.NewLookupCode.IsEmpty);
			Assert(!importClass2.NewTariffNum.IsEmpty);

			topLevelObject = new AUImportTariffBulkChange(Factory);
			topLevelObject.CheckAndRemoveAnyExistingPendingChanges();

			AssertEquals(One2OneNewTariffNum, part1.PivotsForBinding[0].NewTariffNum);
			Assert(!part1.PivotsForBinding[0].CI_TariffChangePending);
			Assert(importClass1.CC_TariffChangePending);
			Assert(importClass1.NewLookupCode.IsEmpty);
			AssertEquals(One2OneNewTariffNum, importClass1.NewTariffNum);
			Assert(part2.PivotsForBinding[0].CI_TariffChangePending);
			Assert(!part2.PivotsForBinding[0].NewLookUpPK.IsEmpty);
			Assert(!part2.PivotsForBinding[0].NewLookUpCode.IsEmpty);
			Assert(importClass2.CC_TariffChangePending);
			Assert(!importClass2.NewLookupCode.IsEmpty);
			Assert(!importClass2.NewTariffNum.IsEmpty);
		}

		protected override BusinessObject GetNewBusinessObject() => new AUImportTariffBulkChange(Factory);

		protected override string NoChangeTariffNum => "0104.00.00 99";

		protected override string NoChangeTariffNumForDB => NoChangeTariffNum;

		protected override string One2OneOldTariffNum => "0105.92.00 01";

		protected override string One2OneOldTariffNumForDB => "0105.92.00 " + zeroZero + "01";

		protected override string One2OneNewTariffNum => "0105.94.00 02";

		protected override string One2OneOldTariffNumWithDifferentSuffix => "0105.92.00 31";

		protected override string One2OneOldTariffNumForDBWithDifferentSuffix => "0105.92.00 " + zeroZero + "31";

		protected override string One2OneNewTariffNumWithDifferentSuffix => "0105.94.00 32";

		protected override string One2ManyOldInvalidTariffNum => "2005.90.00 03";

		protected override string One2ManyOldInvalidTariffNumForDB => "2005.90.00 " + zeroZero + "03";

		protected override string One2ManyOldInvalidNew1TariffNum => "2005.91.00 04";

		protected override string One2ManyOldInvalidNew2TariffNum => "2005.99.00 05";

		protected override string One2ManyOldValidTariffNum => "2827.39.00 10";

		protected override string One2ManyOldValidTariffNumForDB => "2827.39.00 " + zeroZero + "10";

		protected override string One2ManyOldValidNew1TariffNum => "2827.39.00 10";

		protected override string One2ManyOldValidNew2TariffNum => "2852.00.90 12";

		protected override string One2ManyOldValidTariffNumWithDifferentSuffix => "2827.39.00 21";

		protected override string One2ManyOldValidTariffNumForDBWithDifferentSuffix => "2827.39.00 " + zeroZero + "21";

		protected override string One2ManyOldValidNew1TariffNumWithDifferentSuffix => "2827.39.00 21";

		protected override string One2ManyOldValidNew2TariffNumWithDifferentSuffix => "2852.00.90 22";

		protected override string One2ManyOldValidNew3TariffNumWithDifferentSuffix => "2852.00.90 23";

		protected override string One2MannyNoDotsOldTariffNum => "9017.20.90 13";

		protected override string One2MannyNoDotsOldTariffNumForDB => "9017.20.90 " + zeroZero + "13";

		protected override string One2MannyNoDotsNew1TariffNum => "8486.40.10 14";

		protected override string One2MannyNoDotsNew2TariffNum => "9017.20.90 13";

		protected override string One2MannyNoDotsNew3TariffNum => "9017.20.91 16";

		string zeroZero = "";

		Classification createClass(ZString lookupCode, ZString tariffNum, ZString instrumentType, ZString instrumentCode)
		{
			var result = Factory.New<Classification>();
			result.CC_TariffNum = tariffNum;
			result.CC_ClassificationType = Common.ClassificationType.IMP;
			result.CC_LookupCode = lookupCode;
			result.InstrumentType = instrumentType;
			result.InstrumentCode = instrumentCode;
			return result;
		}

		AUOrgSupplierPart createPart(ZString partCode, ZString tariffNum, ZString instrumentType, ZString instrumentCode)
		{
			var result = Factory.New<AUOrgSupplierPart>();
			var classx = result.ClassificationsForBinding.AddNew();
			classx.CC_TariffNum = tariffNum;
			classx.CC_ClassificationType = Common.ClassificationType.IMP;
			classx.CC_LookupCode = "C" + partCode;
			var pivot = result.PivotsForBinding[0];
			pivot.AddInfo.ZA_InstrumentType_Hidden = instrumentType;
			pivot.AddInfo.ZA_InstrumentCode_Hidden = instrumentCode;
			return result;
		}

		string GetTestResourcePath(string filename) => "Enterprise.Customs.AU.Declaration.Business.Testing.Business.TariffBulkChange.TestFiles." + filename;

#pragma warning disable IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		[TestedType(typeof(AUImportTariffBulkChange.TariffBulkChangeOldTariff))]
#pragma warning restore IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		sealed class TariffBulkChangeOldTariffTest : NonPersistentBusinessObjectTestCase
		{
		}

#pragma warning disable IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		[TestedType(typeof(AUImportTariffBulkChange.TariffBulkChangeNewTariff))]
#pragma warning restore IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		sealed class TariffBulkChangeNewTariffTest : NonPersistentBusinessObjectTestCase
		{
		}

#pragma warning disable IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		[TestedType(typeof(TariffBulkChangeOldTariffCollection))]
		sealed class TariffBulkChangeOldTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AUImportTariffBulkChange.TariffBulkChangeOldTariffCollection>
		{
			protected override AUImportTariffBulkChange.TariffBulkChangeOldTariffCollection GetCollectionToTest() => new AUImportTariffBulkChange.TariffBulkChangeOldTariffCollection(TBC);

			protected override BusinessObject GetNewElementToAddToTheCollection() => new AUImportTariffBulkChange.TariffBulkChangeOldTariff(TBC);

			AUImportTariffBulkChange TBC => tbc ?? (tbc = new AUImportTariffBulkChange(Factory));
			AUImportTariffBulkChange tbc;
		}
#pragma warning restore IDE0001 // suppress simplification to base type higher in inheritance hierachy.

#pragma warning disable IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		[TestedType(typeof(TariffBulkChangeNewTariffCollection))]
		sealed class TariffBulkChangeNewTariffCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AUImportTariffBulkChange.TariffBulkChangeNewTariffCollection>
		{
			protected override AUImportTariffBulkChange.TariffBulkChangeNewTariffCollection GetCollectionToTest()
				=> new AUImportTariffBulkChange.TariffBulkChangeNewTariffCollection(TBC, OldTariffNum);

			protected override BusinessObject GetNewElementToAddToTheCollection() => new AUImportTariffBulkChange.TariffBulkChangeNewTariff(TBC, OldTariffNum);

			AUImportTariffBulkChange TBC => tbc ?? (tbc = new AUImportTariffBulkChange(Factory));
			AUImportTariffBulkChange tbc;

			AUImportTariffBulkChange.TariffBulkChangeOldTariff OldTariffNum => oldtariffnum ?? (oldtariffnum = new AUImportTariffBulkChange.TariffBulkChangeOldTariff());
			AUImportTariffBulkChange.TariffBulkChangeOldTariff oldtariffnum;
		}
#pragma warning restore IDE0001 // suppress simplification to base type higher in inheritance hierachy.

#pragma warning disable IDE0001 // suppress simplification to base type higher in inheritance hierachy.
		[TestedType(typeof(AUImportTariffBulkChange.TBCClassificationCollection<TBCClassification>))]
		sealed class TBCClassificationCollectionTest : BusinessObjectCollectionTestCase
		{
			protected override BusinessObjectCollection GetCollectionToTest() => new AUImportTariffBulkChange.TBCClassificationCollection<TBCClassification>(Factory);
		}
#pragma warning restore IDE0001 // suppress simplification to base type higher in inheritance hierachy.

		[TestedType(typeof(TBCClassification))]
		sealed class TBCClassificationBusinessObjectTest : EnterpriseBusinessObjectTestCase
		{
			public void TestTBCClassificationNewProperties()
			{
				var classification = Factory.New<TBCClassification>();
				classification.NewLookupCode = "ABC";
				AssertEquals("ABC", classification.NewLookupCode);
				classification.NewTariffNum = "1234567890";
				AssertEquals("1234.56.78 90", classification.NewTariffNum);
			}
		}
	}
}
