using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobDeclarationDeepCloneStrategyTester : TestCaseWithFactory
	{
		public void TestGuaranteesCloning()
		{
			JobDeclaration oldDec = Factory.New<JobDeclaration>();
			var entryInstruction = oldDec.CustomsEntryInstructions.AddNew();

			var guarantee1 = oldDec.Guarantees.AddNew();
			guarantee1.PW_ActivityCode = "AC1";
			guarantee1.PW_BondType = "A";
			guarantee1.PW_BondFiledPort = "Port1";
			guarantee1.PW_BondNumber = "BondNumber_1";
			guarantee1.PW_BondNumber2 = "BondNumber2_1";
			guarantee1.PW_SuretyCode = "SC1";
			guarantee1.PW_Password = "1234";
			guarantee1.PW_BondAmount = 1234;
			guarantee1.EntryInstructionID = entryInstruction.PK;

			var guarantee2 = oldDec.Guarantees.AddNew();
			guarantee2.PW_ActivityCode = "AC2";
			guarantee2.PW_BondType = "B";
			guarantee2.PW_BondFiledPort = "Port2";
			guarantee2.PW_BondNumber = "BondNumber_2";
			guarantee2.PW_BondNumber2 = "BondNumber2_2";
			guarantee2.PW_SuretyCode = "SC2";
			guarantee2.PW_Password = "5678";
			guarantee2.PW_BondAmount = 5678;
			guarantee2.EntryInstructionID = entryInstruction.PK;

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var newDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());

			AssertEquals("Guarantees count of clone should be the same as cloned entry instruction.", 2, newDec.Guarantees.Count);
			for (int i = 0; i < 2; i++)
			{
				var originalGuarantee = oldDec.Guarantees[i];
				var clonedGuarantee = newDec.Guarantees[i];
				CombineAssertions("All Guarantees fields must be cloned", () =>
				{
					AssertEquals("Cloned Guarantee object must be different from original", false, ReferenceEquals(originalGuarantee, clonedGuarantee));

					AssertEquals("PW_ActivityCode ", originalGuarantee.PW_ActivityCode, clonedGuarantee.PW_ActivityCode);
					AssertEquals("PW_BondType ", originalGuarantee.PW_BondType, clonedGuarantee.PW_BondType);
					AssertEquals("PW_BondFiledPort ", originalGuarantee.PW_BondFiledPort, clonedGuarantee.PW_BondFiledPort);
					AssertEquals("PW_BondNumber ", originalGuarantee.PW_BondNumber, clonedGuarantee.PW_BondNumber);
					AssertEquals("PW_BondNumber2 ", originalGuarantee.PW_BondNumber2, clonedGuarantee.PW_BondNumber2);
					AssertEquals("PW_SuretyCode ", originalGuarantee.PW_SuretyCode, clonedGuarantee.PW_SuretyCode);
					AssertEquals("PW_Password ", originalGuarantee.PW_Password, clonedGuarantee.PW_Password);
					AssertEquals("PW_BondAmount ", originalGuarantee.PW_BondAmount, clonedGuarantee.PW_BondAmount);
					AssertNotEquals("Entry InstructionID not equal to  ", originalGuarantee.EntryInstructionID, clonedGuarantee.EntryInstructionID);
					AssertEquals("Entry InstructionID exist in EntryInstruction ", true, newDec.CustomsEntryInstructions.Any(c => c.PK == clonedGuarantee.EntryInstructionID));
				});
			}
		}

		public void TestDV1DetailsCloning()
		{
			var oldDec = Factory.New<JobDeclaration>();
			oldDec.JE_MessageType = MessageTypeList.Codes.Import;

			var cusDV1Detail = oldDec.DV1Details.AddNew();
			cusDV1Detail.DV1_CustomsDecisionNumber = "deep dark";
			cusDV1Detail.DV1_Relationship = "Y";

			var cusDV1Detail2 = oldDec.DV1Details.AddNew();
			cusDV1Detail2.DV1_CustomsDecisionNumber = "fantasy";
			cusDV1Detail2.DV1_Relationship = "N";

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var newDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());

			CombineAssertions(() =>
			{
				AssertEquals("Count is same", newDec.DV1Details.Count, oldDec.DV1Details.Count);
				for (int i = 0; i < newDec.DV1Details.Count; i++)
				{
					var sequence = i + 1;
					AssertEquals($"Sequence{sequence}", sequence, newDec.DV1Details[i].Sequence);
					AssertEquals($"DV1_CustomsDecisionNumber{sequence}", oldDec.DV1Details[i].DV1_CustomsDecisionNumber, newDec.DV1Details[i].DV1_CustomsDecisionNumber);
					AssertEquals($"DV1_Relationship{sequence}", oldDec.DV1Details[i].DV1_Relationship, newDec.DV1Details[i].DV1_Relationship);
				}
			});
		}

		public void TestCustomsOfficesCloning()
		{
			var oldDec = Factory.New<JobDeclaration>();
			oldDec.JE_MessageType = MessageTypeList.Codes.Import;

			var cusOffice = oldDec.CustomsOffices.AddNew();
			cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
			cusOffice.CY_Data = "XX111111";

			var cusOffice2 = oldDec.CustomsOffices.AddNew();
			cusOffice2.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
			cusOffice2.CY_Data = "XX222222";

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var newDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());

			AssertEquals("Customs offices count of clone declaration should be the same as cloned declaration'", newDec.CustomsOffices.Count, oldDec.CustomsOffices.Count);

			for (int i = 0; i < Math.Max(newDec.CustomsOffices.Count, oldDec.CustomsOffices.Count); i++)
			{
				AssertEquals("EU strategy should clone the declaration customs offices. Count is ok but codes don't match", oldDec.CustomsOffices[i].CY_Code, newDec.CustomsOffices[i].CY_Code);
				AssertEquals("EU strategy should clone the declaration customs offices. Count is ok but data don't match", oldDec.CustomsOffices[i].CY_Data, newDec.CustomsOffices[i].CY_Data);
			}

			var cusOffices = Factory.Load<EuOfficeCode>(new ZQuery());
			AssertEquals(4, cusOffices.Length);
			Assert(cusOffices.All(x => x.CY_ParentID != ZGuid.Empty));
		}

		public void TestJobDeclarationDeepCloneStrategyTesterDeclarationsJobComInvoiceHeaderAndInvoiceLine()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.MainAddress.Address1 = "XYZ";
			JobDeclaration oldDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader oldInvOne = oldDec.Invoices.AddNew();
			oldInvOne.JZ_InvoiceNumber = "ABC123";
			var oldPrevDocOne = oldInvOne.PreviousDocuments.AddNew();
			oldPrevDocOne.CSI_Description = "My cat's breath smells like catfood";
			var oldPrevDocTwo = oldInvOne.PreviousDocuments.AddNew();
			oldPrevDocTwo.CSI_Description = "Me fail English?  That's unpossible";
			var oldAdditionalInfoOne = oldInvOne.AdditionalInfos.AddNew();
			oldAdditionalInfoOne.CSI_Description = "Lies make Baby Jesus cry";
			var oldAdditionalInfoTwo = oldInvOne.AdditionalInfos.AddNew();
			oldAdditionalInfoTwo.CSI_Description = "Yay! That means we BOTH come second!  We're number two! We're number two!";
			var oldSupportingDocumentsOne = oldInvOne.SupportingDocuments.AddNew();
			oldSupportingDocumentsOne.CSI_Description = "Happy birthday Mister Smithers";
			var oldSupportingDocumentsTwo = oldInvOne.SupportingDocuments.AddNew();
			oldSupportingDocumentsTwo.CSI_Description = "Hello Smithers - you're - quite - good at - turning - me - on";
			var sourceInvoiceLine = oldInvOne.InvoiceLines.AddNew();
			sourceInvoiceLine.PreviousDocuments.AddNew().CSI_ReferenceNumber = "DAN1";
			sourceInvoiceLine.AdditionalInfos.AddNew().CSI_Description = "DAN2";
			sourceInvoiceLine.SupportingDocuments.AddNew().CSI_ReferenceNumber = "DAN3";
			sourceInvoiceLine.Taxes.AddNew().Data.G4_RateDuty = "DC4";
			sourceInvoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "1111111";
			var cusAuthorizationUsage = sourceInvoiceLine.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = "ABC";
			cusAuthorizationUsage.AGC_Number = "DEF";
			cusAuthorizationUsage.AGC_OH_Owner = header.PK;
			var cusSupplyChainActorReference = sourceInvoiceLine.CusSupplyChainActorReferences.AddNew();
			cusSupplyChainActorReference.CFR_Code = "ABC";
			cusSupplyChainActorReference.CFR_OA_Owner = header.MainAddress.PK;
			var fiscalReference = sourceInvoiceLine.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "ABC";
			fiscalReference.CFR_Reference = "DEF";
			fiscalReference.CFR_OA_Owner = header.MainAddress.PK;
			sourceInvoiceLine.BuyerDocAddress.E2_OA_Address = header.MainAddress.PK;
			sourceInvoiceLine.SellerDocAddress.E2_OA_Address = header.MainAddress.PK;
			sourceInvoiceLine.JI_SupplementaryCode1 = "SUP1";
			sourceInvoiceLine.JI_SupplementaryCode2 = "SUP2";
			sourceInvoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "ADDSUP";

			CloneUsingBaseNotEuStrategyAndMakeSimpleAssertions(oldDec);

			var otherFactory = new BusinessObjectFactory();
			var euCloneStrategy_Deep = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.DeepTemplateCopy);
			var euCloneStrategy_Template = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var cloneArgsDeepCopyOtherFactory = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.DeepTemplateCopy, oldDec.GetType(), otherFactory);
			var cloneArgsTemplateCopyOtherFactory = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, oldDec.GetType(), otherFactory);
			var cloneArgsDeepCopyThisFactory = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.DeepTemplateCopy, oldDec.GetType(), Factory);
			var cloneArgsTemplateCopyThisFactory = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, oldDec.GetType(), Factory);

			PerformCloneAndMakeAssertions(oldDec, euCloneStrategy_Deep, cloneArgsDeepCopyOtherFactory, header);
			PerformCloneAndMakeAssertions(oldDec, euCloneStrategy_Template, cloneArgsTemplateCopyOtherFactory, header);
			PerformCloneAndMakeAssertions(oldDec, euCloneStrategy_Deep, cloneArgsDeepCopyThisFactory, header);
			PerformCloneAndMakeAssertions(oldDec, euCloneStrategy_Template, cloneArgsTemplateCopyThisFactory, header);
		}

		[ExpectNoExceptions]
		public void TestJobDeclarationDeepCloneStrategyTesterDeclarationsDirectChildren()
		{
			// ie DocAddresses, AdditionalReferenceNumbers, DocsAndCartage

			JobDeclaration oldDec = Factory.New<JobDeclaration>();
			oldDec.ZG_StyleOfEntrySOE = "B";
			var cusEntryHeader = oldDec.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "ABC-123456/789"; // we want to assert that this is NOT cloned!

			var oldAdditionalReferenceNumberOne = oldDec.AdditionalReferenceNumbers.AddNew();
			var oldAdditionalReferenceNumberTwo = oldDec.AdditionalReferenceNumbers.AddNew();
			var oldAdditionalReferenceNumberThree = oldDec.AdditionalReferenceNumbers.AddNew();
			oldAdditionalReferenceNumberOne.CE_EntryNum = "Holy crip I'm a crapple!";
			oldAdditionalReferenceNumberTwo.CE_EntryNum = "And that, sir, is an idiot";
			oldAdditionalReferenceNumberThree.CE_EntryNum = "Ah, I am trying to throw exception for ya";
			oldAdditionalReferenceNumberThree.CE_EntryIsSystemGenerated = true;

			var oldDocAddressesOne = oldDec.DocAddresses.AddNew();
			oldDocAddressesOne.E2_City = "Quahog";
			var oldDocAddressesTwo = oldDec.DocAddresses.AddNew();
			oldDocAddressesTwo.E2_City = "Newport";

			JobDocsAndCartage.New(oldDec);
			oldDec.DocsAndCartage.JP_CustomAttrib1 = "Rhode Island";

			JobService service = oldDec.DocsAndCartage.Services.AddNew();
			service.ES_ServiceNote = "Don't scrape my asphalt";

			BusinessObjectCloneStrategy baseCloneStrat = new Customs.Business.JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			JobDeclaration newDecClonedWithBaseStrategy = (JobDeclaration)baseCloneStrat.Clone(new BusinessObjectCloneArgs());

			AssertEquals("Cloning using the base strategy should not clone the dec's AdditionalReferenceNumbers (if it does, someone may have enhanced Base and will need to remove from cloning here to prevent duplicates)",
								newDecClonedWithBaseStrategy.AdditionalReferenceNumbers.Count, 0);

			AssertEquals("Cloning using the base strategy should not clone the dec's DocsAndCartage (if it does, someone may have enhanced Base and will need to remove from cloning here to prevent duplicates)",
								newDecClonedWithBaseStrategy.DocsAndCartage.JP_ExportStatement, ZString.Empty);

			AssertEquals("Cloning using the base strategy should not clone the dec's DocsAndCartage's services (if it does, someone may have enhanced Base and will need to remove from cloning here to prevent duplicates)",
								newDecClonedWithBaseStrategy.DocsAndCartage.Services.Count, 0);

			BusinessObjectCloneStrategy newCloneStrat = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			JobDeclaration newDecClonedWithEuStrategy = (JobDeclaration)newCloneStrat.Clone(new BusinessObjectCloneArgs());

			AssertEquals("Dec cloned with EU strategy should also clone the dec's DocAddresses. This is now done in base, but still a business requirement here.", oldDec.DocAddresses[0].E2_City, newDecClonedWithEuStrategy.DocAddresses[0].E2_City);
			AssertEquals("Dec cloned with EU strategy should also clone the dec's AdditionalReferenceNumbers", oldDec.AdditionalReferenceNumbers[0].CE_EntryNum, newDecClonedWithEuStrategy.AdditionalReferenceNumbers[0].CE_EntryNum);
			AssertEquals("Dec cloned with EU strategy should also clone the dec's DocsAndCartage", oldDec.DocsAndCartage.JP_CustomAttrib1, newDecClonedWithEuStrategy.DocsAndCartage.JP_CustomAttrib1);

			AssertEquals("Checking that the cloned dec's CusEntryNumbers do not have any REAL CusEntryNumbers (ie none that come from customs and only those that are user-generated",
							0,
							newDecClonedWithEuStrategy.CustomsEntryHeaders.Count);

			AssertEquals("Dec cloned with EU strategy should also clone the dec's DocsAndCartage's services",
								oldDec.DocsAndCartage.Services[0].ES_ServiceNote, newDecClonedWithEuStrategy.DocsAndCartage.Services[0].ES_ServiceNote);

			AssertEquals("B", newDecClonedWithEuStrategy.ZG_StyleOfEntrySOE);
		}

		public void TestJobDeclarationDeepCloneStrategyTesterContainersPivot()
		{
			JobDeclaration oldDec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader oldInvOne = oldDec.Invoices.AddNew();
			oldInvOne.JZ_InvoiceNumber = "ABC123";
			CusContainer oldContOne = oldDec.CusContainers.AddNew();
			oldContOne.CO_ContainerNumber = "MSCU1234560";
			JobComInvoiceLine oldInvLineOne = oldDec.InvoiceLines.AddNew();
			oldInvLineOne.ContainersPivot.AddPivotFor(oldContOne);
			oldInvLineOne.JI_JZ = oldInvOne.PK;
			oldInvLineOne.JI_InvoiceQuantity = 123m;
			CusContainer oldContTwo = oldDec.CusContainers.AddNew();
			oldContTwo.CO_ContainerNumber = "DANU7654321";
			JobComInvoiceHeader oldInvTwo = oldDec.Invoices.AddNew();
			oldInvTwo.JZ_InvoiceNumber = "DEC456";
			JobComInvoiceLine oldInvLineTwo = oldDec.InvoiceLines.AddNew();
			oldInvLineTwo.JI_JZ = oldInvTwo.PK;

			// Use fully qualified namespaces to prevent confusion
			JobDeclarationDeepCloneStrategy newCloneStrat = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			JobDeclaration newDecClonedWithEuStrategy = (JobDeclaration)newCloneStrat.Clone(CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.TemplateCopy, typeof(JobDeclaration)));

			// Check a few things to ensure the key bits are cloned:

			AssertEquals(0, newDecClonedWithEuStrategy.CusContainers.Count);
			AssertEquals(0, newDecClonedWithEuStrategy.InvoiceLines[0].ContainersPivot.Count);
			AssertEquals(0, newDecClonedWithEuStrategy.InvoiceLines[1].ContainersPivot.Count);

			// Use fully qualified namespaces to prevent confusion
			newCloneStrat = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.DeepTemplateCopy);
			newDecClonedWithEuStrategy = (JobDeclaration)newCloneStrat.Clone(CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.DeepTemplateCopy, typeof(JobDeclaration)));

			// Check a few things to ensure the key bits are cloned:

			AssertEquals("EU-Cloned dec's first inv should have one pivot", 1, newDecClonedWithEuStrategy.InvoiceLines[0].ContainersPivot.Count);
			AssertEquals("EU-Cloned dec's second inv should have no pivot", 0, newDecClonedWithEuStrategy.InvoiceLines[1].ContainersPivot.Count);

			AssertEquals("EU-Cloned dec's first inv should have a cont labelled as 'IsForInvoice'", newDecClonedWithEuStrategy.InvoiceLines[0].ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine, true);

			AssertEquals("EU-Cloned dec's first inv should have right details: cont.  If not, the pivot part of the clone failed.",
						newDecClonedWithEuStrategy.InvoiceLines[0].ContainersPivot[0].Container,
						newDecClonedWithEuStrategy.CusContainers[0]);

			AssertEquals("EU-Cloned dec's first inv should have right details: inv.  If not, the pivot part of the clone failed.",
						newDecClonedWithEuStrategy.InvoiceLines[0].ContainersPivot[0].C2_JI,
						newDecClonedWithEuStrategy.InvoiceLines[0].PK);

			AssertEquals("EU-Cloned dec's first inv should be the same as its source dec's. If it's not, the base clone failed.",
								newDecClonedWithEuStrategy.InvoiceLines[0].JI_InvoiceQuantity,
								oldDec.InvoiceLines[0].JI_InvoiceQuantity);

			AssertEquals("EU-Cloned dec's first cont should be the same as its source dec's. If it's not, the base clone failed.",
											newDecClonedWithEuStrategy.CusContainers[0].CO_ContainerNumber,
											oldDec.CusContainers[0].CO_ContainerNumber);
		}

		public void TestLocationOfGoodsCloning_Export()
		{
			AssertLocationOfGoodsCloning(MessageTypeList.Codes.Export);
		}

		public void TestLocationOfGoodsCloning_Import()
		{
			AssertLocationOfGoodsCloning(MessageTypeList.Codes.Import);
		}

		void AssertLocationOfGoodsCloning(ZString messageType)
		{
			var oldDec = Factory.New<JobDeclaration>();
			oldDec.JE_MessageType = messageType;

			oldDec.GoodsLocation.CGL_Qualifier = "V";
			oldDec.GoodsLocation.CGL_Type = "A";
			oldDec.GoodsLocation.Address.E2_Address1 = "old_address";
			oldDec.GoodsLocation.Address.E2_City = "old_city";
			oldDec.GoodsLocation.CGL_CustomsOffice = "old_custom";
			oldDec.GoodsLocation.Address.E2_Email = "abc@xyz.com";
			oldDec.GoodsLocation.Address.E2_Contact = "old_contactPerson";
			oldDec.GoodsLocation.Address.E2_Phone = "1234567890";

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var nonUcc6ClonedDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());

			CombineAssertions("Message version is not UCC6", () =>
			{
				AssertNotEquals(nameof(CusGoodsLocation.CGL_Qualifier), "V", nonUcc6ClonedDec.GoodsLocation.CGL_Qualifier);
				AssertNotEquals(nameof(CusGoodsLocation.CGL_Type), "A", nonUcc6ClonedDec.GoodsLocation.CGL_Type);
				AssertNotEquals(nameof(CusGoodsLocation.Address.E2_Address1), "old_address", nonUcc6ClonedDec.GoodsLocation.Address.E2_Address1);
				AssertNotEquals(nameof(CusGoodsLocation.Address.E2_City), "old_city", nonUcc6ClonedDec.GoodsLocation.Address.E2_City);
				AssertNotEquals(nameof(CusGoodsLocation.CGL_CustomsOffice), "old_custom", nonUcc6ClonedDec.GoodsLocation.CGL_CustomsOffice);
				AssertNotEquals(nameof(CusGoodsLocation.Address.E2_Email), "abc@xyz.com", nonUcc6ClonedDec.GoodsLocation.Address.E2_Email);
				AssertNotEquals(nameof(CusGoodsLocation.Address.E2_Contact), "old_contactPerson", nonUcc6ClonedDec.GoodsLocation.Address.E2_Contact);
				AssertNotEquals(nameof(CusGoodsLocation.Address.E2_Phone), "1234567890", nonUcc6ClonedDec.GoodsLocation.Address.E2_Phone);
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(oldDec, true))
			{
				var ucc6ClonedDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());
				CombineAssertions("Message version is UCC6", () =>
				{
					AssertEquals(nameof(CusGoodsLocation.CGL_Qualifier), "V", ucc6ClonedDec.GoodsLocation.CGL_Qualifier);
					AssertEquals(nameof(CusGoodsLocation.CGL_Type), "A", ucc6ClonedDec.GoodsLocation.CGL_Type);
					AssertEquals(nameof(CusGoodsLocation.Address.E2_Address1), "old_address", ucc6ClonedDec.GoodsLocation.Address.E2_Address1);
					AssertEquals(nameof(CusGoodsLocation.Address.E2_City), "old_city", ucc6ClonedDec.GoodsLocation.Address.E2_City);
					AssertEquals(nameof(CusGoodsLocation.CGL_CustomsOffice), "old_custom", ucc6ClonedDec.GoodsLocation.CGL_CustomsOffice);
					AssertEquals(nameof(CusGoodsLocation.Address.E2_Email), "abc@xyz.com", ucc6ClonedDec.GoodsLocation.Address.E2_Email);
					AssertEquals(nameof(CusGoodsLocation.Address.E2_Contact), "old_contactPerson", ucc6ClonedDec.GoodsLocation.Address.E2_Contact);
					AssertEquals(nameof(CusGoodsLocation.Address.E2_Phone), "1234567890", ucc6ClonedDec.GoodsLocation.Address.E2_Phone);
					AssertNotEquals(nameof(CusGoodsLocation.CGL_ParentID), oldDec.GoodsLocation.CGL_ParentID, ucc6ClonedDec.GoodsLocation.CGL_ParentID);
				});
			}
		}

		public void TestIncotermPlaceCodeCloning()
		{
			var oldDec = Factory.New<JobDeclaration>();
			oldDec.JE_MessageType = MessageTypeList.Codes.Export;
			oldDec.EUD_AgreedPlaceCode = "ADALV";

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var nonUcc6ClonedDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());
			AssertNotEquals(nameof(JobDeclaration.EUD_AgreedPlaceCode), "ADALV", nonUcc6ClonedDec.EUD_AgreedPlaceCode);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(oldDec, true))
			{
				var ucc6ClonedDec = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());
				AssertEquals(nameof(JobDeclaration.EUD_AgreedPlaceCode), "ADALV", ucc6ClonedDec.EUD_AgreedPlaceCode);
			}
		}

		static void PerformCloneAndMakeAssertions(JobDeclaration oldDec, BusinessObjectCloneStrategy newCloneStrat, BusinessObjectCloneArgs cloneArgs, OrgHeader header)
		{
			var newDecClonedWithEuStrategy = (JobDeclaration)newCloneStrat.Clone(cloneArgs);
			AssertEquals("Dec cloned with EU strategy should also clone the invoice's PreviousDocuments", oldDec.Invoices[0].PreviousDocuments[0].CSI_Description, newDecClonedWithEuStrategy.Invoices[0].PreviousDocuments[0].CSI_Description);
			AssertEquals("Dec cloned with EU strategy should also clone the invoice's AdditionalInfos", oldDec.Invoices[0].AdditionalInfos[0].CSI_Description, newDecClonedWithEuStrategy.Invoices[0].AdditionalInfos[0].CSI_Description);
			AssertEquals("Dec cloned with EU strategy should also clone the invoice's SupportingDocuments", oldDec.Invoices[0].SupportingDocuments[0].CSI_Description, newDecClonedWithEuStrategy.Invoices[0].SupportingDocuments[0].CSI_Description);
			var newInvoiceLine = newDecClonedWithEuStrategy.Invoices[0].InvoiceLines[0];
			AssertEquals("DAN1", newInvoiceLine.PreviousDocuments[0].CSI_ReferenceNumber);
			AssertEquals("DAN2", newInvoiceLine.AdditionalInfos[0].CSI_Description);
			AssertEquals("DAN3", newInvoiceLine.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertEquals("DC4", newInvoiceLine.Taxes[0].Data.G4_RateDuty);
			AssertEquals("1111111", newInvoiceLine.AdditionalProcedureCodes[0].CY_Code);
			AssertEquals("ABC", newInvoiceLine.CusAuthorizationUsages[0].AGC_Code);
			AssertEquals("DEF", newInvoiceLine.CusAuthorizationUsages[0].AGC_Number);
			AssertEquals(header.PK, newInvoiceLine.CusAuthorizationUsages[0].AGC_OH_Owner);
			AssertEquals("ABC", newInvoiceLine.CusSupplyChainActorReferences[0].CFR_Code);
			AssertEquals(header.MainAddress.PK, newInvoiceLine.CusSupplyChainActorReferences[0].CFR_OA_Owner);
			AssertEquals("ABC", newInvoiceLine.FiscalReferences[0].CFR_Code);
			AssertEquals("DEF", newInvoiceLine.FiscalReferences[0].CFR_Reference);
			AssertEquals(header.MainAddress.PK, newInvoiceLine.FiscalReferences[0].CFR_OA_Owner);
			AssertEquals(header.MainAddress.PK, newInvoiceLine.BuyerDocAddress.E2_OA_Address);
			AssertEquals(header.MainAddress.PK, newInvoiceLine.SellerDocAddress.E2_OA_Address);
			AssertEquals("JI_SupplementaryCode1", "SUP1", newInvoiceLine.JI_SupplementaryCode1);
			AssertEquals("JI_SupplementaryCode2", "SUP2", newInvoiceLine.JI_SupplementaryCode2);
			AssertEquals("AdditionalSupplementaryCodes[0]", "ADDSUP", newInvoiceLine.AdditionalSupplementaryCodes[0].CY_Code);
		}

		static void CloneUsingBaseNotEuStrategyAndMakeSimpleAssertions(JobDeclaration oldDec)
		{
			var baseCloneStrat = new Customs.Business.JobDeclarationDeepCloneStrategy(oldDec, CloneType.TemplateCopy);
			var newDecClonedWithBaseStrategy = (JobDeclaration)baseCloneStrat.Clone(new BusinessObjectCloneArgs());

			AssertEquals("Cloning using the base strategy should not clone the invoice's PreviousDocuments (if it does, someone may have enhanced Base and will need to remove from cloning here to prevent duplicates)",
								newDecClonedWithBaseStrategy.Invoices[0].PreviousDocuments.Count, 0);

			AssertEquals("Cloning using the base strategy should not clone the invoice's AdditionalInfos (if it does, someone may have enhanced Base and will need to remove from cloning here to prevent duplicates)",
								newDecClonedWithBaseStrategy.Invoices[0].AdditionalInfos.Count, 0);

			AssertEquals("Cloning using the base strategy should not clone the invoice's SupportingDocuments (if it does, someone may have enhanced Base and will need to remove from cloning here to prevent duplicates)",
								newDecClonedWithBaseStrategy.Invoices[0].SupportingDocuments.Count, 0);
		}
	}
}
