using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.ExportDeclarationTypeProcedureList.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExportDeclarationTypeProcedureListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsIncompleteDeclaration()
		{
			foreach (var typeProcedure in new ExportDeclarationTypeProcedureList().GetAllCodesZString())
			{
				NUnit.Framework.Assert.That(ExportDeclarationTypeProcedureList.IsIncompleteDeclaration(typeProcedure), Is.EqualTo(typeProcedure.SubstringSafe(4, 1) == "1"), typeProcedure.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestIsPresentationOutsideOfficialPlace_TypeTime()
		{
			foreach (var typeTime in new ExportDeclarationTypeTimeList().GetAllCodes())
			{
				NUnit.Framework.Assert.That(ExportDeclarationTypeProcedureList.IsPresentationOutsideOfficialPlace(Factory, typeTime, _000110), Is.EqualTo(typeTime == ExportDeclarationTypeTimeList.Codes._00), typeTime);
			}
		}

		[ExpectNoExceptions]
		public void TestIsPresentationOutsideOfficialPlace_TypeProcedure()
		{
			foreach (var typeProcedure in new ExportDeclarationTypeProcedureList().GetAllCodesZString())
			{
				var thirdAndFourthChars = typeProcedure.SubstringSafe(2, 2);
				NUnit.Framework.Assert.That(ExportDeclarationTypeProcedureList.IsPresentationOutsideOfficialPlace(Factory, ExportDeclarationTypeTimeList.Codes._00, typeProcedure), Is.EqualTo(thirdAndFourthChars == "01" || thirdAndFourthChars == "02" || thirdAndFourthChars == "09"), typeProcedure.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestIs000000()
		{
			foreach (var typeProcedure in new ExportDeclarationTypeProcedureList().GetAllCodesZString())
			{
				NUnit.Framework.Assert.That(ExportDeclarationTypeProcedureList.Is000000(typeProcedure), Is.EqualTo(typeProcedure == _000000), typeProcedure.ToString());
			}
		}
	}
}
