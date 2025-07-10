using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Modules;
#if WINZOR
using Graphics = System.Drawing.BGraphics;

#endif

namespace Enterprise.ZArchitecture.GUI
{
	public interface IZDisplayGridInternals
	{
		bool ForceShowMassUpdateMenuItem { get; set; }
	}

	[Testing.SuppressCheckControlLookupList]
	[Testing.SuppressCheckControlModuleId]
	[ToolboxItem(true)]
	public class ZDisplayGrid : ZFilterGrid, IZDisplayGridInternals
	{
		public ZDisplayGrid()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				LightColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.GridAlternatingRowColor;
			}
			CopyColumnCaptionsToBoundFields = false;
		}

		protected ZFilterGridModule ParentFilterGridModule { get { return ParentModule as ZFilterGridModule; } }

		protected internal override bool ShouldShowNotifications
		{
			get { return ParentFilterGridModule != null && ParentFilterGridModule.ModuleDecisionProvider.ShouldDisplayNotifications; }
		}

		protected override void UpdateGridNotificationType(Graphics graphics)
		{
			// do nothing, we do not want the grid to go into error and display icon in the top left corner
		}

		protected override void ValidateOnPositionChangedIfIncreasesSavePerformance()
		{
			// do nothing, not appropriate for findbox/module grids
		}

		[DefaultValue(false)]
		public override bool CopyColumnCaptionsToBoundFields
		{
			get { return base.CopyColumnCaptionsToBoundFields; }
			set { base.CopyColumnCaptionsToBoundFields = value; }
		}

		#region Excel Export

		internal override ExcelExporter ExcelExporter
		{
			get
			{
				if (ParentFilterGridModule != null && typeof(NonPersistentBusinessObject).IsAssignableFrom(ParentFilterGridModule.GetElementType()))
				{
					var businessObjectsToExport = ParentFilterGridModule.LoadMatchingFilterInNewFactory();
					return new ExcelExporter(businessObjectsToExport, ParentFilterGridModule.GetElementType(), GetNewColumnsForExport(), GetNewExcelExporterGuiNotifications());
				}
				else
				{
					return base.ExcelExporter;
				}
			}
		}

		protected internal override void ExportVisibleIntoAndOpenExcel()
		{
			if (ParentFilterGridModule == null || ParentFilterGridModule.ExportSecurityCheckpoint.IsAllowed)
			{
				try
				{
					base.ExportVisibleIntoAndOpenExcel();
				}
				catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 8623)
				{
					Globals.Message.ShowWarning(Res.GetString("e9ac607e-b6b6-4d0f-a3b1-d5b1979b89a2", "Your query is too complicated, please simplify your search conditions."));
				}
			}
			else
			{
				ParentFilterGridModule.ExportSecurityCheckpoint.ShowError();
			}
		}

		protected internal override void ExportIntoAndOpenExcel()
		{
			if (ParentFilterGridModule == null || ParentFilterGridModule.ExportSecurityCheckpoint.IsAllowed)
			{
				try
				{
					base.ExportIntoAndOpenExcel();
				}
				catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 8623)
				{
					Globals.Message.ShowWarning(Res.GetString("e9ac607e-b6b6-4d0f-a3b1-d5b1979b89a2", "Your query is too complicated, please simplify your search conditions."));
				}
			}
			else
			{
				ParentFilterGridModule.ExportSecurityCheckpoint.ShowError();
			}
		}

#if DEBUG
		public bool CanContinueWithExportExposedForTest
		{
			get { return CanContinueWithExport; }
		}
#endif

		protected override bool CanContinueWithExport
		{
			get
			{
				var result = base.CanContinueWithExport;

				if (!result)
				{
					try
					{
						result = GetFirstBizOInList() != null;
					}
					catch (NotImplementedException)
					{
						return false;
					}
				}

				return result;
			}
		}

		internal override bool ShowExportToExcelMenuItem
		{
			get { return ForceShowExportToExcelMenuItem; } // filter grids show an export menuitem in the Export/Import sub-menu
		}

		[DefaultValue(false)]
		public bool ForceShowExportToExcelMenuItem { get; set; }

		public ZQuery ExportQuery
		{
			get
			{
				return ParentFilterGridModule == null ? new ZQuery() : ParentFilterGridModule.ExportQuery.DeepClone();
			}
		}

		protected internal override BusinessObjectReader ReaderForExcelExport
		{
			get
			{
				if (ParentFilterGridModule == null)
				{
					return base.ReaderForExcelExport;
				}
				else
				{
					BusinessObjectReader result;

					if (List != null && List.Count > 0)
					{
						result = base.ReaderForExcelExport;
					}
					else
					{
						result = ParentFilterGridModule.BusinessObjectReaderWithQuery;
						var listReader = result as BusinessObjectListReader;
						if (listReader != null)
						{
							listReader.BatchSize = 1000;
						}
					}

					return result;
				}
			}
		}
#if DEBUG
		internal
#endif
		protected override BusinessObject GetFirstBizOInList()
		{
			if (ParentFilterGridModule == null)
			{
				return base.GetFirstBizOInList();
			}
			else
			{
				return (List != null && List.Count > 0) ? base.GetFirstBizOInList() : LoadTop1MatchingFilterInNewFactory();
			}
		}

		internal BusinessObject LoadTop1MatchingFilterInNewFactory()
		{
			var result = ParentFilterGridModule.GetFirstBizOInList();
			if (result == null && ParentFilterGridModule.ShouldLoadTop1WhenGridEmpty)
			{
				if (typeof(NonPersistentBusinessObject).IsAssignableFrom(ParentFilterGridModule.GetElementType()))
				{
					result = ParentFilterGridModule.LoadTop1MatchingFilterInNewFactory();
				}
				else
				{
					result = ParentFilterGridModule.GetNewFactoryForInternal().LoadTop1(ReaderForExcelExport.BusinessObjectType, ParentFilterGridModule.ExportQuery);
				}
			}
			return result;
		}

		protected override void ShowCannotCopyMessage()
		{
			Globals.Message.Show(Res.GetString("fc47a94f-79db-4132-99fc-45c8a6d6a54d", "Copy is disabled on this grid as you do not have Export To Excel Security rights."));
		}

		#endregion

		#region Import Data Tool

#if DEBUG
		internal
#else
		protected 
#endif
 override bool ShowImportDataMenuItem
		{
			get { return false; }
		}

#if DEBUG
		protected internal
#else
		protected 
#endif
 override IImportCollectionInfoProvider GetImportCollectionInfoProvider()
		{
			return ParentFilterGridModule as IImportCollectionInfoProvider;
		}

		#endregion

		public override bool ShowMassUpdateMenuItem
		{
			get => forceShowMassUpdateMenuItem;
			set { }
		}
		bool forceShowMassUpdateMenuItem;

		bool IZDisplayGridInternals.ForceShowMassUpdateMenuItem
		{
			get => forceShowMassUpdateMenuItem;
			set => forceShowMassUpdateMenuItem = value;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Enter || keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
			{
				var processDialogKeyMethod = typeof(Control).GetMethod("ProcessDialogKey", BindingFlags.Instance | BindingFlags.NonPublic);
				return (bool)processDialogKeyMethod.Invoke(Parent, new object[] { keyData });
			}
			else
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		protected override void SetInitialCellPosition()
		{
			//do nothing, so that if we sort before selecting anything we get 'nothing selected' sort behaviour
		}

#if !WINZOR
		protected internal override Brush ReadOnlyBrushFromRowNum(int rowNum)
		{
			return (rowNum % 2 == 1) ? LightBrush : Brushes.White;
		}
#endif

		protected internal override Color ReadOnlyColorForRowNum(int rowNum)
		{
			return (rowNum % 2 == 1) ? LightColor : Color.White;
		}

		public void SetScrollBarsInvisible()
		{
			VertScrollBar.Visible = false;
			HorizScrollBar.Visible = false;
		}

#if !WINZOR
		protected Brush LightBrush
		{
			get
			{
				if (fLightBrush == null)
				{
					fLightBrush = BrushProvider.FromColor(LightColor);
				}
				return fLightBrush;
			}
		}

		Brush fLightBrush;
#endif
		protected Color LightColor { get; private set; }
	}
}
