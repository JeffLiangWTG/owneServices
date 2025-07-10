using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class EdiFactV921ESMessagePrettyFormatterTest<TResponseMessageProvider> : EdiFactMessagePrettyFormatterTest
		where TResponseMessageProvider : class, ICUSRESV921ESMessageProvider
	{
		public abstract void TestRegisterData();

		public abstract void TestCircuitData();

		public abstract void TestClearanceData();

		protected virtual Mock<TResponseMessageProvider> SetResponseData(ZDateTime admissionDate, ZString registrationNumber, ZString messageFunction, ZString csvReleaseCode, ZDateTime csvReleaseCreationDate, ZString printActionRequired, ZString? customsClearanceStatus = null, ZDateTime? transitMaxDate = null)
		{
			var mockTestHelper = new Mock<TResponseMessageProvider> { CallBase = true };
			mockTestHelper.Setup(m => m.DocumentMessageName).Returns(UniversalReferenceConstants.DeclarationResponseCode.Accepted);
			mockTestHelper.Setup(m => m.AdmissionDate).Returns(admissionDate);
			mockTestHelper.Setup(m => m.RegistrationNumber).Returns(registrationNumber);
			mockTestHelper.Setup(m => m.MessageFunction).Returns(messageFunction);
			mockTestHelper.Setup(m => m.CSVReleaseCode).Returns(csvReleaseCode);
			mockTestHelper.Setup(m => m.CSVReleaseCreationDate).Returns(csvReleaseCreationDate);
			mockTestHelper.Setup(m => m.PrintActionRequired).Returns(printActionRequired);

			return mockTestHelper;
		}

		protected abstract ZString GetInterpretationText(Mock<TResponseMessageProvider> mockTestHelper);

		protected virtual Mock<IExportResponseMessageProvider> SetResponseData_IExportResponseMessageProvider(ZDateTime admissionDate, ZString registrationNumber, ZString messageFunction, ZString csvReleaseCode, ZDateTime csvReleaseCreationDate, ZString printActionRequired, ZString? customsClearanceStatus = null, ZDateTime? transitMaxDate = null)
		{
			var mockTestHelper = new Mock<IExportResponseMessageProvider>();
			mockTestHelper.Setup(m => m.DocumentMessageName).Returns(UniversalReferenceConstants.DeclarationResponseCode.Accepted);
			mockTestHelper.Setup(m => m.AdmissionDate).Returns(admissionDate);
			mockTestHelper.Setup(m => m.RegistrationNumber).Returns(registrationNumber);
			mockTestHelper.Setup(m => m.MessageFunction).Returns(messageFunction);
			mockTestHelper.Setup(m => m.CSVReleaseCode).Returns(csvReleaseCode);
			mockTestHelper.Setup(m => m.CSVReleaseCreationDate).Returns(csvReleaseCreationDate);
			mockTestHelper.Setup(m => m.PrintActionRequired).Returns(printActionRequired);

			return mockTestHelper;
		}

		protected abstract ZString GetInterpretationText_IExportResponseMessageProvider(Mock<IExportResponseMessageProvider> mockTestHelper);
	}
}
