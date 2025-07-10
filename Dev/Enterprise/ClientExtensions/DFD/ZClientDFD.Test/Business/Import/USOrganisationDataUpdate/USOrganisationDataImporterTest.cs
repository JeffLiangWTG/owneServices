using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USOrganisationDataImporterTest : TestCaseWithFactory
	{
		[TestDate(2010, 05, 01)]
		public void TestImportDataToFactoryCore()
		{
			var buffer = new NotificationBuffer();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper1 = OrgHeaderWrapper.New(org1);
			var wrapper2 = OrgHeaderWrapper.New(org2);
			var wrapper3 = OrgHeaderWrapper.New(org3);
			var wrapper4 = OrgHeaderWrapper.New(org4);
			org1.OH_IsConsignee = false;
			org1.OH_Code = "CODE1";
			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "264520", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates));
			org2.OH_IsConsignee = false;
			org2.OH_Code = "CODE2";
			org2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "26-222642300", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates));
			org3.OH_Code = "CODE3";
			org3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "003801-00579", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates));
			org4.OH_Code = "CODE4";
			org4.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "092-70404201", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(0, wrapper1.BondDetails.Count);
			AssertEquals(0, wrapper2.BondDetails.Count);
			AssertEquals(0, wrapper3.BondDetails.Count);
			AssertEquals(0, wrapper4.BondDetails.Count);
			Assert(wrapper1.ZO_PaymentType.IsEmpty);
			Assert(wrapper1.ZO_AccountNo.IsEmpty);
			Assert(wrapper2.ZO_PaymentType.IsEmpty);
			Assert(wrapper2.ZO_AccountNo.IsEmpty);
			Assert(wrapper3.ZO_PaymentType.IsEmpty);
			Assert(wrapper3.ZO_AccountNo.IsEmpty);
			Assert(wrapper4.ZO_PaymentType.IsEmpty);
			Assert(wrapper4.ZO_AccountNo.IsEmpty);
			Factory.Save();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.SaveResourceToFile(TestResource);
				Importer.ImportData(testFile, buffer, SourceInfo.EmptySourceInfo);
			}
			Assert(buffer.AsString.Contains(@"Unable to find Customs Agent Broker organisation with code 00001142"));
			var org000001142 = Factory.New<OrgHeader>();
			org000001142.OH_Code = "00001142";
			var party1 = org1.AllRelatedParties.AddNew();
			party1.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			party1.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			party1.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			party1.PR_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			var party2 = org2.AllRelatedParties.AddNew();
			party2.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			party2.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			party2.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			party2.PR_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.SaveResourceToFile(TestResource);
				Importer.ImportData(testFile, buffer, SourceInfo.EmptySourceInfo);
			}
			var newFactory = new BusinessObjectFactory();
			org1 = newFactory.Load<OrgHeader>(org1.PK);
			org2 = newFactory.Load<OrgHeader>(org2.PK);
			org3 = newFactory.Load<OrgHeader>(org3.PK);
			org4 = newFactory.Load<OrgHeader>(org4.PK);
			wrapper1 = OrgHeaderWrapper.New(org1);
			wrapper2 = OrgHeaderWrapper.New(org2);
			wrapper3 = OrgHeaderWrapper.New(org3);
			wrapper4 = OrgHeaderWrapper.New(org4);
			var reqDocs1 = Factory.Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, org1.PK));
			var reqDocs2 = Factory.Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, org2.PK));
			var reqDocs3 = Factory.Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, org3.PK));
			var reqDocs4 = Factory.Load<JobRequiredDocument>(new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, org4.PK));
			Assert(org1.OH_IsConsignee);
			Assert(org2.OH_IsConsignee);
			AssertEquals(1, wrapper1.BondDetails.Count);
			AssertEquals(ActivityCodeList.Codes._1, wrapper1.BondDetails[0].PW_ActivityCode);
			AssertEquals("8", wrapper1.BondDetails[0].PW_BondType);
			AssertEquals("460324340", wrapper1.BondDetails[0].PW_BondNumber);
			AssertEquals(700000m, wrapper1.BondDetails[0].PW_BondAmount);
			AssertEquals("856", wrapper1.BondDetails[0].PW_SuretyCode);
			AssertEquals(ZDateTime.Now, wrapper1.BondDetails[0].PW_BondEffectiveDate);
			AssertEquals("3", wrapper1.ZO_PaymentType);
			AssertEquals("10603", wrapper1.ZO_AccountNo);
			AssertEquals(1, org1.CustomsCodes.Cast<OrgCusCode>().Count(cusCode => cusCode.OK_CodeType == OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
			AssertEquals(1, org1.AllRelatedParties.Count);
			AssertEquals(org000001142.PK, org1.AllRelatedParties.Cast<OrgRelatedParty>().First().PR_OH_RelatedParty);
			AssertEquals(1, reqDocs1.Length);
			AssertEquals(org1.PK, reqDocs1.First().EQ_ParentID);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, reqDocs1.First().EQ_ParentTableCode);
			AssertEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDocs1.First().EQ_DocCategory);
			AssertEquals(Core.Constants.RefDocTypes.PowerOfAttorney, reqDocs1.First().EQ_DocType);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDocs1.First().EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Now, reqDocs1.First().EQ_DateReceived);
			AssertEquals(ZDateTime.Now.AddYears(5), reqDocs1.First().EQ_ValidToDate);
			AssertEquals(JobRequiredDocument.DocUsage.Broker, reqDocs1.First().EQ_DocUsage);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, reqDocs1.First().EQ_RN_NKRelatedCountry);
			AssertEquals(1, wrapper2.BondDetails.Count);
			AssertEquals(ActivityCodeList.Codes._1, wrapper2.BondDetails[0].PW_ActivityCode);
			AssertEquals("9", wrapper2.BondDetails[0].PW_BondType);
			AssertEquals("0", wrapper2.BondDetails[0].PW_BondNumber);
			AssertEquals(ZDecimal.Zero, wrapper2.BondDetails[0].PW_BondAmount);
			AssertEquals("856", wrapper2.BondDetails[0].PW_SuretyCode);
			AssertEquals(ZDateTime.Now, wrapper2.BondDetails[0].PW_BondEffectiveDate);
			AssertEquals("2", wrapper2.ZO_PaymentType);
			AssertEquals(ZString.Empty, wrapper2.ZO_AccountNo);
			AssertEquals(2, org2.AllRelatedParties.Count);
			AssertEquals(1, org2.AllRelatedParties.Find(new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, org000001142.PK)).Length);
			AssertEquals(1, reqDocs2.Length);
			AssertEquals(org2.PK, reqDocs2.First().EQ_ParentID);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, reqDocs2.First().EQ_ParentTableCode);
			AssertEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDocs2.First().EQ_DocCategory);
			AssertEquals(Core.Constants.RefDocTypes.PowerOfAttorney, reqDocs2.First().EQ_DocType);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDocs2.First().EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Now, reqDocs2.First().EQ_DateReceived);
			AssertEquals(ZDateTime.Now.AddYears(5), reqDocs2.First().EQ_ValidToDate);
			AssertEquals(JobRequiredDocument.DocUsage.Broker, reqDocs2.First().EQ_DocUsage);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, reqDocs2.First().EQ_RN_NKRelatedCountry);
			AssertEquals(1, wrapper3.BondDetails.Count);
			AssertEquals(ActivityCodeList.Codes._1, wrapper3.BondDetails[0].PW_ActivityCode);
			AssertEquals("8", wrapper3.BondDetails[0].PW_BondType);
			AssertEquals("0", wrapper3.BondDetails[0].PW_BondNumber);
			AssertEquals(50000m, wrapper3.BondDetails[0].PW_BondAmount);
			AssertEquals("856", wrapper3.BondDetails[0].PW_SuretyCode);
			AssertEquals(ZDateTime.Now, wrapper3.BondDetails[0].PW_BondEffectiveDate);
			AssertEquals("2", wrapper3.ZO_PaymentType);
			AssertEquals(ZString.Empty, wrapper3.ZO_AccountNo);
			AssertEquals(1, org3.AllRelatedParties.Count);
			AssertEquals(org000001142.PK, org3.AllRelatedParties.Cast<OrgRelatedParty>().First().PR_OH_RelatedParty);
			AssertEquals(1, reqDocs3.Length);
			AssertEquals(org3.PK, reqDocs3.First().EQ_ParentID);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, reqDocs3.First().EQ_ParentTableCode);
			AssertEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDocs3.First().EQ_DocCategory);
			AssertEquals(Core.Constants.RefDocTypes.PowerOfAttorney, reqDocs3.First().EQ_DocType);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDocs3.First().EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Now, reqDocs3.First().EQ_DateReceived);
			AssertEquals(ZDateTime.Now.AddYears(5), reqDocs3.First().EQ_ValidToDate);
			AssertEquals(JobRequiredDocument.DocUsage.Broker, reqDocs3.First().EQ_DocUsage);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, reqDocs3.First().EQ_RN_NKRelatedCountry);
			AssertEquals(1, wrapper4.BondDetails.Count);
			AssertEquals(ActivityCodeList.Codes._1, wrapper4.BondDetails[0].PW_ActivityCode);
			AssertEquals("9", wrapper4.BondDetails[0].PW_BondType);
			AssertEquals("0", wrapper4.BondDetails[0].PW_BondNumber);
			AssertEquals(ZDecimal.Zero, wrapper4.BondDetails[0].PW_BondAmount);
			AssertEquals("856", wrapper4.BondDetails[0].PW_SuretyCode);
			AssertEquals(ZDateTime.Now, wrapper4.BondDetails[0].PW_BondEffectiveDate);
			AssertEquals("2", wrapper4.ZO_PaymentType);
			AssertEquals(ZString.Empty, wrapper4.ZO_AccountNo);
			AssertEquals(1, org4.AllRelatedParties.Count);
			AssertEquals(org000001142.PK, org4.AllRelatedParties.Cast<OrgRelatedParty>().First().PR_OH_RelatedParty);
			AssertEquals(1, reqDocs4.Length);
			AssertEquals(org4.PK, reqDocs4.First().EQ_ParentID);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, reqDocs4.First().EQ_ParentTableCode);
			AssertEquals(Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDocs4.First().EQ_DocCategory);
			AssertEquals(Core.Constants.RefDocTypes.PowerOfAttorney, reqDocs4.First().EQ_DocType);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDocs4.First().EQ_DocPeriod);
			AssertEquals(ZDateTimeOffset.Now, reqDocs4.First().EQ_DateReceived);
			AssertEquals(ZDateTime.Now.AddYears(5), reqDocs4.First().EQ_ValidToDate);
			AssertEquals(JobRequiredDocument.DocUsage.Broker, reqDocs4.First().EQ_DocUsage);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, reqDocs4.First().EQ_RN_NKRelatedCountry);
			AssertContains(@"Unable not match organisation with following details provided: 
Legacy Code:  EIN: 624-56-7168
Legacy Code: 264080 EIN: 56-166752500
Legacy Code:  EIN: 22-364205000
4 organisations updated.", buffer.AsString);
		}

		#region Implementation
		const string TestResource = "test.csv";
		USOrganisationDataImporter Importer
		{
			get
			{
				return importer ?? (importer = new USOrganisationDataImporter());
			}
		}

		USOrganisationDataImporter importer;
		#endregion
	}
}
