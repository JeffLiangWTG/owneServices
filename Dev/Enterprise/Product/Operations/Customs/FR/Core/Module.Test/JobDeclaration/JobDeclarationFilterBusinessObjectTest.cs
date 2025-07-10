using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	public class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<JobDeclarationFilterLookups>(filterBizObj.Lookups);
		}

		public void TestTrigPointForValFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();

			var entryHeader11 = declaration1.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader11.CH_TriggeringPointForValidation = "PAB";

			var entryHeader12 = declaration1.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader12.CH_TriggeringPointForValidation = "";
			Factory.Save();

			var entryHeader13 = declaration1.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader13.CH_TriggeringPointForValidation = "";
			entryHeader13.CH_TriggeringPointForValidation = "NUL";
			Factory.Save();

			var declaration2 = Factory.New<JobDeclaration>();

			var entryHeader21 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader21.CH_TriggeringPointForValidation = "PAB";
			Factory.Save();

			var declaration3 = Factory.New<JobDeclaration>();

			var entryHeader31 = declaration3.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader31.CH_TriggeringPointForValidation = "";
			Factory.Save();

			var declaration4 = Factory.New<JobDeclaration>();

			var entryHeader41 = declaration4.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader41.CH_TriggeringPointForValidation = "";
			entryHeader41.CH_TriggeringPointForValidation = "NUL";
			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.FilterConstants.TrigPointForVal];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PAB";

			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3.MatchesFilter(filterObj.Filter));
			Assert(!declaration4.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3.MatchesFilter(filterObj.Filter));
			Assert(declaration4.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3.MatchesFilter(filterObj.Filter));
			Assert(!declaration4.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3.MatchesFilter(filterObj.Filter));
			Assert(declaration4.MatchesFilter(filterObj.Filter));
		}

		public void TestCorrelationIDFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			Factory.Save();
			var entryHeader1 = dec1.CustomsEntryHeaders.AddNew();
			CusEntryNumber entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.Parent = entryHeader1;
			entryNum1.CE_EntryType = "LRN";
			entryNum1.CE_EntryNum = "1234567890";
			entryHeader1.CH_JE = dec1.PK;
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			Factory.Save();
			var entryHeader2 = dec2.CustomsEntryHeaders.AddNew();
			CusEntryNumber entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.Parent = entryHeader2;
			entryNum2.CE_EntryType = "LRN";
			entryNum2.CE_EntryNum = "987654321";
			entryHeader2.CH_JE = dec2.PK;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.CorrelationID];
			AssertEquals("The default comparation operator is 'Contains'.", SQLComparisonOperator.Contains, filter.SqlComparisonOperator);

			filter.IsActive = true;
			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertEquals(true, dec1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(filterBizObj.Filter));

			entryNum1.CE_EntryNum = "4321";
			entryNum2.CE_EntryNum = "6789";
			Factory.Save();
			filter.Property = "32";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals(true, dec1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(filterBizObj.Filter));

			entryNum1.CE_EntryNum = "4321";
			Factory.Save();
			filter.Property = "4321";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals(true, dec1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(filterBizObj.Filter));

			entryNum1.CE_EntryNum = "1234";
			entryNum2.CE_EntryNum = "4321";
			Factory.Save();
			filter.Property = "4321";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals(true, dec1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(filterBizObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(true, dec1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, dec2.MatchesFilter(filterBizObj.Filter));

			entryNum1.CE_EntryNum = "6789";
			entryNum2.CE_EntryNum = "4321";
			Factory.Save();
			filter.Property = "4321";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			AssertEquals(true, dec1.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, dec2.MatchesFilter(filterBizObj.Filter));
		}

		public void TestCorrelationIDFilter_DefaultProperty()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "WTL"))
			{
				Env.Registry.PhysicalServerID = "FRM";
				GlbCompany.CurrentCompany.GC_Code = "DFR";

				var licenceCode = "WTLDFRFRM";

				var correlationIdFilter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.CorrelationID];
				var messageSystemFilter = (ModuleTextFilter)filterBizObj[Customs.Module.DeclarationFilterConstants.SubmitType];

				CombineAssertions(() =>
				{
					correlationIdFilter.Property = ZString.Empty;
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.DeltaIE;
					AssertEquals("CorrelationId value = Empty, messageSystem = DI, DefaultProperty:", licenceCode, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = Empty, messageSystem = DI, Property:", licenceCode, correlationIdFilter.Property);

					correlationIdFilter.Property = ZString.Empty;
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.DeltaG;
					AssertEquals("CorrelationId value = Empty, messageSystem = DG, DefaultProperty:", ZString.Empty, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = Empty, messageSystem = DG, Property:", ZString.Empty, correlationIdFilter.Property);

					correlationIdFilter.Property = ZString.Empty;
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.Interface;
					AssertEquals("CorrelationId value = Empty, messageSystem = ITF, DefaultProperty:", ZString.Empty, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = Empty, messageSystem = ITF, Property:", ZString.Empty, correlationIdFilter.Property);

					correlationIdFilter.Property = licenceCode;
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.DeltaIE;
					AssertEquals("CorrelationId value = WTLDFRFRM, messageSystem = DI, DefaultProperty:", licenceCode, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = WTLDFRFRM, messageSystem = DI, Property:", licenceCode, correlationIdFilter.Property);

					correlationIdFilter.Property = licenceCode;
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.DeltaG;
					AssertEquals("CorrelationId value = WTLDFRFRM, messageSystem = DG, DefaultProperty:", ZString.Empty, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = WTLDFRFRM, messageSystem = DG, Property:", ZString.Empty, correlationIdFilter.Property);

					correlationIdFilter.Property = licenceCode;
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.Interface;
					AssertEquals("CorrelationId value = WTLDFRFRM, messageSystem = IFT, DefaultProperty:", ZString.Empty, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = WTLDFRFRM, messageSystem = IFT, Property:", ZString.Empty, correlationIdFilter.Property);

					correlationIdFilter.Property = "XXX";
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.DeltaIE;
					AssertEquals("CorrelationId value = XXX, messageSystem = DI, DefaultProperty:", licenceCode, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = XXX, messageSystem = DI, Property:", "XXX", correlationIdFilter.Property);

					correlationIdFilter.Property = "XXX";
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.DeltaG;
					AssertEquals("CorrelationId value = XXX, messageSystem = DG, DefaultProperty:", ZString.Empty, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = XXX, messageSystem = DG, Property:", "XXX", correlationIdFilter.Property);

					correlationIdFilter.Property = "XXX";
					messageSystemFilter.Property = DeclarationApplicationCodeList.Codes.Interface;
					AssertEquals("CorrelationId value = XXX, messageSystem = IFT, DefaultProperty:", ZString.Empty, correlationIdFilter.DefaultProperty);
					AssertEquals("CorrelationId value = XXX, messageSystem = IFT, Property:", "XXX", correlationIdFilter.Property);
				});
			}
		}

		public void TestJobDeclarationFilterWithDeltaMode()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.DeltaMode];
			filter.IsActive = true;
			filter.Property = OrgCusAccountDeltaGTypeList.Codes.G1;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			CombineAssertions(() =>
			{
				var query = filterBizObj.Filter;
				AssertEquals("Dec 1 matches", true, dec1.MatchesFilter(query));
				AssertEquals("Dec 2 no match", false, dec2.MatchesFilter(query));
			});
		}

		public void TestJobDeclarationFilterWithIsDeltaDOneStep()
		{
			var orgHeaderPK = SetupOrgWithAccount().PK;
			var dec1 = SetupDeclaration(orgHeaderPK, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES100);
			var dec2 = SetupDeclaration(orgHeaderPK, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130);
			var dec3 = SetupDeclaration(orgHeaderPK, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100);
			Factory.Save();

			var filter = (ModuleFlagsFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.IsDeltaDOneStepSent];
			filter.IsActive = true;
			filter.Property0 = ZBool.True;
			CombineAssertions(() =>
			{
				var query = filterBizObj.Filter;
				AssertEquals("Dec 1 matches", true, dec1.MatchesFilter(query));
				AssertEquals("Dec 2 no match", false, dec2.MatchesFilter(query));
				AssertEquals("Dec 3 no match", false, dec3.MatchesFilter(query));
			});
		}

		public void TestJobDeclarationFilterWithIsDeltaDTwoStep()
		{
			var orgHeaderPK = SetupOrgWithAccount().PK;
			var dec1 = SetupDeclaration(orgHeaderPK, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130);
			var dec2 = SetupDeclaration(orgHeaderPK, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES100);
			var dec3 = SetupDeclaration(orgHeaderPK, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES130);
			Factory.Save();

			var filter = (ModuleFlagsFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.IsDeltaDTwoStepSent];
			filter.IsActive = true;
			filter.Property0 = ZBool.True;
			CombineAssertions(() =>
			{
				var query = filterBizObj.Filter;
				AssertEquals("Dec 1 matches", true, dec1.MatchesFilter(query));
				AssertEquals("Dec 2 no match", false, dec2.MatchesFilter(query));
				AssertEquals("Dec 3 no match", false, dec3.MatchesFilter(query));
			});
		}

		public void TestJobDeclarationFilterWithTaxesAndFees()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = "IMP";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_TotalPaid = 50;
			var entry2 = dec1.CustomsEntryHeaders.AddNew();
			entry2.CH_TotalPaid = 100;
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = "IMP";
			var entry3 = dec2.CustomsEntryHeaders.AddNew();
			entry3.CH_TotalPaid = 10;
			var entry4 = dec2.CustomsEntryHeaders.AddNew();
			entry4.CH_TotalPaid = 30;
			Factory.Save();

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = "IMP";
			var entry5 = dec3.CustomsEntryHeaders.AddNew();
			entry5.CH_TotalPaid = 300;
			var entry6 = dec3.CustomsEntryHeaders.AddNew();
			entry6.CH_TotalPaid = 200;
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.TaxesAndFees];
			filter.IsActive = true;
			filter.Property1 = 90;
			filter.Property2 = 200;
			Assert(dec1.MatchesFilter(filterBizObj.Filter));
			Assert(!dec2.MatchesFilter(filterBizObj.Filter));
			Assert(!dec3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestJobDeclarationFilterWithDeltaGFallbackNumber()
		{
			var declaration = CreateFallbackData();

			var filter = (ModuleNumberFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.DeltaGFallbackNumber];
			filter.IsActive = true;
			filter.Property = "1234567890";
			Assert(!declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = declaration.CustomsEntryHeaders[0].FRCustomsFallbackNumber;
			Assert(declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = declaration.CustomsEntryHeaders[1].FRCustomsFallbackNumber;
			Assert(declaration.MatchesFilter(filterBizObj.Filter));
		}

		public void TestJobDeclarationFilterWithDeltaGFallbackStatus()
		{
			var declaration = CreateFallbackData();

			CusEntryNumber entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			entryNum.CE_ParentID = declaration.CustomsEntryHeaders[0].PK;
			entryNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNum.CE_EntryStatus = "PPW";
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum.CE_EntryNum = "1234567890";

			CusEntryNumber entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			entryNum2.CE_ParentID = declaration.CustomsEntryHeaders[1].PK;
			entryNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNum2.CE_EntryStatus = "PPS";
			entryNum2.CE_IssueDate = ZDateTime.Today;
			entryNum2.CE_EntryNum = "1234567890";
			declaration.CustomsEntryHeaders[0].InitFRCustomsFallbackEntryNumber();
			Factory.Save();

			var filter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.DeltaGFallbackStatus];
			filter.IsActive = true;
			filter.Property = "PPW";
			Assert(declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = "PPS";
			Assert(declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = "PDS";
			Assert(!declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = "RGA";
			Assert(!declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = "RGM";
			Assert(!declaration.MatchesFilter(filterBizObj.Filter));

			filter.Property = "ERR";
			Assert(!declaration.MatchesFilter(filterBizObj.Filter));
		}

		public void TestApplicationCodeFilterCaption()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertEquals("Messaging System", stripBO.ApplicationCodeFilterCaption.ToString());
		}

		public void TestECSStatusFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = "IMP";
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitedStatus = "";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = "IMP";
			var entry3 = dec2.CustomsEntryHeaders.AddNew();
			entry3.CH_ExitedStatus = "SOR";
			var entry4 = dec2.CustomsEntryHeaders.AddNew();
			entry4.CH_ExitedStatus = "ENF";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = "IMP";
			var entry5 = dec3.CustomsEntryHeaders.AddNew();
			entry5.CH_ExitedStatus = "ENF";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = "IMP";
			Factory.Save();

			CombineAssertions(() =>
			{
				var filter = (ModuleTextFilter)filterBizObj[JobDeclarationFilterBusinessObject.FilterConstants.EntryExitedStatus];
				AssertContainsExactElementsInAnyOrder("filter list", Factory.GetCachedValue<ExportControlStatusList>(), filter.List);

				filter.IsActive = true;
				filter.Property = "ENF";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				Assert("ENF not match empty", !dec1.MatchesFilter(filterBizObj.Filter));
				Assert("ENF match multi header", dec2.MatchesFilter(filterBizObj.Filter));
				Assert("ENF match", dec3.MatchesFilter(filterBizObj.Filter));
				Assert("ENF not match empty header", !dec4.MatchesFilter(filterBizObj.Filter));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				Assert("not ENF match empty", dec1.MatchesFilter(filterBizObj.Filter));
				Assert("not ENF not match multi header", !dec2.MatchesFilter(filterBizObj.Filter));
				Assert("not ENF not match", !dec3.MatchesFilter(filterBizObj.Filter));
				Assert("not ENF match empty header", dec4.MatchesFilter(filterBizObj.Filter));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				Assert("is blank match", dec1.MatchesFilter(filterBizObj.Filter));
				Assert("is blank not match", !dec2.MatchesFilter(filterBizObj.Filter));
				Assert("is blank not match", !dec3.MatchesFilter(filterBizObj.Filter));
				Assert("is blank match empty header", dec1.MatchesFilter(filterBizObj.Filter));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				Assert("is not blank not match", !dec1.MatchesFilter(filterBizObj.Filter));
				Assert("is not blank match", dec2.MatchesFilter(filterBizObj.Filter));
				Assert("is not blank match", dec3.MatchesFilter(filterBizObj.Filter));
				Assert("is not blank match empty header", !dec1.MatchesFilter(filterBizObj.Filter));
			});
		}

		public void TestAssesmentDateFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryheader = declaration.ActiveEntryHeaders.AddNew();
			entryheader.CH_CEI_Instruction = entryinstruction.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			var entryinstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			var entryheader2 = declaration2.ActiveEntryHeaders.AddNew();
			entryheader2.CH_CEI_Instruction = entryinstruction2.PK;
			var entryinstruction21 = declaration2.CustomsEntryInstructions.AddNew();
			var entryheader21 = declaration2.ActiveEntryHeaders.AddNew();
			entryheader21.CH_CEI_Instruction = entryinstruction21.PK;

			var declaration3 = Factory.New<JobDeclaration>();
			var entryinstruction3 = declaration3.CustomsEntryInstructions.AddNew();
			var entryheader3 = declaration3.ActiveEntryHeaders.AddNew();
			entryheader3.CH_CEI_Instruction = entryinstruction3.PK;
			entryinstruction3.CEI_DateForDuty = ZDateTime.Empty;

			var setDateTime = new ZDateTime(2021, 08, 27, 00, 00, 00);
			var setDateTime2 = new ZDateTime(2022, 08, 27, 00, 00, 00);
			entryinstruction.CEI_DateForDuty = setDateTime;
			entryinstruction21.CEI_DateForDuty = setDateTime;
			entryinstruction2.CEI_DateForDuty = setDateTime2;
			Factory.Save();

			AssertEquals("Prerequisite: make sure declaration.Assessment Date has a value.", setDateTime.ToString("dd/MM/yyyy"), declaration.AssessmentDate);
			AssertEquals("Prerequisite: make sure declaration2.Assessment Date has value 'MLT'.", "MLT", declaration2.AssessmentDate);
			AssertEquals("Prerequisite: make sure declaration3.Assessment Date has no value.", "", declaration3.AssessmentDate);

			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[JobDeclarationFilterBusinessObject.FilterConstants.AssessmentDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			CombineAssertions("Property1 : empty, property2: empty => ", () =>
			{
				AssertEquals("First declaration should match for the filter has no boundaries.", true, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should match for the filter has no boundaries.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should match for the filter has no boundaries.", true, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime.AddDays(-10);
			filter.Property2 = setDateTime.AddDays(-1);
			CombineAssertions("Property1 : setDateTime.AddDays(-10), property2: setDateTime.AddDays(-1) => ", () =>
			{
				AssertEquals("First declaration should not match because its Assessment Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should not match because its Assessment Date is MLT but every CEI_DateForDuty is outside of the filter boundaries.", false, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime.AddDays(-1);
			filter.Property2 = setDateTime.AddDays(1);
			CombineAssertions("Property1 : setDateTime.AddDays(-1), property2: setDateTime.AddDays(1) => ", () =>
			{
				AssertEquals("First declaration should match because its Assessment Date is inside of the filter boundaries.", true, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should match because its Assessment Date is MLT but one of his CEI_DateForDuty is inside of the filter boundaries.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime.AddDays(1);
			filter.Property2 = setDateTime.AddDays(10);
			CombineAssertions("Property1 : setDateTime.AddDays(1), property2: setDateTime.AddDays(10) => ", () =>
			{
				AssertEquals("First declaration should not match because its Assessment Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should not match because its Assessment Date is MLT but every CEI_DateForDuty is outside of the filter boundaries.", false, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime.AddDays(-10);
			filter.Property2 = ZDateTime.Empty;
			CombineAssertions("Property1 : setDateTime.AddDays(-10), property2: ZDateTime.Empty => ", () =>
			{
				AssertEquals("First declaration should match because its Assessment Date is inside of the filter boundaries.", true, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should match because its Assessment Date is MLT but one of his CEI_DateForDuty is inside of the filter boundaries.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime;
			filter.Property2 = ZDateTime.Empty;
			CombineAssertions("Property1 : setDateTime, property2: ZDateTime.Empty => ", () =>
			{
				AssertEquals("First declaration should match because its Assessment Date is inside of the filter boundaries.", true, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should match because its Assessment Date is MLT but one of his CEI_DateForDuty is inside of the filter boundaries.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime.AddDays(1);
			filter.Property2 = ZDateTime.Empty;
			CombineAssertions("Property1 : setDateTime.AddDays(1), property2: ZDateTime.Empty => ", () =>
			{
				AssertEquals("First declaration should not match because its Assessment Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should not match because its Assessment Date is MLT but one of CEI_DateForDuty is inside of the filter boundaries.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = setDateTime.AddDays(-1);
			CombineAssertions("Property1 : ZDateTime.Empty, property2:  setDateTime.AddDays(-1) => ", () =>
			{
				AssertEquals("First declaration should not match because its Assessment Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should not match because its Assessment Date is MLT but every CEI_DateForDuty is outside of the filter boundaries.", false, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.Property1 = setDateTime2;
			filter.Property2 = ZDateTime.Empty;
			CombineAssertions("Property1 : setDateTime2, property2: ZDateTime.Empty => ", () =>
			{
				AssertEquals("First declaration should not match because its Assessment Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should match because its Assessment Date is MLT but one of his CEI_DateForDuty is inside of the filter boundaries.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match for the filter has boundaries but Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			CombineAssertions("ModuleDateFilter.HasDateEntered => ", () =>
			{
				AssertEquals("First declaration should match because its Assessment Date is not empty.", true, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should match because its Assessment Date is not empty.", true, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should not match because its Assessment Date is empty.", false, declaration3.MatchesFilter(filterObj.Filter));
			});

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			CombineAssertions("ModuleDateFilter.HasNoDateEntered => ", () =>
			{
				AssertEquals("First declaration should not match because its Assessment Date is not empty.", false, declaration.MatchesFilter(filterObj.Filter));
				AssertEquals("Second declaration should not match because its Assessment Date is not empty.", false, declaration2.MatchesFilter(filterObj.Filter));
				AssertEquals("Third declaration should match because its Assessment Date is empty.", true, declaration3.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestExitTypeFilter()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.FilterConstants.ExportExitType];

			AssertEquals("filter maxlength", filter.MaxLength, JobDeclaration.Schema.JE_ExportExitTypeMaxLength);
			AssertEquals("filter description", filter.MultilingualDescription, JobDeclarationFilterBusinessObject.FilterConstants.ExportExitType);

			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var declaration3 = Factory.New<JobDeclaration>();

			declaration1.JE_ExportExitType = ExportExitTypeList.Codes.OTH;
			declaration2.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			declaration3.JE_ExportExitType = ZString.Empty;
			Factory.Save();

			filter.IsActive = true;
			filter.Property = ExportExitTypeList.Codes.OTH;

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration3.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
		}
		JobDeclarationFilterBusinessObject filterBizObj;

		JobDeclaration CreateFallbackData()
		{
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Empty;
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 10;
			invoiceLine1.JI_Weight = 5;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 10;
			invoiceLine2.JI_Weight = 5;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader.CH_BGMReference = "19212081311";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			var cusEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			cusEntryHeader2.CH_BGMReference = "19212081312";

			cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_CL = cusEntryLine.PK;

			Factory.Save();
			return declaration;
		}

		OrgHeader SetupOrgWithAccount()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.Addresses.AddNewMainAddress();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount.CZ_OH = orgHeader.PK;
			orgCusAccount.CZ_Account = "12345";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			return orgHeader;
		}

		JobDeclaration SetupDeclaration(ZGuid importerPK, ZString deltaMode, ZString entryStatus)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importerPK;
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = deltaMode;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = entryStatus;
			return declaration;
		}
	}
}
