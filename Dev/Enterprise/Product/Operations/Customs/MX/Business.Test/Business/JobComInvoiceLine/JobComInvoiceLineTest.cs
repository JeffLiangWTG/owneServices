using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Mexico, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			var customsTemplate_Company = Factory.New<GlbCompany>();
			customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
			customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico)).RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = customsTemplate_Branch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertType<OrgSupplierPart>("product type still the type", declarationLoaded.InvoiceLines[0].Part);
		}

		public void TestOnFactorySaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Brazil;
			Factory.Save();
			AssertEquals("JI_RN_NKCountryOfExport NOT Empty", Core.Constants.CountryCodes.Brazil, invoiceLine.JI_RN_NKCountryOfExport);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			Assert("JI_RN_NKCountryOfExport Empty", invoiceLine.JI_RN_NKCountryOfExport.IsEmpty);
		}

		public void TestObservations()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			var noteFilter = new ZQuery(StmNoteSchema.ST_ParentID, invoiceLine.PK);
			noteFilter.AddToFilter(StmNoteSchema.ST_Table, "JobComInvoiceLine");
			noteFilter.AddToFilter(StmNoteSchema.ST_Description, "Observations");

			invoiceLine.Observations = "Line1\r\nLine2";
			Factory.Save();
			AssertEquals("Line1\r\nLine2", Factory.LoadTop1<StmNote>(noteFilter).ST_NoteText);
			invoiceLine.Observations = "Line1\r\nLine2\r\nLine3";
			Factory.Save();
			AssertEquals("Line1\r\nLine2\r\nLine3", Factory.LoadTop1<StmNote>(noteFilter).ST_NoteText);
		}

		public void TestIdentifiers()
		{
			Factory.New<Identifier>().CSI_ParentID = InvoiceLine.PK;
			Factory.New<Identifier>().CSI_ParentID = InvoiceLine.PK;
			AssertEquals("Identifiers count should be", 2, InvoiceLine.Identifiers.Count);
		}

		public void TestVehicleVIN_CaptionAndLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("VIN", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().VehicleVINInfo).Caption);
				AssertEquals("VehicleVINInfo.MaxLength", 25, InvoiceLine.VehicleVINInfo.MaxLength);
				AssertEquals("Mileage", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().VehicleMileageInfo).Caption);
				AssertEquals("VehicleMileageInfo.MaxLength", 6, InvoiceLine.VehicleMileageInfo.MaxLength);
				AssertEquals("Mileage UQ", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().VehicleMileageUQInfo).Caption);
				AssertEquals("VehicleMileageInfo.MaxLength", 2, InvoiceLine.VehicleMileageUQInfo.MaxLength);

				AssertEquals(VehicleRelationshipType.One, InvoiceLine.VehicleRelationship);
				InvoiceLine.VehicleVIN = "VIN123";
				AssertEquals("VIN123", InvoiceLine.FirstVehicle.CVH_SerialNumber);
				InvoiceLine.VehicleMileage = 10;
				AssertEquals(10, InvoiceLine.FirstVehicle.CVH_Mileage);
			});
		}

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);

		protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;
	}
}
