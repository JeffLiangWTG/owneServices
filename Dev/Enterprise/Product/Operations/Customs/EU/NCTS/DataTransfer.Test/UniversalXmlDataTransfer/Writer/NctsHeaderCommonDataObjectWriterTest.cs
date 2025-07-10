using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalDataObjects = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Testing
{
	public abstract class NctsHeaderCommonDataObjectWriterTest<T> : TestCaseWithFactory
		where T : NctsHeaderCommonDataObjectWriter
	{
		public void TestGetEDIMessageSubType()
		{
			var header = GetNewHeader();
			ITopLevelDataObjectWriter writer = GetNewWriter(header);
			AssertEquals("EDIMessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, writer.EDIMessageSubType);
		}

		public void TestGetTopLevelDataContextType()
		{
			var header = GetNewHeader();
			ITopLevelDataObjectWriter writer = GetNewWriter(header);
			AssertEquals("TopLevelDataContextType", DataContextType.NctsHeader, writer.TopLevelDataContextType);
		}

		public void TestPopulateDataObject()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "^$@";
			branch2.GB_BranchName = "BRANCH 2";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var header = GetNewHeader();
			header.BH_GB = branch2.PK;
			header.MovementReferenceEntryNumber.CE_EntryNum = "MRN1234";
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", header.Principal, "1", traderTir: "GBR/022/1234567");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CO1", header.Consignor, "2");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", header.Consignee, "3");
			var note1 = header.Notes.AddNew();
			note1.ST_Description = "Description 1";
			note1.ST_NoteDataAsText = "Line 1";
			var note2 = header.Notes.AddNew();
			note2.ST_Description = "Description 2";
			note2.ST_NoteDataAsText = "Line 2";
			SetupAdditionalData(header);
			var shipment = GetDataObject(header);
			CombineAssertions("Main Data", () =>
			{
				AssertEquals("shipment.Branch.Code", "^$@", shipment.Branch.Code);
				AssertEquals("shipment.Branch.Name", "BRANCH 2", shipment.Branch.Name);
				AssertEquals("shipment.MessagingApplicationCode.Code", header.BH_ApplicationCode, shipment.MessagingApplicationCode.Code);
				AssertEquals("shipment.MessagingApplicationCode.Description", new CusInBondApplicationCodeList().GetDescriptionFromCode(header.BH_ApplicationCode), shipment.MessagingApplicationCode.Description);
				AssertNotNull("Should have a EntryNumber for MRN (MRN1234)", shipment.EntryNumberCollection.SingleOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryNumberTypes.Standard.MovementReferenceNumber && "MRN1234".Equals(x.Number)));
				AssertOrganizationAddress("OrganizationAddress-Principal", GetOrganizationAddress(shipment.OrganizationAddressCollection, nameof(DocAddressType.Principal)), "1", tir: "GBR/022/1234567");
				AssertOrganizationAddress("OrganizationAddress-Consignor", GetOrganizationAddress(shipment.OrganizationAddressCollection, nameof(DocAddressType.ConsignorDocumentaryAddress)), "2");
				AssertOrganizationAddress("OrganizationAddress-Consignee", GetOrganizationAddress(shipment.OrganizationAddressCollection, nameof(DocAddressType.ConsigneeAddress)), "3");
				AssertNotNull("Should have a Note (Description 1, Line 1)", shipment.NoteCollection.SingleOrDefault(x => "Description 1".Equals(x.Description) && "Line 1".Equals(x.NoteText)));
				AssertNotNull("Should have a Note (Description 2, Line 2)", shipment.NoteCollection.SingleOrDefault(x => "Description 2".Equals(x.Description) && "Line 2".Equals(x.NoteText)));
			});
			AssertAdditionalData(shipment);
		}

		protected virtual void AssertAdditionalData(Shipment shipment) { }
		protected virtual void SetupAdditionalData(NctsHeader header) { }

		static protected void AssertOrganizationAddress(ZString traderType, OrganizationAddress xmlOrganizationAddressData, string suffix, string name = "Oscorp Industries", string street = "Street and No"
			, string postcode = "MK16 XX", string city = "Milton Keynes", string country = Core.Constants.CountryCodes.UnitedKingdom, string eori = "012345678900", string tir = "")
		{
			AssertEquals(traderType + " Name", name + suffix, xmlOrganizationAddressData.CompanyName);
			AssertEquals(traderType + " Street and number", street + suffix, xmlOrganizationAddressData.Address1);
			AssertEquals(traderType + " Postal code", postcode + suffix, xmlOrganizationAddressData.Postcode);
			AssertEquals(traderType + " City", city + suffix, xmlOrganizationAddressData.City);
			AssertEquals(traderType + " Country code", country, xmlOrganizationAddressData.Country.Code);
			AssertEquals(traderType + " EORI-TIN (Turn)", eori + suffix, GetRegistrationNumberCollectionValue(xmlOrganizationAddressData.RegistrationNumberCollection, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
			AssertEquals(traderType + " Holder ID TIR", tir, GetRegistrationNumberCollectionValue(xmlOrganizationAddressData.RegistrationNumberCollection, OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers));
		}

		static protected OrganizationAddress GetOrganizationAddress(IEnumerable<OrganizationAddress> organizationAddressCollection, ZString addressType)
		{
			return organizationAddressCollection.FirstOrDefault(oa => addressType.Equals(oa.AddressType));
		}

		static protected ZString GetRegistrationNumberCollectionValue(IEnumerable<UniversalDataObjects.RegistrationNumber> registrationNumberCollection, ZString code) =>
			GetRegistrationNumber(registrationNumberCollection, code)?.Value.GetValueOrDefault() ?? ZString.Empty;

		static protected UniversalDataObjects.RegistrationNumber GetRegistrationNumber(IEnumerable<UniversalDataObjects.RegistrationNumber> registrationNumberCollection, ZString code)
		{
			return registrationNumberCollection.FirstOrDefault(r => r.Type.GetCodeAsUpperCase() == code);
		}

		static protected IEnumerable<CustomsReference> GetCustomsReferences(IEnumerable<CustomsReference> customsReferenceCollection, ZString customsReferenceType)
		{
			return customsReferenceCollection.Where(e => e.Type.GetCodeAsUpperCase() == customsReferenceType);
		}

		static protected CustomsReference GetCustomsReference(IEnumerable<CustomsReference> customsReferenceCollection, ZString customsReferenceType) =>
			GetCustomsReferences(customsReferenceCollection, customsReferenceType).FirstOrDefault();

		protected abstract NctsHeader GetNewHeader();
		protected abstract T GetNewWriter(IDataWritingManager manager);
		protected T GetNewWriter(NctsHeader header) => GetNewWriter(new DataWritingManager(new ActionInfo(null, header)));
		protected Shipment GetDataObject(NctsHeader header) => GetNewWriter(header).GetDataObject(header);
	}
}
