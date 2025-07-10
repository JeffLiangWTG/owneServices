using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EU.Business.Reports.Testing
{
	class Report_EUISTAccountingTest : EUReportFunctionalTestCase
	{
		protected override ZString ObjectName => "Report_ISTAccounting";

		protected override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInOutputOrder()
		{
			yield return (typeof(string), "OA_Code");
			yield return (typeof(string), "SRH_Reference");
			yield return (typeof(int), "SRL_LineNumber");
			yield return (typeof(string), "SRT_TransactionType");
			yield return (typeof(DateTime), "SRT_SystemCreateTimeUtc");
			yield return (typeof(string), "SRL_LocationOfGoods");
			yield return (typeof(string), "SRH_PreviousReferenceType");
			yield return (typeof(string), "SRH_PreviousReference");
			yield return (typeof(Guid), "SRT_PK");
			yield return (typeof(string), "SRT_ReferenceType");
			yield return (typeof(string), "SRT_InternalReferenceNumber");
			yield return (typeof(string), "CustomsProcedure");
			yield return (typeof(string), "SJH_CustomsOffice");
			yield return (typeof(string), "CustomsStatus");
			yield return (typeof(string), "TSI_GoodsOrigin");
			yield return (typeof(string), "TSL_GoodsDescription");
			yield return (typeof(int), "QtyIn");
			yield return (typeof(int), "QtyOut");
			yield return (typeof(string), "SRL_PackageType");
			yield return (typeof(string), "SpecificHandlingDescription");
			yield return (typeof(int), "SRL_PackagesRemaining");
		}

		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string, string)[]
			{
				("OA_Code", "presenter address"),
				("SRH_Reference", "TEST"),
				("SRL_LineNumber", "1"),
				("SRT_TransactionType", "OBL"),
				("SRT_SystemCreateTimeUtc", ZDateTime.Today.ToString("s")),
				("SRL_LocationOfGoods", "SRLlocationOfGoods"),
				("SRH_PreviousReferenceType", "820"),
				("SRH_PreviousReference", "sjprevhreference"),
				("SRT_ReferenceType", "IST"),
				("SRT_InternalReferenceNumber", ""),
				("CustomsProcedure", ""),
				("SJH_CustomsOffice", "FR230023"),
				("CustomsStatus", ""),
				("TSI_GoodsOrigin", "22"),
				("TSL_GoodsDescription", "tslgoodDescription"),
				("QtyIn", "1"),
				("QtyOut", "0"),
				("SRL_PackageType", "PK"),
				("SpecificHandlingDescription", "Examination Expected/Same State Expected"),
				("SRL_PackagesRemaining", "1")
			};

			yield return new (string, string)[]
			{
				("OA_Code", "presenter address"),
				("SRH_Reference", "TEST2"),
				("SRL_LineNumber", "1"),
				("SRT_TransactionType", "ADJ"),
				("SRT_SystemCreateTimeUtc", ZDateTime.Today.AddDays(-3).ToString("s")),
				("SRL_LocationOfGoods", "SRLlocationOfGoods2"),
				("SRH_PreviousReferenceType", "740"),
				("SRH_PreviousReference", "sjprevhreference2"),
				("SRT_ReferenceType", "IST"),
				("SRT_InternalReferenceNumber", ""),
				("CustomsProcedure", "IST"),
				("SJH_CustomsOffice", "FR230023"),
				("CustomsStatus", "CLS"),
				("TSI_GoodsOrigin", "33"),
				("TSL_GoodsDescription", "tslgoodDescription2"),
				("QtyIn", "0"),
				("QtyOut", "-1"),
				("SRL_PackageType", "PK"),
				("SpecificHandlingDescription", "Examination Expected/Same State Expected"),
				("SRL_PackagesRemaining", "2")
			};

			yield return new (string, string)[]
			{
				("OA_Code", "presenter address"),
				("SRH_Reference", "TEST2"),
				("SRL_LineNumber", "1"),
				("SRT_TransactionType", "OBL"),
				("SRT_SystemCreateTimeUtc", ZDateTime.Today.AddDays(-3).ToString("s")),
				("SRL_LocationOfGoods", "SRLlocationOfGoods2"),
				("SRH_PreviousReferenceType", "740"),
				("SRH_PreviousReference", "sjprevhreference2"),
				("SRT_ReferenceType", "IST"),
				("SRT_InternalReferenceNumber", ""),
				("CustomsProcedure", ""),
				("SJH_CustomsOffice", "FR230023"),
				("CustomsStatus", ""),
				("TSI_GoodsOrigin", "33"),
				("TSL_GoodsDescription", "tslgoodDescription2"),
				("QtyIn", "3"),
				("QtyOut", "0"),
				("SRL_PackageType", "PK"),
				("SpecificHandlingDescription", "Examination Expected/Same State Expected"),
				("SRL_PackagesRemaining", "2")
			};
		}

		protected override IEnumerable<string> GetParameterNameList()
		{
			yield return "@BranchPK";
			yield return "@CreateDateFrom";
			yield return "@CreateDateTo";
			yield return "@JobReference";
			yield return "@DeclarationStatus";
			yield return "@PresenterHeaderPK";
			yield return "@PresenterAddressPK";
			yield return "@CustomsProfile";
			yield return "@GoodsLocation";
		}

		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			var presenter = GetPresenterAddress();

			yield return ("@BranchPK", GlbBranch.CurrentBranch.PK.ToString());
			yield return ("@CreateDateFrom", ZDate.Today.AddDays(-5).ToString("s"));
			yield return ("@CreateDateTo", ZDate.Today.AddDays(1).ToString("s"));
			yield return ("@JobReference", "");
			yield return ("@DeclarationStatus", "");
			yield return ("@PresenterHeaderPK", presenter.Header.PK.ToString());
			yield return ("@PresenterAddressPK", presenter.PK.ToString());
			yield return ("@CustomsProfile", "");
			yield return ("@GoodsLocation", "");
		}

		OrgAddress GetPresenterAddress()
		{
			if (presenterAddress == null)
			{
				var presenter = Factory.NewWithValidTestData<OrgHeader>();
				presenterAddress = Factory.NewWithValidTestData<OrgAddress>();
				presenterAddress.OA_Address1 = "presenter address";
				presenterAddress.OA_OH = presenter.PK;
			}
			return presenterAddress;
		}
		OrgAddress presenterAddress;

		protected override void PrepareTestData()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			var representative = Factory.NewWithValidTestData<OrgAddress>();

			var cusTempStorageJobHeader = Factory.New<CusTempStorageJobHeader>();
			cusTempStorageJobHeader.FillWithValidTestData();
			cusTempStorageJobHeader.SJH_AppCode = "IST";

			var cusTempStorageDec = Factory.New<CusTempStorageDec>();
			cusTempStorageDec.FillWithValidTestData();
			cusTempStorageDec.STH_DeclarationType = "IST";
			cusTempStorageDec.STH_SJH = cusTempStorageJobHeader.PK;

			cusTempStorageJobHeader.FillWithValidTestData();
			cusTempStorageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			cusTempStorageJobHeader.SJH_JobReference = "sjhreference";
			cusTempStorageJobHeader.SJH_OH_Customer = customer.PK;
			cusTempStorageJobHeader.SJH_OA_Presenter = GetPresenterAddress().PK;
			cusTempStorageJobHeader.SJH_OA_Representative = representative.PK;
			cusTempStorageJobHeader.SJH_CustomsOffice = "FR230023";
			cusTempStorageJobHeader.SJH_IsExaminationExpected = true;
			cusTempStorageJobHeader.SJH_IsSameConditionExpected = true;
			cusTempStorageJobHeader.SJH_CustomsProfile = "profile";

			var fromDec = cusTempStorageJobHeader.CusTempStorageDec;
			fromDec.STH_DeclarationStatus = "OPN";

			var tempstorageline = fromDec.CusTempStorageLines.AddNew();
			tempstorageline.FillWithValidTestData();
			tempstorageline.TSL_LineNo = 1;
			tempstorageline.TSL_ReferenceNumberLine = 1;
			tempstorageline.TSL_GoodsDescription = "tslgoodDescription";

			var tempstoragelineitem = tempstorageline.CusTempStorageLineItems.AddNew();
			tempstoragelineitem.FillWithValidTestData();
			tempstoragelineitem.TSI_GoodsOrigin = "22";

			var storageRegheader = Factory.New<ICusTempStorageRegHeader>();
			((BusinessObject)storageRegheader).FillWithValidTestData();
			storageRegheader.SRH_AppCode = "IST";
			storageRegheader.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			storageRegheader.SRH_Reference = "TEST";
			storageRegheader.SRH_PreviousReferenceType = "820";
			storageRegheader.SRH_PreviousReference = "sjprevhreference";
			storageRegheader.SRH_InternalReference = "sjhreference";

			var storageRegline = storageRegheader.CusTempStorageRegLines.AddNew();
			((BusinessObject)storageRegline).FillWithValidTestData();
			storageRegline.SRL_LineNumber = 1;
			storageRegline.SRL_LocationOfGoods = "SRLlocationOfGoods";
			storageRegline.SRL_PackagesRemaining = 2;
			storageRegline.SRL_PackageType = "PK";

			transaction = storageRegline.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_TransactionType = "OBL";
			transaction.SRT_PackageQty = 1;
			transaction.SRT_SystemCreateTimeUtc = ZDateTime.Today;
			transaction.SRT_ReferenceType = "IST";
			transaction.SRT_Reference = "reftran";

			var cusTempStorageJobHeader2 = Factory.New<CusTempStorageJobHeader>();
			cusTempStorageJobHeader2.FillWithValidTestData();
			cusTempStorageJobHeader2.SJH_AppCode = "IST";

			var cusTempStorageDec2 = Factory.New<CusTempStorageDec>();
			cusTempStorageDec2.FillWithValidTestData();
			cusTempStorageDec2.STH_DeclarationType = "IST";
			cusTempStorageDec2.STH_SJH = cusTempStorageJobHeader2.PK;

			cusTempStorageJobHeader2.SJH_GB = GlbBranch.CurrentBranch.PK;
			cusTempStorageJobHeader2.SJH_JobReference = "sjhreference2";
			cusTempStorageJobHeader2.SJH_OH_Customer = customer.PK;
			cusTempStorageJobHeader2.SJH_OA_Presenter = GetPresenterAddress().PK;
			cusTempStorageJobHeader2.SJH_OA_Representative = representative.PK;
			cusTempStorageJobHeader2.SJH_CustomsOffice = "FR230023";
			cusTempStorageJobHeader2.SJH_IsExaminationExpected = true;
			cusTempStorageJobHeader2.SJH_IsSameConditionExpected = true;
			cusTempStorageJobHeader2.SJH_CustomsProfile = "profile2";

			var fromDec2 = cusTempStorageJobHeader2.CusTempStorageDec;
			fromDec2.STH_DeclarationStatus = "CLS";

			var tempstorageline2 = fromDec2.CusTempStorageLines.AddNew();
			tempstorageline2.FillWithValidTestData();
			tempstorageline2.TSL_LineNo = 1;
			tempstorageline2.TSL_ReferenceNumberLine = 1;
			tempstorageline2.TSL_GoodsDescription = "tslgoodDescription2";

			var tempstoragelineitem2 = tempstorageline2.CusTempStorageLineItems.AddNew();
			tempstoragelineitem2.FillWithValidTestData();
			tempstoragelineitem2.TSI_GoodsOrigin = "33";

			var storageRegheader2 = Factory.New<ICusTempStorageRegHeader>();
			((BusinessObject)storageRegheader2).FillWithValidTestData();
			storageRegheader2.SRH_AppCode = "IST";
			storageRegheader2.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			storageRegheader2.SRH_Reference = "TEST2";
			storageRegheader2.SRH_PreviousReferenceType = "740";
			storageRegheader2.SRH_PreviousReference = "sjprevhreference2";
			storageRegheader2.SRH_InternalReference = "sjhreference2";

			var storageRegline2 = storageRegheader2.CusTempStorageRegLines.AddNew();
			((BusinessObject)storageRegline2).FillWithValidTestData();
			storageRegline2.SRL_LineNumber = 1;
			storageRegline2.SRL_LocationOfGoods = "SRLlocationOfGoods2";
			storageRegline2.SRL_PackagesRemaining = 5;
			storageRegline2.SRL_PackageType = "PK";

			transaction2 = storageRegline2.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_TransactionType = "OBL";
			transaction2.SRT_PackageQty = 3;
			transaction2.SRT_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			transaction2.SRT_ReferenceType = "IST";
			transaction2.SRT_Reference = "reftran2";

			transaction3 = storageRegline2.CusTempStorageRegLineTransactions.AddNew();
			transaction3.SRT_TransactionType = "ADJ";
			transaction3.SRT_PackageQty = -1;
			transaction3.SRT_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			transaction3.SRT_ReferenceType = "IST";
			transaction3.SRT_Reference = "reftran3";

			var cusTempStorageJobHeader3 = Factory.New<CusTempStorageJobHeader>();
			cusTempStorageJobHeader3.FillWithValidTestData();
			cusTempStorageJobHeader3.SJH_AppCode = "IST";

			var cusTempStorageDec3 = Factory.New<CusTempStorageDec>();
			cusTempStorageDec3.FillWithValidTestData();
			cusTempStorageDec3.STH_DeclarationType = "IST";
			cusTempStorageDec3.STH_SJH = cusTempStorageJobHeader3.PK;

			var presenter2 = Factory.NewWithValidTestData<OrgHeader>();
			var presenterAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			presenterAddress2.OA_Address1 = "presenter address 2";
			presenterAddress2.OA_OH = presenter2.PK;

			cusTempStorageJobHeader3.SJH_GB = GlbBranch.CurrentBranch.PK;
			cusTempStorageJobHeader3.SJH_JobReference = "sjhreference3";
			cusTempStorageJobHeader3.SJH_OH_Customer = customer.PK;
			cusTempStorageJobHeader3.SJH_OA_Presenter = presenterAddress2.PK;  // This transaction has a different presenter, so it will be excluded in the report.
			cusTempStorageJobHeader3.SJH_OA_Representative = representative.PK;
			cusTempStorageJobHeader3.SJH_CustomsOffice = "FR230023";
			cusTempStorageJobHeader3.SJH_IsExaminationExpected = true;
			cusTempStorageJobHeader3.SJH_IsSameConditionExpected = true;
			cusTempStorageJobHeader3.SJH_CustomsProfile = "profile3";

			var fromDec3 = cusTempStorageJobHeader3.CusTempStorageDec;
			fromDec3.STH_DeclarationStatus = "CLS";

			var tempstorageline3 = fromDec3.CusTempStorageLines.AddNew();
			tempstorageline3.FillWithValidTestData();
			tempstorageline3.TSL_LineNo = 1;
			tempstorageline3.TSL_ReferenceNumberLine = 1;
			tempstorageline3.TSL_GoodsDescription = "tslgoodDescription3";

			var tempstoragelineitem3 = tempstorageline3.CusTempStorageLineItems.AddNew();
			tempstoragelineitem3.FillWithValidTestData();
			tempstoragelineitem3.TSI_GoodsOrigin = "44";

			var storageRegheader3 = Factory.New<ICusTempStorageRegHeader>();
			((BusinessObject)storageRegheader3).FillWithValidTestData();
			storageRegheader3.SRH_AppCode = "IST";
			storageRegheader3.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			storageRegheader3.SRH_Reference = "TEST3";
			storageRegheader3.SRH_PreviousReferenceType = "740";
			storageRegheader3.SRH_PreviousReference = "sjprevhreference3";
			storageRegheader3.SRH_InternalReference = "sjhreference3";

			var storageRegline3 = storageRegheader3.CusTempStorageRegLines.AddNew();
			((BusinessObject)storageRegline3).FillWithValidTestData();
			storageRegline3.SRL_LineNumber = 1;
			storageRegline3.SRL_LocationOfGoods = "SRLlocationOfGoods2";
			storageRegline3.SRL_PackagesRemaining = 5;
			storageRegline3.SRL_PackageType = "PK";

			var transaction4 = storageRegline3.CusTempStorageRegLineTransactions.AddNew();
			transaction4.SRT_TransactionType = "OBL";
			transaction4.SRT_PackageQty = 3;
			transaction4.SRT_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			transaction4.SRT_ReferenceType = "IST";
			transaction4.SRT_Reference = "reftran3";

			Factory.Save();

			Assert(cusTempStorageJobHeader.IsInDatabase);
		}

		ICusTempStorageRegLineTransaction transaction;
		ICusTempStorageRegLineTransaction transaction2;
		ICusTempStorageRegLineTransaction transaction3;
	}
}
