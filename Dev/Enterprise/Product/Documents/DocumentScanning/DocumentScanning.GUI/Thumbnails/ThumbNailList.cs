using System;
using System.Collections;

namespace Enterprise.DocumentScanning.Thumbnails
{
	public class ThumbNailList : CollectionBase, IDisposable
	{
		ThumbNailNode fBeforeFirstPageNode; // used for drag drop.

		public ThumbNailNode BeforeFirstPageNode
		{
			get { return fBeforeFirstPageNode; }
			set { fBeforeFirstPageNode = value; }
		}

		public ThumbNailNode this[int index]
		{
			get { return ((ThumbNailNode)List[index]); }
		}

		public int Add(ThumbNailNode nodeToAdd)
		{
			return List.Add(nodeToAdd);
		}

		public void ClearAll()
		{
			if (BeforeFirstPageNode != null)
			{
				BeforeFirstPageNode.Dispose();
			}

			for (int i = 0; i < Count; i++)
			{
				this[i].Dispose();
			}

			List.Clear();
		}

		public void Dispose()
		{
			ClearAll();
		}

		public int[] SelectedPageIndexes
		{
			get
			{
				ArrayList resList = new ArrayList();

				for (int i = 0; i < List.Count; i++)
				{
					if (this[i].Selected)
					{
						resList.Add(i);
					}
				}

				int[] result = new int[resList.Count];
				resList.CopyTo(result);
				return result;
			}
		}

		public int[] UnselectedPageIndexes
		{
			get
			{
				ArrayList resList = new ArrayList();

				for (int i = 0; i < List.Count; i++)
				{
					if (!this[i].Selected)
					{
						resList.Add(i);
					}
				}

				int[] result = new int[resList.Count];
				resList.CopyTo(result);
				return result;
			}
		}
	}
}
