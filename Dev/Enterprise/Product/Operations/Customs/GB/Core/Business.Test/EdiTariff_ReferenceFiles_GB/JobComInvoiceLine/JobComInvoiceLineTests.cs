using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode
		{
			get { return "0000000000"; }
		}

		protected override ZString TariffCode2
		{
			get { return "0000000001"; }
		}

		protected override ZString TariffDescription
		{
			get { return TariffDescriptionCore; }
		}
		internal const string TariffDescriptionCore = "";

		protected override ZString TariffDescription2
		{
			get { return TariffDescriptionCore2; }
		}
		internal const string TariffDescriptionCore2 = "";

		protected override Type DeclarationTypeForTest
		{
			get { return typeof(ImportJobDeclarationForTest); }
		}

		protected override void AddLookupToPart(Customs.Business.OrgSupplierPart part, Customs.Business.BaseCusClassification lookup)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_CC = lookup.PK;
			pivot.CI_ChildType = Customs.Business.BaseCusClassification.ClassificationType.Both;
		}

		class ImportJobDeclarationForTest : JobDeclaration, IEDocsProvider
		{
			public ImportJobDeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IEDocsProvider members

			EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
			{
				return new JobInvoicingEDocsProviderSupporter(this);
			}

			#endregion
			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				JE_MessageType = "IMP";
			}

			protected override void SetPKAndDefaults()
			{
				base.SetPKAndDefaults();
				JE_MessageType = "IMP";
			}
		}
	}

	class JobComInvoiceLineTests : TestCaseWithFactory
	{
		public void TestFECMessageErrorChallenges()
		{
			var errorMsg = "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();

			var entryHeader = dec.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			Factory.Save();

			var ji_NettMass = entryHeader.FECChallenges.AddNew();
			ji_NettMass.CY_Code = FECChallengeFields.Codes.JI_NettMass;
			ji_NettMass.CY_ParentID = entryLine.PK;
			ji_NettMass.CY_ParentTableCode = "CL";
			var ji_Supp = entryHeader.FECChallenges.AddNew();
			ji_Supp.CY_Code = FECChallengeFields.Codes.JI_Supp;
			ji_Supp.CY_ParentID = entryLine.PK;
			ji_Supp.CY_ParentTableCode = "CL";
			var ji_Price = entryHeader.FECChallenges.AddNew();
			ji_Price.CY_Code = FECChallengeFields.Codes.JI_Price;
			ji_Price.CY_ParentID = entryLine.PK;
			ji_Price.CY_ParentTableCode = "CL";

			AssertEquals(3, dec.CustomsEntryHeaders[0].FECChallenges.Count);
			Factory.Save();

			ji_NettMass.CY_Data = "123.456";
			// First check that changing the FEC tickbox for the mass will cause the message alert to be shown/hidden.
			invoiceLine.JI_NetWeight = 123.456;
			AssertHasMessageErrorContaining(ji_NettMass.CY_IsOverriddenInfo, errorMsg);
			ji_NettMass.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_NettMass.CY_IsOverriddenInfo, errorMsg);

			// Second check that changing the mass itself will cause the message to be shown/hidden.
			ji_NettMass.CY_IsOverridden = false;
			AssertHasMessageErrorContaining(ji_NettMass.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_NetWeight = 656.456;
			AssertNoMessageErrorContaining(ji_NettMass.CY_IsOverriddenInfo, errorMsg);

			ji_Supp.CY_Data = "123.456";
			// Same on Supplementary quanity's fec flag
			invoiceLine.JI_CustomsSecondQuantity = 123.456;
			AssertHasMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);
			ji_Supp.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);

			// Change supplementary quantity
			ji_Supp.CY_IsOverridden = false;
			AssertHasMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CustomsSecondQuantity = 656.456;
			AssertNoMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);

			ji_Price.CY_Data = "567.89";
			// First check that changing the FEC tickbox for the mass will cause the message alert to be shown/hidden.
			invoiceLine.JI_LinePrice = 567.89M;
			AssertHasMessageErrorContaining(ji_Price.CY_IsOverriddenInfo, errorMsg);
			ji_Price.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_Price.CY_IsOverriddenInfo, errorMsg);

			// Second check that changing the mass itself will cause the message to be shown/hidden.
			ji_Price.CY_IsOverridden = false;
			AssertHasMessageErrorContaining(ji_Price.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_LinePrice = 656.456;
			AssertNoMessageErrorContaining(ji_Price.CY_IsOverriddenInfo, errorMsg);
		}

		public void TestCheckThatChangingTheFieldsValueOrTickingFecBoxWillClearRelatedMsgError()
		{
			var errorMsg = "A FEC challenge has been received. You should either tick the confirm box or change the value or tick 'request route F' on the Misc tab. When multiple invoice lines are merged into this entry line, numeric values must be edited on the invoice lines directly.";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();

			var entryHeader = dec.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			invoiceLine.ZG_FecChallengeDST = true;
			invoiceLine.ZG_CountryOfDestination = "US";
			AssertEquals("Changing country should clear challenge", false, invoiceLine.ZG_FecChallengeDST);

			invoiceLine.ZG_FecChallengeDST = true;
			invoiceLine.ZG_FecDST = true;
			AssertEquals("Changing FEC flag should clear challenge", false, invoiceLine.ZG_FecChallengeDST);

			var ji_ORG = entryHeader.FECChallenges.AddNew();
			ji_ORG.CY_Code = FECChallengeFields.Codes.JI_ORG;
			ji_ORG.CY_ParentID = entryLine.PK;
			ji_ORG.CY_ParentTableCode = "CL";
			ji_ORG.CY_Data = "GB";
			invoiceLine.JI_CountryOfOrigin = "GB";
			AssertHasMessageErrorContaining(ji_ORG.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CountryOfOrigin = "HU";
			AssertNoMessageErrorContaining(ji_ORG.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CountryOfOrigin = "GB";
			ji_ORG.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_ORG.CY_IsOverriddenInfo, errorMsg);

			var ji_NetMass = entryHeader.FECChallenges.AddNew();
			ji_NetMass.CY_Code = FECChallengeFields.Codes.JI_NettMass;
			ji_NetMass.CY_ParentID = entryLine.PK;
			ji_NetMass.CY_ParentTableCode = "CL";
			ji_NetMass.CY_Data = "123";
			invoiceLine.JI_NetWeight = 123;
			AssertHasMessageErrorContaining(ji_NetMass.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_NetWeight = 123.456;
			AssertNoMessageErrorContaining(ji_NetMass.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_NetWeight = 123;
			ji_NetMass.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_NetMass.CY_IsOverriddenInfo, errorMsg);

			var ji_Supp = entryHeader.FECChallenges.AddNew();
			ji_Supp.CY_Code = FECChallengeFields.Codes.JI_Supp;
			ji_Supp.CY_ParentID = entryLine.PK;
			ji_Supp.CY_ParentTableCode = "CL";
			ji_Supp.CY_Data = "123";
			invoiceLine.JI_CustomsSecondQuantity = 123;
			AssertHasMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CustomsSecondQuantity = 123.456;
			AssertNoMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CustomsSecondQuantity = 123;
			ji_Supp.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_Supp.CY_IsOverriddenInfo, errorMsg);

			var ji_SuppUQ = entryHeader.FECChallenges.AddNew();
			ji_SuppUQ.CY_Code = FECChallengeFields.Codes.JI_SuppUQ;
			ji_SuppUQ.CY_ParentID = entryLine.PK;
			ji_SuppUQ.CY_ParentTableCode = "CL";
			ji_SuppUQ.CY_Data = "G";
			invoiceLine.JI_CustomsSecondUnitQty = "G";
			AssertHasMessageErrorContaining(ji_SuppUQ.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			AssertNoMessageErrorContaining(ji_SuppUQ.CY_IsOverriddenInfo, errorMsg);
			invoiceLine.JI_CustomsSecondUnitQty = "G";
			ji_SuppUQ.CY_IsOverridden = true;
			AssertNoMessageErrorContaining(ji_SuppUQ.CY_IsOverriddenInfo, errorMsg);
		}
	}
}
