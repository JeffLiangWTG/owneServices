using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(GBGuarantee))]
	public class GBGuaranteeTest : Business.Declaration.Testing.GBGuaranteeTest
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration.Guarantees.AddNew();
		}

		public void TestPW_BondType()
		{
			var guarantee = (GBGuarantee)GetNewBusinessObject();
			AssertEquals(GuaranteeTypeList.Codes.Guarantee, guarantee.PW_BondType);
		}

		public void TestEntryInstructionID()
		{
			var guarantee = (GBGuarantee)GetNewBusinessObject();
			var declaration = guarantee.Declaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_PackageCount = 55;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			guarantee.EntryInstructionID = entryInstruction.PK;
			AssertEquals(entryInstruction, guarantee.EntryInstruction);

			var genPivot = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, guarantee.PK)).FirstOrDefault();

			AssertNotNull(genPivot);
			AssertEquals(entryInstruction.PK, genPivot.XX_Relation2ID);

			guarantee.EntryInstructionID = Guid.Empty;
			genPivot = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, guarantee.PK)).FirstOrDefault();
			AssertNull("The pivot should be deleted when set entry instruction to blank", genPivot);
		}

		public void TestPW_HolderIdentificationReadOnly()
		{
			var guarantee = (GBGuarantee)GetNewBusinessObject();
			var declaration = guarantee.Declaration;
			AssertEquals(guarantee.PW_BondNumberInfo.ReadOnly, false);
			AssertEquals(guarantee.PW_BondNumber2Info.ReadOnly, false);

			guarantee.PW_HolderIdentification = "JackFlash";
			AssertEquals(guarantee.PW_BondNumberInfo.ReadOnly, true);
			AssertEquals(guarantee.PW_BondNumber2Info.ReadOnly, true);
		}

		public void TestPW_HolderIdentificationChange()
		{
			var guarantee = (GBGuarantee)GetNewBusinessObject();
			var declaration = guarantee.Declaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_PackageCount = 55;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			guarantee.EntryInstructionID = entryInstruction.PK;

			guarantee.PW_HolderIdentification = "JackFlash";
			guarantee.PW_Password = "Y";
			AssertEquals("JackFlash", guarantee.PW_BondNumber);
			AssertEquals("", guarantee.PW_BondNumber2);

			guarantee.PW_HolderIdentification = ZString.Empty;
			guarantee.PW_BondNumber2 = ZString.Empty;
			guarantee.PW_BondNumber = ZString.Empty;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.ZG_MethodOfPayment = "P";
			guarantee.PW_HolderIdentification = "JumpingFlash";

			AssertEquals("JumpingFlash", guarantee.PW_BondNumber);
			AssertEquals("", guarantee.PW_BondNumber2);

			guarantee.PW_HolderIdentification = ZString.Empty;
			guarantee.PW_BondNumber2 = ZString.Empty;
			guarantee.PW_BondNumber = ZString.Empty;
			invoiceLine1.ZG_MethodOfPayment = ZString.Empty;

			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var fee4 = entryLine.Fees.AddNew();
			fee4.CF_MethodOfPayment = "N";
			fee4.CF_ChargeType = "B05";
			fee4.CF_ChargeAmount = 777m;
			guarantee.PW_HolderIdentification = "JumpingFlashJack";

			AssertEquals("JumpingFlashJack", guarantee.PW_BondNumber);
			AssertEquals("", guarantee.PW_BondNumber2);

			guarantee.PW_HolderIdentification = ZString.Empty;
			guarantee.PW_BondNumber2 = ZString.Empty;
			guarantee.PW_BondNumber = ZString.Empty;
			invoiceLine1.ZG_MethodOfPayment = ZString.Empty;
			fee4.CF_MethodOfPayment = "E";
			guarantee.PW_HolderIdentification = "JumpingJackFlash";
			guarantee.PW_Password = "N";

			AssertEquals("JumpingJackFlash", guarantee.PW_BondNumber2);
			AssertEquals("", guarantee.PW_BondNumber);

			guarantee.PW_HolderIdentification = ZString.Empty;
			guarantee.PW_BondNumber2 = ZString.Empty;
			guarantee.PW_BondNumber = ZString.Empty;

			declaration.InvoiceLines.RemoveAndDeleteAll();
			guarantee.PW_HolderIdentification = "JumpingJackFlash";

			AssertEquals("", guarantee.PW_BondNumber);
			AssertEquals("JumpingJackFlash", guarantee.PW_BondNumber2);
		}
	}
}
