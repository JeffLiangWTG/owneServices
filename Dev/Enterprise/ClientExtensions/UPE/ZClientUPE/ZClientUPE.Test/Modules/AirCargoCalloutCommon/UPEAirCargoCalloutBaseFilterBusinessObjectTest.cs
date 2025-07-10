using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public abstract class UPEAirCargoCalloutBaseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Number Filters
		public void TestMAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "MAWB";
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			CusMAWB decoyMAWB = Factory.New<CusMAWB>();
			decoyMAWB.CM_MAWB = "splaty";
			CusHAWB decoyHAWB = decoyMAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.MAWB].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.MAWB]).Property = " MA-W ";
			AssertFilterMatches("MAWB should match with StartsWith operator after removing dashes and whitespaces", hAWB);
		}

		public void TestHAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.CS_HAWB = "HAWB";
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.CS_HAWB = "splaty";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.HAWB].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.HAWB]).Property = " HA-W ";
			AssertFilterMatches("HAWB should match with StartsWith operator after removing dashes and whitespaces", hAWB);
		}

		public void TestHAWBNumberOverridesAllOtherCriteria()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.CS_HAWB = "HAWB";
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.CS_HAWB = "decoy";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.HAWB].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.HAWB]).Property = "HAWB";
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.MAWB].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.MAWB]).Property = "MAWB";
			AssertFilterMatches("The HAWB should match regardless of other criteria", hAWB);
		}

		public void TestRelatedWayBillShortNumber()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			JobRelatedWayBill parentPackageNum = hAWB.ChildRelatedWayBills.AddNew();
			parentPackageNum.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
			parentPackageNum.EB_WaybillShortNumber = "TrackingNum";
			UPECusHAWB decoyHAWB1 = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			JobRelatedWayBill decoyParentPackageNum = hAWB.ChildRelatedWayBills.AddNew();
			decoyParentPackageNum.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
			decoyParentPackageNum.EB_WaybillShortNumber = "splaty";
			UPECusHAWB decoyHAWB2 = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			JobRelatedWayBill decoyChildPackageNum = hAWB.ChildRelatedWayBills.AddNew();
			decoyChildPackageNum.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			decoyChildPackageNum.EB_WaybillShortNumber = "TrackingNum";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.RelatedWayBillShortNumber].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.RelatedWayBillShortNumber]).Property = " Trac-kiNgNu";
			AssertFilterMatches("Parent package short number should match with StartsWith operator after removing dashes and whitespaces", hAWB);
		}

		public void TestCreatedTimeFilter()
		{
			FilterBizObj.LoadModuleFilters();
			var dateFilter = FilterBizObj.ModuleFilters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			AssertNotNull(dateFilter);
			AssertEquals("Created Time filter should always be visible", dateFilter.Visibility, FilterVisibility.AlwaysVisible);
			AssertEquals("By default, the date range is set to last month", dateFilter.PropertySearch, ModuleDateFilter.DateRangeSearchTexts.LastMonth);
		}

		public void TestChildPackageShortOrLongNumber()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			JobRelatedWayBill childLongNum = hAWB.ChildRelatedWayBills.AddNew();
			childLongNum.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			childLongNum.EB_WaybillNumber = "ChildLongNum";
			JobRelatedWayBill childShortNum = hAWB.ChildRelatedWayBills.AddNew();
			childShortNum.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
			childShortNum.EB_WaybillShortNumber = "ChildShort";
			UPECusHAWB decoyHAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			JobRelatedWayBill decoyChildPackageNum = decoyHAWB.ChildRelatedWayBills.AddNew();
			decoyChildPackageNum.EB_WaybillNumber = "splaty";
			decoyChildPackageNum.EB_WaybillShortNumber = "splaty";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.ChildPackageShortOrLongNumber].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.ChildPackageShortOrLongNumber]).Property = " Chil-dShoR";
			AssertFilterMatches("Child package short number should match with StartsWith operator after removing dashes and whitespaces", hAWB);
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.ChildPackageShortOrLongNumber].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.ChildPackageShortOrLongNumber]).Property = " Chil-dLoN";
			AssertFilterMatches("Child package long number should match with StartsWith operator after removing dashes and whitespaces", hAWB);
		}

		public void TestQueueRemarks()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.CurrentQueue[QueueRemarksColumn.Name] = "Queue Remarks";
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.CurrentQueue[QueueRemarksColumn.Name] = "Decoy Queue Remarks";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.QueueRemarks].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.QueueRemarks]).Property = "Queue Rema";
			AssertFilterMatches("Queue remarks should match with StartsWith operator", hAWB);
		}

		public void TestInvoiceNumber()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.InvoiceNumber = "INV1012939";
			UPECusHAWB decoyHAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.InvoiceNumber = "splaty";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.InvoiceNumber].IsActive = true;
			((ModuleNumberFilter)FilterBizObj[UPEFilterConstants.AirCargoNumberTypes.InvoiceNumber]).Property = "I-N V  10-1";
			AssertFilterMatches("Invoice number should match with StartsWith operator after removing dashes and whitespaces", hAWB);
		}

		#endregion
		#region Org Details Filters
		public void TestOrgNameFilters()
		{
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeName, CusHAWBSchema.CS_ConsigneeName, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgHeaderSchema.OH_FullName);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorName, CusHAWBSchema.CS_ConsignorName, CusHAWBSchema.CS_OA_ConsignorAddress, OrgHeaderSchema.OH_FullName);
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToName, OrgHeaderSchema.OH_FullName);
		}

		public void TestConsigneeDetailsFilters()
		{
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeName, CusHAWBSchema.CS_ConsigneeName, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgHeaderSchema.OH_FullName);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeAccountID, CusHAWBSchema.CS_OtherSystemConsigneeCode, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgCusCodeSchema.OK_CustomsRegNo);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeStreet, CusHAWBSchema.CS_ConsigneeStreet, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgAddressSchema.OA_Address1);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeCity, CusHAWBSchema.CS_ConsigneeCity, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgAddressSchema.OA_City);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneeState, CusHAWBSchema.CS_ConsigneeState, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgAddressSchema.OA_State);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePostcode, CusHAWBSchema.CS_ConsigneePostcode, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgAddressSchema.OA_PostCode);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsigneePhone, CusHAWBSchema.CS_ConsigneePhone, CusHAWBSchema.CS_OA_ConsigneeAddress, OrgAddressSchema.OA_Phone);
		}

		public void TestConsignorDetailsFilter()
		{
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorName, CusHAWBSchema.CS_ConsignorName, CusHAWBSchema.CS_OA_ConsignorAddress, OrgHeaderSchema.OH_FullName);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorAccountID, CusHAWBSchema.CS_OtherSystemConsignorCode, CusHAWBSchema.CS_OA_ConsignorAddress, OrgCusCodeSchema.OK_CustomsRegNo);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorStreet, CusHAWBSchema.CS_ConsignorStreet, CusHAWBSchema.CS_OA_ConsignorAddress, OrgAddressSchema.OA_Address1);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorCity, CusHAWBSchema.CS_ConsignorCity, CusHAWBSchema.CS_OA_ConsignorAddress, OrgAddressSchema.OA_City);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorState, CusHAWBSchema.CS_ConsignorState, CusHAWBSchema.CS_OA_ConsignorAddress, OrgAddressSchema.OA_State);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPostcode, CusHAWBSchema.CS_ConsignorPostcode, CusHAWBSchema.CS_OA_ConsignorAddress, OrgAddressSchema.OA_PostCode);
			TestOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.ConsignorPhone, CusHAWBSchema.CS_ConsignorPhone, CusHAWBSchema.CS_OA_ConsignorAddress, OrgAddressSchema.OA_Phone);
		}

		public void TestBillToDetailsFilters()
		{
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToName, OrgHeaderSchema.OH_FullName);
			TestBillToAccountNumberFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToAccountID);
		}

		public void TestBillToCityFilter()
		{
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToCity, OrgAddressSchema.OA_City);
		}

		public void TestBillToStreetFilter()
		{
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToStreet, OrgAddressSchema.OA_Address1);
		}

		public void TestBillToStateFilter()
		{
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToState, OrgAddressSchema.OA_State);
		}

		public void TestBillToPostcodeFilter()
		{
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToPostcode, OrgAddressSchema.OA_PostCode);
		}

		public void TestBillToPhoneFilter()
		{
			TestBillToOrgDetailFieldFilter(UPEFilterConstants.OrgDetailTypes.BillToPhone, OrgAddressSchema.OA_Phone);
		}

		void TestOrgDetailFieldFilter(string orgDetailsFilterType, SchemaStringColumn cusHAWBOrgDetailColumn, SchemaGuidColumn linkedOrganisationFKColumn, SchemaStringColumn organisationOrMainAddressColumn)
		{
			DeleteAllCusHAWBs();
			TestOrgDetailsFieldFilterOnCusHAWB(orgDetailsFilterType, cusHAWBOrgDetailColumn);
			DeleteAllCusHAWBs();
			TestOrgDetailsFieldFilterOnLinkedOrg(orgDetailsFilterType, cusHAWBOrgDetailColumn, linkedOrganisationFKColumn, organisationOrMainAddressColumn);
			((ModuleTextFilter)FilterBizObj[orgDetailsFilterType]).Property = "";
			FilterBizObj[orgDetailsFilterType].IsActive = false;
		}

		void TestOrgDetailsFieldFilterOnCusHAWB(string orgDetailsFilterType, SchemaStringColumn cusHAWBOrgDetailColumn)
		{
			FilterBizObj[orgDetailsFilterType].IsActive = true;
			((ModuleTextFilter)FilterBizObj[orgDetailsFilterType]).Property = "TestVal";
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB[cusHAWBOrgDetailColumn.Name] = "TestValue";
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB[cusHAWBOrgDetailColumn.Name] = "decoy";
			Factory.Save();
			AssertFilterMatches("Match for org detail column " + cusHAWBOrgDetailColumn.Name, hAWB);
		}

		void TestOrgDetailsFieldFilterOnLinkedOrg(string orgDetailsFilterType, SchemaStringColumn cusHAWBOrgDetailColumn, SchemaGuidColumn linkedOrganisationFKColumn, SchemaStringColumn organisationOrMainAddressColumn)
		{
			FilterBizObj[orgDetailsFilterType].IsActive = true;
			((ModuleTextFilter)FilterBizObj[orgDetailsFilterType]).Property = "TestVal";
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB[cusHAWBOrgDetailColumn.Name] = "TestVal"; // should not look at the CusHAWB columns anymore if there is a linked organisation
			LinkNewOrganisationToCusHAWBAndSetProperty(decoyHAWB, linkedOrganisationFKColumn, organisationOrMainAddressColumn, "decoy");
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB[cusHAWBOrgDetailColumn.Name] = "";
			LinkNewOrganisationToCusHAWBAndSetProperty(hAWB, linkedOrganisationFKColumn, organisationOrMainAddressColumn, "TestValue");
			Factory.Save();
			AssertFilterMatches("Match for linked organisation column " + organisationOrMainAddressColumn.Name, hAWB);
		}

		void LinkNewOrganisationToCusHAWBAndSetProperty(CusHAWB hAWB, SchemaGuidColumn linkedOrganisationFKColumn, SchemaStringColumn organisationOrMainAddressColumn, ZString value)
		{
			OrgHeader organisationToAttach = Factory.NewWithValidTestData<OrgHeader>();
			hAWB[linkedOrganisationFKColumn.Name] = organisationToAttach.MainAddress.PK;
			if (organisationOrMainAddressColumn.TableName == OrgHeaderSchema.Constants.TableName)
			{
				organisationToAttach[organisationOrMainAddressColumn.Name] = value;
			}
			else if (organisationOrMainAddressColumn.TableName == OrgAddressSchema.Constants.TableName)
			{
				organisationToAttach.MainAddress[organisationOrMainAddressColumn.Name] = value;
			}
			else if (organisationOrMainAddressColumn == OrgCusCodeSchema.OK_CustomsRegNo)
			{
				OrgCusCode ownerCode = organisationToAttach.CustomsCodes.AddNew();
				ownerCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
				ownerCode.OK_CustomsRegNo = value;
			}
			else
			{
				throw new InvalidOperationException("Don't know how to handle column " + organisationOrMainAddressColumn.Name);
			}
		}

		void TestBillToAccountNumberFieldFilter(string orgDetailsFilterType)
		{
			DeleteAllCusHAWBs();
			FilterBizObj[orgDetailsFilterType].IsActive = true;
			((ModuleTextFilter)FilterBizObj[orgDetailsFilterType]).Property = "TestVal";
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB decoyHAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.BillToAccountNumber = "decoy";
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.BillToAccountNumber = "TestValue";
			Factory.Save();
			AssertFilterMatches("Match for column " + UPECusHAWB.BillToAccountNumberProcessQueueColumn.Name, hAWB);
		}

		void TestBillToOrgDetailFieldFilter(string orgDetailsFilterType, SchemaStringColumn organisationOrMainAddressColumn)
		{
			DeleteAllCusHAWBs();
			FilterBizObj[orgDetailsFilterType].IsActive = true;
			((ModuleTextFilter)FilterBizObj[orgDetailsFilterType]).Property = "TestVal";
			CusMAWB mAWB = Factory.New<CusMAWB>();
			UPECusHAWB decoyHAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			LinkNewBillToOrganisationToCusHAWBAndSetProperty(decoyHAWB, organisationOrMainAddressColumn, "decoy");
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			LinkNewBillToOrganisationToCusHAWBAndSetProperty(hAWB, organisationOrMainAddressColumn, "TestValue");
			Factory.Save();
			AssertFilterMatches("Match for linked organisation column " + organisationOrMainAddressColumn.Name, hAWB);
		}

		void LinkNewBillToOrganisationToCusHAWBAndSetProperty(UPECusHAWB hAWB, SchemaStringColumn organisationOrMainAddressColumn, ZString value)
		{
			OrgHeader organisationToAttach = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode ownerCode = organisationToAttach.CustomsCodes.AddNew();
			ownerCode.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
			ZString valueAsAccountID = value;
			ownerCode.OK_CustomsRegNo = valueAsAccountID;
			hAWB.BillToAccountNumber = valueAsAccountID;
			if (organisationOrMainAddressColumn.TableName == OrgHeaderSchema.Constants.TableName)
			{
				organisationToAttach[organisationOrMainAddressColumn.Name] = value;
			}
			else if (organisationOrMainAddressColumn.TableName == OrgAddressSchema.Constants.TableName)
			{
				organisationToAttach.MainAddress[organisationOrMainAddressColumn.Name] = value;
			}
			else
			{
				throw new InvalidOperationException("Don't know how to handle column " + organisationOrMainAddressColumn.Name);
			}
		}

		#endregion
		#region Account Class
		public void TestBillToAccountClass()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			OrgDebtorGroup debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			debtorGroup.OJ_Code = "CL1";
			OrgDebtorGroup decoyDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			decoyDebtorGroup.OJ_Code = "DCY";
			UPECusHAWB hAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.BillToAccountNumber = "BillToAccountNum";
			BillTo.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			UPECusHAWB decoyHAWB = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.BillToAccountNumber = "BillToAccountNum2";
			BillTo2.CompanyData.OB_OJ_ARDebtorGroup = decoyDebtorGroup.PK;
			UPECusHAWB decoyHAWB2 = (UPECusHAWB)mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB2.BillToAccountNumber = "BillToAccountNum3";
			BillTo3.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			BillTo3.CompanyData.OB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			Factory.Save();
			FilterBizObj["Account Class"].IsActive = true;
			((ModuleGuidFilter)FilterBizObj["Account Class"]).Property = debtorGroup.PK;
			AssertFilterMatches("1 match expected on BillToAccountClass", hAWB);
		}

		#endregion
		#region Port Filters
		public void TestOriginDestinationPortFilter()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.CS_RL_NKOrigin = "AUSYD";
			hAWB.CS_RL_NKDestination = "AUMEL";
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyHAWB.CS_RL_NKOrigin = "MYPKG";
			decoyHAWB.CS_RL_NKDestination = "MYPEN";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoPortTypes.OriginDestination].IsActive = true;
			((ModuleLocationFilter)FilterBizObj[UPEFilterConstants.AirCargoPortTypes.OriginDestination]).Property1 = "AUSYD";
			((ModuleLocationFilter)FilterBizObj[UPEFilterConstants.AirCargoPortTypes.OriginDestination]).Property2 = "AUMEL";
			AssertFilterMatches("Origin/Destination port match", hAWB);
		}

		public void TestLoadDischargePortFilter()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			mAWB.CM_RL_NKLoadPort = "AUSYD";
			mAWB.CM_RL_NKDischargePort = "AUMEL";
			CusMAWB decoyMAWB1 = Factory.New<CusMAWB>();
			CusHAWB decoyHAWB1 = decoyMAWB1.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyMAWB1.CM_RL_NKLoadPort = "AUSYD";
			decoyMAWB1.CM_RL_NKDischargePort = "MYPKG";
			CusMAWB decoyMAWB2 = Factory.New<CusMAWB>();
			CusHAWB decoyHAWB2 = decoyMAWB2.ChildBills.AddNew(typeof(QueuedCusHAWB));
			decoyMAWB2.CM_RL_NKLoadPort = "MYPKG";
			decoyMAWB2.CM_RL_NKDischargePort = "AUMEL";
			Factory.Save();
			FilterBizObj[UPEFilterConstants.AirCargoPortTypes.LoadDischarge].IsActive = true;
			((ModuleLocationFilter)FilterBizObj[UPEFilterConstants.AirCargoPortTypes.LoadDischarge]).Property1 = "AUSYD";
			((ModuleLocationFilter)FilterBizObj[UPEFilterConstants.AirCargoPortTypes.LoadDischarge]).Property2 = "AUMEL";
			AssertFilterMatches("Load/Discharge port match", hAWB);
		}

		#endregion
		#region Task Assigned To
		public void TestCS_GS_NKTaskAssignedTo()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWBByUser1 = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWBByUser1.CurrentQueue[QueueTaskAssignedToColumn.Name] = "OP1";
			CusHAWB hAWBByUser2 = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWBByUser2.CurrentQueue[QueueTaskAssignedToColumn.Name] = "OP2";
			Factory.Save();
			FilterBizObj["Assigned To"].IsActive = true;
			((ModuleNkFilter)FilterBizObj["Assigned To"]).Property = "OP1";
			AssertFilterMatches("Task assigned to match for user1", hAWBByUser1);
			((ModuleNkFilter)FilterBizObj["Assigned To"]).Property = "OP2";
			AssertFilterMatches("Task assigned to match for user2", hAWBByUser2);
		}

		#endregion
		#region Arrival Date
		public void TestCS_ArrivalFromTo()
		{
			CusMAWB mAWB1 = Factory.New<CusMAWB>();
			CusMAWB mAWB2 = Factory.New<CusMAWB>();
			CusMAWB mAWB3 = Factory.New<CusMAWB>();
			CusMAWB mAWB4 = Factory.New<CusMAWB>();
			mAWB1.CM_ArrivalDate = new ZDateTime(2005, 1, 1);
			CusHAWB hAWB1 = mAWB1.ChildBills.AddNew(typeof(QueuedCusHAWB));
			mAWB2.CM_ArrivalDate = new ZDateTime(2005, 2, 2);
			CusHAWB hAWB2 = mAWB2.ChildBills.AddNew(typeof(QueuedCusHAWB));
			mAWB3.CM_ArrivalDate = new ZDateTime(2005, 3, 3);
			CusHAWB hAWB3 = mAWB3.ChildBills.AddNew(typeof(QueuedCusHAWB));
			mAWB4.CM_ArrivalDate = new ZDateTime(2005, 4, 4);
			CusHAWB hAWB4 = mAWB4.ChildBills.AddNew(typeof(QueuedCusHAWB));
			Factory.Save();
			AssertFilterMatchesOnArrivalDates("Arrival From Date", hAWB2.CS_ArrivalDate, ZDateTime.Empty, hAWB2, hAWB3, hAWB4);
			AssertFilterMatchesOnArrivalDates("Arrival To Date", ZDateTime.Empty, hAWB3.CS_ArrivalDate, hAWB1, hAWB2, hAWB3);
			AssertFilterMatchesOnArrivalDates("Both arrival from and arrival to", hAWB2.CS_ArrivalDate, hAWB3.CS_ArrivalDate, hAWB2, hAWB3);
		}

		void AssertFilterMatchesOnArrivalDates(string message, ZDateTime arrivalFrom, ZDateTime arrivalTo, params CusHAWB[] expectedMatches)
		{
			FilterBizObj["Arrival Date"].IsActive = true;
			((ModuleDateFilter)FilterBizObj["Arrival Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)FilterBizObj["Arrival Date"]).Property1 = arrivalFrom;
			((ModuleDateFilter)FilterBizObj["Arrival Date"]).Property2 = arrivalTo;
			AssertFilterMatches(message, expectedMatches);
		}

		#endregion
		#region Goods Value
		public void TestFilterBy_CS_ValueFromTo()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			SetValue(hAWB1, 10);
			CusHAWB hAWB2 = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			SetValue(hAWB2, 20);
			CusHAWB hAWB3 = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			SetValue(hAWB3, 30);
			CusHAWB hAWB4 = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			SetValue(hAWB4, 40);
			Factory.Save();
			AssertFilterMatchesOnValue("CS_Value from", 30, 0, hAWB3, hAWB4);
			AssertFilterMatchesOnValue("CS_Value to", 0, 20, hAWB1, hAWB2);
			AssertFilterMatchesOnValue("CS_Value from and to", 20, 30, hAWB2, hAWB3);
		}

		void SetValue(CusHAWB hAWB, ZDecimal value)
		{
			if (ValueColumn.TableName == ProcessQueueSchema.Constants.TableName)
			{
				hAWB.CurrentQueue[ValueColumn.Name] = value;
			}
			else if (ValueColumn.TableName == CusHAWBSchema.Constants.TableName)
			{
				hAWB[ValueColumn.Name] = value;
			}
		}

		void AssertFilterMatchesOnValue(string message, decimal valueFrom, decimal valueTo, params CusHAWB[] expectedMatches)
		{
			FilterBizObj["Value"].IsActive = true;
			((ModuleNumberRangeFilter)FilterBizObj["Value"]).Property1 = valueFrom;
			((ModuleNumberRangeFilter)FilterBizObj["Value"]).Property2 = valueTo;
			AssertFilterMatches(message, expectedMatches);
		}

		protected abstract SchemaDecimalColumn ValueColumn { get; }

		#endregion
		#region Zone Filter
		public void TestAllMetroOrCountryZoneFilter()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			CusHAWB decoyHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			CusHAWB hAWBNotInAnyPostcodeRange = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.CS_ConsigneePostcode = "100";
			decoyHAWB.CS_ConsigneePostcode = "150";
			hAWBNotInAnyPostcodeRange.CS_ConsigneePostcode = "200";
			var postcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "100", "100");
			var decoyPostcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "150", "150");
			FilterBizObj["Zone"].IsActive = true;
			((ModuleTextFilter)FilterBizObj["Zone"]).Property = UPEFilterConstants.MetroCountry.Metro;
			postcodeRange.Zone.TZ_ZoneName = "Sydney Metro";
			decoyPostcodeRange.Zone.TZ_ZoneName = "Decoy NSW Country";
			Factory.Save();
			AssertFilterMatches("All metro", hAWB);
			((ModuleTextFilter)FilterBizObj["Zone"]).Property = UPEFilterConstants.MetroCountry.Other;
			postcodeRange.Zone.TZ_ZoneName = "NSW Country";
			decoyPostcodeRange.Zone.TZ_ZoneName = "Decoy Sydney Metro";
			Factory.Save();
			AssertFilterMatches("All country", hAWB, hAWBNotInAnyPostcodeRange);
		}

		public void TestZoneFilterWithPostcodeManuallyEnteredOnHAWB()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB lowerBoundHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			CusHAWB upperBoundHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			lowerBoundHAWB.CS_ConsigneePostcode = "300";
			upperBoundHAWB.CS_ConsigneePostcode = "2000";
			TestZoneFilter(lowerBoundHAWB, upperBoundHAWB);
		}

		public void TestZoneFilterWithPostcodeFromLinkedConsigneeOrg()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB lowerBoundHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			CusHAWB upperBoundHAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			lowerBoundHAWB.CS_OA_ConsigneeAddress = CreateOrganisationWithPostcode("300").MainAddress.PK;
			lowerBoundHAWB.CS_ConsigneePostcode = "1"; // decoy
			upperBoundHAWB.CS_OA_ConsigneeAddress = CreateOrganisationWithPostcode("2000").MainAddress.PK;
			upperBoundHAWB.CS_ConsigneePostcode = "1000000"; // decoy
			TestZoneFilter(lowerBoundHAWB, upperBoundHAWB);
		}

		void TestZoneFilter(CusHAWB lowerBoundHAWB, CusHAWB upperBoundHAWB)
		{
			FilterBizObj[UPEFilterConstants.Zone].IsActive = true;
			((ModuleTextFilter)FilterBizObj[UPEFilterConstants.Zone]).Property = RateTransportZoneTestHelper.UPSTestZoneName;
			RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, Factory.NewWithValidTestData(typeof(OrgHeader)).PK, "1", "1000000");
			var postcodeRange = RateTransportZoneTestHelper.CreateTransportProviderWithPostcodeRange(Factory, GlbCompany.CurrentCompany.GC_OH_OrgProxy, "300", "2000");
			Factory.Save();
			AssertFilterMatches("Both HAWBs should match", lowerBoundHAWB, upperBoundHAWB);
			var tempPostCode = Factory.New<RefPostCode>();
			tempPostCode.RK_CityTownPostCode = "199Z";
			postcodeRange.TQ_ToPostCode = tempPostCode.RK_CityTownPostCode;
			Factory.Save();
			AssertFilterMatches("Only the HAWB with the postcode lower bound should match", lowerBoundHAWB);
			tempPostCode = Factory.New<RefPostCode>();
			tempPostCode.RK_CityTownPostCode = "30A";
			postcodeRange.TQ_FromPostCode = tempPostCode.RK_CityTownPostCode;
			tempPostCode = Factory.New<RefPostCode>();
			tempPostCode.RK_CityTownPostCode = "2000";
			postcodeRange.TQ_ToPostCode = tempPostCode.RK_CityTownPostCode;
			Factory.Save();
			AssertFilterMatches("Only the HAWB with the postcode upper bound should match", upperBoundHAWB);
		}

		OrgHeader CreateOrganisationWithPostcode(ZString postcode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.MainAddress.OA_PostCode = postcode;

			return result;
		}

		#endregion
		#region Air Cargo / Callout Queue Property Mappings
		protected abstract SchemaStringColumn QueueNameColumn { get; }

		protected abstract SchemaStringColumn QueueReasonColumn { get; }

		protected abstract SchemaStringColumn QueueStatusColumn { get; }

		protected abstract SchemaStringColumn QueueRemarksColumn { get; }

		protected abstract SchemaStringColumn QueueTaskAssignedToColumn { get; }

		#endregion
		#region Refund Enquiry
		public void TestRefundEnquiryFilter()
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData<UPEJobDeclaration>().PK;
			callout.IsRefundEnquiry = true;
			callout.CurrentQueue.P4_QueueName = new CommercialQueueCodeDescriptionPairList()[0].Code;
			ClientRefund refund = callout.Refund;
			Factory.Save();
			AssertFilterMatchesOnRefundEnquiry(ZBool.True);
			AssertFilterMatches("CusHawb matched through the declaration", callout);
		}

		void AssertFilterMatchesOnRefundEnquiry(ZBool value)
		{
			FilterBizObj["Refund"].IsActive = true;
			((ModuleFlagsFilter)FilterBizObj["Refund"]).Property0 = value;
		}

		#endregion
		#region CMRMAWBQuery
		public void TestCMRMAWBQuery()
		{
			var mAWB = Factory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			mAWB.CM_ApplicationCode = "CMR";
			Factory.Save();
			AssertFilterMatches("Included when ApplicationCode = CMR", hAWB);
			Db.Connection.ExecuteNonQuery($"UPDATE CusMAWB SET CM_ApplicationCode = 'ZZZ', CM_SystemLastEditTimeUtc = GETUTCDATE(), CM_SystemLastEditUser = '~BP' WHERE CM_PK = '{mAWB.PK}'");
			Factory.ClearQueryCache();
			AssertFilterMatches("Not included when ApplicationCode != CMR");
		}
		#endregion
		#region Test Classes
		protected class QueuedCusHAWB : UPECusHAWB
		{
			public QueuedCusHAWB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				CurrentQueue[ProcessQueueSchema.P4_QueueName.Name] = "XXX";
				CurrentQueue[ProcessQueueSchema.P4_CustomsQueue.Name] = "XXX";
			}
		}

		#endregion
		#region Implementation
		void DeleteAllCusHAWBs()
		{
			CusHAWB[] allHAWBs = (CusHAWB[])Factory.Load(typeof(QueuedCusHAWB), new ZQuery());
			foreach (CusHAWB hAWB in allHAWBs)
			{
				hAWB.Delete();
			}
		}

		protected void AssertFilterMatches(string message, params CusHAWB[] expectedMatches)
		{
			CusHAWB[] matches = (CusHAWB[])Factory.Load(typeof(QueuedCusHAWB), FilterBizObj.Filter);
			AssertEquals(message + "; Correct number of matches", expectedMatches.Length, matches.Length);
			foreach (CusHAWB expectedMatch in expectedMatches)
			{
				AssertEquals(message + "; Should match on the correct CusHAWB(s)", true, ArrayContainsPK(matches, expectedMatch));
			}
		}

		bool ArrayContainsPK(BusinessObject[] list, BusinessObject expectedItemInList)
		{
			foreach (BusinessObject next in list)
			{
				if (next.PK == expectedItemInList.PK)
				{
					return true;
				}
			}

			return false;
		}

		protected UPEAirCargoCalloutBaseFilterBusinessObject FilterBizObj;
		OrgHeader BillTo
		{
			get
			{
				if (fBillTo == null)
				{
					fBillTo = Factory.NewWithValidTestData<OrgHeader>();
					OrgCusCode billToAccountNumber = fBillTo.CustomsCodes.AddNew();
					billToAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
					billToAccountNumber.OK_CustomsRegNo = "BillToAccountNum";
				}

				return fBillTo;
			}
		}

		OrgHeader fBillTo;
		OrgHeader BillTo2
		{
			get
			{
				if (fBillTo2 == null)
				{
					fBillTo2 = Factory.NewWithValidTestData<OrgHeader>();
					OrgCusCode billToAccountNumber = fBillTo2.CustomsCodes.AddNew();
					billToAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
					billToAccountNumber.OK_CustomsRegNo = "BillToAccountNum2";
				}

				return fBillTo2;
			}
		}

		OrgHeader fBillTo2;
		OrgHeader BillTo3
		{
			get
			{
				if (fBillTo3 == null)
				{
					fBillTo3 = Factory.NewWithValidTestData<OrgHeader>();
					OrgCusCode billToAccountNumber = fBillTo3.CustomsCodes.AddNew();
					billToAccountNumber.OK_CodeType = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber;
					billToAccountNumber.OK_CustomsRegNo = "BillToAccountNum3";
				}

				return fBillTo3;
			}
		}

		OrgHeader fBillTo3;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
			TestCaseHelper.ClearTable(CusHAWB.Schema.TableName);
			FilterBizObj = (UPEAirCargoCalloutBaseFilterBusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType());
			FilterBizObj.QueryObjectType = typeof(CusHAWB);
		}
		#endregion
	}
}
