using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MX;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestUCRNumberMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(7, instruction.UCRNumberInfo.MaxLength);
		}

		public void TestUCRNumberReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			Assert(instruction.UCRNumberInfo.ReadOnly);
		}

		public void TestDeleteUCRNumberWhenDeletingInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.UCRNumber = "1234567";

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);
			var cusEntryNum = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("1234567", cusEntryNum.CE_EntryNum);

			instruction.Delete();
			cusEntryNum = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals(null, cusEntryNum);
		}

		public void TestIdentifiersCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var identifier = entryInstruction.Identifiers.AddNew();

			identifier.CSI_Code = "1";
			identifier.CSI_ReferenceNumber = "Ref";
			identifier.CSI_ReferenceNumber2 = "Ref 2";
			identifier.CSI_Description = "Description";

			AssertEquals("Identifiers count should be 1", 1, entryInstruction.Identifiers.Count);
			AssertEquals("CSI_Code should be", "1", entryInstruction.Identifiers[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber should be", "Ref", entryInstruction.Identifiers[0].CSI_ReferenceNumber);
			AssertEquals("CSI_ReferenceNumber2 count should be", "Ref 2", entryInstruction.Identifiers[0].CSI_ReferenceNumber2);
			AssertEquals("CSI_Description should be", "Description", entryInstruction.Identifiers[0].CSI_Description);
		}

		public void TestClearanceCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var clearance = entryInstruction.Clearances.AddNew();

			clearance.CSI_Code = "1234";
			clearance.CSI_ReferenceNumber = "RefNumb";
			clearance.CSI_CustomsOffice = "123";
			clearance.CSI_SubType = "XX";
			clearance.CSI_DateOfIssue = new ZDateTime(2024, 08, 21);
			clearance.CSI_Tariff = "1234578";
			clearance.CSI_UnitOfQuantity = "12";
			clearance.CSI_Quantity = 10.5;
			clearance.CSI_ReferenceNumber2 = "24";

			AssertEquals("Clearance count should be 1", 1, entryInstruction.Clearances.Count);
			AssertEquals("CSI_Code should be", "1234", entryInstruction.Clearances[0].CSI_Code);
			AssertEquals("CSI_ReferenceNumber should be", "RefNumb", entryInstruction.Clearances[0].CSI_ReferenceNumber);
			AssertEquals("CSI_CustomsOffice should be", "123", entryInstruction.Clearances[0].CSI_CustomsOffice);
			AssertEquals("CSI_SubType should be", "XX", entryInstruction.Clearances[0].CSI_SubType);
			AssertEquals("CSI_DateOfIssue should be", new ZDateTime(2024, 08, 21), entryInstruction.Clearances[0].CSI_DateOfIssue);
			AssertEquals("CSI_Tariff should be", "1234578", entryInstruction.Clearances[0].CSI_Tariff);
			AssertEquals("CSI_UnitOfQuantity should be", "12", entryInstruction.Clearances[0].CSI_UnitOfQuantity);
			AssertEquals("CSI_Quantity should be", 10.5m, entryInstruction.Clearances[0].CSI_Quantity);
			AssertEquals("CSI_ReferenceNumber2 count should be", "24", entryInstruction.Clearances[0].CSI_ReferenceNumber2);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)entryInstruction).GetCusSupportingInfoTypes();
			AssertEquals("OOR - Clearance", typeof(Clearance), actualTypes[CusSupportingInfoTypeList.Codes.Clearance]);
			AssertEquals("IDF - Identifier", typeof(Identifier), actualTypes[CusSupportingInfoTypeList.Codes.Identifier]);
		}

		public void TestGetFetchStrategies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var expectedTypes = new[] { typeof(CusSupportingInfoTypeSupporterFetchStrategy) };
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)entryInstruction).GetFetchStrategies().Select(c => c.GetType());

			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}
	}
}
