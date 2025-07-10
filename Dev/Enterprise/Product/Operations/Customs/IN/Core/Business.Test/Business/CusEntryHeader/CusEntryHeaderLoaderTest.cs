using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;
using Loader = Enterprise.Customs.IN.Business.CusEntryHeader.Loader;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(Loader))]
sealed class CusEntryHeaderLoaderTest : LoaderTestCase
{
	public void TestFindByEntryNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeader1 = CreateCusEntryHeader(declaration, CusEntryNumberTypes.Indian.ShippingBill, "1234567", new DateTime(2025, 2, 12));
		var cusEntryHeader2 = CreateCusEntryHeader(declaration, CusEntryNumberTypes.Indian.ImportGeneralManifest, "1234567", new DateTime(2025, 2, 12));
		var cusEntryHeader3 = CreateCusEntryHeader(declaration, CusEntryNumberTypes.Indian.ShippingBill, "7654321", new DateTime(2025, 2, 12));
		var cusEntryHeader4 = CreateCusEntryHeader(declaration, CusEntryNumberTypes.Indian.ShippingBill, "1234567", new DateTime(2025, 2, 11));
		Factory.Save();
		var loader = new Loader(Factory);
		var result = loader.FindByEntryNumber(CusEntryNumberTypes.Indian.ShippingBill, "1234567", new DateTime(2025, 2, 12));
		AssertEquals(cusEntryHeader1.PK, result.PK);
	}

	CusEntryHeader CreateCusEntryHeader(JobDeclaration declaration, ZString type , ZString number, ZDateTime issueDate)
	{
		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		declaration.ActiveEntryHeaders.Add(cusEntryHeader);
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		cusEntryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
		var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
		entryNumber.CE_EntryType = type;
		entryNumber.CE_EntryNum = number;
		entryNumber.CE_IssueDate = issueDate;
		entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		entryNumber.CE_ParentID = cusEntryInstruction.PK;
		entryNumber.CE_ParentTable = CusEntryInstruction.Schema.TableName;
		return cusEntryHeader;
	}

	protected override BusinessObject.Loader GetNewLoaderToTest() => new Loader(Factory);
}

