using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DocumentaryOverrides))]
	sealed class DocumentaryOverridesTest : NonPersistentBusinessObjectTestCase
	{
		#region TestParent

		public void TestParent()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var documentaryOverrides = new DocumentaryOverrides(dummy);

			AssertEquals(nameof(DocumentaryOverrides.Parent), dummy, documentaryOverrides.Parent);
		}

		#endregion

		#region TestImportOverrides

		public void TestImportOverrides()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			var override1 = Factory.New<VisualizerDocumentData>();
			override1.JDD_ParentID = dummy.PK;
			override1.JDD_ParentTableCode = dummy.TablePrefix;
			override1.JDD_Name = "aaa";
			override1.JDD_OverriddenData = "<AAA />";

			var override2 = Factory.New<VisualizerDocumentData>();
			override2.JDD_ParentID = dummy.PK;
			override2.JDD_ParentTableCode = dummy.TablePrefix;
			override2.JDD_Name = "bbb";
			override2.JDD_OverriddenData = "<BBB />";

			var other = Factory.New<DummyBusinessObject>();

			var documentaryOverrides = new DocumentaryOverrides(other);
			documentaryOverrides.ImportOverrides(dummy);

			var newOverrides = documentaryOverrides
				.GetOverrides()
				.Select(o => $"{o.JDD_ParentID}|{o.JDD_ParentTableCode}|{o.JDD_Name}|{o.JDD_OverriddenData}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("imported overrides",
				new[]
				{
					$"{other.PK}|Z0|aaa|<AAA />",
					$"{other.PK}|Z0|bbb|<BBB />"
				},
				newOverrides);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			return new DocumentaryOverrides(dummy);
		}

		#endregion
	}
}
