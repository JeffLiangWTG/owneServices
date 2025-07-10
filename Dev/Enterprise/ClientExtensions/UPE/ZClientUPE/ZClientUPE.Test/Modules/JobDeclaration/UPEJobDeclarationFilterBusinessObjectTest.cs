using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.UPE.Module.UPEJobDeclarationFilterBusinessObject;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEJobDeclarationFilterBusinessObject))]
	public class UPEJobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultFromDate()
		{
			filterBizObj = null;
			ModuleDateFilter filter = (ModuleDateFilter)FilterBizObj[UPEFilterConstants.ArrivalDate];
			AssertEquals("Default value", UPEFilterConstants.DefaultFromDate.ToShortDateString(), filter.Property1.ToShortDateString());
			filter.Property1 = ZDateTime.Empty;
			AssertEquals("has errors", true, filter.Property1Info.HasErrors());
		}

		#region NK Filters
		public void TestAssignedToFilter()
		{
			ModuleNkFilter assignedToLevelFilter = (ModuleNkFilter)FilterBizObj[UPEFilterConstants.JobDeclarationNumberTypes.AssignedTo];
			UPEJobDeclaration expectedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedJobDec.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "OP1";
			UPEJobDeclaration excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			excludedJobDec.CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = "OP2";
			assignedToLevelFilter.Property = "OP1";
			QueueFilterHelper.ProcessQueueSubGroup assignedToSubGroup = new QueueFilterHelper.ProcessQueueSubGroup(typeof(UPEJobDeclaration));
			AssertFilterWithExpectedResults(assignedToSubGroup.GetSubQuery(assignedToLevelFilter.Query), expectedJobDec);
		}

		public void TestServiceLevelFilter()
		{
			ModuleNkFilter serviceLevelFilter = (ModuleNkFilter)FilterBizObj[DeclarationFilterConstants.ServiceLevel];
			UPEJobDeclaration expectedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedJobDec.JE_RS_NKServiceLevel = "BLA";
			UPEJobDeclaration excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			excludedJobDec.JE_RS_NKServiceLevel = "BOB";
			serviceLevelFilter.Property = "BLA";
			AssertFilterWithExpectedResults(serviceLevelFilter.Query, expectedJobDec);
		}

		#endregion
		#region Text Filters
		public void TestConsigneeDetailsFilters()
		{
			var expectedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedJobDec.JE_OH_Importer = CreateOrgAndAddress("blah").PK;
			var excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			excludedJobDec.JE_OH_Importer = CreateOrgAndAddress("bob").PK;
			ZString value = "blah";
			QueueFilterHelper.OrgCusCodeSubGroup orgCusCodeSubGroup = new QueueFilterHelper.OrgCusCodeSubGroup(JobDeclarationSchema.JE_OH_Importer);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeAccountID, value, expectedJobDec, orgCusCodeSubGroup);
			QueueFilterHelper.OrgFullNameSubGroup orgFullNameImporterSubGroup = new QueueFilterHelper.OrgFullNameSubGroup(JobDeclarationSchema.JE_OH_Importer);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeName, value, expectedJobDec, orgFullNameImporterSubGroup);
			QueueFilterHelper.OrgAddressSubGroup orgAddressSubGroup = new QueueFilterHelper.OrgAddressSubGroup(typeof(JobDeclaration), JobDeclarationSchema.JE_OH_Importer);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeStreet, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeCity, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeState, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePostcode, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePhone, value, expectedJobDec, orgAddressSubGroup);
		}

		public void TestConsignorDetailsFilters()
		{
			var expectedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedJobDec.JE_OH_Supplier = CreateOrgAndAddress("blah").PK;
			var excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			excludedJobDec.JE_OH_Supplier = CreateOrgAndAddress("bob").PK;
			ZString value = "blah";
			QueueFilterHelper.OrgCusCodeSubGroup orgCusCodeSubGroup = new QueueFilterHelper.OrgCusCodeSubGroup(JobDeclarationSchema.JE_OH_Supplier);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorAccountID, value, expectedJobDec, orgCusCodeSubGroup);
			QueueFilterHelper.OrgFullNameSubGroup orgFullNameSupplierSubGroup = new QueueFilterHelper.OrgFullNameSubGroup(JobDeclarationSchema.JE_OH_Supplier);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorName, value, expectedJobDec, orgFullNameSupplierSubGroup);
			QueueFilterHelper.OrgAddressSubGroup orgAddressSubGroup = new QueueFilterHelper.OrgAddressSubGroup(typeof(JobDeclaration), JobDeclarationSchema.JE_OH_Supplier);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorStreet, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorCity, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorState, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPostcode, value, expectedJobDec, orgAddressSubGroup);
			AssertSchemaFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPhone, value, expectedJobDec, orgAddressSubGroup);
		}

		void AssertSchemaFilter(ZString filterName, ZString value, UPEJobDeclaration expectedJobDec, ModuleFilterSubGroup subGroup)
		{
			var orgDetailsFilter = (ModuleTextFilter)FilterBizObj[filterName];
			orgDetailsFilter.Property = value;
			if (subGroup != null)
			{
				orgDetailsFilter.SubGroup = subGroup;
				AssertFilterWithExpectedResults(subGroup.GetSubQuery(orgDetailsFilter.Query), expectedJobDec);
			}
			else
			{
				AssertFilterWithExpectedResults(orgDetailsFilter.Query, expectedJobDec);
			}
		}

		public void TestQueueRemarksFilter()
		{
			ModuleTextFilter queueRemarksFilter = (ModuleTextFilter)FilterBizObj[UPEFilterConstants.JobDeclarationNumberTypes.QueueRemarks];
			UPEJobDeclaration expectedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedJobDec.CurrentQueue.P4_CustomsReason = "blah blah";
			UPEJobDeclaration excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			excludedJobDec.CurrentQueue.P4_CustomsReason = "bob blah";
			queueRemarksFilter.Property = "blah";
			QueueFilterHelper.ProcessQueueSubGroup queueRemarkSubGroup = new QueueFilterHelper.ProcessQueueSubGroup(typeof(UPEJobDeclaration));
			AssertFilterWithExpectedResults(queueRemarkSubGroup.GetSubQuery(queueRemarksFilter.Query), expectedJobDec);
		}

		public void TestZoneFilter()
		{
			ModuleTextFilter zoneFilter = (ModuleTextFilter)FilterBizObj[UPEFilterConstants.Zone];
			UPEJobDeclaration jobDecWithMetroPostcode = Factory.NewWithValidTestData<UPEJobDeclaration>();
			jobDecWithMetroPostcode.JE_HouseBill = "job dec 1";
			jobDecWithMetroPostcode.JE_OH_Importer = CreateOrgWithPostcode("100").PK;
			UPEJobDeclaration jobDecWithCountryPostcode = Factory.NewWithValidTestData<UPEJobDeclaration>();
			jobDecWithCountryPostcode.JE_HouseBill = "job dec 2";
			jobDecWithCountryPostcode.JE_OH_Importer = CreateOrgWithPostcode("150").PK;
			UPEJobDeclaration jobDecWithOutsidePostcodeRange = Factory.NewWithValidTestData<UPEJobDeclaration>();
			jobDecWithOutsidePostcodeRange.JE_HouseBill = "job dec 3";
			jobDecWithOutsidePostcodeRange.JE_OH_Importer = CreateOrgWithPostcode("201").PK;
			var metroPostcode = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "100", "149", "Sydney Metro");
			var countryPostcode = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "150", "200", "NSW Country");
			zoneFilter.Property = "Sydney Metro";
			AssertFilterWithExpectedResults(zoneFilter.Query, jobDecWithMetroPostcode);
			zoneFilter.Property = "NSW Country";
			AssertFilterWithExpectedResults(zoneFilter.Query, jobDecWithCountryPostcode);
			zoneFilter.Property = UPEFilterConstants.MetroCountry.Metro;
			AssertFilterWithExpectedResults(zoneFilter.Query, jobDecWithMetroPostcode);
			zoneFilter.Property = UPEFilterConstants.MetroCountry.Other;
			AssertFilterWithExpectedResults(zoneFilter.Query, jobDecWithCountryPostcode, jobDecWithOutsidePostcodeRange);
		}

		#endregion
		#region Date Filters
		public void TestArrivalDateFilter()
		{
			ModuleDateFilter arrivalDateFilter = (ModuleDateFilter)FilterBizObj[UPEFilterConstants.ArrivalDate];
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			UPEJobDeclaration[] expectedResults = new UPEJobDeclaration[2];
			expectedResults[0] = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedResults[0].JE_DateOfArrival = testDate.AddDays(-1);
			expectedResults[1] = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedResults[1].JE_DateOfArrival = testDate.AddDays(1);
			arrivalDateFilter.Property1 = testDate;
			arrivalDateFilter.Property2 = ZDateTime.Empty;
			AssertFilterWithExpectedResults(arrivalDateFilter.Query, expectedResults[1]);
			arrivalDateFilter.Property1 = ZDateTime.Empty;
			arrivalDateFilter.Property2 = testDate;
			AssertFilterWithExpectedResults(arrivalDateFilter.Query, expectedResults[0]);
			arrivalDateFilter.Property1 = testDate.AddDays(-2);
			arrivalDateFilter.Property2 = testDate.AddDays(2);
			AssertFilterWithExpectedResults(arrivalDateFilter.Query, expectedResults);
		}

		#endregion
		#region Flag Filters
		public void TestRefundAndAuditFilter()
		{
			UPEJobDeclaration excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			excludedJobDec.IsRefundEnquiry = ZBool.False;
			excludedJobDec.IsAudit = ZBool.False;
			ModuleFlagsFilter refundAndAuditdFilter = (ModuleFlagsFilter)FilterBizObj[UPEFilterConstants.JobDeclarationNumberTypes.RefundAndAudit];
			UPEJobDeclaration expectedResult = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedResult.IsRefundEnquiry = ZBool.True;
			expectedResult.IsAudit = ZBool.False;
			refundAndAuditdFilter.Property0 = ZBool.True;
			refundAndAuditdFilter.Property1 = ZBool.False;
			AssertFilterWithExpectedResults(refundAndAuditdFilter.Query, expectedResult);
			expectedResult = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedResult.IsRefundEnquiry = ZBool.False;
			expectedResult.IsAudit = ZBool.True;
			refundAndAuditdFilter.Property0 = ZBool.False;
			refundAndAuditdFilter.Property1 = ZBool.True;
			AssertFilterWithExpectedResults(refundAndAuditdFilter.Query, expectedResult);
			expectedResult = Factory.NewWithValidTestData<UPEJobDeclaration>();
			expectedResult.IsRefundEnquiry = ZBool.True;
			expectedResult.IsAudit = ZBool.True;
			refundAndAuditdFilter.Property0 = ZBool.True;
			refundAndAuditdFilter.Property1 = ZBool.True;
			AssertFilterWithExpectedResults(refundAndAuditdFilter.Query, expectedResult);
		}

		public void TestUnworkedFilter()
		{
			UPEJobDeclaration[] expectedResults = new UPEJobDeclaration[2];
			UPEJobDeclaration excludedJobDec = Factory.NewWithValidTestData<UPEJobDeclaration>();
			SetJobDecLastedEditedDate(excludedJobDec, testDate.AddDays(1));
			expectedResults[0] = Factory.NewWithValidTestData<UPEJobDeclaration>();
			ModuleFlagsFilter unworkedFilter = (ModuleFlagsFilter)FilterBizObj[UPEFilterConstants.QueueReasonFilters.Unworked];
			unworkedFilter.Property0 = ZBool.True;
			unworkedFilter.Property1 = ZBool.False;
			AssertFilterWithExpectedResults(unworkedFilter.Query, expectedResults[0]);
			expectedResults[1] = Factory.NewWithValidTestData<UPEJobDeclaration>();
			SetJobDecLastedEditedDate(expectedResults[1], testDate.AddDays(-1));
			unworkedFilter.Property0 = ZBool.False;
			unworkedFilter.Property1 = ZBool.True;
			AssertFilterWithExpectedResults(unworkedFilter.Query, expectedResults);
			unworkedFilter.Property0 = ZBool.True;
			unworkedFilter.Property1 = ZBool.True;
			AssertFilterWithExpectedResults(unworkedFilter.Query, expectedResults[0]);
		}

		#endregion
		#region Guid Filters
		public void TestAccountClassFilter()
		{
			OrgDebtorGroup debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			debtorGroup.OJ_Code = "CL1";
			OrgDebtorGroup decoyDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			decoyDebtorGroup.OJ_Code = "DCY";
			UPECusHAWB cusHAWB = CreateCusHAWBWithDeclaration();
			cusHAWB.BillToAccountNumber = "BillToAccountNum";
			OrgHeader billTo = CreateOrgWithCustomsRegNo(cusHAWB.BillToAccountNumber);
			billTo.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			UPECusHAWB decoyCusHAWB = CreateCusHAWBWithDeclaration();
			decoyCusHAWB.BillToAccountNumber = "BillToAccountNum2";
			billTo = CreateOrgWithCustomsRegNo(cusHAWB.BillToAccountNumber);
			billTo.CompanyData.OB_OJ_ARDebtorGroup = decoyDebtorGroup.PK;
			decoyCusHAWB = CreateCusHAWBWithDeclaration();
			decoyCusHAWB.BillToAccountNumber = "BillToAccountNum3";
			billTo = CreateOrgWithCustomsRegNo(cusHAWB.BillToAccountNumber);
			billTo.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			billTo.CompanyData.OB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			Factory.Save();
			ModuleGuidFilter accountClassFilter = (ModuleGuidFilter)FilterBizObj[UPEFilterConstants.JobDeclarationNumberTypes.AccountClass];
			accountClassFilter.Property = debtorGroup.PK;
			AccountClassSubGroup accountClassSubGroup = new AccountClassSubGroup();
			AssertFilterWithExpectedResults(accountClassSubGroup.GetSubQuery(accountClassFilter.Query), cusHAWB.Declaration);
		}

		#endregion
		#region Implementation
		ZDateTime testDate;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			testDate = ZDateTime.Now;
			ModuleDateFilter filter = (ModuleDateFilter)FilterBizObj[UPEFilterConstants.ArrivalDate];
			filter.Property1 = ZDateTime.Empty;
			FilterBizObj.ClearAllNotifications();
		}

		#region Tools
		OrgHeader CreateOrgAndAddress(ZString value)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = value;
			org.OH_FullName = value;
			OrgAddress address = org.Addresses.AddNew();
			address.OA_Address1 = value;
			address.OA_City = value;
			address.OA_State = value;
			address.OA_PostCode = value;
			address.OA_Phone = value;
			OrgCusCode ownerCode = org.CustomsCodes.AddNew();
			ownerCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			ownerCode.OK_CustomsRegNo = value;
			return org;
		}

		OrgHeader CreateOrgWithPostcode(ZString postcode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.MainAddress.OA_PostCode = postcode;
			return result;
		}

		void SetJobDecLastedEditedDate(JobDeclaration jobDec, ZDateTime lastEditedDate)
		{
			Factory.Save();
			jobDec.JE_RL_NKOrigin = "MYPKG";
			Factory.Save();
			StmALog editedLog = jobDec.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			((INeedRow)editedLog).Row[StmALogSchema.SL_PostedTimeUtc.Name] = Env.Time.GetUtcFromLocalTime(lastEditedDate.ToDateTime());
			editedLog.HasChanges = true;
			Factory.Save();
		}

		UPECusHAWB CreateCusHAWBWithDeclaration()
		{
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			return cusHAWB;
		}

		OrgHeader CreateOrgWithCustomsRegNo(ZString cusRegNo)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode billToAccountNumber = result.CustomsCodes.AddNew();
			billToAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			billToAccountNumber.OK_CustomsRegNo = cusRegNo;
			return result;
		}

		void AssertFilterWithExpectedResults(ZQuery query, params UPEJobDeclaration[] expectedResults)
		{
			Factory.Save();
			UPEJobDeclaration[] actualResults = Factory.Load<UPEJobDeclaration>(query);
			AssertEquals(expectedResults.Length, actualResults.Length);
			foreach (UPEJobDeclaration expectedResult in expectedResults)
			{
				Assert(ArrayContainsPK(actualResults, expectedResult));
			}
		}

		bool ArrayContainsPK(BusinessObject[] list, BusinessObject expectedItemInList)
		{
			foreach (BusinessObject bizObj in list)
			{
				if (bizObj.PK == expectedItemInList.PK)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
		#region Overrides
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UPEJobDeclarationFilterBusinessObject();
		}

		#endregion
		#region FilterBizObj
		UPEJobDeclarationFilterBusinessObject FilterBizObj
		{
			get
			{
				if (filterBizObj == null)
				{
					filterBizObj = new UPEJobDeclarationFilterBusinessObject();
				}

				return filterBizObj;
			}
		}

		UPEJobDeclarationFilterBusinessObject filterBizObj;
		#endregion
		#endregion
	}
}
