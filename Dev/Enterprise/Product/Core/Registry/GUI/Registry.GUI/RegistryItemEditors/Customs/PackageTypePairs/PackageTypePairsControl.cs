using System.Collections.Generic;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Registry.GUI
{
	public partial class PackageTypePairsControl : RegistryZUserControl
	{
		public PackageTypePairsControl()
		{
			InitializeComponent();
			using (PackageTypesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				PackageTypesGrid.ReOrderColumns(PackageTypesGridColumnNamesInSortOrder);
			}
		}

		#region PackageTypesGridColumnNamesInSortOrder

		string[] PackageTypesGridColumnNamesInSortOrder
		{
			get
			{
				if (packageTypesGridColumnNamesInSortOrder == null)
				{
					List<string> columnNamesInSortOrderList = new List<string>();
					columnNamesInSortOrderList.Add(PackageTypePair.Schema.FreightPackageType);
					columnNamesInSortOrderList.Add(PackageTypePair.Schema.FreightPackageTypeDescription);
					columnNamesInSortOrderList.Add(PackageTypePair.Schema.CustomsPackageType);
					columnNamesInSortOrderList.Add(PackageTypePair.Schema.CustomsPackageTypeDescription);
					packageTypesGridColumnNamesInSortOrder = columnNamesInSortOrderList.ToArray();
				}
				return packageTypesGridColumnNamesInSortOrder;
			}
		}
		string[] packageTypesGridColumnNamesInSortOrder;

		#endregion

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PackageTypesGrid.ReadOnly = readOnly;
		}
	}
}
