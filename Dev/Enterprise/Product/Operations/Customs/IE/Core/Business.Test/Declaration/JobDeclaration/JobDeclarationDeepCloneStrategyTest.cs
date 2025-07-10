using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class JobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCustomsOfficesCloning()
		{
			var oldDeclaration = Factory.New<JobDeclaration>();
			oldDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE111111");
			oldDeclaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IE222222");

			var cloneStrategy = new JobDeclarationDeepCloneStrategy(oldDeclaration, CloneType.TemplateCopy, Factory);
			var newDeclaration = (JobDeclaration)cloneStrategy.Clone(new BusinessObjectCloneArgs());
			CombineAssertions(() =>
			{
				AssertEquals("CustomsOffices.Count", 2, newDeclaration.CustomsOffices.Count);
				OfficeCode officeOfPresentation = newDeclaration.CustomsOffices[0];
				OfficeCode supervisingOffice = newDeclaration.CustomsOffices[1];
				if (officeOfPresentation.CY_Code == EuOfficeCodesTypes.Codes.SupervisingOffice)
				{
					officeOfPresentation = newDeclaration.CustomsOffices[1];
					supervisingOffice = newDeclaration.CustomsOffices[0];
				}
				AssertOfficeCode(officeOfPresentation, EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE111111");
				AssertOfficeCode(supervisingOffice, EuOfficeCodesTypes.Codes.SupervisingOffice, "IE222222");
			});
		}

		void AssertOfficeCode(OfficeCode officeCode, ZString code, ZString data)
		{
			AssertEquals("CY_Code", code, officeCode.CY_Code);
			AssertEquals("CY_Data", data, officeCode.CY_Data);
		}
	}
}
