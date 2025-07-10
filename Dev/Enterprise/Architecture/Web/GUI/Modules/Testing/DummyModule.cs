#if DEBUG
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	public class WebDummyModule : ZFilterGridModule
	{
		public WebDummyModule(BusinessObjectFactory factory) : this(factory, null)
		{
		}

		public WebDummyModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.Dummy; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(GetType(), "DummyFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Testing");
		}

		public override Type FilterControlType
		{
			get { return typeof(GUI.Modules.Testing.DummyFilterControl); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults fFilterBusinessObjectDefaults = new FilterBusinessObjectDefaults();
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(DummyBizoSchema.Constants.Z0_Description, "Property", (ZString)"WebDummy"));
			return fFilterBusinessObjectDefaults;
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(DummyFilterBusinessObject); }
		}

		public override Type GridCollectionType
		{
			get { return typeof(DummyBusinessObjectCollection); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(DummyBizoSchema.Z0_Code.Name, DefaultSortOrder) };
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[5];

			fGridColumnFields[0] = new ZButtonColumn("Code", DummyBizoSchema.Constants.Z0_Code);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";
			fGridColumnFields[1] = new ZTextEditColumn("NVarChar", DummyBizoSchema.Constants.Z0_NVarChar);
			fGridColumnFields[2] = new ZCalcEditColumn("Text", DummyBizoSchema.Constants.Z0_VarCharMax);
			fGridColumnFields[3] = new ZTextEditColumn("Text2", DummyBizoSchema.Constants.Z0_NVarCharMax);

			ZGroupColumn group = new ZGroupColumn("TestGroup", fGridColumnFields[1], fGridColumnFields[2]);

			fGridColumnFields[4] = group;

			return fGridColumnFields;
		}

		protected override DataGridColumn[] GetDefaultGridColumnFields()
		{
			List<DataGridColumn> result = new List<DataGridColumn>();
			DataGridColumn[] allColumns = GridColumnFields;

			result.Add(allColumns[0]);
			result.Add(allColumns[4]);

			return result.ToArray();
		}
	}
}

#endif
