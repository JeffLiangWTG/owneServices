using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	public class GbCDSH7PreviousDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestWrapper()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_SubType = "Z";
			previousDocument.CSI_Code = "380";
			previousDocument.CSI_ReferenceNumber = "123456";
			previousDocument.CSI_LineNo = 123;
			previousDocument.CSI_SystemCreateTimeUtc = new ZDateTime(2022, 1, 1);

			var wrapper = new GbCDSH7PreviousDocumentWrapper(previousDocument);

			CombineAssertions(() =>
			{
				AssertEquals("CategoryCode", "Z", wrapper.CategoryCode);
				AssertEquals("TypeCode", "380", wrapper.TypeCode);
				AssertEquals("ID", "123456", wrapper.ID);
				AssertEquals("LineNo", 123, wrapper.LineNumeric);
				AssertEquals("SystemCreateTime", new ZDateTime(2022, 1, 1), wrapper.SystemCreateTime);
			});
		}
	}
}
