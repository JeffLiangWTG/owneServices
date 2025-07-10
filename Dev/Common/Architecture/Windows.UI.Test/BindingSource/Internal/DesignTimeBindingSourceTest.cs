using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	class DesignTimeBindingSourceTest : TestCase
	{
		public void TestWithBindingContext()
		{
			DesignTimeBindingSource topLevelSource = new DesignTimeBindingSource();
			topLevelSource.DataSource = typeof(MasterEntity);

			CurrencyManager relatedCM = (CurrencyManager)BindingContext[topLevelSource, "DetailObjects.RelatedEntity"];
			DesignTimeBindingSource relatedSource = (DesignTimeBindingSource)relatedCM.List;
			AssertEquals("Should return a related design-time binding source for the right type", typeof(RelatedEntity), relatedSource.DataSource);
		}

		#region Test Classes

		protected class MasterEntity : ComponentModel.Testing.KComponent
		{
			public ChildEntityCollection DetailObjects
			{
				get { return detailObjects ?? (detailObjects = new ChildEntityCollection()); }
			}
			ChildEntityCollection detailObjects;
		}

		protected class ChildEntityCollection : ComponentModel.Testing.KBindingList<ChildEntity>
		{
		}

		protected class ChildEntity : ComponentModel.Testing.KComponent
		{
			public RelatedEntity RelatedEntity
			{
				get { return relatedEntity ?? (relatedEntity = new RelatedEntity()); }
			}
			RelatedEntity relatedEntity;
		}

		protected class RelatedEntity : ComponentModel.Testing.KComponent
		{
			public string Property { get; set; }
		}

		#endregion

		#region Implementation

		readonly BindingContext BindingContext = new BindingContext();

		#endregion
	}
}
