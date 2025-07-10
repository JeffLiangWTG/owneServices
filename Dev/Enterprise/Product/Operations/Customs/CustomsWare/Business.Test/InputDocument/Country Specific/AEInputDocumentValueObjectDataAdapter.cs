using System.Linq;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(AEInputDocumentValueObjectDataAdapter))]
	sealed class AEInputDocumentValueObjectDataAdapterTest : InputDocumentValueObjectDataAdapterTest
	{
		[ExpectNoExceptions]
		public void TestExportCCD()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedArabEmirates))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_FullName = "importer Full Name";
				importer.MainAddress.OA_Address1 = "importer Address1";
				importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;

				importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "1234567AUCCD", Core.Constants.CountryCodes.Australia);
				importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "1234567AECCD", Core.Constants.CountryCodes.UnitedArabEmirates);

				var dataAdapter = InputDocumentValueObjectDataAdapter.New();
				NUnit.Framework.Assert.That(dataAdapter is ICustomsWareAEInputDocumentValueObjectDataAdapter, Is.True);
				var context = new ValueObjectExportContext(new NotificationBuffer());
				var result = dataAdapter.ExportToValueObject(declaration, context);
				var consigneeParty = result.ConsignmentList.Consignment[0].ConsignmentHeader.Party.Cast<XSD.Party>().FirstOrDefault(x => x.PartyType == "Consignee");
				NUnit.Framework.Assert.That(consigneeParty, Is.Not.EqualTo(default(XSD.Party)));

				var ccd = consigneeParty.Reference.Cast<XSD.Reference>().FirstOrDefault(x => x.RefCode == "CCD");
				NUnit.Framework.Assert.That(ccd, Is.Not.EqualTo(default(XSD.Reference)));
				NUnit.Framework.Assert.That(ccd.RefText, Is.EqualTo("1234567AECCD").Using(CustomComparers.TypeComparison));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportAEDeclaration()
		{
			AEDeclaration.JE_DeclarationReference = "AEResponseSampleXMLFile1";
			Factory.Save();

			DoImport("AEResponseSampleXMLFile1.xml", Core.Constants.CountryCodes.UnitedArabEmirates);

			NUnit.Framework.Assert.That(AEDeclaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
			var entry = AEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"AEResponseSampleXMLFile1\001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));

			DoImport("AEResponseSampleXMLFile1.xml", Core.Constants.CountryCodes.UnitedArabEmirates);
			NUnit.Framework.Assert.That(AEDeclaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestImportAEMessageType()
		{
			XSD.InputDocument inputDocument = new XSD.InputDocument();
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var xsddeclaration = inputDocument.DeclarationList.AddNew();
			xsddeclaration.DeclarationHeader.DocumentRef = "REF";
			xsddeclaration.DeclarationHeader.DocumentSubRef = "001";
			xsddeclaration.DeclarationHeader.DeclarationType = "AEMIRSAL2";
			xsddeclaration.DeclarationHeader.DeclarationInfo.DeclarationRegime = 1;

			dataAdapter.ImportFromValueObject(AEDeclaration, inputDocument, context);

			var entry = AEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));

			xsddeclaration.DeclarationHeader.DeclarationInfo.DeclarationRegime = 4;
			dataAdapter.ImportFromValueObject(AEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));

			xsddeclaration.DeclarationHeader.DeclarationInfo.DeclarationRegime = 2;
			dataAdapter.ImportFromValueObject(AEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));

			xsddeclaration.DeclarationHeader.DeclarationInfo.DeclarationRegime = 3;
			dataAdapter.ImportFromValueObject(AEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("TRA").Using(CustomComparers.TypeComparison));

			xsddeclaration.DeclarationHeader.DeclarationInfo.DeclarationRegime = 3;
			dataAdapter.ImportFromValueObject(AEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("TRA").Using(CustomComparers.TypeComparison));
		}

		#region AE Declaration

		JobDeclaration AEDeclaration
		{
			get { return aeDeclaration ?? (aeDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration aeDeclaration;

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedArabEmirates);
		}

		#endregion
	}
}
