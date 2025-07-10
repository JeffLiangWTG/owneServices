using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ImportCommonC44CertificateDocumentWrapperTest : WrapperHelperTest<ImportCommonC44CertificateDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new ImportCommonC44CertificateDocumentWrapper(null));
		}

		public void TestCertQuantityUnit()
		{
			CombineAssertions(() =>
			{
				document.CSI_UnitOfQuantity = SupportingDocumentData.QtyUnitCW1;
				AssertEquals("Expected filled CertQuantityUnit with mapped value", SupportingDocumentData.QtyUnitCustoms, wrapper.CertQuantityUnit);

				document.CSI_UnitOfQuantity = SupportingDocumentData.QtyUnitNotMapped;
				AssertEquals("Expected filled CertQuantityUnit with original value because the value is not mapped", SupportingDocumentData.QtyUnitNotMapped, wrapper.CertQuantityUnit);
			});
		}

		public void TestCertQuantityAmount()
		{
			document.CSI_Quantity = SupportingDocumentData.Quantity;
			AssertEquals("Expected filled CertQuantityAmount", SupportingDocumentData.Quantity, wrapper.CertQuantityAmount);
		}

		public void TestCertDate()
		{
			CombineAssertions(() =>
			{
				ZDateTime.TryParseExact(SupportingDocumentData.DateOfIssue, out var issueDate, CustomsDateTimeExtension.DateFormat);
				document.CSI_DateOfIssue = issueDate;
				AssertEquals("Expected filled CertDate with DateOfIssue", issueDate, wrapper.CertDate);

				ZDateTime.TryParseExact(SupportingDocumentData.DateOfExpiry, out var expiryDate, CustomsDateTimeExtension.DateFormat);
				document.CSI_DateOfExpiry = expiryDate;
				AssertEquals("Expected filled CertDate with DateOfExpiry (even when dateOfIssue is declared)", expiryDate, wrapper.CertDate);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), CountryCodes.Spain);
			Factory.Save();

			document = Factory.New<SupportingDocument>();
			wrapper = new ImportCommonC44CertificateDocumentWrapper(document);
		}

		SupportingDocument document;
		ImportCommonC44CertificateDocumentWrapper wrapper;

		protected override ImportCommonC44CertificateDocumentWrapper GetProvider() => wrapper;
	}
}
