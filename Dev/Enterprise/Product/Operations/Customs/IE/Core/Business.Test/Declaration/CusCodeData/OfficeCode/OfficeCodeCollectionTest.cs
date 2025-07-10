using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(OfficeCodeCollection))]
	class OfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetPresentationOffice()
		{
			AssertGetData(x => x.CustomsOffices.GetPresentationOffice(), "IE111111");
		}

		public void TestGetSupervisingOffice()
		{
			AssertGetData(x => x.CustomsOffices.GetSupervisingOffice(), "IE222222");
		}

		public void TestGetOfficeOfExit()
		{
			AssertGetData(x => x.CustomsOffices.GetOfficeOfExit(), "IE3333333");
		}

		public void TestGetOfficeOfDischarge()
		{
			AssertGetData(x => x.CustomsOffices.GetOfficeOfDischarge(), "IE4444444");
		}

		void AssertGetData(Func<JobDeclaration, OfficeCode> getData, ZString expectedResult)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE111111");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IE222222");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IE3333333");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDischarge, "IE4444444");

			AssertEquals(getData.Target.ToString() + ", expecting result", expectedResult, getData(declaration).CY_Data);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OfficeCodeCollection(Factory.New<JobDeclaration>());
	}
}
