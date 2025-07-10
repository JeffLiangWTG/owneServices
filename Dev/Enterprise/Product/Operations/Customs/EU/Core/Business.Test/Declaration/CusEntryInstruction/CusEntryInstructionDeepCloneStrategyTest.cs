using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryInstructionDeepCloneStrategyTest : TestCaseWithFactory
	{
		protected const string OriginalFactoryName = "OriginalFactory";
		protected const string CloneFactoryName = "CloneFactory";

		[ExpectNoExceptions]
		public void TestCusEntryInstructionDeepCloneStrategyClonesChildren()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var declaration = Factory.New<JobDeclaration>();
			var oldCEI = declaration.CustomsEntryInstructions.AddNew();
			var authorisation1 = oldCEI.CusAuthorizationUsages.AddNew();
			authorisation1.AGC_Code = "AAA";
			authorisation1.AGC_Number = "12345";

			var fiscalRef1 = oldCEI.FiscalReferences.AddNew();
			fiscalRef1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			fiscalRef1.CFR_Reference = "CCC";

			var supplyChainActorRef1 = oldCEI.CusSupplyChainActorReferences.AddNew();
			supplyChainActorRef1.CFR_Code = "FR1";
			supplyChainActorRef1.CFR_Reference = "EEE";

			var guarantee1 = oldCEI.Guarantees.AddNew();
			guarantee1.PW_ActivityCode = "AC1";
			guarantee1.PW_BondType = "A";
			guarantee1.PW_BondFiledPort = "Port1";
			guarantee1.PW_BondNumber = "BondNumber_1";
			guarantee1.PW_BondNumber2 = "BondNumber2_1";
			guarantee1.PW_SuretyCode = "SC1";
			guarantee1.PW_Password = "1234";
			guarantee1.PW_BondAmount = 1234;

			var supportingDocument1 = oldCEI.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "1001";
			supportingDocument1.CSI_ReferenceNumber = "SupportingDocument_1";
			supportingDocument1.CSI_Quantity = 3;
			supportingDocument1.CSI_UnitOfQuantity = "KG";
			supportingDocument1.CSI_Quantity2 = 2;
			supportingDocument1.CSI_UnitOfQuantity2 = "MTK";
			supportingDocument1.CSI_Value = 200;
			supportingDocument1.CSI_RX_NKCurrency = "EUR";
			supportingDocument1.CSI_AdditionalDescription = "Authority name";
			supportingDocument1.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument1.CSI_DateOfExpiry = ZDateTime.Today.AddMonths(1);
			supportingDocument1.CSI_ItemNumber = 1;

			var additionalDocument1 = oldCEI.AdditionalInfos.AddNew();
			additionalDocument1.CSI_SubType = "INF";
			additionalDocument1.CSI_Code = "00100";
			additionalDocument1.CSI_ReferenceNumber = "AdditionalDocument_1";
			additionalDocument1.CSI_Description = "Additional document 1";

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(oldCEI, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var newCEI = (CusEntryInstruction)cloneStrategy.Clone();

			AssertEquals("Authorisations count of clone should be the same as cloned entry instruction.", 1, newCEI.CusAuthorizationUsages.Count);
			CombineAssertions("EU strategy should clone the authorisations. Count is ok but info doesn't match.", () =>
			{
				AssertEquals("Authorization codes don't match.", oldCEI.CusAuthorizationUsages[0].AGC_Code, newCEI.CusAuthorizationUsages[0].AGC_Code);
				AssertEquals("Authorization numbers don't match.", oldCEI.CusAuthorizationUsages[0].AGC_Number, newCEI.CusAuthorizationUsages[0].AGC_Number);
				AssertEquals("Authorization owned by wrong factory.", CloneFactoryName, newCEI.CusAuthorizationUsages[0].Factory.NameForDebugging);
			});

			AssertEquals("Fiscal references count of clone should be the same as cloned entry instruction.", 1, newCEI.FiscalReferences.Count);
			CombineAssertions("EU strategy should clone the fiscal references. Count is ok but info doesn't match.", () =>
			{
				AssertEquals("Fiscal ref codes don't match.", oldCEI.FiscalReferences[0].CFR_Code, newCEI.FiscalReferences[0].CFR_Code);
				AssertEquals("Fiscal ref references don't match.", oldCEI.FiscalReferences[0].CFR_Reference, newCEI.FiscalReferences[0].CFR_Reference);
				AssertEquals("Fiscal ref owned by wrong factory.", CloneFactoryName, newCEI.FiscalReferences[0].Factory.NameForDebugging);
			});

			AssertEquals("SupplyChainActorReferences count of clone should be the same as cloned entry instruction.", 1, newCEI.CusSupplyChainActorReferences.Count);
			CombineAssertions("EU strategy should clone the SupplyChainActorReferences. Count is ok but info doesn't match.", () =>
			{
				AssertEquals("SupplyChainActorReferences codes don't match.", oldCEI.CusSupplyChainActorReferences[0].CFR_Code, newCEI.CusSupplyChainActorReferences[0].CFR_Code);
				AssertEquals("SupplyChainActorReferences references don't match.", oldCEI.CusSupplyChainActorReferences[0].CFR_Reference, newCEI.CusSupplyChainActorReferences[0].CFR_Reference);
				AssertEquals("SupplyChainActorReferences owned by wrong factory.", CloneFactoryName, newCEI.CusSupplyChainActorReferences[0].Factory.NameForDebugging);
			});

			AssertEquals("Guarantees count of clone should be the same as cloned entry instruction.", 1, newCEI.Guarantees.Count);
			CombineAssertions("EU strategy should clone the Guarantees. Count is ok but info doesn't match.", () =>
			{
				AssertEquals("PW_ActivityCode don't match.", oldCEI.Guarantees[0].PW_ActivityCode, newCEI.Guarantees[0].PW_ActivityCode);
				AssertEquals("PW_BondType don't match.", oldCEI.Guarantees[0].PW_BondType, newCEI.Guarantees[0].PW_BondType);
				AssertEquals("PW_BondFiledPort don't match.", oldCEI.Guarantees[0].PW_BondFiledPort, newCEI.Guarantees[0].PW_BondFiledPort);
				AssertEquals("PW_BondNumber don't match.", oldCEI.Guarantees[0].PW_BondNumber, newCEI.Guarantees[0].PW_BondNumber);
				AssertEquals("PW_BondNumber2 don't match.", oldCEI.Guarantees[0].PW_BondNumber2, newCEI.Guarantees[0].PW_BondNumber2);
				AssertEquals("PW_SuretyCode don't match.", oldCEI.Guarantees[0].PW_SuretyCode, newCEI.Guarantees[0].PW_SuretyCode);
				AssertEquals("PW_Password don't match.", oldCEI.Guarantees[0].PW_Password, newCEI.Guarantees[0].PW_Password);
				AssertEquals("PW_BondAmount don't match.", oldCEI.Guarantees[0].PW_BondAmount, newCEI.Guarantees[0].PW_BondAmount);
				AssertEquals("Guarantees owned by wrong factory.", CloneFactoryName, newCEI.Guarantees[0].Factory.NameForDebugging);
			});

			AssertEquals("Supporting Documents count of clone should be the same as cloned entry instruction.", 1, newCEI.SupportingDocuments.Count);
			CombineAssertions("EU strategy should clone the Supporting Documents. Count is ok but info doesn't match.", () =>
			{
				AssertEquals("CSI_Code don't match.", oldCEI.SupportingDocuments[0].CSI_Code, newCEI.SupportingDocuments[0].CSI_Code);
				AssertEquals("CSI_ReferenceNumber don't match.", oldCEI.SupportingDocuments[0].CSI_ReferenceNumber, newCEI.SupportingDocuments[0].CSI_ReferenceNumber);
				AssertEquals("CSI_Quantity don't match.", oldCEI.SupportingDocuments[0].CSI_Quantity, newCEI.SupportingDocuments[0].CSI_Quantity);
				AssertEquals("CSI_UnitOfQuantity don't match.", oldCEI.SupportingDocuments[0].CSI_UnitOfQuantity, newCEI.SupportingDocuments[0].CSI_UnitOfQuantity);
				AssertEquals("CSI_Quantity2 don't match.", oldCEI.SupportingDocuments[0].CSI_Quantity2, newCEI.SupportingDocuments[0].CSI_Quantity2);
				AssertEquals("CSI_UnitOfQuantity2 don't match.", oldCEI.SupportingDocuments[0].CSI_UnitOfQuantity2, newCEI.SupportingDocuments[0].CSI_UnitOfQuantity2);
				AssertEquals("CSI_Value don't match.", oldCEI.SupportingDocuments[0].CSI_Value, newCEI.SupportingDocuments[0].CSI_Value);
				AssertEquals("CSI_RX_NKCurrency don't match.", oldCEI.SupportingDocuments[0].CSI_RX_NKCurrency, newCEI.SupportingDocuments[0].CSI_RX_NKCurrency);
				AssertEquals("CSI_AdditionalDescription don't match.", oldCEI.SupportingDocuments[0].CSI_AdditionalDescription, newCEI.SupportingDocuments[0].CSI_AdditionalDescription);
				AssertEquals("CSI_DateOfIssue don't match.", oldCEI.SupportingDocuments[0].CSI_DateOfIssue, newCEI.SupportingDocuments[0].CSI_DateOfIssue);
				AssertEquals("CSI_DateOfExpiry don't match.", oldCEI.SupportingDocuments[0].CSI_DateOfExpiry, newCEI.SupportingDocuments[0].CSI_DateOfExpiry);
				AssertEquals("CSI_ItemNumber don't match.", oldCEI.SupportingDocuments[0].CSI_ItemNumber, newCEI.SupportingDocuments[0].CSI_ItemNumber);
			});

			AssertEquals("Additional Documents count of clone should be the same as cloned entry instruction.", 1, newCEI.AdditionalInfos.Count);
			CombineAssertions("EU strategy should clone the Additional Documents. Count is ok but info doesn't match.", () =>
			{
				AssertEquals("CSI_SubType don't match.", oldCEI.AdditionalInfos[0].CSI_SubType, newCEI.AdditionalInfos[0].CSI_SubType);
				AssertEquals("CSI_Code don't match.", oldCEI.AdditionalInfos[0].CSI_Code, newCEI.AdditionalInfos[0].CSI_Code);
				AssertEquals("CSI_ReferenceNumber don't match.", oldCEI.AdditionalInfos[0].CSI_ReferenceNumber, newCEI.AdditionalInfos[0].CSI_ReferenceNumber);
				AssertEquals("CSI_Description don't match.", oldCEI.AdditionalInfos[0].CSI_Description, newCEI.AdditionalInfos[0].CSI_Description);
			});
		}

		[ExpectNoExceptions]
		public void TestCusEntryInstructionDeepCloneStrategyClonesAuthorizations()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var oldCEI = Factory.New<CusEntryInstruction>();

			var authorisation1 = oldCEI.CusAuthorizationUsages.AddNew();
			authorisation1.AGC_Code = "AAA";
			authorisation1.AGC_Number = "12345";
			var authorisation2 = oldCEI.CusAuthorizationUsages.AddNew();
			authorisation2.AGC_Code = "BBB";
			authorisation2.AGC_Number = "67890";

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(oldCEI, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var newCEI = (CusEntryInstruction)cloneStrategy.Clone();

			AssertEquals("Authorisations count of clone should be the same as cloned entry instruction.", 2, newCEI.CusAuthorizationUsages.Count);
			for (int i = 0; i < 2; i++)
			{
				CombineAssertions("EU strategy should clone the authorisations. Count is ok but info doesn't match.", () =>
				{
					AssertEquals("Authorization codes don't match.", oldCEI.CusAuthorizationUsages[i].AGC_Code, newCEI.CusAuthorizationUsages[i].AGC_Code);
					AssertEquals("Authorization numbers don't match.", oldCEI.CusAuthorizationUsages[i].AGC_Number, newCEI.CusAuthorizationUsages[i].AGC_Number);
					AssertEquals("Authorization owned by wrong factory.", CloneFactoryName, newCEI.CusAuthorizationUsages[i].Factory.NameForDebugging);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestCusEntryInstructionDeepCloneStrategyClonesFiscalReferences()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var oldCEI = Factory.New<CusEntryInstruction>();

			var fiscalRef1 = oldCEI.FiscalReferences.AddNew();
			fiscalRef1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			fiscalRef1.CFR_Reference = "CCC";
			var fiscalRef2 = oldCEI.FiscalReferences.AddNew();
			fiscalRef2.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalRef2.CFR_Reference = "DDD";

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(oldCEI, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var newCEI = (CusEntryInstruction)cloneStrategy.Clone();

			AssertEquals("Fiscal references count of clone should be the same as cloned entry instruction.", 2, newCEI.FiscalReferences.Count);
			for (int i = 0; i < 2; i++)
			{
				CombineAssertions("EU strategy should clone the Fiscal References. Count is ok but info doesn't match.", () =>
				{
					AssertEquals("Fiscal ref codes don't match.", oldCEI.FiscalReferences[i].CFR_Code, newCEI.FiscalReferences[i].CFR_Code);
					AssertEquals("Fiscal ref references don't match.", oldCEI.FiscalReferences[i].CFR_Reference, newCEI.FiscalReferences[i].CFR_Reference);
					AssertEquals("Fiscal ref owned by wrong factory.", CloneFactoryName, newCEI.FiscalReferences[i].Factory.NameForDebugging);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestCusEntryInstructionDeepCloneStrategyClonesCusSupplyChainActorReferences()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var oldCEI = Factory.New<CusEntryInstruction>();

			var supplyChainActorRef1 = oldCEI.CusSupplyChainActorReferences.AddNew();
			supplyChainActorRef1.CFR_Code = "FR1";
			supplyChainActorRef1.CFR_Reference = "EEE";
			var supplyChainActorRef2 = oldCEI.CusSupplyChainActorReferences.AddNew();
			supplyChainActorRef2.CFR_Code = "FR2";
			supplyChainActorRef2.CFR_Reference = "FFF";

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(oldCEI, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var newCEI = (CusEntryInstruction)cloneStrategy.Clone();

			AssertEquals("SupplyChainActorReferences count of clone should be the same as cloned entry instruction.", 2, newCEI.CusSupplyChainActorReferences.Count);
			for (int i = 0; i < 2; i++)
			{
				CombineAssertions("EU strategy should clone the SupplyChainActorReferences. Count is ok but info doesn't match.", () =>
				{
					AssertEquals("SupplyChainActorReferences codes don't match.", oldCEI.CusSupplyChainActorReferences[i].CFR_Code, newCEI.CusSupplyChainActorReferences[i].CFR_Code);
					AssertEquals("SupplyChainActorReferences references don't match.", oldCEI.CusSupplyChainActorReferences[i].CFR_Reference, newCEI.CusSupplyChainActorReferences[i].CFR_Reference);
					AssertEquals("SupplyChainActorReferences owned by wrong factory.", CloneFactoryName, newCEI.CusSupplyChainActorReferences[i].Factory.NameForDebugging);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestCusEntryInstructionDeepCloneStrategyClonesGuarantees()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var oldCEI = Factory.New<CusEntryInstruction>();

			var guarantee1 = oldCEI.Guarantees.AddNew();
			guarantee1.PW_ActivityCode = "AC1";
			guarantee1.PW_BondType = "A";
			guarantee1.PW_BondFiledPort = "Port1";
			guarantee1.PW_BondNumber = "BondNumber_1";
			guarantee1.PW_BondNumber2 = "BondNumber2_1";
			guarantee1.PW_SuretyCode = "SC1";
			guarantee1.PW_Password = "1234";
			guarantee1.PW_BondAmount = 1234;
			var guarantee2 = oldCEI.Guarantees.AddNew();
			guarantee2.PW_ActivityCode = "AC2";
			guarantee2.PW_BondType = "B";
			guarantee2.PW_BondFiledPort = "Port2";
			guarantee2.PW_BondNumber = "BondNumber_2";
			guarantee2.PW_BondNumber2 = "BondNumber2_2";
			guarantee2.PW_SuretyCode = "SC2";
			guarantee2.PW_Password = "5678";
			guarantee2.PW_BondAmount = 5678;

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(oldCEI, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var newCEI = (CusEntryInstruction)cloneStrategy.Clone();

			AssertEquals("Guarantees count of clone should be the same as cloned entry instruction.", 2, newCEI.Guarantees.Count);
			for (int i = 0; i < 2; i++)
			{
				CombineAssertions("EU strategy should clone the Guarantees. Count is ok but info doesn't match.", () =>
				{
					AssertEquals("PW_ActivityCode don't match.", oldCEI.Guarantees[i].PW_ActivityCode, newCEI.Guarantees[i].PW_ActivityCode);
					AssertEquals("PW_BondType don't match.", oldCEI.Guarantees[i].PW_BondType, newCEI.Guarantees[i].PW_BondType);
					AssertEquals("PW_BondFiledPort don't match.", oldCEI.Guarantees[i].PW_BondFiledPort, newCEI.Guarantees[i].PW_BondFiledPort);
					AssertEquals("PW_BondNumber don't match.", oldCEI.Guarantees[i].PW_BondNumber, newCEI.Guarantees[i].PW_BondNumber);
					AssertEquals("PW_BondNumber2 don't match.", oldCEI.Guarantees[i].PW_BondNumber2, newCEI.Guarantees[i].PW_BondNumber2);
					AssertEquals("PW_SuretyCode don't match.", oldCEI.Guarantees[i].PW_SuretyCode, newCEI.Guarantees[i].PW_SuretyCode);
					AssertEquals("PW_Password don't match.", oldCEI.Guarantees[i].PW_Password, newCEI.Guarantees[i].PW_Password);
					AssertEquals("PW_BondAmount don't match.", oldCEI.Guarantees[i].PW_BondAmount, newCEI.Guarantees[i].PW_BondAmount);
					AssertEquals("Guarantees owned by wrong factory.", CloneFactoryName, newCEI.Guarantees[i].Factory.NameForDebugging);
				});
			}
		}

		public void TestCloneEntryInstruction()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_SealsCount = 50;

			Factory.Save();

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(entryInstruction, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var cloneEntryInstruction = (CusEntryInstruction)cloneStrategy.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("[PREREQ] ZG_SealsCount of the original entryInstruction", 50, entryInstruction.ZG_SealsCount);
				AssertEquals("Test for no Clone ZG_SealsCount when cloning entryInstruction", ZInt.Zero, cloneEntryInstruction.ZG_SealsCount);
			});
		}

		public void TestCopyGoodsLocation()
		{
			Factory.NameForDebugging = OriginalFactoryName;
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.GoodsLocation.CGL_Qualifier = "Z";
			entryInstruction.GoodsLocation.Address.E2_Address1AndE2_Address2 = "Address1";

			Factory.Save();

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(entryInstruction, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			var cloneEntryInstruction = (CusEntryInstruction)cloneStrategy.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("GoodsLocation.CGL_Qualifier of the original entryInstruction", "Z", cloneEntryInstruction.GoodsLocation.CGL_Qualifier);
				AssertEquals("GoodsLocation.Address.E2_Address1AndE2_Address2 of the original entryInstruction", "Address1", cloneEntryInstruction.GoodsLocation.Address.E2_Address1AndE2_Address2);
				AssertEquals("GoodsLocation owned by wrong factory.", CloneFactoryName, cloneEntryInstruction.GoodsLocation.Factory.NameForDebugging);
			});
		}

		public void TestCopyGoodsLocationWhenGoodLocationIsNull()
		{
			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstructionForTest>();
			Factory.Save();

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			alternativeFactoryToInstantiateCloneIn.NameForDebugging = CloneFactoryName;

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(entryInstruction, Customs.Business.CloneType.TemplateCopy, alternativeFactoryToInstantiateCloneIn);
			AssertNoExceptionThrown("WI00671740 - No Null exception.", () => cloneStrategy.Clone());

			var cloneEntryInstruction = (CusEntryInstruction)cloneStrategy.Clone();
			AssertNotNull("entryInstruction has been cloned even if there is no GoodsLocation", cloneEntryInstruction);
		}

		protected class CusEntryInstructionForTest : CusEntryInstruction
		{
			public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
			protected override CusGoodsLocation GetGoodsLocation() => null;
		}
	}
}
