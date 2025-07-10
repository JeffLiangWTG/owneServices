using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DJPDocumentWrapperTest : WrapperHelperTest<DJPDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new DJPDocumentWrapper(null));
		}

		public void TestDate()
		{
			var issueDate = new ZDateTime(2020, 10, 20, 15, 50, 00);
			var expiryDate = new ZDateTime(2022, 10, 20, 15, 50, 00);

			CombineAssertions(() =>
			{
				document.CSI_DateOfIssue = ZDateTime.Empty;
				AssertEquals("Expected empty Date when both Issue and Expiry dates are empty", ZDateTime.Empty, wrapper.Date);

				document.CSI_DateOfIssue = issueDate;
				document.CSI_DateOfExpiry = ZDateTime.Empty;
				AssertEquals("Expected filled Date with Issue date when Expiry date is empty", issueDate, wrapper.Date);

				document.CSI_DateOfExpiry = expiryDate;
				AssertEquals("Expected filled Date with Expiry date when it's not empty", expiryDate, wrapper.Date);
			});
		}

		public void TestIndicator()
		{
			document.CSI_Procedure = "N";
			AssertEquals("Expected filled Indicator", "N", wrapper.Indicator);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<SupportingDocument>();
			wrapper = new DJPDocumentWrapper(document);
		}

		SupportingDocument document;
		DJPDocumentWrapper wrapper;

		protected override DJPDocumentWrapper GetProvider() => wrapper;
	}
}
