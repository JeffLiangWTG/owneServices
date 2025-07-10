using System.Collections;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	[ToolboxItem(false)]
	sealed class DummyListControl : ZListUserControl
	{
		protected override void UpdateLookups(IList list)
		{
			lookupCollectionCount = list?.Count ?? -1;
		}

		public int LookupCollectionCount
		{
			get
			{
				return lookupCollectionCount;
			}
		}

		int lookupCollectionCount;
	}
}
