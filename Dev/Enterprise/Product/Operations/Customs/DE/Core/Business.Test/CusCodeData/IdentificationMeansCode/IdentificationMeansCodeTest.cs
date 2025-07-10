using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(IdentificationMeansCode))]
	class IdentificationMeansCodeTestTest : CusCodeDataTest<IdentificationMeansCode>
	{
		[ExpectNoExceptions]
		public void TestCY_DataAllowWesternEuropeanCharactersOnly()
		{
			NUnit.Framework.Assert.That(identificationMeansCode.CY_DataAllowWesternEuropeanCharactersOnly, NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestCY_Code()
		{
			NUnit.Framework.Assert.That(identificationMeansCode.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestCY_Data()
		{
			NUnit.Framework.Assert.That(identificationMeansCode.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(255));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			NUnit.Framework.Assert.That(identificationMeansCode.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.IdentificationMeansCode).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			NUnit.Framework.Assert.That(identificationMeansCode.HumanReadableName, NUnit.Framework.Is.EqualTo("Identification Means").Using(CustomComparers.TypeComparison));
		}

		protected override IEnumerable<IdentificationMeansCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (IdentificationMeansCode)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
			return entryInstruction.IdentificationMeanCodes.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			identificationMeansCode = Factory.CreateIdentificationMeansCode();
		}

		IdentificationMeansCode identificationMeansCode;
	}
}
