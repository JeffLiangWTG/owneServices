using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocBuilder;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(EXTemplateSectionCollection))]
	public class EXTemplateSectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EXTemplateSectionCollection>
	{
		#region Implementation

		public override void TestAddNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), delegate { new EXTemplateSectionCollection().AddNew(); });
		}

		public override void TestTypedAddNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), delegate { new EXTemplateSectionCollection().AddNew(typeof(EXTemplateSection)); });
		}

		protected override EXTemplateSectionCollection GetCollectionToTest()
		{
			return new EXTemplateSectionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var templateSection = new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader + " , My Favourite Martian", 1, 2);

			return new EXTemplateSection(templateSection, false);
		}

		#endregion
	}
}
