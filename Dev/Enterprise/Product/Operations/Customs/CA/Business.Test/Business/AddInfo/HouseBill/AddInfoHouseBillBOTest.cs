using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoHouseBill))]
	sealed class AddInfoHouseBillBOTest : AddInfoBOTest
	{
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoHouseBillLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoHouseBillValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Bill bill = declaration.Bills.AddNew();
			return new AddInfoHouseBill(bill.CU_AddInfoInfo);
		}
	}
}
