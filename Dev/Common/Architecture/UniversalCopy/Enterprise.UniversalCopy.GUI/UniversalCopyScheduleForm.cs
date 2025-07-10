using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public partial class UniversalCopyScheduleForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public UniversalCopyScheduleForm()
		{
			InitializeComponent();
		}

		public UniversalCopyScheduleForm(StmUniversalCopyScheduleTask bo)
			: base(bo)
		{
			InitializeComponent();

			this.deleteButton.Visible = bo.IsInDatabase;
			this.showButton.Visible = bo.IsInDatabase;
			var deleteMenuItem = Menu.MenuItems.FindByName(ZFormMenuStrategy.FileDeleteMenuItemName, true);
			deleteMenuItem.Enabled = bo.IsInDatabase;
			deleteMenuItem.Click += deleteButton_Click;
		}

		public override string FormCaption
		{
			get { return Res.GetString("00b65f06-32eb-4c79-8c75-9260a9ea0ca6", "Copy Schedule"); }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		void deleteButton_Click(object sender, EventArgs e)
		{
			if (ShowConfirmationForDelete() == DialogResult.Yes)
			{
				this.Delete();
			}
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return Globals.Message.Show(
							Res.GetString("731eaba3-cb8b-49fe-9b97-3e269ee68ce6", "Are you sure you want to delete this copy schedule?"),
							Res.GetString("00b65f06-32eb-4c79-8c75-9260a9ea0ca6", "Copy Schedule"),
							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		protected override void DeleteCore()
		{
			((StmUniversalCopyScheduleTask)(this.BusinessEntity)).Delete();
			this.BusinessEntity.Factory.Save();
			Close();
		}

		void showButton_Click(object sender, EventArgs e)
		{
			StmUniversalCopy uc = ((StmUniversalCopyScheduleTask)this.BusinessEntity).Parent;
			BusinessObject businessObject = uc.CopyObject;
			ModuleIdentifier moduleID = uc.ModuleIdentifier;

			if (businessObject != null)
			{
				using (ZFilterModule module = NewModuleFromModuleID(moduleID))
				{
					if (module != null && module.HasActions)
					{
						if (module.AllowEdit)
						{
							module.ShowEditForm(businessObject);
						}
						else if (module.AllowView)
						{
							module.ShowViewForm(businessObject);
						}
					}
				}
			}
		}

		protected ZFilterModule NewModuleFromModuleID(ModuleIdentifier moduleID)
		{
			ZFilterModule module = null;

			if (moduleID != null && moduleID != ModuleIDs.NotAssigned)
			{
				var tempModule = GetZModule(moduleID) ?? throw new ZException("ZModuleFactory did not return a module for ID : " + moduleID.ToString());

				module = tempModule as ZFilterModule;
				if (module == null)
				{
					tempModule.Dispose();
					throw new ZException("Module with ID " + moduleID.ToString() + " of type " + tempModule.GetType().Name + " is not ZFilterModule.");
				}
			}

			return module;
		}

		[DefaultValue("")]
		public string CountryOverride { get; set; }

		ZModule GetZModule(ModuleIdentifier moduleID)
		{
			if (!string.IsNullOrEmpty(CountryOverride))
			{
				return ZModuleFactory.Instance.Create(moduleID, CountryOverride);
			}
			else
			{
				return ZModuleFactory.Instance.Create(moduleID);
			}
		}
	}
}
