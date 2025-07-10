using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Testing
{
	abstract class NctsHeaderCommonDataObjectReaderTest<T> : DataObjectReaderTest
		where T : NctsHeaderCommonDataObjectReader
	{
		public void TestDataContextType()
		{
			var header = GetNewHeader();
			var headerData = GetShipmentData(header);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			AssertEquals("DataContextType", DataContextType.NctsHeader, reader.DataContextType);
		}

		public void TestMatchingExisting()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var currentCompanyBranch2 = currentCompany.Branches.AddNew();
			currentCompanyBranch2.GB_Code = "^$@";
			currentCompanyBranch2.GB_BranchName = "BRANCH 2";
			currentCompanyBranch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			company.GC_Name = "LV COMP";
			company.GC_Code = "LV$";
			company.GC_OH_OrgProxy = currentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "LV@";
			branch.GB_BranchName = "LV BRANCH";
			branch.GB_OH_OrgProxy = currentCompanyBranch2.GB_OH_OrgProxy;
			const string mrn = "MRN1234567890";
			var header1 = GetNewHeader();
			header1.BH_GB = branch.PK;
			CusEntryNumber.LoadOrCreate(header1, CusEntryNumberTypes.Standard.MovementReferenceNumber, currentCompany.GC_RN_NKCountryCode).CE_EntryNum = mrn;
			Factory.SaveForTesting();
			var header2 = GetNewHeader();
			header2.BH_GB = currentCompanyBranch2.PK;
			CusEntryNumber.LoadOrCreate(header2, CusEntryNumberTypes.Standard.MovementReferenceNumber, currentCompany.GC_RN_NKCountryCode).CE_EntryNum = mrn;
			Factory.SaveForTesting();
			header2.BH_IsActive = false;
			var header3 = GetNewHeader();
			header3.BH_GB = currentCompanyBranch2.PK;
			CusEntryNumber.LoadOrCreate(header3, CusEntryNumberTypes.Standard.MovementReferenceNumber, currentCompany.GC_RN_NKCountryCode).CE_EntryNum = mrn;
			Factory.SaveForTesting();
			var header4 = GetNewHeader();
			header4.BH_GB = currentCompanyBranch2.PK;
			CusEntryNumber.LoadOrCreate(header4, CusEntryNumberTypes.Standard.MovementReferenceNumber, currentCompany.GC_RN_NKCountryCode).CE_EntryNum = mrn;
			Factory.SaveForTesting();

			var reader = GetNewReader(header1);
			var header = reader.ReadIntoBusinessObject();
			AssertEquals("Should matched same company, active and latest", header4, header);
		}

		public void TestCreateNew()
		{
			var header1 = GetNewHeader();
			Factory.SaveForTesting();
			var headerData = GetShipmentData(header1);
			headerData.SetEntryNumberCollection(() => null);
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var header = reader.ReadIntoBusinessObject();
			AssertEquals("header.IsInDatabase", false, header.IsInDatabase);
			AssertEquals("BH_ApplicationCode", header1.BH_ApplicationCode, header.BH_ApplicationCode);
			AssertEquals("BH_HeaderType", header1.BH_HeaderType, header.BH_HeaderType);
		}

		public void TestImportBranch()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var currentCompanyBranch2 = currentCompany.Branches.AddNew();
			currentCompanyBranch2.GB_Code = "^$@";
			currentCompanyBranch2.GB_BranchName = "BRANCH 2";
			currentCompanyBranch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.SaveForTesting();
			var header1 = GetNewHeader();
			var headerData = GetShipmentData(header1);
			headerData.Branch.Code = currentCompanyBranch2.GB_Code;
			var reader = GetNewReader(headerData, new TestErrorLogger());
			var header = reader.ReadIntoBusinessObject();
			AssertEquals("header.BH_GB", currentCompanyBranch2.PK, header.BH_GB);
		}

		public void TestImportNotes()
		{
			var header = GetNewHeader();

			var note1 = header.Notes.AddNew();
			note1.ST_Description = "Description 1";
			note1.ST_NoteDataAsText = "Line 1";

			var note2 = header.Notes.AddNew();
			note2.ST_Description = "Description 2";
			note2.ST_NoteDataAsText = "Line 2";

			var reader = GetNewReader(header);
			var readerBo = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var notes = readerBo.Notes.GetAllNotesVisibleToCurrentCompany().ToArray();
				AssertEquals("Note Collection should have 2 notes", 2, notes.Length);
				Assert(notes.Any(n => n.ST_Description == "Description 1" && n.ST_NoteDataAsText == "Line 1"));
				Assert(notes.Any(n => n.ST_Description == "Description 2" && n.ST_NoteDataAsText == "Line 2"));
			});
		}

		public void TestImportJobDocAddresses()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "O!@#GR1";
			org1.MainAddress.OA_Address1 = "ADD 1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "O!@#GR2";
			org2.MainAddress.OA_Address1 = "ADD 1";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "O!@#GR3";
			org3.MainAddress.OA_Address1 = "ADD 1";
			Factory.SaveForTesting();

			var header = GetNewHeader();
			header.Principal.E2_OA_Address = org1.MainAddress.PK;
			header.Consignee.E2_OA_Address = org2.MainAddress.PK;
			header.Consignor.E2_OA_Address = org3.MainAddress.PK;
			var headerData = GetShipmentData(header);
			header.Delete();
			var reader = GetNewReader(headerData, new TestErrorLogger());
			header = reader.ReadIntoBusinessObject();
			AssertEquals("header.Principal.E2_OA_Address", org1.MainAddress.PK, header.Principal.E2_OA_Address);
			AssertEquals("header.Consignee.E2_OA_Address", org2.MainAddress.PK, header.Consignee.E2_OA_Address);
			AssertEquals("header.Consignor.E2_OA_Address", org3.MainAddress.PK, header.Consignor.E2_OA_Address);
		}

		protected UniversalShipment GetShipmentData(NctsHeader header)
		{
			var manager = (IShipmentDataContextManager)header.GetUniversalDataContextManager();
			var writeManager = new DataWritingManager(new ActionInfo(null, header));
			var writer = manager.GetShipmentDataObjectWriter(writeManager);
			return (UniversalShipment)writer.GetDataObject(header);
		}

		protected T GetNewReader(NctsHeader header)
		{
			var headerData = GetShipmentData(header);
			return GetNewReader(headerData, new TestErrorLogger());
		}

		protected abstract T GetNewReader(UniversalShipment headerData, TestErrorLogger testErrorLogger);
		protected abstract NctsHeader GetNewHeader();
	}
}
