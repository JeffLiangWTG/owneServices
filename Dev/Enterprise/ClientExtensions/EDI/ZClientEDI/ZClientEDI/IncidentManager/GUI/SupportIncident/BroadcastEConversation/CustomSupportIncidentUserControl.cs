using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class CustomSupportIncidentUserControl : ZUserControl
	{
		public CustomSupportIncidentUserControl(List<SupportIncident> incidents)
			: base()
		{
			InitializeComponent();

			var collection = new CustomSupportIncidentCollection(new BusinessObjectFactory());
			foreach (var item in incidents.OrderBy(i => i.IM_IncidentNumber))
			{
				collection.Add(new CustomSupportIncident(item));
			}
			SetDataBinding(collection, "");
		}

		public IEnumerable<CustomSupportIncident> SelectedItems
				=> ((CustomSupportIncidentCollection)IncidentGrid.ListManager.List).Cast<CustomSupportIncident>().Where(j => j.IsChecked);

		internal void SelectAllForTest()
		{
			((CustomSupportIncidentCollection)IncidentGrid.ListManager.List).Cast<CustomSupportIncident>().ForEach(c => c.IsChecked = true);
		}
	}
}
