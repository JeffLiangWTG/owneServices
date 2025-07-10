using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class AreaCollectionTest : TestCase
	{
		public void TestHeight()
		{
			var areas = new AreaCollection();
			AssertEquals("areas.Height", 0, areas.Height);

			var area1 = new DummyArea();
			area1.SetHeightInXls(8);
			areas.Add(area1);
			AssertEquals("areas.Height", 8, areas.Height);

			var area2 = new DummyArea();
			area2.SetHeightInXls(6);
			areas.Add(area2);
			AssertEquals("areas.Height", 14, areas.Height);

			areas.Clear();
			AssertEquals("areas.Height", 0, areas.Height);
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
