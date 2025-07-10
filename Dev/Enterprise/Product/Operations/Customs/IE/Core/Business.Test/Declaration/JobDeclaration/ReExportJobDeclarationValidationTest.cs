using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ReExportJobDeclarationValidation))]
	sealed class ReExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		public void TestCheckJE_ContainerMode()
		{
			var targetInfo = declaration.JE_ContainerModeInfo;
			declaration.JE_TransportMode = ZString.Empty;
			declaration.JE_ContainerMode = ZString.Empty;
			AssertHasMessageError(targetInfo, "Container Indicator field is mandatory");
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertNoMessageErrors(targetInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XXX", "CNT");
		}

		public void TestCheckJE_LocationOtherInformation()
		{
			var targetInfo = declaration.JE_LocationOtherInformationInfo;
			declaration.JE_LocationOtherInformation = ZString.Empty;
			AssertHasMessageErrorContaining("JE_LocationOtherInformation required", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationOtherInformation = "Something";
			AssertNoMessageErrorContaining("JE_LocationOtherInformation provided", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_LocationQualifier()
		{
			var targetInfo = declaration.JE_LocationQualifierInfo;
			declaration.JE_LocationQualifier = ZString.Empty;
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

		protected override string MessageType => IEJobMessageTypeList.Codes.ReExport;

		protected override JobDeclarationValidation GetValidation() => new ReExportJobDeclarationValidation(declaration);
	}
}
