using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class LocationModule : ZFilterGridModule
	{
		public LocationModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.Location; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "LocationFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Location");
		}

		public override Type FilterControlType
		{
			get { return typeof(GUI.Modules.Location.LocationFilterControl); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			if (fFilterBusinessObjectDefaults == null)
			{
				fFilterBusinessObjectDefaults = new FilterBusinessObjectDefaults();
			}

			return fFilterBusinessObjectDefaults;
		}
		FilterBusinessObjectDefaults fFilterBusinessObjectDefaults;

		public override Type FilterBusinessObjectType
		{
			get { return typeof(WebLocationFilterBusinessObject); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			ColumnAndSortOrder[] result = null;
			WebLocationFilterBusinessObject locationFilter = filter as WebLocationFilterBusinessObject;
			if (locationFilter != null)
			{
				if (locationFilter.IsRegion)
				{
					result = new[] { new ColumnAndSortOrder(RefZoneHeaderSchema.FZ_Code.Name, DefaultSortOrder) };
				}
				else if (locationFilter.IsCountry)
				{
					result = new[] { new ColumnAndSortOrder(RefCountrySchema.RN_Code.Name, DefaultSortOrder) };
				}
				else if (locationFilter.IsPort)
				{
					result = new[] { new ColumnAndSortOrder(RefUNLOCOSchema.RL_Code.Name, DefaultSortOrder) };
				}
			}

			return result;
		}

		public override Type GridCollectionType
		{
			get { return typeof(LocationCollection); }
		}

		protected override IBusinessObjectCollection LoadCollectionCore(FilterBusinessObject filterBizO1, ZQuery query, IBusinessObjectCollection collection, bool ignoreCache = false)
		{
			WebLocationFilterBusinessObject filterBizO = filterBizO1 as WebLocationFilterBusinessObject;
			LocationCollection locationCollection = (LocationCollection)collection;

			if (filterBizO != null)
			{
				switch (filterBizO.LocationType)
				{
					case LocationType.Country:
						locationCollection.LoadCountry(query);
						break;

					case LocationType.Zone:
						locationCollection.LoadZone(query);
						break;

					case LocationType.Port:
						locationCollection.LoadUNLoco(query);
						break;
				}
			}

			return locationCollection;
		}

		public override string GetBusinessObjectTableName(FilterBusinessObject filter)
		{
			string result = "";
			WebLocationFilterBusinessObject locationFilter = filter as WebLocationFilterBusinessObject;
			if (locationFilter != null)
			{
				if (locationFilter.IsRegion)
				{
					result = RefZoneHeaderSchema.Constants.TableName;
				}
				else if (locationFilter.IsCountry)
				{
					result = RefCountrySchema.Constants.TableName;
				}
				else if (locationFilter.IsPort)
				{
					result = RefUNLOCOSchema.Constants.TableName;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "column binding name")]
		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[3];

			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("2b2bafa4-df4b-4bfc-96b5-f1832e8376c2", "Code"), "Code");
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";

			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("ed246719-6adf-4772-a1e8-5642cc29a24f", "Name"), "Description");
			fGridColumnFields[2] = new ZTextEditColumn(Res.GetString("c8780521-a883-487f-84ee-d5f3b812e985", "State"), "StateDescription");
			return fGridColumnFields;
		}

		#region Sorting

		public override bool AllowSort
		{
			get { return false; }
		}

		#endregion
	}
}
