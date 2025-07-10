using System.Linq;
using Enterprise.Customs.CustomsWare.Business.XSD;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(EUInputDocumentValueObjectDataAdapter))]
	class EUInputDocumentValueObjectDataAdapterTest : InputDocumentValueObjectDataAdapterTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportBEDeclaration()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, EUDeclaration.CountryCode, Universal.Constants.RateTypes.Duty, Customs.Business.FeeTypeList.Codes.A00);
			EUDeclaration.JE_DeclarationReference = "BEResponseSampleXMLFile1";
			Factory.Save();
			DoImport("BEResponseSampleXMLFile1.xml", Core.Constants.CountryCodes.Belgium);
			NUnit.Framework.Assert.That(EUDeclaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(cei.CEI_SubStyle, Is.EqualTo("A").Using(CustomComparers.TypeComparison), "EUDeclaration CusEntryInstruction has a SubStyle value");
			var entry = EUDeclaration.ActiveEntryHeaders[0];
			entry.CH_CEI_Instruction = cei.PK;
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"BEResponseSampleXMLFile1\016").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.EntryInstruction.CEI_SubStyle, Is.EqualTo("A").Using(CustomComparers.TypeComparison), "EntryInstruction has a SubStyle value");
			var entryLine = entry.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine.DutyAmount, Is.EqualTo(2.16m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.GSTVATAmount, Is.EqualTo(280.8m).Using(CustomComparers.TypeComparison));
			entryLine = entry.MergedLines[1];
			NUnit.Framework.Assert.That(entryLine.DutyAmount, Is.EqualTo(0.05m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.GSTVATAmount, Is.EqualTo(0.17m).Using(CustomComparers.TypeComparison));
			DoImport("BEResponseSampleXMLFile1.xml", Core.Constants.CountryCodes.Belgium);
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestImportNLMessageType()
		{
			InputDocument inputDocument = new InputDocument();
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsddeclaration = inputDocument.DeclarationList.AddNew();
			xsddeclaration.DeclarationHeader.DocumentRef = "REF";
			xsddeclaration.DeclarationHeader.DocumentSubRef = "001";
			xsddeclaration.DeclarationHeader.DeclarationType = "NLNCTSDEP";
			dataAdapter.ImportFromValueObject(EUDeclaration, inputDocument, context);
			var entry = (CusEntryHeader)EUDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("DEP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "NLSAGIMP";
			dataAdapter.ImportFromValueObject(EUDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "NLSAGEMP";
			dataAdapter.ImportFromValueObject(EUDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EMP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImportCHMessageType()
		{
			InputDocument inputDocument = new InputDocument();
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsddeclaration = inputDocument.DeclarationList.AddNew();
			xsddeclaration.DeclarationHeader.DocumentRef = "REF";
			xsddeclaration.DeclarationHeader.DocumentSubRef = "001";
			xsddeclaration.DeclarationHeader.DeclarationType = "CHEDECEX";
			dataAdapter.ImportFromValueObject(EUDeclaration, inputDocument, context);
			var entry = (CusEntryHeader)EUDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "CHEDECIM";
			dataAdapter.ImportFromValueObject(EUDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImport_()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				InputDocument inputDocument = new InputDocument();
				var xsddeclaration = inputDocument.DeclarationList.AddNew();
				var xsdReference = xsddeclaration.DeclarationHeader.Reference.AddNew();
				xsdReference.RefCode = "UCR";
				xsdReference.RefText = "UCRTEST";
				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				dataAdapter.ImportFromValueObject(EUDeclaration, inputDocument, context);
				var entry = (CusEntryHeader)EUDeclaration.ActiveEntryHeaders[0];
				NUnit.Framework.Assert.That(entry.DeclarationUCR, Is.EqualTo("UCRTEST").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestExport_()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				EUDeclaration.JE_TransportModeInland = "RAI";
				EUDeclaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Albania;
				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				var context = new ValueObjectExportContext(new NotificationBuffer());
				var result = dataAdapter.ExportToValueObject(EUDeclaration, context);
				NUnit.Framework.Assert.That(result.ConsignmentList.Consignment[0].ConsignmentHeader.Transport.Count, Is.EqualTo(1));
				var transport = result.ConsignmentList.Consignment[0].ConsignmentHeader.Transport[0];
				NUnit.Framework.Assert.That(transport.TransportType, Is.EqualTo(TransportTransportType.Inland));
				NUnit.Framework.Assert.That(transport.TPMode.Text[0], Is.EqualTo("2"));
				NUnit.Framework.Assert.That(transport.ConveyanceNat.Text[0], Is.EqualTo(Core.Constants.CountryCodes.Albania));
				NUnit.Framework.Assert.That(transport.TPMode.CodeType, Is.EqualTo(TPModeCodeType.NUM));
			}
		}

		[ExpectNoExceptions]
		public void TestExportEORI()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Sweden))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_FullName = "importer Full Name";
				importer.MainAddress.OA_Address1 = "importer Address1";
				importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
				EUDeclaration.JE_OH_Importer = importer.PK;
				importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "CONSIGNEEVAT", Core.Constants.CountryCodes.Sweden);
				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				var context = new ValueObjectExportContext(new NotificationBuffer());
				var result = dataAdapter.ExportToValueObject(EUDeclaration, context);
				var consigneeParty = result.ConsignmentList.Consignment[0].ConsignmentHeader.Party.Cast<Party>().FirstOrDefault(x => x.PartyType == "Consignee");
				NUnit.Framework.Assert.That(consigneeParty, Is.Not.EqualTo(default(Party)));
				var eori = consigneeParty.Reference.Cast<Reference>().FirstOrDefault(x => x.RefCode == "EORI");
				NUnit.Framework.Assert.That(eori, Is.Not.EqualTo(default(Reference)));
				NUnit.Framework.Assert.That(eori.RefText, Is.EqualTo("SECONSIGNEEVAT").Using(CustomComparers.TypeComparison));
				importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "CONSIGNEEEORI", Core.Constants.CountryCodes.Sweden);
				result = dataAdapter.ExportToValueObject(EUDeclaration, context);
				consigneeParty = result.ConsignmentList.Consignment[0].ConsignmentHeader.Party.Cast<Party>().FirstOrDefault(x => x.PartyType == "Consignee");
				NUnit.Framework.Assert.That(consigneeParty, Is.Not.EqualTo(default(Party)));
				eori = consigneeParty.Reference.Cast<Reference>().FirstOrDefault(x => x.RefCode == "EORI");
				NUnit.Framework.Assert.That(eori, Is.Not.EqualTo(default(Reference)));
				NUnit.Framework.Assert.That(eori.RefText, Is.EqualTo("SECONSIGNEEEORI").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestExportBTW()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_FullName = "importer Full Name";
				importer.MainAddress.OA_Address1 = "importer Address1";
				importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
				EUDeclaration.JE_OH_Importer = importer.PK;
				importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "CONSIGNEEBTW", Core.Constants.CountryCodes.Belgium);
				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				var context = new ValueObjectExportContext(new NotificationBuffer());
				var result = dataAdapter.ExportToValueObject(EUDeclaration, context);
				var consigneeParty = result.ConsignmentList.Consignment[0].ConsignmentHeader.Party.Cast<Party>().FirstOrDefault(x => x.PartyType == "Consignee");
				NUnit.Framework.Assert.That(consigneeParty, Is.Not.EqualTo(default(Party)));
				var btw = consigneeParty.Reference.Cast<Reference>().FirstOrDefault(x => x.RefCode == "BTW");
				NUnit.Framework.Assert.That(btw, Is.Not.EqualTo(default(Reference)));
				NUnit.Framework.Assert.That(btw.RefText, Is.EqualTo("BECONSIGNEEBTW").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestExportVAT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Sweden))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_FullName = "importer Full Name";
				importer.MainAddress.OA_Address1 = "importer Address1";
				importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
				importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "CONSIGNEEVAT", Core.Constants.CountryCodes.Sweden);
				EUDeclaration.JE_OH_Importer = importer.PK;
				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				var context = new ValueObjectExportContext(new NotificationBuffer());
				var result = dataAdapter.ExportToValueObject(EUDeclaration, context);
				var consigneeParty = result.ConsignmentList.Consignment[0].ConsignmentHeader.Party.Cast<Party>().FirstOrDefault(x => x.PartyType == "Consignee");
				NUnit.Framework.Assert.That(consigneeParty, Is.Not.EqualTo(default(Party)));
				var vat = consigneeParty.Reference.Cast<Reference>().FirstOrDefault(x => x.RefCode == "VAT");
				NUnit.Framework.Assert.That(vat, Is.Not.EqualTo(default(Reference)));
				NUnit.Framework.Assert.That(vat.RefText, Is.EqualTo("SECONSIGNEEVAT").Using(CustomComparers.TypeComparison));
			}
		}

		#region EU Declaration
		JobDeclaration EUDeclaration
		{
			get
			{
				if (euDeclaration == null)
				{
					euDeclaration = Factory.New<JobDeclaration>();
				}

				if (cei == null)
				{
					cei = euDeclaration.CustomsEntryInstructions.AddNew();
				}

				return euDeclaration;
			}
		}

		JobDeclaration euDeclaration;
		CusEntryInstruction cei;
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Latvia);
		}
	}
}
