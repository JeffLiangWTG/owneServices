using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(SealNumber))]
	class SealNumberTest : Customs.Business.Testing.CusCodeDataTest<SealNumber>
	{
		[ExpectNoExceptions]
		public void TestValidation()
		{
			var sealNumber = Factory.New<SealNumber>();
			NUnit.Framework.Assert.That(sealNumber.Validation, NUnit.Framework.Is.TypeOf<SealNumberValidation>(), "Validation Type");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				var sealNumber = Factory.New<SealNumber>();
				NUnit.Framework.Assert.That(sealNumber.CY_Type, NUnit.Framework.Is.EqualTo("SNO").Using(CustomComparers.TypeComparison), "CY_Type");
				NUnit.Framework.Assert.That(sealNumber.CY_Code, NUnit.Framework.Is.EqualTo("SNO").Using(CustomComparers.TypeComparison), "CY_Code");
			});
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var sealNumber = Factory.New<SealNumber>();
			sealNumber.CY_ParentID = entryInstruction.PK;
			sealNumber.CY_ParentTableCode = entryInstruction.TablePrefix;
			NUnit.Framework.Assert.That(sealNumber.Parent, NUnit.Framework.Is.SameAs(entryInstruction), "SealNumber Parent");
		}

		[ExpectNoExceptions]
		public void TestCY_DataMaxLength()
		{
			var sealNumber = Factory.New<SealNumber>();
			NUnit.Framework.Assert.That(sealNumber.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(20), "CY_Data MaxLength");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew().Seals.AddNew();
		}
	}
}
