using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCExportAWBRateLineValidationTest : JXCValidationTestCase
	{
		public void TestDomainValidationShouldSubclassFromAutoValidationType()
		{
			AssertEquals(typeof(Freight.Forwarding.AWB.Business.AutoExportAWBRateLineValidation), typeof(JXCExportAWBRateLineValidation).BaseType);
		}

		public void TestValidateER_NoOfPiecesOrRCP()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.FBDNFieldBoundaries.NoOfPiecesMaxLength, ExportAWBRateLineSchema.ER_NoOfPiecesOrRCP);
			AWBRateLine.ER_NoOfPiecesOrRCP = "1";
			AssertHasNoJXCWarnings(AWBRateLine.ER_NoOfPiecesOrRCPInfo);
			AWBRateLine.ER_GrossWeight = 1;
			AWBRateLine.ER_WeightInLBsOrKGs = "K";
			AWBRateLine.ER_NoOfPiecesOrRCP = "";
			AssertHasNotEnteredJXCWarning(AWBRateLine.ER_NoOfPiecesOrRCPInfo);
		}

		public void TestValidateER_CommodityItemNumber()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.FBDNFieldBoundaries.CommodityItemNumberMaxLength, ExportAWBRateLineSchema.ER_CommodityItemNumber);
		}

		public void TestValidateER_RateClass()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid " + AWBRateLine.ER_RateClassInfo.HumanReadableName;
			AWBRateLine.ER_RateClass = "_";
			AssertHasJXCWarning(AWBRateLine.ER_RateClassInfo, expectedWarningMessage);
			AWBRateLine.ER_RateClass = "Q";
			AssertHasNoJXCWarnings(AWBRateLine.ER_RateClassInfo);
		}

		public void TestValidateER_WeightInLbsOrKgs()
		{
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Invalid Weight Unit";
			AWBRateLine.ER_GrossWeight = 1;
			AWBRateLine.ER_WeightInLBsOrKGs = "(";
			AssertHasJXCWarning(AWBRateLine.ER_WeightInLBsOrKGsInfo, expectedWarningMessage);
			AWBRateLine.ER_WeightInLBsOrKGs = "K";
			AssertHasNoJXCWarnings(AWBRateLine.ER_WeightInLBsOrKGsInfo);
		}

		public void TestShouldNotValidateIfRateLineIsEmpty()
		{
			Assert("Sanity check", AWBRateLine.IsEmpty);
			AWBRateLine.Validation.ValidateAll();
			Assert("Should not be calling the validate methods if RateLine is empty", !ValidationHelper.HasJXCWarnings(AWBRateLine));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Validation.MainGroup.RegisterValidationType<ExportAWBRateLine, JXCExportAWBRateLineValidation>();
		}

		ExportAWBRateLine AWBRateLine
		{
			get
			{
				if (fAWBRateLine == null)
				{
					fAWBRateLine = Factory.New<ExportAWBRateLine>();
				}

				return fAWBRateLine;
			}
		}

		ExportAWBRateLine fAWBRateLine;
	}
}
