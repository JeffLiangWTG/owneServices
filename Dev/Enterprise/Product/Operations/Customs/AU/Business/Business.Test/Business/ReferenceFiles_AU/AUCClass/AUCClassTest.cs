using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUCClass))]
	sealed class AUCClassTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetDescriptionForPartialTariffs()
		{
			var class2 = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "8704.22.00");
			AssertEquals("Description", class2.UJ_STDesc, AUCClass.GetClassForPartialCode(Factory, "8704.22.00 07").UJ_STDesc);

			class2 = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "8704.22.00");
			AssertEquals("Description", class2.UJ_STDesc, AUCClass.GetClassForPartialCode(Factory, "8704.22.00 29").UJ_STDesc);
		}

		public void TestGetImportDescription()
		{
			var class2 = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "0106.90.00 69");
			AssertEquals("ImportDescription", "OTHER LIVE ANIMALS EXCL MAMMALS,REPTILES,BIRDS", class2.ImportDescription);
		}

		public void TestGetImportDescription2()
		{
			var class2 = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2202.90.00 04");
			Assert("ImportDescription Should not be Other", "Other" != class2.ImportDescription);
		}

		public void TestChapter()
		{
			AssertEquals("04", aucClass.Chapter.UH_Chapter);
		}

		public void TestCollectionInstantiation()
		{
			Assert(aucClass.AUCClasses.Count > 0);
		}

		public void TestHasChildren()
		{
			Assert(aucClass.HasChildren);
		}

		public void TestGetHierarchyWhenUJIsInvalidAndChapterIsEmpty()
		{
			AUCClass aucClassWithInvalidUJ = Factory.New<AUCClass>();
			aucClassWithInvalidUJ.UJ_UJ = ZGuid.Empty;
			aucClassWithInvalidUJ.UJ_Txt = "The class has no valid parent class due to invalid UJ";
			aucClassWithInvalidUJ.UJ_Code = "0";
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				aucClassWithInvalidUJ.GetHierarchy();
			});
		}

		public void TestHasChildrenReturnsFalseOnChildlessItem()
		{
			var class2 = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "0306.23.00 62");
			Assert(!class2.HasChildren);
		}

		public void TestGetHierarchyForNoChapter()
		{
			var businessObject = Factory.New<AUCClass>();
			businessObject.UJ_Code = "abcd";
			businessObject.UJ_STDesc = "Top Level";

			IFamilyMember[] chapterHierarchy = businessObject.GetHierarchy();
			AssertEquals("Hierarchy depth", 1, chapterHierarchy.Length);
			AssertEquals("Class", businessObject, chapterHierarchy[0]);
			AssertNull(businessObject.Chapter);
		}

		AUCClass aucClass;
		protected override void SetUp()
		{
			base.SetUp();
			aucClass = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "0405");
		}
	}
}
