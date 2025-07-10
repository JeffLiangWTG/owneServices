using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideGovernmentAllocatedIDValidationTest : TestCaseWithFactory
	{
		public void TestCheckAH_GovernmentAllocatedID_RunsValidation()
		{
			var apInvoice = Factory.New<APInvoice>();
			apInvoice.SetContext(BusinessContext.OverrideGovernmentAllocatedID);

			var mockIGovernmentAllocatedIDValidationProvider = new Mock<IGovernmentAllocatedIDValidationProvider>();
			mockIGovernmentAllocatedIDValidationProvider
				.Setup(x => x.ValidateGovernmentAllocatedID(It.IsAny<GovernmentAllocatedIDValidationData>()))
				.Returns("An error message");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory
				.As<IInstanceProvider<IGovernmentAllocatedIDValidationProvider>>()
				.Setup(x => x.Get())
				.Returns(mockIGovernmentAllocatedIDValidationProvider.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory
				.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()))
				.Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: true))
			{
				apInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
				apInvoice.AH_GovernmentAllocatedID = "12345678";

				mockIGovernmentAllocatedIDValidationProvider
					.Verify(x => x.ValidateGovernmentAllocatedID(It.Is<IGovernmentAllocatedIDValidationData>(
						x => x.EReportingStatus == ""
						&& x.GovernmentAllocatedID == "12345678"
						&& x.Ledger == apInvoice.AH_Ledger
						&& x.OrgCountryCode == "AU")), Times.Once);

				AssertHasError(apInvoice.AH_GovernmentAllocatedIDInfo, "An error message");
			}

			mockIGovernmentAllocatedIDValidationProvider.Invocations.Clear();
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				apInvoice.AH_GovernmentAllocatedID = "1234567890";
				mockIGovernmentAllocatedIDValidationProvider
					.Verify(x => x.ValidateGovernmentAllocatedID(It.IsAny<IGovernmentAllocatedIDValidationData>()), Times.Never);

				AssertNoErrors(apInvoice.AH_GovernmentAllocatedIDInfo);
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
