using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CustomsOfficerCollection))]
	sealed class CustomsOfficerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			return new CustomsOfficerCollection(entryHeader);
		}

		public void TestCreateOrUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var customsOfficers = entryHeader.CustomsOfficers;
			customsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.InspectionCustomsOfficer, "COF100", "김영옥", ZDateTime.Empty);

			var customsOfficerCodeIsIPO = customsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.InspectionCustomsOfficer);
			AssertEquals("PreCondition: 1 officer exists", 1, customsOfficerCodeIsIPO.Count());
			AssertEquals("COF100-김영옥", customsOfficerCodeIsIPO.FirstOrDefault().CY_Data);
			AssertEquals(ZDateTime.Empty, customsOfficerCodeIsIPO.FirstOrDefault().CY_Date);

			customsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.InspectionCustomsOfficer, "COF200", "박영배", ZDateTime.Empty);
			AssertEquals("PreCondition: 1 officer exists", 1, customsOfficerCodeIsIPO.Count());
			AssertEquals("COF200-박영배", customsOfficerCodeIsIPO.FirstOrDefault().CY_Data);
			AssertEquals(ZDateTime.Empty, customsOfficerCodeIsIPO.FirstOrDefault().CY_Date);

			customsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.InspectionCustomsOfficer, "COF300", "김춘수", new ZDateTime("2022-01-01"));
			AssertEquals("2 officer exists", 2, customsOfficerCodeIsIPO.Count());
			AssertEquals("COF200-박영배", customsOfficerCodeIsIPO.FirstOrDefault().CY_Data);
			AssertEquals(ZDateTime.Empty, customsOfficerCodeIsIPO.FirstOrDefault().CY_Date);
			AssertEquals("COF300-김춘수", customsOfficerCodeIsIPO.LastOrDefault().CY_Data);
			AssertEquals(new ZDateTime("2022-01-01"), customsOfficerCodeIsIPO.LastOrDefault().CY_Date);

			var customsOfficerCodeIsRCO = customsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer);
			customsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "COF400", "이민주", new ZDateTime("2022-01-02"));
			AssertEquals("1 officers exist", 1, customsOfficerCodeIsRCO.Count());
			AssertEquals("COF400-이민주", customsOfficerCodeIsRCO.FirstOrDefault().CY_Data);
			AssertEquals(new ZDateTime("2022-01-02"), customsOfficerCodeIsRCO.FirstOrDefault().CY_Date);

			customsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "COF500", "김민지", new ZDateTime("2022-01-02"));
			AssertEquals("1 officers exist", 1, customsOfficerCodeIsRCO.Count());
			AssertEquals("COF500-김민지", customsOfficerCodeIsRCO.FirstOrDefault().CY_Data);
			AssertEquals(new ZDateTime("2022-01-02"), customsOfficerCodeIsRCO.FirstOrDefault().CY_Date);

			customsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "COF600", "김주영", new ZDateTime("2022-01-03"));
			AssertEquals("2 officers exist", 2, customsOfficerCodeIsRCO.Count());
			AssertEquals("COF500-김민지", customsOfficerCodeIsRCO.FirstOrDefault().CY_Data);
			AssertEquals(new ZDateTime("2022-01-02"), customsOfficerCodeIsRCO.FirstOrDefault().CY_Date);
			AssertEquals("COF600-김주영", customsOfficerCodeIsRCO.LastOrDefault().CY_Data);
			AssertEquals(new ZDateTime("2022-01-03"), customsOfficerCodeIsRCO.LastOrDefault().CY_Date);

			AssertEquals(4, customsOfficers.Count);
		}

		public void TestStringIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var customsOfficers = entryHeader.CustomsOfficers;
			var customsOfficer1 = customsOfficers.AddNew();
			customsOfficer1.CY_Code = CustomsOfficerTypeList.Codes._5BDResponsibleCustomsOfficer;
			customsOfficer1.CY_Data = "COF123-김영옥";

			var customsOfficer2 = customsOfficers.AddNew();
			customsOfficer2.CY_Code = CustomsOfficerTypeList.Codes.InspectionCustomsOfficer;
			customsOfficer2.CY_Data = "COF111-박영배";
			Factory.Save();

			var customsOfficer3 = customsOfficers.AddNew();
			customsOfficer3.CY_Code = CustomsOfficerTypeList.Codes.CancellationCustomsOfficer;
			customsOfficer3.CY_Data = "COF678-이민주";
			Factory.Save();

			AssertEquals(3, entryHeader.CustomsOfficers.Count);
			AssertEquals("COF123-김영옥", entryHeader.CustomsOfficers["\"5RCO\""].CY_Data);
			AssertEquals("COF111-박영배", entryHeader.CustomsOfficers["\"IPO\""].CY_Data);
			AssertEquals("COF678-이민주", entryHeader.CustomsOfficers["\"5BF\""].CY_Data);
		}

		public void TestGetCustomsOfficer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var customsOfficers = entry.CustomsOfficers;

			var customsOfficer3 = customsOfficers.AddNew();
			customsOfficer3.CY_Code = CustomsOfficerTypeList.Codes.CancellationCustomsOfficer;
			customsOfficer3.CY_Data = "COF678-이민주";
			Factory.Save();

			AssertEquals("", entry.CustomsOfficers.GetCustomsOfficer(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer));

			var customsOfficer = customsOfficers.AddNew();
			customsOfficer.CY_Code = CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer;
			customsOfficer.CY_Data = "COF123-김영옥";
			Factory.Save();

			AssertEquals("COF123-김영옥", entry.CustomsOfficers.GetCustomsOfficer(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer));
		}
	}
}
