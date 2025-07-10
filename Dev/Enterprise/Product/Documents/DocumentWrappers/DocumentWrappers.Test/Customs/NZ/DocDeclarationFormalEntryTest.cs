using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Bill = Enterprise.Customs.NZ.Business.Declaration.Bill;
using BillTypeList = Enterprise.Customs.NZ.Business.Declaration.BillTypeList;
using JobMessageTypeList = Enterprise.Customs.NZ.Business.JobMessageTypeList;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocDeclarationFormalEntryTest : DocDeclarationTest
	{
		public void TestMiscSuppliersAndImporters()
		{
			ZGuid miscOrgPK = Declaration.CachedMiscOrgPK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_OH_Supplier = miscOrgPK;
			Declaration.MiscSupplierName = "MY CAT'S MISC SUPPLIER";
			Declaration.JE_OH_Importer = miscOrgPK;
			Declaration.MiscImporterName = "MY DOG'S MISC IMPORTER";

			AssertEquals("DeclarationWrapper.Supplier.Code", "MISC", DeclarationWrapper.Supplier.Code);
			AssertEquals("DeclarationWrapper.Supplier.ToString()", "MY CAT'S MISC SUPPLIER", DeclarationWrapper.Supplier.ToString());
			AssertEquals("DeclarationWrapper.Supplier.LocalCustomsClientCode", "", DeclarationWrapper.Supplier.LocalCustomsSupplierCode);
			AssertEquals("DeclarationWrapper.Supplier.PostalAddress", "MY CAT'S MISC SUPPLIER\nNEW ZEALAND", DeclarationWrapper.Supplier.PostalAddress);

			AssertEquals("DeclarationWrapper.Importer.Code", "MISC", DeclarationWrapper.Importer.Code);
			AssertEquals("DeclarationWrapper.Importer.ToString()", "MY DOG'S MISC IMPORTER", DeclarationWrapper.Importer.ToString());
			AssertEquals("DeclarationWrapper.Importer.LocalCustomsClientCode", "", DeclarationWrapper.Importer.LocalCustomsClientCode);
			AssertEquals("DeclarationWrapper.Importer.PostalAddress", "MY DOG'S MISC IMPORTER\nNEW ZEALAND", DeclarationWrapper.Importer.PostalAddress);

			Factory.Save();
			AssertEquals("SupplierWrapper.IsInDatabase", false, ((BusinessObject)DeclarationWrapper.Supplier.WrappedObject).IsInDatabase);
			AssertEquals("ImporterWrapper.IsInDatabase", false, ((BusinessObject)DeclarationWrapper.Importer.WrappedObject).IsInDatabase);

			OrgHeader normalSupplier = Factory.New<OrgHeader>();
			normalSupplier.OH_FullName = "ABNORMAL SUPPLIER";
			normalSupplier.OH_Code = "AB_SUP";
			normalSupplier.LocalCustomsSupplierCode = "123456Z";

			OrgHeader normalImporter = Factory.New<OrgHeader>();
			normalImporter.OH_FullName = "ABNORMAL IMPORTER";
			normalImporter.OH_Code = "AB_IMP";
			normalImporter.LocalCustomsClientCode = "654321Z";

			Declaration.JE_OH_Supplier = normalSupplier.PK;
			Declaration.JE_OH_Importer = normalImporter.PK;

			AssertEquals("DeclarationWrapper.Supplier.Code", "AB_SUP", DeclarationWrapper.Supplier.Code);
			AssertEquals("DeclarationWrapper.Supplier.ToString()", "ABNORMAL SUPPLIER", DeclarationWrapper.Supplier.ToString());
			AssertEquals("DeclarationWrapper.Supplier.LocalCustomsSupplierCode", "123456Z", DeclarationWrapper.Supplier.LocalCustomsSupplierCode);
			AssertEquals("DeclarationWrapper.Supplier.PostalAddress", "ABNORMAL SUPPLIER\nNEW ZEALAND", DeclarationWrapper.Supplier.PostalAddress);

			AssertEquals("DeclarationWrapper.Importer.Code", "AB_IMP", DeclarationWrapper.Importer.Code);
			AssertEquals("DeclarationWrapper.Importer.ToString()", "ABNORMAL IMPORTER", DeclarationWrapper.Importer.ToString());
			AssertEquals("DeclarationWrapper.Importer.LocalCustomsClientCode", "654321Z", DeclarationWrapper.Importer.LocalCustomsClientCode);
			AssertEquals("DeclarationWrapper.Importer.PostalAddress", "ABNORMAL IMPORTER\nNEW ZEALAND", DeclarationWrapper.Importer.PostalAddress);
		}

		public void TestEntryHeaderCollection()
		{
			AssertEquals("EntryHeader Collection type", typeof(FormalEntry.DocCusEntryHeaderCollection), DeclarationWrapper.RateEntryHeaders.GetType());
		}

		public void TestEntryLinesForEntryPrint()
		{
			AssertNotNull("DeclarationWrapper.EntryLinesForEntryPrint", DeclarationWrapper.EntryLinesForEntryPrint);
		}

		public void TestDeclarantNameAndCode()
		{
			NZCustomsDataRegistry.Instance.HideDeclarantCodeOnCustomsDocumentation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "BA4";
			staff.GS_FullName = "Blackadder";
			staff.GetNZWrapper().NZBPassword.GP_UserID = "98654321Z";

			GlbStaff.CurrentUser.GetNZWrapper().NZBPassword.GP_UserID = "65432198B";
			GlbStaff.CurrentUser.GS_FullName = "Baldric";

			AssertEquals("DeclarationWrapper.DeclarantName", "Baldric", DeclarationWrapper.DeclarantName);
			AssertEquals("DeclarationWrapper.DeclarantCustomsCode", "65432198B", DeclarationWrapper.DeclarantCustomsCode);
			AssertEquals("DeclarationWrapper.DeclarantNameAndCode", "Baldric (65432198B)", DeclarationWrapper.DeclarantNameAndCode);

			Declaration.JE_GS_NKCusAgent = staff.GS_Code;

			AssertEquals("DeclarationWrapper.DeclarantName", "Blackadder", DeclarationWrapper.DeclarantName);
			AssertEquals("DeclarationWrapper.DeclarantCustomsCode", "98654321Z", DeclarationWrapper.DeclarantCustomsCode);
			AssertEquals("DeclarationWrapper.DeclarantNameAndCode", "Blackadder (98654321Z)", DeclarationWrapper.DeclarantNameAndCode);

			NZCustomsDataRegistry.Instance.HideDeclarantCodeOnCustomsDocumentation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertEquals("DeclarationWrapper.DeclarantName", "Blackadder", DeclarationWrapper.DeclarantName);
			AssertEquals("DeclarationWrapper.DeclarantCustomsCode", "", DeclarationWrapper.DeclarantCustomsCode);
			AssertEquals("DeclarationWrapper.DeclarantNameAndCode", "Blackadder", DeclarationWrapper.DeclarantNameAndCode);
		}

		public void TestClient()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			OrgHeader supplier = OrgHeader.New(Factory);
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.JE_OH_Supplier = supplier.PK;

			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)DeclarationWrapper.Client.WrappedObject;
			AssertEquals("Client shoud equal Importer", importer.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);

			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Export;
			intermediateWrapper = (DocBaseWrapper)DeclarationWrapper.Client.WrappedObject;
			AssertEquals("Client shoud equal Supplier", supplier.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		public void TestEntryTypeAndStyle()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;
			AssertEquals("DeclarationWrapper.EntryTypeAndStyle", Declaration.EntryTypeAndStyle, DeclarationWrapper.EntryTypeAndStyle);
		}

		public void TestEntryNumberAndDate()
		{
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("DeclarationWrapper.EntryNumberAndDate", "12345678 / (none)", DeclarationWrapper.EntryNumberAndDate);
		}

		public void TestEntryDate()
		{
			var message = Declaration.CusEntryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 3, 31, 20, 6, 0);
			AssertEquals("EntryDate is local date (not UTC)", new ZDate(2011, 4, 1), DeclarationWrapper.EntryDate.Date);
		}

		public void TestClientReference()
		{
			Declaration.JE_OwnerRef = "12345";
			AssertEquals("DeclarationWrapper.ClientReference", Declaration.JE_DeclarationReference + " / 12345", DeclarationWrapper.ClientReference);
		}

		public void TestWeightIncludingUnit()
		{
			Declaration.JE_TotalWeight = 12.3m;
			Declaration.JE_TotalWeightUnit = "XX";
			AssertEquals("DeclarationWrapper.WeightIncludingUnit", "12 XX", DeclarationWrapper.WeightIncludingUnit);
		}

		public void TestVolumeIncludingUnit()
		{
			Declaration.JE_TotalVolume = 12.345m;
			Declaration.JE_TotalVolumeUnit = "XX";
			AssertEquals("DeclarationWrapper.VolumeIncludingUnit", "12.345 XX", DeclarationWrapper.VolumeIncludingUnit);
		}

		public void TestTotalWeightInKG()
		{
			Declaration.JE_TotalWeight = 12.3m;
			Declaration.JE_TotalWeightUnit = "LB";
			AssertEquals("DeclarationWrapper.TotalWeightInKG - must match the transmitted value for Air and Sea to state the total gross weight of the Declaration in whole figure", 6m, DeclarationWrapper.TotalWeightInKG);
		}

		public void TestDrawbackIfDrawbackApplicable()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;
			AssertEquals("DeclarationWrapper.DrawbackIfDrawbackApplicable", "", DeclarationWrapper.DrawbackIfDrawbackApplicable);

			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("DeclarationWrapper.DrawbackIfDrawbackApplicable", "Drawback", DeclarationWrapper.DrawbackIfDrawbackApplicable);
		}

		public void TestEntryHeader()
		{
			AssertNotNull("EntryHeader is not null", DeclarationWrapper.EntryHeader);
		}

		public void TestExtraFSALineSectionsRequired()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;

			var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_AdValoremTariff = "0302.11.0002.A";   // FSA permit needed for this tariff

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_AdValoremTariff = "8208.30.0000.D";   // FSA permit NOT needed for this tariff

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_AdValoremTariff = "0302.32.0011.B"; // FSA permit needed for this tariff
			Declaration.ResumeApportionment();

			AssertEquals("Two FSA lines now for this entry - extra print line document section is required", true, DeclarationWrapper.ExtraFSALineSectionsRequired);

			entryHeader.AllEntryLines.RemoveAndDelete(entryLine3);
			Declaration.ResumeApportionment();
			AssertEquals("Only 1 FSA line now for this entry", false, DeclarationWrapper.ExtraFSALineSectionsRequired);
		}

		public void TestExtraFSALineSectionsRequired2()
		{
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;

			var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_AdValoremTariff = "0302.69.01.55C";   // FSA permit needed for this tariff

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_AdValoremTariff = "8208.30.0000.D";   // FSA permit NOT needed for this tariff

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_AdValoremTariff = "0304.29.00.66D"; // FSA permit needed for this tariff
			Declaration.ResumeApportionment();

			AssertEquals("Two FSA lines now for this entry - extra print line document section is required", true, DeclarationWrapper.ExtraFSALineSectionsRequired);

			entryHeader.AllEntryLines.RemoveAndDelete(entryLine3);
			Declaration.ResumeApportionment();
			AssertEquals("Only 1 FSA line now for this entry", false, DeclarationWrapper.ExtraFSALineSectionsRequired);
		}

		#region Insurance Certificate Field Tests
		public void TestMarineCertificate()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.MarineCertificateNumber, "ABCD");
			AssertEquals("Marine Certificate:", "ABCD", DeclarationWrapper.MarineCertificate);
		}

		public void TestMarinePolicyNumber()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.MarinePolicyNumber, "Policy:ABCD");
			AssertEquals("Marine Policy Number:", "Policy:ABCD", DeclarationWrapper.MarinePolicyNumber);
		}

		public void TestShipperReference()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.ShipperReference, "Shipper");
			AssertEquals("Shipper Reference:", "Shipper", DeclarationWrapper.ShipperReference);
		}

		public void TestPaymentCurrency()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.ClaimsPayableInCurrency, "999");
			AssertEquals("Payment Currency:", "999", DeclarationWrapper.PaymentCurrency);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.ClaimsPayableInCurrency, "AUD");
			AssertEquals("Payment Currency:", "AUD-Australian Dollar", DeclarationWrapper.PaymentCurrency);
		}

		public void TestInsuredWith()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.InsuredWith, "NRMA");
			AssertEquals("Insured With:", "NRMA", DeclarationWrapper.InsuredWith);
		}

		public void TestSubjectTo()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.SubjectTo, "This is subject to");
			AssertEquals("SubjectTo:", "This is subject to", DeclarationWrapper.SubjectTo);
		}

		public void TestSubjectOnlyToConditions()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.SubjectOnlyToConditions, "Conditions");
			AssertEquals("SubjectOnlyToConditions:", "Conditions", DeclarationWrapper.SubjectToConditions);
		}

		public void TestPlaceAndDateOfIssue()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.InsuranceCertificate.PlaceandDateOfIssue, "AUMEL 02Feb2006");
			AssertEquals("PlaceAndDateOfIssue:", "AUMEL 02Feb2006", DeclarationWrapper.PlaceOfIssue);
		}
		#endregion

		#region Document Of Origin Field Tests
		public void TestNoOfDeliverance()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.NoOfDeliverance, "201/12032006");
			AssertEquals("No of Deliverance:", "201/12032006", DeclarationWrapper.NoOfDeliverance);
		}

		public void TestCNCode()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.CNCode, "ABCD");
			AssertEquals("CN Code:", "ABCD", DeclarationWrapper.CNCode);
		}

		public void TestDutyRate()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.DutyRate, "10.5");
			AssertEquals("Duty Rate:", "10.5", DeclarationWrapper.DutyRate);
		}

		public void TestCarcaseMass()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.CarcaseMassInKG, "10.5");
			AssertEquals("Carcase Mass (kg):", "10.5", DeclarationWrapper.CarcaseMass);
		}

		public void TestDocOriginPlace()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.Place, "Sydney");
			AssertEquals("Document of Origin Place:", "Sydney", DeclarationWrapper.DocOriginPlace);
		}

		public void TestDocOriginDate()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.Date, "23-Jul-2006");
			AssertEquals("Document of Origin Date:", "23-Jul-2006", DeclarationWrapper.DocOriginDate);
		}

		public void TestDocOriginExpiryDate()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.DocumentOfOrigin.ExpiryDate, "23-Jul-2006");
			AssertEquals("Expiry Date:", "23-Jul-2006", DeclarationWrapper.DocOriginExpiryDate);
		}
		#endregion

		#region Sanitary Certificate Field Tests
		public void TestColdStore()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.ColdStore, "4 degrees below zero");
			AssertEquals("Cold Store:", "4 degrees below zero", DeclarationWrapper.ColdStore);
		}

		public void TestProcessedAt()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.ProcessedAt, "abatoir");
			AssertEquals("Processed At:", "abatoir", DeclarationWrapper.ProcessedAt);
		}

		public void TestSpecies()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.Species, "beef");
			AssertEquals("Species:", "beef", DeclarationWrapper.Species);
		}

		public void TestSlaughteredAt()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.SlaughteredAt, "abatoir");
			AssertEquals("Slaughtered At:", "abatoir", DeclarationWrapper.SlaughteredAt);
		}

		public void TestDoneAt()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.DoneAt, "Place");
			AssertEquals("Document of Origin Date:", "Place", DeclarationWrapper.DoneAt);
		}

		public void TestDocOn()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.DoneOn, "23-Jul-2006");
			AssertEquals("Sanitary Certificate Done On", "23-Jul-2006", DeclarationWrapper.DoneOn);
		}

		public void TestOfficialVeterinarian()
		{
			DocumentNote note = DocumentNote.LoadNote(Declaration);
			note.SetSystemDefinedFieldValue(DocDeclaration.SDFields.SanitaryCertificate.OfficialVeterinarian, "JOHN");
			AssertEquals("Official Veterinarian", "JOHN", DeclarationWrapper.OfficialVeterinarian);
		}
		#endregion

		#region AUPrefCertOfOrigin Field Tests
		public void TestPackingCostsIncluded()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "Yes";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PackingCostsIncluded);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PackingCostsIncluded, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PackingCostsIncluded);
		}

		public void TestPackingCostsAmount()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "44.32";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PackingCostsAmount);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PackingCostsAmount, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PackingCostsAmount);
		}

		public void TestPrepaidOverseasFreightIncluded()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "No";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PrepaidOverseasFreightIncluded);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PrepaidOverseasFreightIncluded, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PrepaidOverseasFreightIncluded);
		}

		public void TestPrepaidOverseasFreightAmount()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "545.55";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PrepaidOverseasFreightAmount);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PrepaidOverseasFreightAmount, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PrepaidOverseasFreightAmount);
		}

		public void TestPrepaidDomesticFreightIncluded()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "YARS";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PrepaidDomesticFreightIncluded);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PrepaidDomesticFreightIncluded, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PrepaidDomesticFreightIncluded);
		}

		public void TestPrepaidDomesticFreightAmount()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "4.40";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PrepaidDomesticFreightAmount);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PrepaidDomesticFreightAmount, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PrepaidDomesticFreightAmount);
		}

		public void TestPrepaidInsuranceIncluded()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "ZZ";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PrepaidInsuranceIncluded);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PrepaidInsuranceIncluded, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PrepaidInsuranceIncluded);
		}

		public void TestPrepaidInsuranceAmount()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "3.00";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.PrepaidInsuranceAmount);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.PrepaidInsuranceAmount, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.PrepaidInsuranceAmount);
		}

		public void TestOtherPrepaidCostsDescription()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "Saddle Wear & Tear";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.OtherPrepaidCostsDescription);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.OtherPrepaidCostsDescription, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.OtherPrepaidCostsDescription);
		}

		public void TestOtherPrepaidCostsIncluded()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "Nope";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.OtherPrepaidCostsIncluded);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.OtherPrepaidCostsIncluded, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.OtherPrepaidCostsIncluded);
		}

		public void TestOtherPrepaidCostsAmount()
		{
			ZString expectedDefaultValue = "";
			ZString validValueToTestWith = "0.15";
			DocumentNote documentNote = DocumentNote.LoadNote(Declaration);
			AssertEquals(expectedDefaultValue, DeclarationWrapper.OtherPrepaidCostsAmount);
			documentNote.SetSystemDefinedFieldValue(DocDeclaration.SDFields.AUPrefCertOfOrigin.OtherPrepaidCostsAmount, validValueToTestWith);
			AssertEquals(validValueToTestWith, DeclarationWrapper.OtherPrepaidCostsAmount);
		}
		#endregion

		#region ApplyWeightRoundingTests

		public void TestApplyWeightRounding_ShouldApplyOnTotalWeightInKG()
		{
			Declaration.JE_TotalWeight = 0.454;
			Declaration.JE_TotalWeightUnit = "KG";
			AssertEquals("After calling ApplyWeightRounding, 0.454 is round up to 1.", new ZDecimal(1), DeclarationWrapper.TotalWeightInKG);
		}

		public void TestApplyWeightRounding_ShouldApplyOnWeightIncludingUnit()
		{
			Declaration.JE_TotalWeight = 0.454;
			Declaration.JE_TotalWeightUnit = "KG";
			AssertEquals("After calling ApplyWeightRounding, 0.454 is round up to 1.", "1 KG", DeclarationWrapper.WeightIncludingUnit);
		}

		#endregion

		#region Implementation
		#region SetupPackages
		protected override void SetupPackages()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, new CodeDescriptionPair("BA", "Barrel"), new CodeDescriptionPair("BK", "Basket"));

			Bill houseBill1 = Declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "MYHOUSE1";
			PackingGroup packingGroup1 = houseBill1.PackingGroups.AddNew();
			Package package1 = packingGroup1.Packages.AddNew();
			package1.CW_PackQty = 15;
			package1.CW_PackType = "BA";

			Package package2 = packingGroup1.Packages.AddNew();
			package2.CW_PackQty = 25;
			package2.CW_PackType = "BK";
		}
		#endregion

		#region GetExpectedPackagesInfoString
		protected override ZString GetExpectedPackagesInfoString()
		{
			return new ZString("15 BA, 25 BK");
		}

		#endregion

		#endregion
	}
}
