using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	class EntryHeaderFilterBusinessObjectTest : Customs.Module.Testing.EntryHeaderFilterBusinessObjectAbstractTest
	{
		public void TestACDANumberFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader1 = Factory.NewWithValidTestData<CusEntryHeader>();
			var cusEntryInstruction1 = Factory.NewWithValidTestData<CusEntryInstruction>();
			var cusEntryHeader2 = Factory.NewWithValidTestData<CusEntryHeader>();
			var cusEntryInstruction2 = Factory.NewWithValidTestData<CusEntryInstruction>();
			var cusAttachment1 = Factory.New<CusAttachment>();
			cusEntryHeader1.CH_CEI_Instruction = cusEntryInstruction1.PK;
			cusAttachment1.CY_ParentID = cusEntryInstruction1.PK;
			cusAttachment1.CY_ParentTableCode = cusEntryInstruction1.TablePrefix;
			cusAttachment1.CY_Code = CSDDocTypeList.Codes._10000001;
			cusAttachment1.CY_Data = "12345678910111213";
			var cusAttachment2 = Factory.New<CusAttachment>();
			cusEntryHeader2.CH_CEI_Instruction = cusEntryInstruction2.PK;
			cusAttachment2.CY_ParentID = cusEntryInstruction2.PK;
			cusAttachment2.CY_ParentTableCode = cusEntryInstruction2.TablePrefix;
			cusAttachment2.CY_Code = CSDDocTypeList.Codes._10000002;
			cusAttachment2.CY_Data = "12345678910111213";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterConstants.ACDANumber];
			filter.Property = "123";
			filter.IsActive = true;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "321";
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "123";
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "321";
			filter.IsActive = true;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "123";
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "321";
			filter.IsActive = true;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "123";
			filter.IsActive = true;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "12345678910111213";
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestUNINumberFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.China.DeclarationUnifiedNumber;
			cusEntryNumber1.CE_EntryNum = "test1";
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.China.DeclarationUnifiedNumber;
			cusEntryNumber2.CE_EntryNum = "test2";
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_ParentID = cusEntryHeader1.PK;
			cusEntryNumber4.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber4.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber4.CE_EntryNum = "test4";
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber5 = Factory.New<CusEntryNumber>();
			cusEntryNumber5.CE_ParentID = cusEntryHeader2.PK;
			cusEntryNumber5.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber5.CE_EntryType = CusEntryNumberTypes.China.DeclarationUnifiedNumber;
			cusEntryNumber5.CE_EntryNum = "test5";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterConstants.DeclarationUnifiedNumber];
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test2";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test3";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test4";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test5";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestCIQNumberFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber1.CE_EntryNum = "test1";
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber2.CE_EntryNum = "test2";
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_ParentID = cusEntryHeader1.PK;
			cusEntryNumber4.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber4.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber4.CE_EntryNum = "test4";
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber5 = Factory.New<CusEntryNumber>();
			cusEntryNumber5.CE_ParentID = cusEntryHeader2.PK;
			cusEntryNumber5.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber5.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber5.CE_EntryNum = "test5";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterConstants.CIQNumber];
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test2";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test3";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test4";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test5";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestBLNumberFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = instruction.PK;
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = instruction.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryInstructionSchema.Constants.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.China.BillOfLading;
			cusEntryNumber1.CE_EntryNum = "test1";
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = instruction.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryInstructionSchema.Constants.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.China.BillOfLading;
			cusEntryNumber2.CE_EntryNum = "test2";
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = instruction.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryInstructionSchema.Constants.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader1.CH_CEI_Instruction = instruction1.PK;
			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_ParentID = instruction1.PK;
			cusEntryNumber4.CE_ParentTable = CusEntryInstructionSchema.Constants.TableName;
			cusEntryNumber4.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber4.CE_EntryNum = "test4";
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader2.CH_CEI_Instruction = instruction2.PK;
			var cusEntryNumber5 = Factory.New<CusEntryNumber>();
			cusEntryNumber5.CE_ParentID = instruction2.PK;
			cusEntryNumber5.CE_ParentTable = CusEntryInstructionSchema.Constants.TableName;
			cusEntryNumber5.CE_EntryType = CusEntryNumberTypes.China.BillOfLading;
			cusEntryNumber5.CE_EntryNum = "test5";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterObj[EntryHeaderFilterConstants.BillOfLading];
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test2";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test3";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test4";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.Property = "test5";
			filter.IsActive = true;
			Assert(!cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "test1";
			filter.IsActive = true;
			Assert(cusEntryHeader.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestCIQStatusFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = cusEntryHeader1.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber1.CE_EntryNum = "test1";
			cusEntryNumber1.CE_EntryStatus = "";
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = cusEntryHeader2.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber2.CE_EntryNum = "test2";
			cusEntryNumber2.CE_EntryStatus = "QIR";
			var cusEntryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = cusEntryHeader3.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			cusEntryNumber3.CE_EntryStatus = "QIF";
			var cusEntryHeader4 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_ParentID = cusEntryHeader4.PK;
			cusEntryNumber4.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber4.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber4.CE_EntryNum = "test4";
			cusEntryNumber4.CE_EntryStatus = "QER";
			var cusEntryHeader5 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber5 = Factory.New<CusEntryNumber>();
			cusEntryNumber5.CE_ParentID = cusEntryHeader5.PK;
			cusEntryNumber5.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber5.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber5.CE_EntryNum = "test5";
			cusEntryNumber5.CE_EntryStatus = "QIF";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterConstants.CIQStatus];
			filter.Property = "";
			filter.IsActive = true;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader3.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader4.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader5.MatchesFilter(filterObj.Filter));
			filter.Property = "QIR";
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader3.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader4.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader5.MatchesFilter(filterObj.Filter));
			filter.Property = "QIF";
			filter.IsActive = true;
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader3.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader4.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader5.MatchesFilter(filterObj.Filter));
		}

		public void TestDeclarationDateFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = cusEntryHeader1.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber1.CE_EntryNum = "test1";
			cusEntryNumber1.CE_IssueDate = new ZDate(2019, 7, 1);
			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = cusEntryHeader2.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber2.CE_EntryNum = "test2";
			cusEntryNumber2.CE_IssueDate = new ZDate(2015, 7, 1);
			var cusEntryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = cusEntryHeader3.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.China.CIQNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			cusEntryNumber3.CE_IssueDate = new ZDate(2019, 7, 1);
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EntryHeaderFilterConstants.DeclarationDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			filter.Property1 = new ZDateTime(2019, 06, 1);
			filter.Property2 = new ZDateTime(2019, 08, 1);
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader2.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader3.MatchesFilter(filterObj.Filter));
			filter.Property1 = new ZDateTime(2015, 06, 1);
			filter.Property2 = new ZDateTime(2015, 08, 1);
			Assert(!cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			Assert(!cusEntryHeader3.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Assert(cusEntryHeader1.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader2.MatchesFilter(filterObj.Filter));
			Assert(cusEntryHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsProcedureFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			entry1.CH_CEI_Instruction = instruction1.PK;
			instruction1.CEI_Style = "11";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entry2.CH_CEI_Instruction = instruction2.PK;
			instruction2.CEI_Style = "12";
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterConstants.CustomsProcedure];
			filter.Property = "11";
			filter.IsActive = true;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property = ZString.Empty;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "12";
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestManufacturerBuyerFilter()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "AAA";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "BBB";
			Factory.Save();
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_OH_Manufacturer = orgHeader1.PK;
			declaration2.JE_OH_Buyer = orgHeader2.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleGuidsFilter)filterObj[DeclarationFilterConstants.ManufacturerBuyer];
			filter.IsActive = true;
			filter.Property1 = orgHeader1.PK;
			filter.Property2 = ZGuid.Empty;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = orgHeader2.PK;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			filter.Property1 = orgHeader1.PK;
			filter.Property2 = orgHeader2.PK;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsOfficeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_CustomsOffice = "1500";
			declaration2.JE_CustomsOffice = "2000";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNkFilter)filterObj[DeclarationFilterConstants.CustomsOffice];
			filter.IsActive = true;
			filter.Property = "1500";
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property = "2000";
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			filter.Property = string.Empty;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestTotalWeightFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var invoiceLine11 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine12 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine11.JI_Weight = 10.1m;
			invoiceLine12.JI_Weight = 20.5m;
			var invoiceLine21 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine22 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine21.JI_Weight = 1000m;
			invoiceLine22.JI_Weight = 2001m;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();
			var entryLine2 = entry2.MergedLines.AddNew();
			invoiceLine11.JI_CL = entryLine1.PK;
			invoiceLine12.JI_CL = entryLine1.PK;
			invoiceLine21.JI_CL = entryLine2.PK;
			invoiceLine22.JI_CL = entryLine2.PK;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleNumberRangeFilter)filterObj[DeclarationFilterConstants.TotalWeight];
			filter.IsActive = true;
			filter.Decimals = 3;
			filter.BetweenDefaultProperty1 = 20m;
			filter.BetweenDefaultProperty2 = 40m;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.BetweenDefaultProperty1 = 2000m;
			filter.BetweenDefaultProperty2 = 4000m;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			filter.BetweenDefaultProperty1 = 1m;
			filter.BetweenDefaultProperty2 = 10m;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.BetweenDefaultProperty1 = 10m;
			filter.BetweenDefaultProperty2 = 4000m;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestArchiveFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry1.AddRecordArchivedLog();
			Factory.Save();
			var dateFilterObj = new EntryHeaderFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)dateFilterObj[EntryHeaderFilterConstants.ArchiveDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			var date = ZDateTimeOffset.Today;
			dateFilter.Property1 = date.ToZDateTime();
			dateFilter.Property2 = date.ToZDateTime().AddDays(1);
			Assert(entry1.MatchesFilter(dateFilterObj.Filter));
			Assert(!entry2.MatchesFilter(dateFilterObj.Filter));
			var userFilterObj = new EntryHeaderFilterBusinessObject();
			var userFilter = (ModuleNkFilter)userFilterObj[EntryHeaderFilterConstants.ArchiveUser];
			userFilter.IsActive = true;
			userFilter.Property = ((ICodeDescription)Environment.Env.CurrentUser).Code;
			Assert(entry1.MatchesFilter(userFilterObj.Filter));
			Assert(!entry2.MatchesFilter(userFilterObj.Filter));
		}

		public void TestAuditedFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry1.Logs.AddNew(Events.RecordAudited, "Test audit.");
			Factory.Save();
			var dateFilterObj = new EntryHeaderFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)dateFilterObj[EntryHeaderFilterConstants.AuditedDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			var date = ZDateTimeOffset.Today;
			dateFilter.Property1 = date.ToZDateTime();
			dateFilter.Property2 = date.ToZDateTime().AddDays(1);
			Assert(entry1.MatchesFilter(dateFilterObj.Filter));
			Assert(!entry2.MatchesFilter(dateFilterObj.Filter));
			var userFilterObj = new EntryHeaderFilterBusinessObject();
			var userFilter = (ModuleNkFilter)userFilterObj[EntryHeaderFilterConstants.AuditedUser];
			userFilter.IsActive = true;
			userFilter.Property = ((ICodeDescription)Environment.Env.CurrentUser).Code;
			Assert(entry1.MatchesFilter(userFilterObj.Filter));
			Assert(!entry2.MatchesFilter(userFilterObj.Filter));
		}

		public void TestGenAddOnColumnFilters()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			declaration.JE_OfficeOfEntryExit = "2200";
			instruction.CEI_ManualNo = "MMMMM";
			instruction.CEI_Packages = 3;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var officeOfEntryExitFilter = (ModuleNkFilter)filterObj[EntryHeaderFilterConstants.DeclarationOfficeOfEntryExit];
			var manualNoFilter = (ModuleTextFilter)filterObj[EntryHeaderFilterConstants.EntryInstructionManualNo];
			var packagesFilter = (ModuleNumberRangeFilter)filterObj[EntryHeaderFilterConstants.EntryInstructionPackages];
			officeOfEntryExitFilter.Property = "2201";
			officeOfEntryExitFilter.IsActive = true;
			manualNoFilter.Property = "MMMMA";
			manualNoFilter.IsActive = true;
			packagesFilter.Property1 = 1;
			packagesFilter.Property2 = 2;
			packagesFilter.IsActive = true;
			Assert("Should not match the data with incorrect values.", !entry.MatchesFilter(filterObj.Filter));
			officeOfEntryExitFilter.Property = "2200";
			manualNoFilter.Property = "MMMMM";
			packagesFilter.Property1 = 1;
			packagesFilter.Property2 = 4;
			Assert("Should match the data when properties input.", entry.MatchesFilter(filterObj.Filter));
			packagesFilter.Property1 = 0;
			packagesFilter.Property2 = 0;
			Assert("Should NOT match the data when values = 0.", !entry.MatchesFilter(filterObj.Filter));
			instruction.CEI_Packages = 0;
			Factory.Save();
			Assert("Should match the data when values = 0.", entry.MatchesFilter(filterObj.Filter));
			packagesFilter.Property2 = 1;
			Assert("Should match the data when values = 0.", entry.MatchesFilter(filterObj.Filter));
		}

		public void TestReadyForCompleteDeclarationFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			declaration2.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = JobMessageStatusList.Codes.ClearedPreliminaryDeclaration;
			entry2.CH_Status = JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration;
			Factory.Save();
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterConstants.ReadyForCompleteDeclaration];
			filter.IsActive = true;
			filter.Property = ReadyForCompleteDeclarationFilterHelper.Options.Ready;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(!entry2.MatchesFilter(filterObj.Filter));
			filter.Property = ReadyForCompleteDeclarationFilterHelper.Options.NotReady;
			Assert(!entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
			filter.Property = ReadyForCompleteDeclarationFilterHelper.Options.All;
			Assert(entry1.MatchesFilter(filterObj.Filter));
			Assert(entry2.MatchesFilter(filterObj.Filter));
		}

		public void TestModuleFiltersAreAdded()
		{
			var moduleFilters = new List<ZString>()
			{ EntryHeaderFilterConstants.DeclarationUnifiedNumber, EntryHeaderFilterConstants.CIQNumber, EntryHeaderFilterConstants.BillOfLading, EntryHeaderFilterConstants.CIQStatus, EntryHeaderFilterConstants.DeclarationDate, EntryHeaderFilterConstants.CustomsProcedure, EntryHeaderFilterConstants.DeclarationOfficeOfEntryExit, EntryHeaderFilterConstants.EntryInstructionManualNo, EntryHeaderFilterConstants.EntryInstructionPackages, Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, DeclarationFilterConstants.ManufacturerBuyer, Customs.Module.DeclarationFilterConstants.ShipmentType, Customs.Module.DeclarationFilterConstants.TransportMode, DeclarationFilterConstants.CustomsOffice, DeclarationFilterConstants.TotalWeight, EntryHeaderFilterConstants.ArchiveDate, EntryHeaderFilterConstants.ArchiveUser, EntryHeaderFilterConstants.AuditedDate, EntryHeaderFilterConstants.AuditedUser, EntryHeaderFilterConstants.ReadyForCompleteDeclaration };
			var filterObj = GetNewFilterStripBusinessObject();
			foreach (var filter in moduleFilters)
			{
				AssertNotNull(filterObj[filter]);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
