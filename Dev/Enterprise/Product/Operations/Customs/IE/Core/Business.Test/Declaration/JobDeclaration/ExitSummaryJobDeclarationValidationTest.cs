using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ExitSummaryJobDeclarationValidation))]
	sealed class ExitSummaryJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		public void TestCheckJE_LocationOtherInformation()
		{
			var targetInfo = declaration.JE_LocationOtherInformationInfo;
			declaration.JE_LocationOtherInformation = string.Empty;
			AssertHasMessageErrorContaining("JE_LocationOtherInformation required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationOtherInformation = "Something";
			AssertNoMessageErrorContaining("JE_LocationOtherInformation provided", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_LocationQualifier()
		{
			var targetInfo = declaration.JE_LocationQualifierInfo;
			declaration.JE_LocationQualifier = string.Empty;
			AssertHasMessageErrorContaining("JE_LocationQualifier required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationQualifier = "ABC";
			AssertNoMessageErrorContaining("JE_LocationQualifier provided", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_LocationOfGoods()
		{
			var targetInfo = declaration.JE_LocationOfGoodsInfo;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining("JE_LocationOfGoods required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationOfGoods = "Something";
			AssertNoMessageErrorContaining("JE_LocationOfGoods provided", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_OH_ShippingLine()
		{
			var targetInfo = declaration.JE_OH_ShippingLineInfo;
			declaration.Validation.ValidateJE_OH_ShippingLine();

			AssertHasMessageErrorContaining("JE_OH_ShippingLine required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.Validation.ValidateJE_OH_ShippingLine();
			AssertNoMessageErrorContaining("JE_OH_ShippingLine provided", targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("An EORI number is required", targetInfo, "An EORI number is required");

			declaration.ShippingLine.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.Validation.ValidateJE_OH_ShippingLine();
			AssertNoMessageErrorContaining("An EORI number existed", targetInfo, "An EORI number is required");
		}

		public void TestCheckJE_CustomsOffice()
		{
			var targetInfo = declaration.JE_CustomsOfficeInfo;

			declaration.JE_CustomsOffice = string.Empty;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageErrorContaining("JE_CustomsOffice required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = "value";
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageErrorContaining("JE_CustomsOffice provided", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_ContainerMode_Mandatory()
		{
			declaration.JE_ContainerMode = string.Empty;
			AssertHasMessageErrorContaining("JE_ContainerMode is mandatory", declaration.JE_ContainerModeInfo, MandatoryValidation.YouHaveNotEntered);
			const string message = "At least one Container must be entered.";
			declaration.JE_ContainerMode = "NCT";
			AssertNoMessageErrorContaining("JE_ContainerMode is populated - should not have this message", declaration.JE_ContainerModeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Should not have error", declaration.JE_ContainerModeInfo, message);
			declaration.JE_ContainerMode = "CNT";
			AssertHasMessageError("Should have error", declaration.JE_ContainerModeInfo, message);
			declaration.CusContainers.AddNew();
			declaration.Validation.ValidateJE_ContainerMode();
			AssertNoMessageErrorContaining("Should not have error", declaration.JE_ContainerModeInfo, message);
		}

		public void TestCheckJE_TransportMeans()
		{
			var targetInfo = declaration.JE_TransportMeansInfo;
			foreach (var transportModeInland in new TransportTypeList().GetAllCodes())
			{
				declaration.JE_TransportModeInland = transportModeInland;
				declaration.JE_TransportMeans = "AB";
				declaration.Validation.ValidateJE_TransportMeans();
				AssertHasMessageErrorContaining($"For {transportModeInland}, Inland Transport Mode is NOT required.", targetInfo, MandatoryValidation.DoNotEntered);
			}
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.ExitSummary;

		protected override JobDeclarationValidation GetValidation() => new ExitSummaryJobDeclarationValidation(declaration);
	}
}
