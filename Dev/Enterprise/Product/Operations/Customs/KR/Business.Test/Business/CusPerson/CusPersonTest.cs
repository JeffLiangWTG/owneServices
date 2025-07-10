using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusPerson))]
	sealed class CusPersonTest : CusPersonAbstractTest<CusPersonCountry>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cusPerson = Factory.New<JobDeclaration>().Persons.AddNew();
			var glbPerson = Factory.New<GlbPerson>();
			glbPerson.PER_FullName = "Kenny G";
			cusPerson.CPN_PER_Person = glbPerson.PK;
			return cusPerson;
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<CusPersonValidation>(declaration.Persons.AddNew().Validation);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertType<LocalExportCusPersonValidation>(declaration.Persons.AddNew().Validation);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			AssertType<PIDCusPersonValidation>(declaration.Persons.AddNew().Validation);
		}
  }
}
