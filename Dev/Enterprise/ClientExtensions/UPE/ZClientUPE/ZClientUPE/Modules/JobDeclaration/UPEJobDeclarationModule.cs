using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEJobDeclarationModule : ZFilterGridModule
	{
		public UPEJobDeclarationModule()
		{
		}

		public new UPEJobDeclarationFilterBusinessObject FilterBusinessObject
		{
			get { return (UPEJobDeclarationFilterBusinessObject)base.FilterBusinessObject; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.JobDeclaration; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new UPEJobDeclarationController();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UPEJobDeclarationFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JobDeclarationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UPEJobDeclarationFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Move To Classifier Queue", new EventHandler(MoveToClassifierQueue_Click)));
			return result.ToArray();
		}

		protected void MoveToClassifierQueue_Click(object sender, EventArgs e)
		{
			if (SelectedElements.Count == 0)
			{
				Globals.Message.ShowWarning("You must select a declaration to move.");
			}
			else if (SelectedElements.Count > 1)
			{
				Globals.Message.ShowWarning("You may only move one declaration at a time.");
			}
			else
			{
				MoveToClassifierQueue(SelectedElements[0].PK);
			}
		}

		protected virtual IReadOnlyList<BusinessObject> SelectedElements
		{
			get { return Grid.SelectedElements; }
		}

		void MoveToClassifierQueue(ZGuid declarationPK)
		{
			BusinessObjectFactory factoryForSaving = new BusinessObjectFactory();
			UPEJobDeclaration declaration = (UPEJobDeclaration)factoryForSaving.Load(typeof(UPEJobDeclaration), declarationPK);

			declaration.MoveToQueue(
				DeclarationQueueCodeDescriptionPairList.Codes.Classification,
				ZString.Empty,
				ZString.Empty,
				"Manual Move to Classifier Queue");

			factoryForSaving.Save();
		}

		#region BulkStatusUpdatingToolbarButton

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (GlbStaff.CurrentUser.GS_IsController)
			{
				result.Add(BulkStatusUpdatingMenuItem);
			}
			return result.ToArray();
		}

		MenuItem BulkStatusUpdatingMenuItem
		{
			get
			{
				if (bulkStatusUpdatingMenuItem == null)
				{
					bulkStatusUpdatingMenuItem = new ZMenuItem("Bulk Status Update");
					bulkStatusUpdatingMenuItem.Click += new EventHandler(OnBulkStatusUpdatingToolbarButton_Click);
				}
				return bulkStatusUpdatingMenuItem;
			}
		}
		MenuItem bulkStatusUpdatingMenuItem;

		void OnBulkStatusUpdatingToolbarButton_Click(object sender, EventArgs e)
		{
			UPEJobDeclarationFilterControl filterControl = (UPEJobDeclarationFilterControl)EmbeddedControl;
			using (BulkStatusUpdatingForm form = NewBulkStatusUpdatingForm(filterControl.FilteredGrid.SelectedElements))
			{
				form.ShowDialog(EmbeddedControl);
			}
		}

		protected virtual BulkStatusUpdatingForm NewBulkStatusUpdatingForm(BusinessObject[] selectedItems)
		{
			return new BulkStatusUpdatingForm(FilterBusinessObject, selectedItems);
		}

		#endregion

	}
}
