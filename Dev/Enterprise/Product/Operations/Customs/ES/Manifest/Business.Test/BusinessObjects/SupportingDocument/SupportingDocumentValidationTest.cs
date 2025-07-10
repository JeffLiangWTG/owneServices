using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var supportingDocumentType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;
			helper.CreateNewOrGetExistingCusCodeType(supportingDocumentType, "Supporting Documents Transit NCTS (BOX44)");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "DUA", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "DUA";
			AssertNoMessageErrors("No message errors when code is in list", supportingDocument.CSI_CodeInfo);
			supportingDocument.CSI_Code = "AAA";
			AssertHasMessageErrorContaining(supportingDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestCSI_ReferenceNumberMaxLength()
		{
			try
			{
				var supportingDocument = Factory.New<SupportingDocument>();
				supportingDocument.CSI_ReferenceNumber = "ABCDEF012346578901234567890123456789";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}
	}
}
