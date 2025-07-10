using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(SEInputDocumentValueObjectDataAdapter))]
	sealed class SEInputDocumentValueObjectDataAdapterTest : EUInputDocumentValueObjectDataAdapterTest
	{
		[ExpectNoExceptions]
		public void TestImportSEMessageType()
		{
			XSD.InputDocument inputDocument = new XSD.InputDocument();
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsddeclaration = inputDocument.DeclarationList.AddNew();
			xsddeclaration.DeclarationHeader.DocumentRef = "REF";
			xsddeclaration.DeclarationHeader.DocumentSubRef = "001";
			xsddeclaration.DeclarationHeader.DeclarationType = "SEDNU";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			var entry = SEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEDRT";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEDBK";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEDNK";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEHNU";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEHNK";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEHRT";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEHBK";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SETNU";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SETRT";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEALI";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SETQN";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEUNU";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEURT";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SEUGE";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SENCTSDEP";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("NCT").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SENCTSARR";
			dataAdapter.ImportFromValueObject(SEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("NCT").Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportSEDeclaration()
		{
			SEDeclaration.JE_DeclarationReference = "S700051281";
			Factory.Save();
			DoImport("SEResponseSampleXMLFile1.xml", Core.Constants.CountryCodes.Sweden);
			NUnit.Framework.Assert.That(SEDeclaration.JE_EntryStyle, Is.EqualTo("IM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(cei.CEI_SubStyle, Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SEDeclaration.JE_LocationOfGoods, Is.EqualTo("AZZ").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SEDeclaration.JE_TransportModeInland, Is.EqualTo("SEA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(SEDeclaration.JE_RN_NKTransportNationality, Is.EqualTo("SG").Using(CustomComparers.TypeComparison));
			var entry = SEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"S700051281\001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			var entryNum = entry.CusEntryNumber;
			NUnit.Framework.Assert.That(entryNum.CE_EntryNum, Is.EqualTo("IRE1000181").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryNum.CE_EntryType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			var entryLine = entry.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValue, Is.EqualTo(1099636m).Using(CustomComparers.TypeComparison));
		}

		#region SE Declaration
		JobDeclaration SEDeclaration
		{
			get
			{
				return seDeclaration ?? (seDeclaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration seDeclaration;
		#endregion
		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Sweden);
			cei = SEDeclaration.CustomsEntryInstructions.AddNew();
		}

		CusEntryInstruction cei;
		#endregion
	}
}
