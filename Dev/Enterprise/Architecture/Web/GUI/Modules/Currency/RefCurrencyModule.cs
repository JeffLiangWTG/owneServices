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
	public class RefCurrencyModule : ZFilterGridModule
	{
		public RefCurrencyModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefCurrency; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefCurrencyFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Currency");
		}

		public override Type FilterControlType
		{
			get { return typeof(GUI.Modules.Currency.RefCurrencyFilterControl); }
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
			get { return typeof(GUI.Modules.Currency.WebRefCurrencyFilterBusinessObject); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefCurrencySchema.RX_Code.Name, DefaultSortOrder) };
		}

		public override Type GridCollectionType
		{
			get { return typeof(RefCurrencyCollection); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[2];

			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("432d1724-1dd9-4d74-a222-f5987a958627", "Code"), RefCurrencySchema.RX_Code.Name);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";

			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("11678650-2b05-4d92-8929-95c45bcff5ec", "Description"), RefCurrencySchema.RX_Desc.Name);
			return fGridColumnFields;
		}
	}
}
