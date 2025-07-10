using Enterprise.Freight.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingSeaPackLineValidationTest : JXCValidationTestCase
	{
		public void TestValidateJL_Description()
		{
			PackLine.JL_FreightMode = FreightConstants.OuterPackType;
			AssertHasNoJXCWarnings("Pre-condition", PackLine.JL_DescriptionInfo);
			PackLine.JL_Description = "Desc";
			AssertHasNoJXCWarnings(PackLine.JL_DescriptionInfo);
			PackLine.JL_Description = "";
			AssertHasNotEnteredJXCWarning(PackLine.JL_DescriptionInfo);
		}

		public void TestShouldNotValidateJL_DescriptionIfDoesNotRequireValidation()
		{
			PackLine.JL_FreightMode = FreightConstants.InnerPackType;
			PackLine.JL_Description = "";
			PackLine.Validation.ValidateJL_Description();
			AssertHasNoJXCWarnings("Should not validate when Freight Mode is not outer pack type", PackLine.JL_DescriptionInfo);
		}

		public void TestValidateLinePriceCurrency()
		{
			PackLine.JL_FreightMode = FreightConstants.OuterPackType;
			AssertHasNoJXCWarnings("Pre-condition", PackLine.JL_CustomAttrib1Info);
			PackLine.LinePriceCurrency = "988";
			AssertHasInvalidCurrencyCodeJXCWarning(PackLine.JL_CustomAttrib1Info);
			PackLine.LinePriceCurrency = "";
			AssertHasInvalidCurrencyCodeJXCWarning(PackLine.JL_CustomAttrib1Info);
			PackLine.LinePriceCurrency = "SGD";
			AssertHasNoJXCWarnings(PackLine.JL_CustomAttrib1Info);
		}

		public void TestShouldNotValidateLinePriceCurrencyIfDoesNotRequireValidation()
		{
			PackLine.JL_FreightMode = FreightConstants.InnerPackType;
			PackLine.LinePriceCurrency = "098";
			PackLine.Validation.ValidateJL_CustomAttrib1();
			AssertHasNoJXCWarnings("Should not validate when Freight Mode is not outer pack type", PackLine.JL_CustomAttrib1Info);
		}

		#region Implementation
		JASForwardingPackLine PackLine
		{
			get
			{
				if (fPackLine == null)
				{
					fPackLine = Factory.New<JASForwardingPackLine>();
				}

				return fPackLine;
			}
		}

		JASForwardingPackLine fPackLine;
		protected override void SetUp()
		{
			base.SetUp();
			new JXCDomainValidationManager(Factory).ManageJXCValidations(JXCExportValidationType.Ocean);
		}
		#endregion
	}
}
