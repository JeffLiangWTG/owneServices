using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(DEInputDocumentValueObjectDataAdapter))]
	sealed class DEInputDocumentValueObjectDataAdapterTest : EUInputDocumentValueObjectDataAdapterTest
	{
		[ExpectNoExceptions]
		public void TestImportDEMessageType()
		{
			XSD.InputDocument inputDocument = new XSD.InputDocument();
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsddeclaration = inputDocument.DeclarationList.AddNew();
			xsddeclaration.DeclarationHeader.DocumentRef = "REF";
			xsddeclaration.DeclarationHeader.DocumentSubRef = "001";
			xsddeclaration.DeclarationHeader.DeclarationType = "DEEXP";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			var entry = DEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "EXP";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "EXT";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "DEEXT";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "IMAVUV";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "DEIMAVUV";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "DESUMA";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "SUMA";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "DEZIA";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "ZIA";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			xsddeclaration.DeclarationHeader.DeclarationType = "DEIMWREM";
			dataAdapter.ImportFromValueObject(DEDeclaration, inputDocument, context);
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportDEDeclaration1()
		{
			DEDeclaration.JE_DeclarationReference = "DEResponseSampleXMLFile1";
			Factory.Save();
			DoImport("DEResponseSampleXMLFile1.xml", Core.Constants.CountryCodes.Germany);
			NUnit.Framework.Assert.That(DEDeclaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
			var entry = DEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"DEResponseSampleXMLFile1\001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MovementReferenceNumber, Is.EqualTo("ATC400000220720135875").Using(CustomComparers.TypeComparison));
			var entryLine = entry.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValue, Is.EqualTo(3000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.DutyAmount, Is.EqualTo(0.45m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.GSTVATAmount, Is.EqualTo(619.06m).Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportDEDeclaration2()
		{
			DEDeclaration.JE_DeclarationReference = "DEResponseSampleXMLFile2";
			Factory.Save();
			DoImport("DEResponseSampleXMLFile2.xml", Core.Constants.CountryCodes.Germany);
			NUnit.Framework.Assert.That(DEDeclaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
			var entry = DEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"DEResponseSampleXMLFile2\001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(3));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MovementReferenceNumber, Is.EqualTo("ATC400000250720135875").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CustomsValue, Is.EqualTo(134380m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.TotalDutyAmount, Is.EqualTo(20993.36m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.GSTAmount, Is.EqualTo(310028m).Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportDEDeclaration3()
		{
			DEDeclaration.JE_DeclarationReference = "DEResponseSampleXMLFile3";
			Factory.Save();
			DoImport("DEResponseSampleXMLFile3.xml", Core.Constants.CountryCodes.Germany);
			NUnit.Framework.Assert.That(DEDeclaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
			var entry = DEDeclaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"DEResponseSampleXMLFile3\001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
		}

		#region DE Declaration
		JobDeclaration DEDeclaration
		{
			get
			{
				return deDeclaration ?? (deDeclaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration deDeclaration;
		#endregion
		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Germany, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);
		}
		#endregion
	}
}
