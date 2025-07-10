namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class NavigationElementCollection : System.Collections.CollectionBase
	{
		public NavigationElement this[int index]
		{
			get
			{
				return ((NavigationElement)List[index] );
			}
			set
			{
				List[index] = value;
			}
		}

		public int Add(NavigationElement value )
		{
			return (List.Add(value ) );
		}

		public int IndexOf(NavigationElement value )
		{
			return (List.IndexOf(value ) );
		}

		public void Insert(int index, NavigationElement value )
		{
			List.Insert(index, value );
		}

		public void Remove(NavigationElement value )
		{
			List.Remove(value );
		}

		public bool Contains(NavigationElement value )
		{
			// If value is not of type Int16, this will return false.
			return (List.Contains(value ) );
		}
	}
}
