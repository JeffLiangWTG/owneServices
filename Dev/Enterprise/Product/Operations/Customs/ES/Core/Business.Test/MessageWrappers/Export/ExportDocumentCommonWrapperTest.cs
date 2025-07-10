using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExportDocumentCommonWrapperTest : WrapperHelperTest<ExportDocumentCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new ExportDocumentCommonWrapper(null));
		}

		public void TestDateOfIssue()
		{
			ZDateTime.TryParseExact(SupportingDocumentData.DateOfIssue, out var issueDate, CustomsDateTimeExtension.DateFormat);
			document.CSI_DateOfIssue = issueDate;
			AssertEquals("Expected filled DateOfIssue", issueDate, wrapper.DateOfIssue);
		}

		public void TestDateOfExpiry()
		{
			ZDateTime.TryParseExact(SupportingDocumentData.DateOfExpiry, out var expiryDate, CustomsDateTimeExtension.DateFormat);
			document.CSI_DateOfExpiry = expiryDate;
			AssertEquals("Expected filled DateOfExpiry", expiryDate, wrapper.DateOfExpiry);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<SupportingDocument>();
			wrapper = new ExportDocumentCommonWrapper(document);
		}

		SupportingDocument document;
		ExportDocumentCommonWrapper wrapper;

		protected override ExportDocumentCommonWrapper GetProvider() => wrapper;
	}
}
