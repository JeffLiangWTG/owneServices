using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Thumbnails.Testing
{
	sealed class ThumbNailListTest : TestCase
	{
		public void TestAdd()
		{
			using (ThumbNailList list1 = new ThumbNailList())
			{
				AssertEquals("elements count before add", 0, list1.Count);

				ThumbNailNode node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				AssertEquals("elements count after add", 1, list1.Count);
				AssertEquals("Indexer", node, list1[0]);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				AssertEquals("Controls count after add", 2, list1.Count);
				AssertEquals("Indexer", node, list1[1]);
			}
		}

		public void TestIndexer()
		{
			using (ThumbNailList list1 = new ThumbNailList())
			{
				ThumbNailNode node1 = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node1);
				AssertEquals("Indexer one element", node1, list1[0]);

				ThumbNailNode node2 = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node2);
				AssertEquals("Indexer first element", node1, list1[0]);
				AssertEquals("Indexer second element", node2, list1[1]);
			}
		}

		public void TestEnsureIEnumerable()
		{
			using (ThumbNailList list1 = new ThumbNailList())
			{
				ThumbNailNode node = new ThumbNailNode(new Panel(), new PictureBox(),
					new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(),
					new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				AssertEquals("List count", 2, list1.Count);

				foreach (ThumbNailNode curNode in list1)
				{
					Assert("Runtime sanity check", curNode.TheLabel != null);
				}
			}
		}

		public void TestSelectedPageIndexes()
		{
			using (ThumbNailList list1 = new ThumbNailList())
			{
				ThumbNailNode node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				node.Selected = true;
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				node.Selected = true;
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				int[] result = list1.SelectedPageIndexes;
				AssertEquals("Selected Count", 2, result.Length);
				AssertEquals("Selected 0", 1, result[0]);
				AssertEquals("Selected 1", 3, result[1]);
			}
		}

		public void TestUnselectedPageIndexes()
		{
			using (ThumbNailList list1 = new ThumbNailList())
			{
				ThumbNailNode node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				node.Selected = true;
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				node.Selected = true;
				list1.Add(node);

				node = new ThumbNailNode(new Panel(), new PictureBox(), new Label(), new Panel(), new Panel(), new Label(), null);
				list1.Add(node);

				int[] resultNotSelected = list1.UnselectedPageIndexes;
				AssertEquals("Not Selected Count", 3, resultNotSelected.Length);
				AssertEquals("Not Selected 0", 0, resultNotSelected[0]);
				AssertEquals("Not Selected 1", 2, resultNotSelected[1]);
				AssertEquals("Not Selected 2", 4, resultNotSelected[2]);
			}
		}
	}
}
