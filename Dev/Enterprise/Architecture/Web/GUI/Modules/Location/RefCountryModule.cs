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
	public class RefCountryModule : ZFilterGridModule
	{
		public RefCountryModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefCountry; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefCountryFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Location");
		}

		public override Type FilterControlType
		{
			get { return typeof(GUI.Modules.Location.RefCountryFilterControl); }
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
			get { return typeof(WebRefCountryFilterBusinessObject); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefCountrySchema.RN_Code.Name, DefaultSortOrder) };
		}

		public override Type GridCollectionType
		{
			get { return typeof(RefCountryCollection); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "column binding name")]
		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[2];

			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("5c527cc8-131b-43c1-973c-ad12060363bc", "Code"), "Code");
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";

			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("bded1dc2-a9ca-48e9-bba5-3899f9c71da5", "Description"), "Description");
			return fGridColumnFields;
		}
	}
}
