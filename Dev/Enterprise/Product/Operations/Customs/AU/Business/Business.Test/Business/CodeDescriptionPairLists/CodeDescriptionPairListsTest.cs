using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CodeDescriptionPairListsTest : TestCase
	{
		public void TestAUCusApplicationCodeListXML()
		{
			AssertEquals(AUCusApplicationCodeList.Codes.ForceLegacyMessages, Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages);
			AssertEquals(AUCusApplicationCodeList.Codes.ForceCMRMessages, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
		}

		public void TestAUCusContainerModeListXML()
		{
			AssertEquals(AUCusContainerModeList.Codes.FCL, Core.Constants.ContainerModes.FCL);
			AssertEquals(AUCusContainerModeList.Codes.LCL, Core.Constants.ContainerModes.LCL);
			AssertEquals(AUCusContainerModeList.Codes.FCLMixedShipper, Core.Constants.ContainerModes.FCLMixedShipper);
		}

		public void TestCMRCusEntryCPDecAnswerCodeListXML()
		{
			AssertEquals(CMRCusEntryCPDecAnswerCodeList.Codes.Yes, CMRCusEntryCPDec.Answers.YES);
			AssertEquals(CMRCusEntryCPDecAnswerCodeList.Codes.No, CMRCusEntryCPDec.Answers.NO);
		}

		public void TestCusSCAPivotWeightCVUQListXML()
		{
			AssertEquals(CusSCAPivotWeightCVUQList.Codes.Kilogram, Core.Constants.Weight.Kilograms);
			AssertEquals(CusSCAPivotWeightCVUQList.Codes.Kilotonnes, Core.Constants.Weight.Kilotonnes);
			AssertEquals(CusSCAPivotWeightCVUQList.Codes.ShortTons, Core.Constants.Weight.ShortTons);
		}

		public void TestDrawbackAmberReasonTypesListXML()
		{
			AssertEquals(DrawbackAmberReasonTypesList.Codes.Calculation, JobDeclaration.DrawbackAmberReasonTypes.Calculation);
			AssertEquals(DrawbackAmberReasonTypesList.Codes.Declaration, JobDeclaration.DrawbackAmberReasonTypes.Declaration);
			AssertEquals(DrawbackAmberReasonTypesList.Codes.LegacyMigration, JobDeclaration.DrawbackAmberReasonTypes.LegacyMigration);
			AssertEquals(DrawbackAmberReasonTypesList.Codes.Time, JobDeclaration.DrawbackAmberReasonTypes.Time);
		}

		public void TestDrawbackAssessmentMethodsListXML()
		{
			AssertEquals(DrawbackAssessmentMethodsList.Codes.ActualShipment, JobDeclaration.DrawbackAssessmentMethods.ActualShipment);
			AssertEquals(DrawbackAssessmentMethodsList.Codes.Imputation, JobDeclaration.DrawbackAssessmentMethods.Imputation);
			AssertEquals(DrawbackAssessmentMethodsList.Codes.OtherMethod, JobDeclaration.DrawbackAssessmentMethods.OtherMethod);
			AssertEquals(DrawbackAssessmentMethodsList.Codes.RepresentativeShipment, JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment);
		}
	}
}
