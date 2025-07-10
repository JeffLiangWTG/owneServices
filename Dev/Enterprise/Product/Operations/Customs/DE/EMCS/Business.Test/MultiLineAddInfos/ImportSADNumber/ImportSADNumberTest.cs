using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ImportSADNumber))]
	public class ImportSADNumberTest : Customs.Business.Testing.CusSupportingInfoTest<ImportSADNumber>
	{
		public void TestValidation()
		{
			AssertType<ImportSADNumberValidation>(importSadNumber.Validation);
		}

		protected override IEnumerable<ImportSADNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<EMCSJobDeclaration>();
			yield return declaration.ImportSADNumbers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			importSadNumber = Factory.New<ImportSADNumber>();
		}
		ImportSADNumber importSadNumber;
	}
}
