using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using static Enterprise.ZArchitecture.Web.Modules.TestFilterGridModulCustomSorter.Testing.TestFilterGridModulCustomSort;

namespace Enterprise.ZArchitecture.Web.Modules.TestFilterGridModulCustomSorter.Testing
{
	sealed class ZFilterGridModuleForTest : ZFilterGridModule
	{
		public ZFilterGridModuleForTest(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			var columns = new List<DataGridColumn>();

			columns.Add(new ZTextEditColumn("Code", "Z0_Code"));
			columns.Add(new TestColumnForSorting("Customs Clearance Commenced", "Z0_Date"));
			columns.Add(new ZCheckBoxColumn("Customs Clearance Commenced", "Z0_Date", "*CustommSorterColumn=" + ZGuid.NewZGuid().ToString()));

			return columns.ToArray();
		}

		protected override ZWebResource GetNewFilterControlResource() => new ZWebResource(GetType(), "DummyFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Testing");

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults() => new FilterBusinessObjectDefaults();

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(DummyBizoSchema.Z0_Date.Name, ListSortDirection.Ascending) };

		public override Type GridCollectionType => typeof(DummyBusinessObjectCollection);

		public override Type FilterBusinessObjectType => typeof(DummyFilterBusinessObject);

		public override Type FilterControlType => typeof(GUI.Modules.Testing.DummyFilterControl);

		public override ZArchitecture.Modules.ModuleIdentifier ID => WebModuleIDs.TrackingOrdersTimeline;
	}
}
