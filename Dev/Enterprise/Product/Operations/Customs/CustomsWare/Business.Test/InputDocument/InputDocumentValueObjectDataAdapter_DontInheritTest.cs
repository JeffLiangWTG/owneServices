using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(InputDocumentValueObjectDataAdapter))]
	sealed class InputDocumentValueObjectDataAdapter_DontInheritTest : BaseInputDocumentValueObjectDataAdapterTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportDeclaration()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Declaration.CountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);
			Declaration.JE_DeclarationReference = "CHResponseSampleXMLFile1";
			Factory.Save();
			DoImport("CHResponseSampleXMLFile1.xml", CountryCodes.Switzerland);
			NUnit.Framework.Assert.That(Declaration.ActiveEntryHeaders.Count, Is.EqualTo(1));
			var entry = Declaration.ActiveEntryHeaders[0];
			NUnit.Framework.Assert.That(entry.CH_BGMReference, Is.EqualTo(@"CHResponseSampleXMLFile1\001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(entry.CH_MessageType, Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.MovementReferenceNumber, Is.EqualTo("2262435").Using(CustomComparers.TypeComparison));
			DoImport("CHResponseSampleXMLFile1.xml", CountryCodes.Switzerland);
			NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(1));
			var entryLine = entry.MergedLines[0];
			NUnit.Framework.Assert.That(entryLine.CL_CustomsValue, Is.EqualTo(51000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.DutyAmount, Is.EqualTo(4135.03m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryLine.GSTVATAmount, Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			DoImport("CHResponseSampleXMLFile2.xml", CountryCodes.Switzerland);
			NUnit.Framework.Assert.That(Declaration.ActiveEntryHeaders.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(Declaration.ActiveEntryHeaders[1].CH_MessageType, Is.EqualTo("EXP").Using(CustomComparers.TypeComparison));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestCusEntryLineFees()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Netherlands))
			{
				Declaration.JE_DeclarationReference = "NLResponseSampleXMLFile1";
				DoImport("NLResponseSampleXMLFile1.xml", CountryCodes.Netherlands);
				var entry = Declaration.ActiveEntryHeaders[0];
				NUnit.Framework.Assert.That(entry.MergedLines.Count, Is.EqualTo(1));
				var line = entry.MergedLines[0];
				NUnit.Framework.Assert.That(line.Fees.Count, Is.EqualTo(1));
				var fee = line.Fees.OfType<CusEntryLineFee>().FirstOrDefault(f => f.CF_ChargeType == "A00");
				NUnit.Framework.Assert.That(fee, Is.Not.EqualTo(default(CusEntryLineFee)));
				NUnit.Framework.Assert.That(fee.CF_ChargeAmount, Is.EqualTo(496.94m).Using(CustomComparers.TypeComparison));
			}
		}
	}
}
