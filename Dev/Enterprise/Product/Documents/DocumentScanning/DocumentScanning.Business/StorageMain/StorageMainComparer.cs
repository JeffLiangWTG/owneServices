using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// This comparer is used to sort storagemain records in a grid on the eDocs tab. 
	/// However, this comparer will always return the storagemain record with IsTopLevelParent = true
	/// as the top of the list.
	/// </summary>
	public class StorageMainComparer : PropertyComparer
	{
		public StorageMainComparer(PropertyDescriptor property, ListSortDirection direction)
			: base(property, direction)
		{
		}

		public override int Compare(BusinessObject x, BusinessObject y)
		{
			if ((StorageMain)x == (StorageMain)y)
			{
				return 0;
			}
			else if (((StorageMain)x).IsTopLevelParent)
			{
				return -1;
			}
			else if (((StorageMain)y).IsTopLevelParent)
			{
				return 1;
			}
			else
			{
				return base.Compare(x, y);
			}
		}
	}
}
