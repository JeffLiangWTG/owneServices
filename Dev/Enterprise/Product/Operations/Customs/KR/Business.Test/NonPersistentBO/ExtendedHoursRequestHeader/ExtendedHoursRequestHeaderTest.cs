using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExtendedHoursRequestHeader))]
	sealed class ExtendedHoursRequestHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);

		public void TestWithArgumentException()
		{
			AssertExceptionThrown<ArgumentException>("Param MessageType value is '5GW' or '5AC'. But, sent value is 830", () => new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._830, GlbCompany.CurrentCompany.PK));
		}
		public void TestPopulateEntryData5AC()
		{
			new ExtendedHoursEntryDataRetrieverTest().SetUpData();

			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.StartDate = DateTime.Today.AddDays(1);
			var extendedHoursRequestLineObj1 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj1.ReferenceNumber = "2362520050702X";
			var extendedHoursRequestLineObj2 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj2.ReferenceNumber = "2362520042782X";

			extendedHoursRequestHeaderObj.PopulateEntryData();
			AssertEquals("ReferenceNumber", "2362520050702X", extendedHoursRequestLineObj1.ReferenceNumber);
			AssertEquals("CustomsValue", 600000m, extendedHoursRequestLineObj1.CustomsValue);
			AssertEquals("PackageCount", 100, extendedHoursRequestLineObj1.PackageCount);
			AssertEquals("TotalWeight", 120m, extendedHoursRequestLineObj1.TotalWeight);
			AssertEquals("SupplierName", "레디코리아", extendedHoursRequestLineObj1.SupplierName);

			AssertEquals("ReferenceNumber", "2362520042782X", extendedHoursRequestLineObj2.ReferenceNumber);
			AssertEquals("CustomsValue", 200000m, extendedHoursRequestLineObj2.CustomsValue);
			AssertEquals("PackageCount", 200, extendedHoursRequestLineObj2.PackageCount);
			AssertEquals("TotalWeight", 100m, extendedHoursRequestLineObj2.TotalWeight);
			AssertEquals("SupplierName", "레디코리아", extendedHoursRequestLineObj2.SupplierName);
		}
		public void TestPopulateEntryData5GW()
		{
			new ExtendedHoursEntryDataRetrieverTest().SetUpData();

			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.StartDate = DateTime.Today.AddDays(1);
			var extendedHoursRequestLineObj1 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj1.ReferenceNumber = "2362520050702X";
			var extendedHoursRequestLineObj2 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj2.ReferenceNumber = "2362520042782X";

			extendedHoursRequestHeaderObj.PopulateEntryData();
			AssertEquals("ReferenceNumber", "2362520050702X", extendedHoursRequestLineObj1.ReferenceNumber);
			AssertEquals("CustomsValue", 300000m, extendedHoursRequestLineObj1.CustomsValue);
			AssertEquals("PackageCount", 100, extendedHoursRequestLineObj1.PackageCount);
			AssertEquals("TotalWeight", 120m, extendedHoursRequestLineObj1.TotalWeight);
			AssertEquals("UQ", "OU", extendedHoursRequestLineObj1.UQ);
			AssertEquals("ReferenceNumber", ReferenceNumberTypeList.Codes.IMP, extendedHoursRequestLineObj1.ReferenceNumberType);
			AssertEquals("HSDescription", "USED EXCAVATOR", extendedHoursRequestLineObj1.HSDescription);
			AssertEquals("BondedAreaCode", "99999999", extendedHoursRequestLineObj1.BondedAreaCode);
			AssertEquals("PayerCompanyName", "모나리자(주)", extendedHoursRequestLineObj1.PayerCompanyName);

			AssertEquals("ReferenceNumber", "2362520042782X", extendedHoursRequestLineObj2.ReferenceNumber);
			AssertEquals("CustomsValue", 100000m, extendedHoursRequestLineObj2.CustomsValue);
			AssertEquals("PackageCount", 200, extendedHoursRequestLineObj2.PackageCount);
			AssertEquals("TotalWeight", 100m, extendedHoursRequestLineObj2.TotalWeight);
			AssertEquals("UQ", "OU", extendedHoursRequestLineObj2.UQ);
			AssertEquals("ReferenceNumber", ReferenceNumberTypeList.Codes.IMP, extendedHoursRequestLineObj2.ReferenceNumberType);
			AssertEquals("HSDescription", "PAPER CALENDARS", extendedHoursRequestLineObj2.HSDescription);
			AssertEquals("BondedAreaCode", "99999999", extendedHoursRequestLineObj2.BondedAreaCode);
			AssertEquals("PayerCompanyName", "모나리자(주)", extendedHoursRequestLineObj2.PayerCompanyName);
		}
	}
}
