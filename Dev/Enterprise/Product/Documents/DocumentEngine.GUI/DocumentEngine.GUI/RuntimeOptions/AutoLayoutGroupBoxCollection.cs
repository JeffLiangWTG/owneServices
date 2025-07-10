using System.Collections;
using System.Linq;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public class AutoLayoutGroupBoxCollection : IEnumerable
	{
		#region Standared Collection Properties
		public IEnumerator GetEnumerator()
		{
			return OrderedGroupBoxes.GetEnumerator();
		}

		public AutoLayoutGroupBox this[string index]
		{
			get
			{
				return (AutoLayoutGroupBox)GroupBoxes[index];
			}
		}

		public void Add(AutoLayoutGroupBox groupBox)
		{
			OrderedGroupBoxes.Add(groupBox);
			GroupBoxes.Add(groupBox.Name, groupBox);
		}

		public int Count
		{
			get
			{
				return OrderedGroupBoxes.Count;
			}
		}

		public bool Contains(string keyValue)
		{
			return GroupBoxes.Contains(keyValue);
		}
		#endregion

		#region AutoLayout Specific Properties
		public void RearrangeGroupsInNColumns(int n)
		{
			foreach (AutoLayoutGroupBox group in this)
			{
				group.RearrangeInNColumns(n);
			}
		}

		public int TotalHeight
		{
			get
			{
				int height = 0;
				foreach (AutoLayoutGroupBox groupBox in this)
				{
					height += groupBox.Visible ? groupBox.Height : 0;
				}
				return height;
			}
		}

		public void MakeElementsVisibleIfTheyContainControls()
		{
			foreach (AutoLayoutGroupBox groupBox in this)
			{
				groupBox.Visible = groupBox.Controls.Count > 0;
			}
		}

		public bool ContainsControls
		{
			get
			{
				bool result = false;
				foreach (AutoLayoutGroupBox groupBox in this)
				{
					if (groupBox.Controls.Count > 0)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public int MaxDesiredWidth => this.Cast<AutoLayoutGroupBox>().Select(box => box.DesiredWidth).DefaultIfEmpty().Max();

		public int MaxDesiredHeight => this.Cast<AutoLayoutGroupBox>().Select(box => box.DesiredHeight).DefaultIfEmpty().Max();

		ArrayList OrderedGroupBoxes
		{
			get
			{
				if (fOrderedGroupBoxes == null)
				{
					fOrderedGroupBoxes = new ArrayList();
					GroupBoxes = new Hashtable();
				}
				return fOrderedGroupBoxes;
			}
		}
		#endregion

		ArrayList fOrderedGroupBoxes;
		Hashtable GroupBoxes;
	}
}
