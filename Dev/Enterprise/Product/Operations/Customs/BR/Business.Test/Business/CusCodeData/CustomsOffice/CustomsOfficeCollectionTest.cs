using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CustomsOfficeCollectionForTest))]
	public class CustomsOfficeCollectionTest : CusCodeDataCollectionTest<CustomsOffice>
	{
		public void TestCustomsOfficesCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.BoardingOfficeCode = "1234567";

			var boardingOffice = declaration.CustomsOffices.First();
			AssertEquals("CY_Code", Constants.CustomsOfficeCodes.BoardingOffice, boardingOffice.CY_Code);
			AssertEquals("CY_Type", Enterprise.Customs.Common.BR.CusCodeDataTypeList.Codes.CustomsOffice, boardingOffice.CY_Type);
			AssertEquals("CustomsOffices should be 1", 1, declaration.CustomsOffices.Count);
		}

		protected override CusCodeDataCollection<CustomsOffice> GetCusCodeDataCollection()
		{
			return new CustomsOfficeCollectionForTest(Declaration);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
					declaration.CustomsEnclosures.RemoveAndDeleteAll();
					declaration.CustomsOffices.RemoveAndDeleteAll();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;

		public class CustomsOfficeCollectionForTest : CustomsOfficeCollection
		{
			public CustomsOfficeCollectionForTest(JobDeclaration parent)
				: base(parent)
			{
			}

			public void AddNew(CustomsOffice customsOffice)
			{
				base.Add(customsOffice);
			}
		}
	}
}
