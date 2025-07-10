using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class B3AndCADDocumentHelperTest : TestCaseWithFactory
	{
		public void TestConvertDutyAndTaxType()
		{
			CreateCusCodeListWithAttribute("Alcohol", CasualImportConstants.CasualImpCommodityType.Alcohol);
			CreateCusCodeListWithAttribute("Cannabis", CasualImportConstants.CasualImpCommodityType.Tobacco);
			CreateCusCodeListWithAttribute("Tobacco", CasualImportConstants.CasualImpCommodityType.Tobacco);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.ADD, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.ADD, invoiceLine));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.CVD, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CVD, invoiceLine));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.GST, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.GST, invoiceLine));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.SUR, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SUR, invoiceLine));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.FET, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.ExciseTax, invoiceLine));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.AAI, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CPT, invoiceLine));
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.CUD, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CustomsDuty, invoiceLine));
			AssertEquals(DutyAndTaxTypes.Codes.SIMADuty, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SIMADuty, invoiceLine));

			invoiceLine.CA_CasualImportCommodity = "Alcohol";
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.TAC, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CTA, invoiceLine));

			invoiceLine.CA_CasualImportCommodity = "Cannabis";
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.PAT, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CTA, invoiceLine));

			invoiceLine.CA_CasualImportCommodity = "Tobacco";
			AssertEquals(CADDutyTaxFeeTypeCodes.Codes.AAD, Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CTA, invoiceLine));
		}

		public void TestGetCusAgentNameAndPhone()
		{
			var brokerBranch = Factory.New<GlbBranch>();
			brokerBranch.GB_Phone = "789";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "654";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			using (CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, Guid.Empty))
			{
				AssertEquals("Broker, 654", Helper.GetCusAgentNameAndPhone(declaration));
				broker.GS_PublishWorkPhone = false;
				AssertEquals("Broker, 789", Helper.GetCusAgentNameAndPhone(declaration));
				broker.GS_GB_HomeBranch = ZGuid.Empty;
				AssertEquals("Broker", Helper.GetCusAgentNameAndPhone(declaration));
			}

			declaration.JE_GS_NKCusAgent = ZString.Empty;
			broker.GS_Code = "BRO";
			broker.GS_FullName = "Broker1";
			broker.GS_WorkPhone = "6543";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;
			using (CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, broker.PK.ToGuid()))
			{
				AssertEquals("Broker1, 6543", Helper.GetCusAgentNameAndPhone(declaration));
				broker.GS_PublishWorkPhone = false;
				AssertEquals("Broker1, 789", Helper.GetCusAgentNameAndPhone(declaration));
				broker.GS_GB_HomeBranch = ZGuid.Empty;
				AssertEquals("Broker1", Helper.GetCusAgentNameAndPhone(declaration));
			}
		}

		public void TestGetBroker()
		{
			var broker1 = Factory.NewWithValidTestData<GlbStaff>();
			broker1.GS_Code = "BR1";
			var broker2 = Factory.NewWithValidTestData<GlbStaff>();
			broker2.GS_Code = "BR2";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker1.GS_Code;

			using (CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, Guid.Empty))
			{
				AssertEquals(broker1.PK, Helper.GetBroker(declaration).PK);
			}
			using (CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB3.SetTemporaryValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, broker2.PK.ToGuid()))
			{
				AssertEquals(broker2.PK, Helper.GetBroker(declaration).PK);
			}
		}

		public void TestGetCurrentUserNameAndPhone()
		{
			var currentUser = GlbStaff.CurrentUser;
			var originalProvider = Env.GetCurrentProvider();
			var originalUserFullName = currentUser.GS_FullName;
			var originalUserPublishPhoneNumber = currentUser.GS_PublishWorkPhone;
			var originalUserPhoneNumber = currentUser.GS_WorkPhone;
			var originalBranchPhoneNumber = GlbBranch.CurrentBranch.GB_Phone;
			var originalCompanyPhoneNumber = GlbCompany.CurrentCompany.GC_Phone;

			try
			{
				currentUser.GS_FullName = "Test User";
				currentUser.GS_WorkPhone = "123";
				currentUser.GS_PublishWorkPhone = true;
				GlbBranch.CurrentBranch.GB_Phone = "741";
				GlbCompany.CurrentCompany.GC_Phone = "258";
				AssertEquals("Test User, 123", Helper.GetCurrentUserNameAndPhone());
				currentUser.GS_WorkPhone = ZString.Empty;
				AssertEquals("Test User, 741", Helper.GetCurrentUserNameAndPhone());
				GlbBranch.CurrentBranch.GB_Phone = ZString.Empty;
				AssertEquals("Test User, 258", Helper.GetCurrentUserNameAndPhone());
				GlbCompany.CurrentCompany.GC_Phone = ZString.Empty;
				AssertEquals("Test User", Helper.GetCurrentUserNameAndPhone());
				using (var provider = new NullEnvProvider())
				{
					provider.Enable();
					AssertEquals(Core.Constants.ProductName, Helper.GetCurrentUserNameAndPhone());
				}
			}
			finally
			{
				originalProvider.Enable();
				currentUser.GS_FullName = originalUserFullName;
				currentUser.GS_PublishWorkPhone = originalUserPublishPhoneNumber;
				currentUser.GS_WorkPhone = originalUserPhoneNumber;
				GlbBranch.CurrentBranch.GB_Phone = originalBranchPhoneNumber;
				GlbCompany.CurrentCompany.GC_Phone = originalCompanyPhoneNumber;
			}
		}

		public void TestGetPostalAddressAsASingleLine()
		{
			var vendor = Factory.NewWithValidTestData<OrgHeader>();
			var vendorAddress = vendor.MainAddress;
			vendorAddress.CompanyName = "VEN COM1";
			vendorAddress.Address1 = "VEN ADDRESS1";
			vendorAddress.City = "VEN CITY";
			vendorAddress.StateCode = "13";
			vendorAddress.Postcode = "239001";
			var vendorDocAddress = Factory.New<JobDocAddress>();
			vendorDocAddress.E2_OA_Address = vendorAddress.PK;
			var vendorDetails = "VEN COM1 VEN ADDRESS1 VEN CITY 13 239001";
			AssertEquals(vendorDetails, Helper.GetPostalAddressAsASingleLine(Factory, vendorAddress));
			AssertEquals(vendorDetails, Helper.GetPostalAddressAsASingleLine(Factory, vendorDocAddress));
			AssertEquals(vendorDetails, Helper.GetPostalAddressAsASingleLine(Factory, vendorAddress.CompanyName, vendorAddress.Address1, vendorAddress.City, vendorAddress.StateCode, vendorAddress.Postcode, ""));
		}

		public void TestGetDescriptionFromIClassificationLine1()
		{
			var classificationLine = new Mock<IClassificationLine1>();
			classificationLine.Setup(m => m.PartNumberDescriptions).Returns(new ZString[] { "I", "LOVE", "YOU" });
			AssertEquals("I;LOVE;YOU", Helper.GetDescriptionFromIClassificationLine1(classificationLine.Object));
			classificationLine.Setup(m => m.TRSNumber).Returns("1234");
			AssertEquals("TRS #:1234;I;LOVE;YOU", Helper.GetDescriptionFromIClassificationLine1(classificationLine.Object));
			classificationLine.Setup(m => m.PartNumberDescriptions).Returns(new ZString[] { "I", "LOVE", "YOU", "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" });
			AssertEquals("TRS #:1234;I;LOVE;YOU;1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678", Helper.GetDescriptionFromIClassificationLine1(classificationLine.Object));
		}

		void CreateCusCodeListWithAttribute(ZString code, ZString attribute)
		{
			new UniversalReferenceTestDataHelper(Factory).CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				code, code,ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, attribute);
		}

		B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
		B3AndCADDocumentHelper helper;
	}
}
