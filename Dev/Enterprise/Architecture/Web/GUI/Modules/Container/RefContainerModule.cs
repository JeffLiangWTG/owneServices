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
	public class RefContainerModule : ZFilterGridModule
	{
		public RefContainerModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.RefContainer; }
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(ZFindBox), "RefContainerFilterControl.ascx", Page, "Enterprise.ZArchitecture.Web.GUI.Modules.Container");
		}

		public override Type FilterControlType
		{
			get { return typeof(RefContainerFilterControl); }
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(RefContainerFilterBusinessObject); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults fFilterBusinessObjectDefaults = new FilterBusinessObjectDefaults();
			return fFilterBusinessObjectDefaults;
		}

		public override Type GridCollectionType
		{
			get { return typeof(RefContainerCollection); }
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(RefContainerSchema.RC_Code.Name, DefaultSortOrder) };
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] fGridColumnFields = new DataGridColumn[3];
			fGridColumnFields[0] = new ZButtonColumn(Res.GetString("73c46250-8afb-40bb-8574-99e81753e625", "Code"), RefContainerSchema.RC_Code.Name);
			((ZButtonColumn)fGridColumnFields[0]).CommandName = "Select";
			fGridColumnFields[1] = new ZTextEditColumn(Res.GetString("b9093698-f0b0-4842-bdb7-02ed243aa991", "Description"), RefContainerSchema.RC_Description.Name);
			fGridColumnFields[2] = new ZTextEditColumn(Res.GetString("8b728a36-438d-4ff1-bafa-19ad0308bfde", "Mode"), RefContainerSchema.RC_ContainerType.Name);
			return fGridColumnFields;
		}
	}
}
