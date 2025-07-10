using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth.Testing
{
	class InvoiceeIDProviderTest : TestCaseWithFactory
	{
		public void TestGetInvoiceeIDForTransactionInfo()
		{
			var transactionInfo = new TransactionInfo();
			var mock = new Mock<Func<OrganizationAddress, ZString, ZString, string>>();

			AssertEquals(new ValueTuple<ZString, ZString>("", ""), InvoiceeIDProvider.GetInvoiceeID(transactionInfo, mock.Object));
			mock.Verify(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Exactly(3));

			mock.Setup(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner)).Returns("Code1");
			AssertEquals(new ValueTuple<ZString, ZString>("03", "9999999999999"), InvoiceeIDProvider.GetInvoiceeID(transactionInfo, mock.Object));
			mock.Verify(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Exactly(3 + 3));

			mock.Setup(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident)).Returns("Code2");
			AssertEquals(new ValueTuple<ZString, ZString>("01", "Code2"), InvoiceeIDProvider.GetInvoiceeID(transactionInfo, mock.Object));
			mock.Verify(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Exactly(3 + 3 + 2));

			mock.Setup(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), OrgCusCode.CodeTypes.VATCode)).Returns("Code3");
			AssertEquals(new ValueTuple<ZString, ZString>("VAT","Code3"), InvoiceeIDProvider.GetInvoiceeID(transactionInfo, mock.Object));
			mock.Verify(x => x.Invoke(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Exactly(3 + 3 + 2 + 1));
		}

		public void TestGetInvoiceeIDForOrgHeader()
		{
			var orgHeader = TestObjectCreator.ABIGAS;

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForForeigner, "Code1", CountryCodes.KoreaSouth);
			AssertEquals("Code1", InvoiceeIDProvider.GetInvoiceeID(orgHeader));

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident, "Code2", CountryCodes.KoreaSouth);
			AssertEquals("Code2", InvoiceeIDProvider.GetInvoiceeID(orgHeader));

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "Code3", CountryCodes.KoreaSouth);
			AssertEquals("Code3", InvoiceeIDProvider.GetInvoiceeID(orgHeader));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
