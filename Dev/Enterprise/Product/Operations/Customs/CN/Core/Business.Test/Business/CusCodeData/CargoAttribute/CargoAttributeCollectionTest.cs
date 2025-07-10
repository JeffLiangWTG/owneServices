using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CargoAttributeCollection))]
	class CargoAttributeCollectionTest : CusCodeDataCollectionTest<CargoAttribute>
	{
		public void TestIsProvidedAny()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			var attr1 = invoiceLine.CargoAttributes.AddNew();
			attr1.CY_ParentID = invoiceLine.PK;
			attr1.CY_ParentTableCode = invoiceLine.TablePrefix;
			attr1.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			attr1.CY_Code = "attr1";
			var attr2 = invoiceLine.CargoAttributes.AddNew();
			attr2.CY_ParentID = invoiceLine.PK;
			attr2.CY_ParentTableCode = invoiceLine.TablePrefix;
			attr2.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			attr2.CY_Code = "attr2";
			Assert(invoiceLine.CargoAttributes.IsProvidedAny("attr1"));
			Assert(invoiceLine.CargoAttributes.IsProvidedAny("attr2", "attr3"));
			Assert(!invoiceLine.CargoAttributes.IsProvidedAny("attr3"));
		}

		public void TestAddCargoAttribute()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CIQEndUse = ZString.Empty;
			Assert(invoiceLine.JI_CIQEndUse.IsEmpty);
			var testCollection = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._12);
			AssertEquals(EndUseList.Codes.OnlyIndustrialUse, invoiceLine.JI_CIQEndUse);
			invoiceLine.JI_NonDangerousChemicalFlag = true;
			testCollection.AddNew(CargoAttributeList.Codes._31);
			AssertEquals("JI_NonDangerousChemicalFlag should be unchecked for 31.", false, invoiceLine.JI_NonDangerousChemicalFlag);
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._31));
			invoiceLine.JI_NonDangerousChemicalFlag = true;
			testCollection.AddNew(CargoAttributeList.Codes._32);
			AssertEquals("JI_NonDangerousChemicalFlag should be unchecked for 32.", false, invoiceLine.JI_NonDangerousChemicalFlag);
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._32));
			testCollection.AddNew(CargoAttributeList.Codes._33);
			AssertEquals("JI_NonDangerousChemicalFlag should be checked for 33.", true, invoiceLine.JI_NonDangerousChemicalFlag);
		}

		public void TestReValidationsAfterAddNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var testCollection = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._32);
			AssertHasMessageErrorContaining(invoiceLine.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_PackageTypeOfUNDGInfo, MandatoryValidation.YouHaveNotEntered);
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._32));
			testCollection.AddNew(CargoAttributeList.Codes._33);
			AssertNoMessageErrorContaining("Message should have been cleared when adding.", invoiceLine.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Message should have been cleared when adding.", invoiceLine.JI_PackageTypeOfUNDGInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestReloadAttachmentLinksOnRemoved()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._32);
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._33);
			var storageDoc1 = instruction.CusStorageDocPivots.AddNew();
			storageDoc1.CSD_DocType = CSDDocTypeList.Codes._80000001;
			storageDoc1.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine;
			var storageDoc2 = instruction.CusStorageDocPivots.AddNew();
			storageDoc2.CSD_DocType = CSDDocTypeList.Codes._80000002;
			storageDoc2.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine;

			AssertEquals("CargoAttributes=31,32,33", 2, invoiceLine.AttachmentLinks.Count);
			invoiceLine.CargoAttributes.Remove(invoiceLine.CargoAttributes[0]);
			AssertEquals("CargoAttributes=32,33", 2, invoiceLine.AttachmentLinks.Count);
			invoiceLine.CargoAttributes.Remove(invoiceLine.CargoAttributes[0]);
			AssertEquals("CargoAttributes=33", 0, invoiceLine.AttachmentLinks.Count);
		}

		protected override CusCodeDataCollection<CargoAttribute> GetCusCodeDataCollection() => new CargoAttributeCollection(Factory.New<JobComInvoiceLine>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CargoAttribute>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			result.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			return result;
		}

		JobComInvoiceLine invLine;
		JobComInvoiceLine InvoiceLine => invLine ?? (invLine = Factory.New<JobComInvoiceLine>());
	}
}
