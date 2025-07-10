using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public partial class ZFilterStripControl : ZFilterStripCommonControl
	{
		public ZFilterStripControl()
		{
			InitializeComponent();
		}

		public ZFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(filterBusinessObject)
		{
			this.gridCollection = gridCollection;
			InitializeComponent();
			AddAuditColumns();
		}

		#region Overrides

		public override IBusinessObjectCollection GridCollection
		{
			get { return gridCollection; }
		}

		readonly IBusinessObjectCollection gridCollection;

		#endregion

		#region FilteredGrid

		public ZDisplayGrid FilteredGrid
		{
			get { return (ZDisplayGrid)Grid; }
		}

		void AddAuditColumns()
		{
			if (GridCollection != null && BusinessObjectFactory.HasTableName(GridCollection.TypeOfElements))
			{
				FilterStripAuditDetails.AddAuditDetailsColumns(Grid, GridCollection.TableName, GridCollection.TypeOfElements);
			}
		}

		#endregion

		#region Bind

		protected override void Bind()
		{
			if (!isBound)
			{
				BindCore();
				isBound = true;
			}
		}

		bool isBound;

		protected virtual void BindCore()
		{
			var activeGridCollection = GridCollection as IActiveBusinessObjectCollection;
			if (activeGridCollection != null)
			{
				activeGridCollection.AdditionalFilter = ZQuery.NoResultQuery;
			}

			FilteredGrid.SetDataBinding(GridCollection, "");
			SetColorContextKeyFromParentModuleID();
		}

		#endregion

		Form parentForm;

		public override void HookFormEvents()
		{
			var form = TopLevelControl?.FindForm();

			if (form is IMainForm)
			{
				parentForm = form;
				parentForm.DragDrop += ParentForm_DragDrop;
				parentForm.DragOver += ParentForm_DragOver;
			}
		}

		public override void UnhookFormEvents()
		{
			if (parentForm != null)
			{
				parentForm.DragDrop -= ParentForm_DragDrop;
				parentForm.DragOver -= ParentForm_DragOver;
			}
		}

		void SetColorContextKeyFromParentModuleID()
		{
			if (ShouldSetColorContextKeyFromParentModuleID && FilterBusinessObject.ParentModule is ZModule module)
			{
				FilteredGrid.ColorContextKey = module.ID.Name;
			}
		}

		protected virtual ZBool ShouldSetColorContextKeyFromParentModuleID => false;

		void ParentForm_DragOver(object sender, DragEventArgs e)
		{
			OnDragOver(e);
		}

		void ParentForm_DragDrop(object sender, DragEventArgs e)
		{
			OnDragDrop(e);
		}

		DataGrid.HitTestInfo currentRowInfo;

		protected override void OnDragOver(DragEventArgs e)
		{
			var clientPoint = FilteredGrid.PointToClient(GetScreenPoint(e));
			var gridBounds = FilteredGrid.Bounds;
			currentRowInfo = null;

			if (gridBounds.Contains(clientPoint.X + gridBounds.X, clientPoint.Y + gridBounds.Y))
			{
				if (!CanAcceptDataCore(e.Data) && !CanAcceptCommonData(e.Data))
				{
					e.Effect = DragDropEffects.None;
				}
				else
				{
					e.Effect = DragDropEffects.Copy;

					currentRowInfo = FilteredGrid.HitTest(clientPoint.X, clientPoint.Y);
					var rowIndexOfItemUnderMouse = currentRowInfo.Row;

					if (rowIndexOfItemUnderMouse != -1)
					{
						FilteredGrid.CurrentRowIndex = rowIndexOfItemUnderMouse;
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017:DpiScaling", Justification = "Just assembling a point out of screen coordinates")]
		Point GetScreenPoint(DragEventArgs e) => new Point(e.X, e.Y);

		protected override void OnDragDrop(DragEventArgs e)
		{
			if (currentRowInfo != null)
			{
				BusinessObject targetBizO = null;

				if (currentRowInfo.Type == DataGrid.HitTestType.Cell || currentRowInfo.Type == DataGrid.HitTestType.RowHeader && currentRowInfo.Row != -1)
				{
					if (FilteredGrid.ListManager != null && currentRowInfo.Row < FilteredGrid.ListManager.Count)
					{
						targetBizO = (BusinessObject)FilteredGrid.ListManager.List[currentRowInfo.Row];
					}
				}

				MainThreadRunner.RunOnMainThread(() =>
				{
					var controller = FilterModule.GetNewController(targetBizO);

					if (controller == null)
					{
						return;
					}

					if (targetBizO != null)
					{
						targetBizO = controller.Factory.Load(controller.TypeOfTopLevelBusinessObject, targetBizO.PK); // need to reload on the controller factory and accept data tohere as otherwise accepted data will be lost when showing edit form
					}
					else if (targetBizO == null && FilterModule.NewMenuItem != null)
					{
						targetBizO = (BusinessObject)controller.GetNewBusinessEntityInLocalFactoryInternal();
					}

					var canAcceptBizO = CanAcceptDataCore(e.Data);

					if (targetBizO != null && canAcceptBizO)
					{
						AcceptDataCore(e.Data, targetBizO); // accept data (on the controller factory)
					}

					if (targetBizO != null && controller.ShowEditForm(targetBizO) is ZForm editForm)
					{
						if (canAcceptBizO)
						{
							AcceptDataCore(e.Data, editForm);
						}
						else
						{
							AcceptCommonData(e.Data, editForm);
						}
					}
				});
			}
		}

		protected virtual bool CanAcceptDataCore(IDataObject dataObject)
		{
			return false;
		}

		protected bool CanAcceptCommonData(IDataObject dataObject)
		{
			return Env.Security.eDocsModify.IsAllowed && dataObject.GetDataPresent(DataFormats.FileDrop);
		}

		protected virtual void AcceptDataCore(IDataObject dataObject, BusinessObject targetBizo)
		{
		}

		protected virtual void AcceptDataCore(IDataObject dataObject, ZForm editForm)
		{
		}

		void AcceptCommonData(IDataObject dataObject, ZForm editForm)
		{
			var eDocPlugIn = editForm.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);

			if (eDocPlugIn?.TopLevelTabControl == null)
			{
				Globals.Message.ShowError(Res.GetString("445CA83A-101A-43EC-AECC-E5DF00C06A58", "Form \'{0}\' does not support eDocs", editForm.CaptionResourceString.Caption));
				return;
			}

			editForm.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
			var value = dataObject.GetData(DataFormats.FileDrop);

			if (value != null)
			{
				((IDragDropSupportBase)eDocPlugIn).Add((string[])value);
			}
		}
	}
}
