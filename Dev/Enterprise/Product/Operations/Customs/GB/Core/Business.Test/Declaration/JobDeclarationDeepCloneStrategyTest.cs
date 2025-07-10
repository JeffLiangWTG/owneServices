using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class JobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestApplicationExtender_ResetBeforeInvoiceCloning()
		{
			using (GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DeclarationApplicationCodeExports, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
				{
					var declaration = Factory.New<JobDeclaration>();
					AssertEquals("PreCondition: Default ApplicationCode should be CDS for this test to be valid", "CDS", declaration.JE_ApplicationCode);
					declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
					declaration.Invoices.AddNew()
						.InvoiceLines.AddNew();

					var strategy = new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory);
					var clonedDeclaration = (JobDeclaration)strategy.Clone(new BusinessObjectCloneArgs(Array.Empty<string>(), true));
					AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>("If ApplicationExtender is reset prior to Invoice cloning then we'll have the right type of CustomsUnitDefaultingStrategy", typeof(BaseJobComInvoiceLine).GetProperty("CustomsUnitDefaultingStrategy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(clonedDeclaration.InvoiceLines[0]));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestJobDeclarationDeepCloneStrategyTesterDeclarationsDirectChildren()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, false))
			{
				var oldDec = Factory.New<JobDeclaration>();
				oldDec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var entryIns = oldDec.CustomsEntryInstructions[0];
				entryIns.CEI_Style = "H3";
				entryIns.CEI_SubStyle = "D";
				entryIns.CEI_Description = "DEC FOR TEMPORARY ADMISSION";
				entryIns.CEI_SplitReference = "77";
				entryIns.CEI_PackageCount = 5;
				var invoice = oldDec.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryIns.PK;

				var guarantee = oldDec.Guarantees.AddNew();
				guarantee.PW_BondType = "A";
				guarantee.PW_Password = "ACE";
				guarantee.PW_BondAmount = 435m;
				guarantee.PW_RX_NKCurrency = "AUD";
				guarantee.PW_ValidityLimitation = "IS";
				guarantee.PW_BondFiledPort = "AD000003";
				guarantee.PW_HolderIdentification = "GB945390992000";
				guarantee.PW_BondNumber = "test";
				guarantee.PW_BondNumber2 = "test2";
				guarantee.EntryInstructionID = entryIns.PK;
				Factory.Save();

				var strategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy, oldDec.Factory);
				var newDecCloned = (JobDeclaration)strategy.Clone(new BusinessObjectCloneArgs(Array.Empty<string>(), true));
				var newEntryIns = newDecCloned.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault();
				AssertNotNull("An Entry Instruction has been added", newEntryIns);
				AssertEquals(newEntryIns.PK, newDecCloned.InvoiceLines[0].JI_CEI);
				AssertEquals("H3", newEntryIns.CEI_Style);
				AssertEquals("D", newEntryIns.CEI_SubStyle);
				AssertEquals("DEC FOR TEMPORARY ADMISSION", newEntryIns.CEI_Description);
				AssertEquals("77", newEntryIns.CEI_SplitReference);
				AssertEquals(5, newEntryIns.CEI_PackageCount);
				var newGuarantee = newDecCloned.Guarantees.OfType<GBGuarantee>().FirstOrDefault();
				AssertNotNull("A guarantee has been added", newGuarantee);
				AssertEquals("A", newGuarantee.PW_BondType);
				AssertEquals("test", newGuarantee.PW_BondNumber);
				AssertEquals("test2", newGuarantee.PW_BondNumber2);
				AssertEquals("ACE", newGuarantee.PW_Password);
				AssertEquals(435m, newGuarantee.PW_BondAmount);
				AssertEquals("AUD", newGuarantee.PW_RX_NKCurrency);
				AssertEquals("IS", newGuarantee.PW_ValidityLimitation);
				AssertEquals("AD000003", newGuarantee.PW_BondFiledPort);
				AssertEquals("GB945390992000", newGuarantee.PW_HolderIdentification);
				var matchingInstruction = newDecCloned.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(t => t.PK == newGuarantee.EntryInstructionID);
				AssertEquals(matchingInstruction.PK, newGuarantee.EntryInstructionID);
				AssertEquals("Application Code should be 'CDS'", "CDS", newDecCloned.JE_ApplicationCode);
				AssertEquals("ApplicationExtender should be 'CDSApplicationExtender'", "CDSApplicationExtender", newDecCloned.ApplicationExtender.GetType().Name);
			}
		}
	}
}
