using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestGetOfficeReferenceNumber_EMCSJobDeclaration()
		{
			AssertEquals(ZString.Empty, Extensions.GetOfficeReferenceNumber(null, ZString.Empty));
			var emcsJobDeclaration = Factory.New<EMCSJobDeclaration>();
			AssertEquals(ZString.Empty, emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDispatch));
			var customsOffice = emcsJobDeclaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
			customsOffice.CY_Data = "OFFICE1";
			AssertEquals("OFFICE1", emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDispatch));
		}

		[TestDate(2019, 12, 22, 16, 43, 51)]
		public void TestToNullableDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid input", new DateTime(2019, 12, 22, 16, 43, 51), ZDateTime.Now.ToNullableDateTime());
				AssertEquals("Invalid input", null, ZDateTime.Empty.ToNullableDateTime());
			});
		}
	}
}
