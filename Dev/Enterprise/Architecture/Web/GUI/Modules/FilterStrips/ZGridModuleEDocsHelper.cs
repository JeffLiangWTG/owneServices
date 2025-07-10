using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Utilities;
using ResString = Enterprise.ZArchitecture.Web.GUI.ResString;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public static class ZGridModuleEDocsHelper
	{
		#region Grid eDocs Columns

		static readonly MultilingualString AllEDocsColumnHeaderText = ResString.GetMultilingualString("5db43795-3ce1-4b05-81f0-6966a8f8748f", "All eDocs");
		const string AllEDocsColumnKey = "AllEDocs";
		const string EDocsDownloadCssClass = "EDocsDownload";
		const string HasEDocsCssClass = "HasEDocs";

		public static void AddEDocsColumnsToProvider(BusinessObjectFactory factory, ZDataGrid grid, ISupportEDocsBulkDownload module, GridColumnProvider columnProvider)
		{
			var columnKey = 10000;
			foreach (var docType in GetRefDocTypes(factory, module))
			{
				columnKey++;
				var column = new ZCheckBoxForSelectColumn(docType.RT_DocType, docType.RT_DocType, grid, module) { ColumnKey = columnKey };
				column.OnCellPreRender = (sender, e) => OnCellPreRender(sender, e, grid, docType.RT_DocType, module);
				columnProvider.AddToDictionaryAsDefault(column);
			}

			var webEDocsDownloadEntry = module.GetRegistryWebEDocsBulkDownload();
			if (webEDocsDownloadEntry != null && webEDocsDownloadEntry.AllowAllDocTypes)
			{
				columnKey++;
				var allColumn = new ZCheckBoxForSelectColumn(AllEDocsColumnHeaderText, AllEDocsColumnKey, grid, module) { ColumnKey = columnKey };
				allColumn.OnCellPreRender = (sender, e) => OnCellPreRender(sender, e, grid, AllEDocsColumnKey, module);
				columnProvider.AddToDictionaryAsDefault(allColumn);
			}
		}

		static void OnCellPreRender(object sender, System.EventArgs e, ZDataGrid grid, string docType, ISupportEDocsBulkDownload module)
		{
			var cell = sender as TableCell;
			ZDataGridItem item = cell != null ? cell.Parent as ZDataGridItem : null;
			if (item != null)
			{
				cell.CssClass = ZCssHelper.Join(cell.CssClass, EDocsDownloadCssClass);
				var eDocs = grid.GetEDocs(item.ItemIndex, module);
				var hasEdocs = docType == AllEDocsColumnKey ? eDocs.Any() : eDocs.Any(doc => doc.DocType == docType);

				if (hasEdocs)
				{
					cell.CssClass = ZCssHelper.Join(cell.CssClass, HasEDocsCssClass);
				}
			}
		}

		static IEnumerable<IRefDocType> GetRefDocTypes(BusinessObjectFactory factory, ISupportEDocsBulkDownload module)
		{
			var collection = new List<IRefDocType>();
			var webEDocsDownloadEntry = module.GetRegistryWebEDocsBulkDownload();
			if (webEDocsDownloadEntry != null && webEDocsDownloadEntry.DocTypeCollection.Count > 0)
			{
				foreach (RefDocTypeEntry entry in webEDocsDownloadEntry.DocTypeCollection)
				{
					var docType = factory.Load<IRefDocType>(entry.RefDocTypePK);
					if (docType != null)
					{
						collection.Add(docType);
					}
				}
			}
			return collection.OrderBy(docType => docType.RT_DocType);
		}

		#endregion

		#region Get eDocs

		public static bool TryGetZippedEDocs(ZDataGrid grid, Stream outputStream)
		{
			using (var zipCreator = new ZGridModuleZippedEDocsCreator())
			{
				foreach (var entry in grid.SelectedCellsValues)
				{
					var gridPK = new ZGuid(entry.Key);
					var relevantPks = entry.Value.RelevantKeys;
					var parentReadableName = entry.Value.HumanReadableName;

					if (entry.Value.Values.Contains(AllEDocsColumnKey))
					{
						var eDocs = ObjectFactory.Get<IEDocsWebHelper>().GetEDocs(relevantPks);
						foreach (var eDoc in eDocs)
						{
							zipCreator.AddEDoc(eDoc, parentReadableName);
						}
					}
					else
					{
						foreach (var docType in entry.Value.Values)
						{
							var eDocs = ObjectFactory.Get<IEDocsWebHelper>().GetEDocsByDocType(relevantPks, docType);
							foreach (var eDoc in eDocs)
							{
								zipCreator.AddEDoc(eDoc, parentReadableName);
							}
						}
					}
				}

				return zipCreator.WriteTo(outputStream);
			}
		}

		class ZGridModuleZippedEDocsCreator : ZipEDocsCreator
		{
			public void AddEDoc(IeDocBase eDoc, string parentReadableName)
			{
				var fileName = GetEDocFileName(parentReadableName, eDoc);
				using (var edocStream = eDoc.GetImageDataReader())
				{
					AddEDoc(edocStream, fileName);
				}
			}

			string GetEDocFileName(string parentName, IeDocBase eDoc) => string.Format("[{0}]-[{1}]-{2}", parentName, eDoc.DocType, eDoc.FileName);
		}

		#endregion
	}
}
