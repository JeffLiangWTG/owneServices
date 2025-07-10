using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Moq.Protected;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class ApplicationExtenderTest : TestCaseWithFactory
	{
		public void TestNoVirtualOrAbstractPublicMethodsAndProperties()
		{
			var excludingMethodNames = new[] { "Equals", "GetHashCode", "ToString" };
			var virtualOrAbstractPublicMethods = typeof(ApplicationExtender)
				.GetMethods()
				.Where(x => x.IsPublic && (x.IsVirtual || x.IsAbstract) && !x.Name.StartsWith("get_") && !excludingMethodNames.Contains(x.Name))
				.Select(x => x.Name)
				.ToArray();
			var virtualOrAbstractPublicProperties = typeof(ApplicationExtender)
				.GetProperties()
				.Where(x =>
				{
					var getMethod = x.GetGetMethod();
					return getMethod.IsVirtual || getMethod.IsAbstract;
				})
				.Select(x => x.Name)
				.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Recommend public methods not be virtual or abstract", string.Empty, string.Join(",", virtualOrAbstractPublicMethods));
				AssertEquals("Recommend public properties not be virtual or abstract", string.Empty, string.Join(",", virtualOrAbstractPublicProperties));
			});
		}

		public void TestGetBondedWarehouseMessageProcessor()
		{
			var chiefDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			chiefDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var cdsDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			cdsDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var mockMessage = Factory.NewMoq<CreateMessageForTestEDIMessageDummy>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("TEST");
			Factory.Save();

			AssertNull(chiefDeclaration.ApplicationExtender.GetBondedWarehouseMessageProcessor(mockMessage.Object.PK, null, (ZArchitecture.Environment.EmailDef emailDef, EDIMessage message) => { }));
			Assert(cdsDeclaration.ApplicationExtender.GetBondedWarehouseMessageProcessor(mockMessage.Object.PK, null, (ZArchitecture.Environment.EmailDef emailDef, EDIMessage message) => { }).GetType().Name == "CDSBondedWarehouseMessageProcessor");
		}

		public void TestChargeAmountNoRounder()
		{
			var chiefDeclaration = Factory.New<JobDeclaration>();
			chiefDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			chiefDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertType<FeeNoRounder>(chiefDeclaration.ApplicationExtender.ChargeAmountNoRounder);

			var cdsDeclaration = Factory.New<JobDeclaration>();
			cdsDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertType<CDS.Declaration.CDSChargeAmountNoRounder>(cdsDeclaration.ApplicationExtender.ChargeAmountNoRounder);
		}

		public void TestGetOldEntryStatus()
		{
			var chiefDeclaration = Factory.New<JobDeclaration>();
			chiefDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var chiefEntry = (CusEntryHeader)chiefDeclaration.ActiveEntryHeaders.AddNew();
			chiefEntry.CH_ImportClearanceStatusICS = "01";
			Factory.Save();
			chiefEntry.CH_ImportClearanceStatusICS = "02";
			AssertEquals("Old Entry Status should be '01'", "01", chiefDeclaration.ApplicationExtender.GetOldEntryStatus(chiefEntry));

			var cdsDeclaration = Factory.New<JobDeclaration>();
			cdsDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cdsEntry = (CusEntryHeader)cdsDeclaration.ActiveEntryHeaders.AddNew();
			cdsEntry.CH_EntryStatus = CDS.Constants.ThreeCharFunctionCodes.MessageRegistered;
			Factory.Save();
			cdsEntry.CH_EntryStatus = CDS.Constants.ThreeCharFunctionCodes.DeclarationAccepted;
			AssertEquals("Old Entry Status should be '01'", CDS.Constants.NumbericFunctionCodes.MessageRegistered, cdsDeclaration.ApplicationExtender.GetOldEntryStatus(cdsEntry));
		}

		public void TestGetEntryStatus()
		{
			var chiefDeclaration = Factory.New<JobDeclaration>();
			chiefDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var chiefEntry = (CusEntryHeader)chiefDeclaration.ActiveEntryHeaders.AddNew();
			chiefEntry.CH_ImportClearanceStatusICS = "01";
			chiefEntry.CH_ImportClearanceStatusICS = "02";
			AssertEquals("Entry Status should be '02'", "02", chiefDeclaration.ApplicationExtender.GetEntryStatus(chiefEntry));

			var cdsDeclaration = Factory.New<JobDeclaration>();
			cdsDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cdsEntry = (CusEntryHeader)cdsDeclaration.ActiveEntryHeaders.AddNew();
			cdsEntry.CH_EntryStatus = CDS.Constants.ThreeCharFunctionCodes.MessageRegistered;
			cdsEntry.CH_EntryStatus = CDS.Constants.ThreeCharFunctionCodes.DeclarationAccepted;
			AssertEquals("Entry Status should be '02'", CDS.Constants.NumbericFunctionCodes.DeclarationAccepted, cdsDeclaration.ApplicationExtender.GetEntryStatus(cdsEntry));
		}
	}

	public class CreateMessageForTestEDIMessageDummy : EDIMessage
	{
		public CreateMessageForTestEDIMessageDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return "TEST";
		}
	}
}
