using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Renderer.Testing
{
	sealed class PageTest : TestCase
	{
		public void TestAreas()
		{
			var page = new Page();
			AssertNotNull("page.Areas", page.Areas);

			var area = new DummyArea();
			page.Areas.Add(area);
			AssertEquals("page.Areas.Count", 1, page.Areas.Count);
			AssertCollectionContains("page.Areas", area, page.Areas);
		}

		public void TestLastArea()
		{
			var page = new Page();
			AssertNull("page.LastArea", page.LastArea);

			var area1 = new DummyArea();
			page.Areas.Add(area1);
			AssertEquals("page.LastArea", area1, page.LastArea);

			var area2 = new DummyArea();
			page.Areas.Add(area2);
			AssertEquals("page.LastArea", area2, page.LastArea);
		}

		public void TestHeight()
		{
			var page = new Page();
			AssertEquals("page.Height", 0, page.Height);

			page.Height = 8;
			AssertEquals("page.Height", 8, page.Height);
		}

		public void TestAvailableHeight()
		{
			var page = new Page();
			page.Height = 100;
			AssertEquals("page.AvailableHeight", 100, page.AvailableHeight);

			var area1 = new DummyArea();
			area1.SetHeightInXls(8);
			page.Areas.Add(area1);
			AssertEquals("page.AvailableHeight", 92, page.AvailableHeight);

			var area2 = new DummyArea();
			area2.SetHeightInXls(6);
			page.Areas.Add(area2);
			AssertEquals("page.AvailableHeight", 86, page.AvailableHeight);
		}

		#region Implementation

		class DummyArea : Area
		{
			internal DummyArea()
				: base(0, 0, new Report(new DocumentPack(), null), string.Empty)
			{
			}

			public override Area Clone(int position)
			{
				throw new System.NotImplementedException();
			}

			public override bool CanCloseAPage
			{
				get { throw new System.NotImplementedException(); }
			}

			public override List<Area> Parents
			{
				get { throw new System.NotImplementedException(); }
			}

			public override ValueProviderDocumenter GetDocumentation()
			{
				throw new System.NotImplementedException();
			}

			int heightInXls;
			public override int HeightInXls
			{
				get { return heightInXls; }
			}

			internal void SetHeightInXls(int heightInXls)
			{
				this.heightInXls = heightInXls;
			}
		}

		#endregion
	}
}
