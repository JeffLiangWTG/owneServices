using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(EXTemplateSectionCollectionView))]
	public class TemplateSectionCollectionViewTest : BusinessObjectCollectionViewTestCase<EXTemplateSectionCollectionView>
	{
		public override void TestAddNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), delegate { Collection.AddNew(); });
			AssertExceptionThrown(typeof(NotSupportedException), delegate { Collection.AddNew(typeof(EXTemplateSection)); });
		}

		#region Implementation

		protected override EXTemplateSectionCollectionView GetCollectionToTest()
		{
			var collectionToFilter = new EXTemplateSectionCollection();
			return new EXTemplateSectionCollectionView(collectionToFilter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var templateSection = new TemplateSection(ZString.Empty, 0, 0);
			return new EXTemplateSection(templateSection, false);
		}

		#endregion
	}
}
