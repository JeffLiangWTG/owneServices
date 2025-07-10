using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(EntryLineConfiguration))]
	public abstract class EntryLineConfigurationAbstractTest<L> : TestCaseWithFactory
		where L : EntryLineConfiguration
	{
		[ExpectNoExceptions]
		public void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(ExpectedSupportingDocumentsSupportResult).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMergeJI_RN_NKCountryOfExport()
		{
			NUnit.Framework.Assert.That(configuration.MergeJI_RN_NKCountryOfExport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(ExpectedMergeJI_RN_NKCountryOfExportResult).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestShouldFilterSupportingDocumentsByMergeKeys()
		{
			NUnit.Framework.Assert.That(configuration.ShouldFilterSupportingDocumentsByMergeKeys(), NUnit.Framework.Is.EqualTo(ExpectedShouldFilterSupportingDocumentsByMergeKeys).Using(CustomComparers.TypeComparison));
		}

		protected virtual bool ExpectedSupportingDocumentsSupportResult => false;

		protected virtual bool ExpectedMergeJI_RN_NKCountryOfExportResult => false;

		protected virtual bool ExpectedShouldFilterSupportingDocumentsByMergeKeys => true;

		protected virtual JobDeclaration CreateDeclaration() => Factory.New<JobDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (L)Activator.CreateInstance(typeof(L));
		}
		protected L configuration;
	}

	[TestedType(typeof(EntryLineConfiguration))]
	sealed class EntryLineConfigurationBaseTest : EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryLine), NUnit.Framework.Is.EqualTo(default(IEntryLineValidationDecider)));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryLine), NUnit.Framework.Is.EqualTo(default(IEntryLineValidationDecider)));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryLine), NUnit.Framework.Is.EqualTo(default(IEntryLineValidationDecider)));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryLine), NUnit.Framework.Is.EqualTo(default(IEntryLineValidationDecider)));
			}
		}
	}
}
