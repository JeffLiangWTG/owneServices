using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.Mexico, GetJobDeclaration().LocalCurrencyCode);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("MX Declaration should support EntryInstructions", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertType<BillCollection<Bill, JobDeclaration>>(declaration.Bills);
		}

		public void TestLookupObjectIsCached()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = declaration.Lookups;
			var secondLookup = declaration.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestDefaultJE_ApplicationCode()
		{
			AssertEquals("Builtin JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);

			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeList.Codes.Builtin };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("Builtin JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("BothBuiltInDefaulted JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("Interfaced JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("BothInterfaceDefaulted JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
		}

		public void TestDefaultCustomsRegimeFromDeclarationType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();

			#region Import

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			foreach (var messageSubType in ReferenceTestDataHelper.DefaultCustomsRegimeImport)
			{
				declaration.JE_MessageSubType = messageSubType.Key;

				CombineAssertions($"JE_MessageSubType={messageSubType.Key}", () =>
				{
					AssertEquals("JE_CustomsProfile should be", messageSubType.Value, declaration.JE_CustomsProfile);
				});
			}

			#endregion

			#region Export

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			foreach (var messageSubType in ReferenceTestDataHelper.DefaultCustomsRegimeExport)
			{
				declaration.JE_MessageSubType = messageSubType.Key;

				CombineAssertions($"JE_MessageSubType={messageSubType.Key}", () =>
				{
					AssertEquals("JE_CustomsProfile should be", messageSubType.Value, declaration.JE_CustomsProfile);
				});
			}

			#endregion
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_GoodsOrigin = GoodsRegionList.Codes._5;
			declaration.JE_GoodsDestination = GoodsRegionList.Codes._8;
			declaration.OnSaving();
			Factory.Save();
			AssertEquals("JE_GoodsOrigin NOT Empty", GoodsRegionList.Codes._5, declaration.JE_GoodsOrigin);
			Assert("JE_GoodsDestination Empty", declaration.JE_GoodsDestination.IsEmpty);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GoodsOrigin = GoodsRegionList.Codes._5;
			declaration.JE_GoodsDestination = GoodsRegionList.Codes._8;
			declaration.OnSaving();
			Assert("JE_GoodsOrigin Empty", declaration.JE_GoodsOrigin.IsEmpty);
			AssertEquals("JE_GoodsDestination NOT Empty", GoodsRegionList.Codes._8, declaration.JE_GoodsDestination);
		}

		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var decAsProvider = declaration as ICurrencyConverterDataProvider;
			AssertEquals("JE_MessageType = IMP, Rate Type should be", ZArchitecture.Core.ExchangeRateType.Customs, decAsProvider.RateType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageType = EXP, Rate Type should be", ZArchitecture.Core.ExchangeRateType.CustomsSecondary, decAsProvider.RateType);
		}

		public void TestMultipleKeysToUse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var multipleKeySupport = (ISupportMultipleResourceStringData)declaration;
			AssertSequencesEqual($"JE_MessageType={declaration.JE_MessageType}", new[] { JobMessageTypeList.Codes.Export }, multipleKeySupport.MultipleKeysToUse);
		}

		public void TestCreateNewDocumentSupporter()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType(typeof(JobDeclarationDocumentSupporter), dec.DocumentSupporter);
		}
	}
}

