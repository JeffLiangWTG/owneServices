using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZCodeDescriptionTreeViewPage.
	/// </summary>
	public class ZCodeDescriptionTreeViewPage : ZIFramePage
	{
		void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				Module.SelectedCode = RequestQueryString[ZCodeDescriptionTreeViewPopup.SelectedValueQuery];
			}
			SetupTreeView();
		}

		#region Module Setups

		WebModuleID fModuleID;

		protected WebModuleID ModuleID
		{
			get
			{
				if (fModuleID == null)
				{
					fModuleID = ZWebModuleFactory.GetWebModuleIDByName(RequestQueryString[ZCodeDescriptionTreeViewPopup.ModuleIDQuery]);
				}
				return fModuleID;
			}
		}

		ZTreeViewModule fModule;

		protected ZTreeViewModule Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = ZWebModuleFactory.Create(ModuleID, Factory) as ZTreeViewModule;
					if (fModule == null)
					{
						throw new ZException("Module ID : " + ModuleID.ToString() + " resolves to null. Web CodeDescriptionTreeView Modules must be of type ZTreeViewModule.");
					}
				}
				return fModule;
			}
		}

		public void SetupTreeView()
		{
			TreeView.Bind(Module.TreeCollection);
			TreeView.CodeMethod = new ZTreeViewWithDescription.IFamilyMemberHelper(Module.GetCode);
			TreeView.DescriptionMethod = new ZTreeViewWithDescription.IFamilyMemberHelper(Module.GetDescription);
			TreeView.QuantityMethod = new ZTreeViewWithDescription.IFamilyMemberHelper(Module.GetQuantity);
			TreeView.IsSelectableMethod = new ZTreeViewWithDescription.IFamilyMemberBoolHelper(Module.IsSelectable);
			TreeView.SelectedItem = Module.SelectedItem;
		}

		#endregion Module Setups

		#region Property Overrides

		protected override string[] OKFunctionArguments
		{
			// get { return new string[] { '\'' + "Code" + '\'' , '\'' + "Description" + '\'' }; }
			get { return new string[] { "GetSelectedItemCode()", "GetSelectedItemDescription()" }; } // javascript function arguments should not be translated
		}

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion Property Overrides

		protected ZTreeViewWithDescription TreeView;

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.Load += new EventHandler(this.Page_Load);
		}
		#endregion
	}
}
