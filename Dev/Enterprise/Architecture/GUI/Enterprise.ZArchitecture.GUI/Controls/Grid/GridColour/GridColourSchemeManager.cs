using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.Res;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.ZArchitecture
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:Types that own disposable fields should be disposable")]
	public class GridColourSchemeManager : IDisposable
	{
		public GridColourSchemeManager(ZGrid grid)
		{
			Argument.NotNull(grid, "grid");

			this.grid = grid;
			colourFactory = new GridColourFactory(grid);
			HandleColourMenus();
			var filterStripControl = grid.GetParentFilterControl();
			if (filterStripControl != null)
			{
				FilterBusinessObject = filterStripControl.FilterBusinessObject;
			}
		}

#if DEBUG
		internal
#endif
		readonly GridColourFactory colourFactory;

		public GridColourFactory GridColourFactory => colourFactory;

		protected virtual FilterStripBusinessObject GetFilterBusinessObjectForGrid()
		{
			using (var filterModule = GetFilterModule())
			{
				if (filterModule != null)
				{
					filterModule.DoNotCheckOrSaveChanges = true;
					var filterStripBusinessObject = filterModule.FilterBusinessObject;
					if (filterStripBusinessObject != null)
					{
						return filterStripBusinessObject;
					}
				}
			}
			return new GridFilterStripBusinessObject(grid);
		}

		ZFilterModule GetFilterModule()
		{
			if (grid.List != null)
			{
				var attributes = (ModuleIDAttribute[])grid.List.GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
				var moduleID = attributes.Length > 0 ? attributes[0].ModuleIdentifier : ModuleIDs.NotAssigned;

				if (moduleID != ModuleIDs.NotAssigned)
				{
					return ZModuleFactory.Instance.CreateNew(moduleID) as ZFilterModule;
				}
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer",
			Justification = "debugging only")]

		public void HandleExceptionWhenDecidingBackgroundColour(Exception exception, ColourDecidingEventArgs cdea)
		{
			const string commonUserMessage = "Problem in the colour scheme data, to fix this problem it will be necessary delete and create this colour scheme again";
			var handled = false;
			if (exception is System.Data.Common.DbException dbException)
			{
				var sqlException = new SqlExceptionWrapper(dbException);
				if (sqlException.Number == -2)
				{
					HandleTimeoutColorScheme(exception, cdea);
					handled = true;
				}
				else if (sqlException.Number == 8623)
				{
					HandleOvercomplicatedColorScheme(exception, cdea);
					handled = true;
				}
				else if (sqlException.Number == 33009)
				{
					throw exception;
				}
				else if (sqlException.Number == 4121 && sqlException.Message.Contains("Cannot find either column"))
				{
					HandleGridColourSchemeError(exception, cdea, commonUserMessage, string.Empty, string.Empty);
					handled = true;
				}
				else if (sqlException.Data["Type"]?.ToString() == "QueryPlanHintRelatedException")
				{
					HandleGridColourSchemeError(exception, cdea, exception.Message, string.Empty, "The query processor could not produce a query plan after removing hint");
					handled = true;
				}
			}
			else if (exception is EvaluateException evaluateException && evaluateException.Message.Contains("Cannot find column"))
			{
				HandleGridColourSchemeError(evaluateException, cdea, commonUserMessage, string.Empty, string.Empty);
				handled = true;
			}
			else if (exception is DatabaseUpgradeInProgressException)
			{
				//DatabaseUpgradeInProgressException is swallowed. Because during the server upgrade, the server should kick users off and close all open windows.
				handled = true;
			}

			if (!handled && !exception.IsCriticalException())
			{
				string reportKey;
				string message;
				string userMessage;
				if (IsGridBoundToInterfaceWithoutSupportGridColour(out var interfaceName))
				{
					reportKey = FormattableString.Invariant($"ExplicitlyImplemented_{interfaceName}");
					message = FormattableString.Invariant($"ZGrid {grid.Name} is bound the collection of interface ({interfaceName}) and the grid colour scheme filter contains a property that is not implemented publicly. To resolve this problem, implement the violating property publicly and add {nameof(SupportGridColourAttribute)} to the interface.");
					userMessage = "Grid colour scheme contains filter that is not supported";
				}
				else
				{
					reportKey = "GridColourSchemeManager.grid_CustomRowBackgroundColourDeciding - " + exception.GetType().Name;
					if (Globals.IsWinzor)
					{
						reportKey += " - Winzor";
					}
					message = $@"{exception.Message}
More infos: EnableQueryHintsForColourSchemeManager={SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.Value}. QueryTimeoutForColourSchemeManager={SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.Value}";
					userMessage = exception.Message;
				}
				HandleGridColourSchemeError(exception, cdea, userMessage, message, reportKey);
				handled = true;
			}

			if (!handled)
			{
				throw exception;
			}
		}

#if DEBUG
		internal
#endif
		void grid_CustomRowBackgroundColourDeciding(object sender, ColourDecidingEventArgs cdea)
		{
			try
			{
				grid_CustomRowBackgroundColourDecidingCore(cdea);
			}
			catch (Exception ex)
			{
				HandleExceptionWhenDecidingBackgroundColour(ex, cdea);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "debugging only")]
		void HandleOvercomplicatedColorScheme(Exception ex, ColourDecidingEventArgs cdea)
		{
			const string userMessage = "Color scheme configuration may be too complicated.\r\nPlease simplify it and try again";
			HandleGridColourSchemeError(ex, cdea, userMessage, string.Empty, string.Empty);
		}

		void HandleTimeoutColorScheme(Exception ex, ColourDecidingEventArgs cdea)
		{
			var timeout = SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.Value;
			var path = SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.GetLocation();
			var userMessage = Res.GetString("454222B2-E8DA-4D60-A551-3E456ECFE5CC", @"The selected Grid Color Scheme could not be applied in a timely manner. Please check if the Grid Color Scheme rules and filters can be made more specific.
The Grid Color Scheme exceeded the timeout value of {0} seconds that is set in the registry at {1}", timeout, path);

			HandleGridColourSchemeError(ex, cdea, userMessage, string.Empty, string.Empty);
		}

		bool IsGridBoundToInterfaceWithoutSupportGridColour(out string elementName)
		{
			if (grid.ElementType != null)
			{
				elementName = grid.ElementType.Name;
				return grid.ElementType.IsInterface && !Attribute.IsDefined(grid.ElementType, typeof(SupportGridColourAttribute));
			}

			if (grid.ElementTypeFromCollection != null)
			{
				elementName = grid.ElementTypeFromCollection.Name;
				return grid.ElementTypeFromCollection.IsInterface && !Attribute.IsDefined(grid.ElementTypeFromCollection, typeof(SupportGridColourAttribute));
			}

			elementName = string.Empty;
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "debugging only")]
		internal void HandleGridColourSchemeError(Exception ex, ColourDecidingEventArgs cdea, string userMessage, string wtgMessage, string reportOnceKey)
		{
			var originalStackTrace = ex.StackTrace;
			var originalMessage = ex.Message;
			var originalType = ex.GetType().FullName;
			GridColourScheme lastUsedScheme = null;
			var isNeedCheckCustomSqlFilter = false;

			if (ex is System.Data.Common.DbException sqlException)
			{
				isNeedCheckCustomSqlFilter = true;
			}

			try
			{
				lastUsedScheme = colourFactory.GetLastUsedSchemeForCurrentUser(FilterBusinessObject);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}

			var currentColorSchemeName = lastUsedScheme?.DisplayName ?? string.Empty;
			grid.CustomRowBackgroundColourDeciding -= grid_CustomRowBackgroundColourDeciding;

			var extraInfo = Res.GetString("F06B4A61-9820-4C1A-8CC1-681754C6309F", "Custom color schemes were temporarily disabled for this grid, please switch to default color scheme and check the current scheme filters and settings for possible errors.");

			if (isNeedCheckCustomSqlFilter && lastUsedScheme != null && IsCustomSqlFilterHasErrors(lastUsedScheme, out var sqlErrorMessage))
			{
				userMessage = sqlErrorMessage;
				reportOnceKey = string.Empty;
			}

			var errorMessage = Res.GetString(
				"9C5A2D62-A614-478E-9894-9E884B4086C1"
				, "Failed to apply color scheme \"{0}\" to grid rows.\r\nRule Name: {1}.\r\nReason: {2}.\r\n\r\n{3}"
				, currentColorSchemeName
				, ex.Data["RuleName"]?.ToString() ?? "Unknown"
				, userMessage
				, extraInfo);

			Globals.Message.ShowError(errorMessage, Res.GetString("7b97ea4a-984d-4455-830f-ae43f819e96c", "Grid Colors"));

			//Invalid column name 'x'
			if (ex is System.Data.Common.DbException sqlEx && excludedDbErrorTypes.Contains(new DbErrorMatch(sqlEx).ExceptionType))
			{
				reportOnceKey = string.Empty;
			}

			if (!string.IsNullOrEmpty(reportOnceKey))
			{
				var reporterErrorMessage = string.Format(
					Culture.Invariant
					, "Failed to apply colour scheme \"{0}\" to grid {1}: \r\nReason: {2}\r\nException info: {3}\r\nColourDecidingEventArgs: {4}\r\nXML of last layout: \"{5}\"\r\nOriginal Exception: {6}\r\nOriginal Exception Message: {7}\r\nOriginal Exception Stack Trace: {8}"
					, currentColorSchemeName
					, grid.Name
					, wtgMessage
					, ex?.ToString()
					, cdea?.ToString()
					, lastUsedScheme?.S9_FilterData.ToUTF8().TrimEndSpaceTab()
					, originalType
					, originalMessage
					, originalStackTrace) ?? string.Empty;

				if (ex.Data.Contains("ColorFilterLayout") || ex.Data.Contains("ColorFilterValueLayout"))
				{
					reporterErrorMessage += $"\r\nXML of Color Filter Layout: \r\n{ex.Data["ColorFilterLayout"]} \r\nXML of Color Filter Value Layout: \r\n{ex.Data["ColorFilterValueLayout"]}";
				}

				ErrorReporter.ReportOnce(reportOnceKey, reporterErrorMessage, ex);
			}
		}
		readonly DbErrorType[] excludedDbErrorTypes = { DbErrorType.InvalidColumnName, DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseMinimumWorktableWasTooLong };

		bool IsCustomSqlFilterHasErrors(GridColourScheme scheme, out string errorMessages)
		{
			foreach (var strip in scheme.ColourStrips.ToArray())
			{
				if (strip.HasCustomSqlFilter)
				{
					foreach (var filter in strip.ActiveModuleFilters.OfType<ModuleSQLFilter>().ToArray())
					{
						filter.Validation.ValidateSqlOnFind(true);
						errorMessages = Res.GetString("5736f88a-5615-4363-98e5-cba7ed983c53", "There is a custom SQL error in rule \"{0}\", the error SQL is: {1}", strip.RuleName, filter.Property1);
						return true;
					}
				}
			}

			errorMessages = string.Empty;
			return false;
		}

		internal bool ShouldPrecalculateColorsForAllRowsInGrid()
		{
			var colourScheme = colourFactory.GetLastUsedSchemeForCurrentUser(FilterBusinessObject);
			if (colourScheme != null)
			{
				return AllowPrecalculateColorsForAllRowsInGrid
					   && colourScheme.ColourStrips.Any(gridColourStrip => !gridColourStrip.Filter.IsDataViewOptimisable)
					   && !typeof(NonPersistentBusinessObject).IsAssignableFrom(grid.ElementTypeFromCollection);
			}
			return false;
		}

#if DEBUG
		protected virtual
#endif
		void grid_CustomRowBackgroundColourDecidingCore(ColourDecidingEventArgs e)
		{
			e.Colour = Color.Empty;

			if (!e.Pk.IsEmpty)
			{
				if (grid.ColorByPK.TryGetValue(e.Pk, out var color))
				{
					e.Colour = color;
				}
				else
				{
					var colourScheme = colourFactory.GetLastUsedSchemeForCurrentUser(FilterBusinessObject);
					if (colourScheme != null)
					{
						if (ShouldPrecalculateColorsForAllRowsInGrid())
						{
							if (grid.IsBackgroundColourLoaded)
							{
								grid.CurrentLoadColorsVersion = Guid.NewGuid();
							}
						}
						else
						{
							grid.ColorByPK[e.Pk] = Color.Empty;
							var bizo = e.ObjectAtRow as BusinessObject;
							if (bizo != null)
							{
								foreach (var gridColourStrip in colourScheme.ColourStrips)
								{
									var query = gridColourStrip.Filter;
									query.AddOptionRecompileConditionally = false;
									((ISupportMainElement)query).SetMainElement(bizo);

									if (bizo.MatchesFilter(query))
									{
										grid.ColorByPK[e.Pk] = gridColourStrip.BGColor;
										e.Colour = gridColourStrip.BGColor;
										break;
									}
								}
							}
						}
					}
					else
					{
						if (AllowPrecalculateColorsForAllRowsInGrid)
						{
							foreach (var o in grid.List)
							{
								var pk = o as BusinessObject != null ? (o as BusinessObject).PK : ZGuid.Empty;
								if (!pk.IsEmpty)
								{
									grid.ColorByPK[pk] = Color.Empty;
								}
							}
						}
					}
				}
			}
		}

#if DEBUG
		public IEnumerable<string> ExecutedCommandsForUT;
		public int DBHitNumberForUT;
		public bool SimulateNullElementForTest;
#endif

		string GetNullElementTypesErrorMessage(IEnumerable<Type> bizoTypesPresentInGrid)
		{
			return $"Types present in grid resulted in null elementType: {string.Join(", ", bizoTypesPresentInGrid.Select(x => x.FullName))}";
		}

		void HandleNullElementTypeError(IEnumerable<Type> bizoTypesPresentInGrid)
		{
			colourFactory.SetLastUsedSchemeForCurrentUser(FilterBusinessObject, null);
			Globals.Message.ShowError(Res.GetString("7DA3E9A2-FD26-4219-A013-12D5944B83E2", "An error occurred while loading the color scheme for this grid. The Standard color scheme has been automatically selected to allow you to continue your work."));
			ErrorReporter.ReportDeveloperExceptionOnce("Element type is null in GetPrecalculateColorsForAllRowsInGridQueries", new Exception(GetNullElementTypesErrorMessage(bizoTypesPresentInGrid)));
		}

		internal LoadColorsData GetPrecalculateColorsForAllRowsInGridQueries(GridColourScheme colourScheme)
		{
			var dataObject = new LoadColorsData();
			dataObject.ColourScheme = colourScheme;

			var rowsThatNeedColor = dataObject.RowsThatNeedsColour;
			var bizos = new List<BusinessObject>();
			SchemaColumn reloadPerformanceIncreaseColumn = null;
			Type elementType = null;
			var queries = dataObject.Queries;

			// Debugging variable to track all element types in the grid to help find issue for WI00907674
			var bizoTypesPresentInGrid = new HashSet<Type>();

			for (var i = 0; i < grid.ListManager.List.Count; i++)
			{
				var rowsBO = grid.ListManager.List[i] as BusinessObject;
				if (rowsBO != null)
				{
					if (!grid.ColorByPK.ContainsKey(rowsBO.PK) || grid.ColorByPK[rowsBO.PK] == Color.Empty || grid.ColorByPKIsExpired)
					{
						bizos.Add(rowsBO);
						(int, object) tuple = (i, null);
						if (rowsBO is IBusinessObjectReload reloader)
						{
							tuple.Item2 = reloader.ReloadPerformanceIncreaseColumnValue;
							reloadPerformanceIncreaseColumn = reloadPerformanceIncreaseColumn ?? reloader.ReloadPerformanceIncreaseColumn;
						}
						rowsThatNeedColor[rowsBO.PK] = tuple;
						dataObject.PkColumn = dataObject.PkColumn ?? rowsBO.PKSchemaColumn;
						var bizoType = rowsBO.GetType();

						bizoTypesPresentInGrid.Add(bizoType);

						if (elementType == null)
						{
							elementType = bizoType;
						}
						else if (elementType != bizoType)
						{
							while ((!elementType.IsAssignableFrom(bizoType) || elementType.IsAbstract) && elementType != typeof(BusinessObject) && elementType != typeof(object))
							{
								elementType = elementType.BaseType;
							}
						}
					}
				}
			}
			
			if (elementType == typeof(BusinessObject) || elementType == typeof(object))
			{
				elementType = null;
			}

#if DEBUG
			if (SimulateNullElementForTest)
			{
				elementType = null;
			}
#endif

			if (rowsThatNeedColor.Count > 0 && dataObject.PkColumn != null)
			{
				if (elementType is null)
				{
					HandleNullElementTypeError(bizoTypesPresentInGrid);
					return new LoadColorsData();
				}
				AddColourStripQueries(dataObject, bizos, elementType, reloadPerformanceIncreaseColumn);
			}

			return dataObject;
		}

		void AddColourStripQueries(LoadColorsData dataObject, List<BusinessObject> bizos, Type elementType, SchemaColumn reloadPerformanceIncreaseColumn)
		{
			var queries = dataObject.Queries;
			var rowsThatNeedColor = dataObject.RowsThatNeedsColour;
			var colourScheme = dataObject.ColourScheme;
			foreach (var gridColourStrip in colourScheme.ColourStrips.ToList())
			{
				var pks = new ZGuid[rowsThatNeedColor.Count];
				rowsThatNeedColor.Keys.CopyTo(pks, 0);

				var query = new ZDBOnlyQuery(elementType);
				query.AllowTableValuedParameters = true;
				query.IsNoLock = true;
				query.AddToFilter(JoinCondition.And, dataObject.PkColumn, pks); // PK filter should appear first. Additional filters should be added after this line.
				query.AddToFilter(gridColourStrip.GetFilterForGridItems(bizos).ShallowClone());
				if (reloadPerformanceIncreaseColumn != null && !query.IsDataViewOptimisable)
				{
					var columnValues = pks.Select(pk => rowsThatNeedColor[pk].Item2).Where(item2 => item2 != null);
					if (columnValues.Any())
					{
						query.AddToFilter(JoinCondition.And, reloadPerformanceIncreaseColumn, columnValues);
					}
				}

				var parentFilterIsForceSeek = query.IsForceSeek;
				var useHint = dataObject.PkColumn.Indexed && SystemDataRegistry.Instance.EnableQueryHintsForColourSchemeManager.Value && !gridColourStrip.ActiveModuleFilters.Any(f => f is ModuleSQLFilter);
				TableIndexHint hint = default(TableIndexHint);
				if (useHint)
				{
					hint = new TableIndexHint(dataObject.PkColumn.TableSchema.PkIndexName);
					query.TableIndexHints.Add(hint);
					query.IsForceSeek = true;
				}

				query.Timeout = SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.Value;
				queries.Add(gridColourStrip, new LoadColorsData.QueryAttrs(elementType, query, parentFilterIsForceSeek, useHint, hint));
			}
		}

		internal LoadColorsData LoadGridColors(LoadColorsData dataObject)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var colorStripMapBizoPks = new Dictionary<string, List<ZGuid>>();
				var factory = new BusinessObjectFactory();
#if DEBUG
				factory.RowsLoaded += (sender, args) =>
				{
					if (args.Query.LiteralTextSqlFormatted.Contains("TestQueryReRunFailsShouldNotifyUser"))
					{
						throw CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(8622, "Query processor could not produce a query plan");
					}
				};
#endif
				var colourScheme = dataObject.ColourScheme;
				var rowsThatNeedColorKeys = dataObject.RowsThatNeedsColour.Keys.ToList();
				foreach (var gridColourStrip in colourScheme.ColourStrips.ToList())
				{
					var attr = dataObject.Queries[gridColourStrip];
					var filteredBizObjects = Array.Empty<BusinessObject>();
					var elementType = attr.ElemenType;
					var query = attr.Query;
					var parentFilterIsForceSeek = attr.ParentFilterIsForceSeek;
					var useHint = attr.UseHint;
					var hint = attr.Hint;
					var queryType = elementType ?? grid.ElementTypeFromCollection;

#if DEBUG
					ParameterSettingsCache.RefreshCache();
					using (((IDbConnected)factory).Connection.TrackExecutedCommands())
					{
#endif
						try
						{
							filteredBizObjects = factory.Load(queryType, query);
						}
						catch (System.Data.Common.DbException ex) when (useHint && IsQueryPlanHintException(ex))
						{
							ex.Data["Type"] = "QueryPlanHintRelatedException";
							ex.Data["RuleName"] = gridColourStrip.RuleName;
							ex.Data["ColorFilterLayout"] = gridColourStrip.FilterStrips.GetLayoutAsXml().ToAscii();
							ex.Data["ColorFilterValueLayout"] = gridColourStrip.FilterStrips.GetLayoutValuesAsXml().ToAscii();
							throw;
						}
#if DEBUG
						finally
						{
							ExecutedCommandsForUT = ((IDbConnected)factory).Connection.ExecutedCommands;
						}
					}
#endif
					var filteredBizObjectPKs = filteredBizObjects.Select(x => x.PK).ToList();

					colorStripMapBizoPks[gridColourStrip.RuleName] = filteredBizObjectPKs;
					//If a non persistent filter exists, we have to check the bizo directly, which is unsafe on a second thread.
					//So note that this filter might not end up being what actually applies its color; therefore we should keep checking rules for this bizo just in case.
					var nonPersistentFilter = gridColourStrip.GetNonPersistentFilterForGridItems();
					if (nonPersistentFilter == null)
					{
						rowsThatNeedColorKeys.RemoveAll(x => filteredBizObjectPKs.Contains(x));
					}
					if (rowsThatNeedColorKeys.Count == 0)
					{
						break;
					}
				}
#if DEBUG
				DBHitNumberForUT = factory.GetTableHitCount(dataObject.PkColumn.TableName);
#endif
				dataObject.ColourResult = colorStripMapBizoPks;
			}
			return dataObject;
		}

		bool IsQueryPlanHintException(System.Data.Common.DbException ex)
		{
			var errorType = new DbErrorMatch(ex).ExceptionType;
			return errorType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseOfHints || errorType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseMinimumWorktableWasTooLong;
		}

		internal void ApplyColors(LoadColorsData dataObject)
		{
			var colourScheme = dataObject.ColourScheme;
			var colorStripMapBizoPks = dataObject.ColourResult;
			var rowsThatNeedColor = dataObject.RowsThatNeedsColour;

			foreach (var gridColourStrip in colourScheme.ColourStrips)
			{
				if (!colorStripMapBizoPks.TryGetValue(gridColourStrip.RuleName, out var filteredBizObjectPks))
				{
					continue;
				}

				var nonPersistentFilter = gridColourStrip.GetNonPersistentFilterForGridItems();

				foreach (var bizObjectPk in filteredBizObjectPks)
				{
					if (rowsThatNeedColor.TryGetValue(bizObjectPk, out var value))
					{
						//Now finally we can check the non-persistent filter.
						//If non-persistent filter fails, we'll keep checking later rules we checked in LoadColors and one of them will have a chance to match.
						var nonPersistentChecked = true;
						if (nonPersistentFilter != null)
						{
							if (dataObject.RowsThatNeedsColour.TryGetValue(bizObjectPk, out var value2))
							{
								var rowIndex = value2.Item1;
								if (grid.ListManager.List[rowIndex] is BusinessObject filteredBizo && filteredBizo != null)
								{
									((ISupportMainElement)nonPersistentFilter).SetMainElement(filteredBizo);
									if (!filteredBizo.MatchesFilter(nonPersistentFilter))
									{
										nonPersistentChecked = false;
									}
								}
							}
						}

						if (nonPersistentChecked)
						{
							grid.ColorByPK[bizObjectPk] = gridColourStrip.BGColor;
							rowsThatNeedColor.Remove(bizObjectPk);
						}
					}
				}
				if (rowsThatNeedColor.Count == 0)
				{
					break;
				}
			}

			if (rowsThatNeedColor.Any())
			{
				rowsThatNeedColor.Keys.ForEach(pk =>
				{
					grid.ColorByPK[pk] = Color.Empty;
				});
			}

			grid.ColorByPKIsExpired = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3012:AvoidBoolLiteralsInLargerBoolExpressions", Justification = "#if directive")]
#if DEBUG
		internal
#endif
 bool AllowPrecalculateColorsForAllRowsInGrid
		{
			get
			{
				return
#if DEBUG
 allowPrecalculateColorsForAllRowsInGridForTest ??
#endif
 true;
			}
#if DEBUG
			set
			{
				allowPrecalculateColorsForAllRowsInGridForTest = value;
			}
#endif
		}

#if DEBUG
		bool? allowPrecalculateColorsForAllRowsInGridForTest;
#endif

		void HandleColourMenus()
		{
			GridColoursParentMenuItem = new ZMenuItem(ResString.GetMultilingualString("5eedea01-4caa-496c-ae9d-3e4a0ac014de", "Grid Colors"));
			GridColoursParentMenuItem.Select += delegate
			{ ToggleAndPopulateGridColourMenuItems(); };
			GridColoursParentMenuItem.Name = "GridColors";

			GridColourSelectMenuItem = new ZMenuItem(ResString.GetMultilingualString("dcd4ac18-11e8-4c15-ace9-4970dfc8295d", "Select Color Scheme"));
			GridColourNewMenuItem = new ZMenuItem(ResString.GetMultilingualString("70ae9534-6b52-4415-a5e9-4fe36b2f9c82", "Create New Scheme"), CreateNewGridColourScheme);
			GridColourManageMenuItem = new ZMenuItem(ResString.GetMultilingualString("fa844d24-512f-496a-a58e-5e8d17740652", "Manage Color Schemes"));

			GridColoursParentMenuItem.MenuItems.Add(0, GridColourSelectMenuItem);
			GridColoursParentMenuItem.MenuItems.Add(1, GridColourNewMenuItem);
			GridColoursParentMenuItem.MenuItems.Add(2, GridColourManageMenuItem);

			grid.CustomRowBackgroundColourDeciding += grid_CustomRowBackgroundColourDeciding;

			grid.ContextMenu.MenuItems.Add(GridColoursParentMenuItem);
		}

#if DEBUG
		internal
#else
		protected
#endif
 void CreateNewGridColourScheme(object sender, EventArgs e)
		{
			var baseControl = GetBaseFilterControl(out var newFilterControl);
			var scheme = new BusinessObjectFactory().New<GridColourScheme>();
			scheme.S9_ModuleID = GridColourFactory.GetModuleId(FilterBusinessObject, grid, true);
			var customColourForm = (ZChildForm)ObjectFactory.Get<IZGridColourCustomiseForm>("IZGridColourCustomiseForm", scheme, FilterBusinessObject, baseControl, grid.ElementTypeFromCollection);
			customColourForm.DisplayMode = ODisplayMode.Edit;
			ZFormModaliser.ShowDialogAndDispose(customColourForm);

			grid.RefreshTableStyles();

			if (newFilterControl)
			{
				baseControl.Dispose();
			}
		}

		ZFilterStripCommonControl GetBaseFilterControl(out bool newControl)
		{
			newControl = false;
			var baseControl = grid.GetParentFilterControl();

			if (baseControl == null)
			{
				using (var filterModule = GetFilterModule())
				{
					if (filterModule != null)
					{
						baseControl = filterModule.GetNewFilterControlForGrid() as ZFilterStripCommonControl;
						newControl = true;
					}
				}
			}

			return baseControl;
		}

		void ToggleAndPopulateGridColourMenuItems()
		{
			if (GridColourSelectMenuItem != null)
			{
				var schemes = colourFactory.GetAllSchemesForActiveFilter(FilterBusinessObject);
				var activeSchemeForUser = colourFactory.GetLastUsedSchemeForCurrentUser(FilterBusinessObject);

				PopulateGridColorNames(GridColourSelectMenuItem, schemes, activeSchemeForUser, SetActiveGridColorForActiveUser, true);
				PopulateGridColorNames(GridColourManageMenuItem, schemes, activeSchemeForUser, ManageColourScheme, false);

				GridColourSelectMenuItem.Enabled = true;
				GridColourManageMenuItem.Enabled = GridColourManageMenuItem.MenuItems.Count > 0;
			}
		}

		void PopulateGridColorNames(MenuItem parentMenu, GridColourScheme[] schemes, GridColourScheme activeSchemeForUser, EventHandler menuHandler, bool withStandard)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("PopulateGridColorNames"))
			{
				parentMenu.MenuItems.Clear();

				if (withStandard)
				{
					var standardSchemeMenuItem = new ZMenuItem(ResString.GetMultilingualString("fb485c8d-bc2d-4581-b7c0-070cf5e0e69e", "Standard*"), menuHandler);
					standardSchemeMenuItem.Tag = null;
					standardSchemeMenuItem.Checked = activeSchemeForUser == standardSchemeMenuItem.Tag;
					parentMenu.MenuItems.Add(standardSchemeMenuItem);

					if (schemes.Length > 0)
					{
						parentMenu.MenuItems.Add(new ZMenuItem("-"));
					}
				}

				foreach (var scheme in schemes)
				{
					var menuName = scheme.S9_FilterNameMultilingual;
					if (scheme.S9_IsPublished && !scheme.S9_IsSystem)
					{
						menuName = MultilingualString.Join("", menuName, (NoResString)"*");
					}
					if (parentMenu.MenuItems[menuName.GetUnresolvedString()] == null)
					{
						var schemeNameMenuItem = new ZMenuItem(menuName, menuHandler);
						schemeNameMenuItem.Name = menuName.GetUnresolvedString();
						schemeNameMenuItem.Tag = scheme;
						var menuChecked = activeSchemeForUser == schemeNameMenuItem.Tag;
						schemeNameMenuItem.Checked = menuChecked;
						parentMenu.MenuItems.Add(schemeNameMenuItem);

						if (menuChecked && withStandard) // Manage Menu color schemes do not need to set strips again
						{
							SetStripsFromFilterForScheme(scheme);
						}
					}
				}
			}
		}

		void SetStripsFromFilterForScheme(GridColourScheme scheme)
		{
			if (scheme != null)
			{
				var filterStripBusinessObject = colourFactory.Control != null ? colourFactory.Control.FilterBusinessObject : FilterBusinessObject;
				scheme.SetStripsFromFilter(filterStripBusinessObject, grid.ElementTypeFromCollection);
			}
		}

		protected void ManageColourScheme(object sender, EventArgs e)
		{
			var scheme = ShowManageSchemeForm(sender);
			if (!scheme.IsDeleted)
			{
				scheme.ResetStrips();
			}
			colourFactory.ResetSchemes();

			ResetGridColours();
		}

#if DEBUG
		protected virtual
#endif
 GridColourScheme ShowManageSchemeForm(object sender)
		{
			var scheme = (GridColourScheme)((MenuItem)sender).Tag;

			var newFilterControl = false;
			ZFilterStripCommonControl baseControl = null;

			try
			{
				baseControl = GetBaseFilterControl(out newFilterControl);
				scheme.S9_ModuleID = GridColourFactory.GetModuleId(FilterBusinessObject, grid, true);

				var newFactory = scheme.Factory.CreateNewFactory();
				newFactory.NameForDebugging = "GridColourSchemeManager.ShowManageSchemeForm";

				var query = new ZDBOnlyQuery(typeof(StmModuleFilter));
				query.AddToFilter(StmModuleFilterSchema.PK, scheme.PK);
				var newScheme = newFactory.LoadTop1<GridColourScheme>(query);
				newScheme.SetStripsFromFilter(FilterBusinessObject, grid.ElementTypeFromCollection);

				ShowManageSchemeFormCore(newScheme, FilterBusinessObject, baseControl, grid.ElementTypeFromCollection);
				return scheme;
			}
			finally
			{
				if (newFilterControl)
				{
					baseControl?.Dispose();
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		void ShowManageSchemeFormCore(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl, Type businessEntityType)
		{
			using (var customColorForm = ObjectFactory.Get<IZGridColourCustomiseForm>("IZGridColourCustomiseForm", scheme, filterStripBusinessObject, stripControl, businessEntityType))
			{
				customColorForm.DisplayMode = ODisplayMode.Edit;
				ZFormModaliser.ShowDialogAndDispose((Form)customColorForm);
			}
		}

		void SetActiveGridColorForActiveUser(object sender, EventArgs e)
		{
			var selectedMenu = (MenuItem)sender;
			var scheme = (GridColourScheme)selectedMenu.Tag;
			SetStripsFromFilterForScheme(scheme);
			colourFactory.SetLastUsedSchemeForCurrentUser(FilterBusinessObject, scheme);

			foreach (MenuItem menu in GridColourSelectMenuItem.MenuItems)
			{
				menu.Checked = false;
			}

			selectedMenu.Checked = true;

			ResetGridColours();
		}

		public void SetActiveGridColour(GridColourScheme colourScheme)
		{
			if (colourScheme == null)
			{
				if (GridColourSelectMenuItem.MenuItems.Count > 0)
				{
					GridColourSelectMenuItem.MenuItems[0].PerformClick();
				}
			}
			else
			{
				foreach (ZMenuItem menu in GridColourSelectMenuItem.MenuItems)
				{
					if (menu.Tag is GridColourScheme menuItemScheme && menuItemScheme.PK == colourScheme.PK)
					{
						menu.PerformClick();
						break;
					}
				}
			}
		}

#if DEBUG
		internal
#endif
 protected MenuItem GridColoursParentMenuItem;

		protected MenuItem GridColourNewMenuItem;

#if DEBUG
		internal
#endif
 protected MenuItem GridColourSelectMenuItem;

#if DEBUG
		internal
#endif
 protected MenuItem GridColourManageMenuItem;

		protected void ResetGridColours()
		{
			grid.ColorByPK.Clear();
			grid.CurrentLoadColorsVersion =  Guid.NewGuid();
			grid.Invalidate();
		}

		readonly ZGrid grid;

		public FilterStripBusinessObject FilterBusinessObject
		{
			get
			{
				return filterBusinessObject ?? (filterBusinessObject = GetFilterBusinessObjectForGrid());
			}
			set
			{
				filterBusinessObject = value;
			}
		}
		FilterStripBusinessObject filterBusinessObject;

		public void Dispose()
		{
			filterBusinessObject = null;
		}
	}

	internal class LoadColorsData
	{
		internal GridColourScheme ColourScheme;
		internal SchemaGuidColumn PkColumn;
		internal Dictionary<GridColourStripBusinessObject, QueryAttrs> Queries = new Dictionary<GridColourStripBusinessObject, QueryAttrs>();
		internal Dictionary<ZGuid, (int, object)> RowsThatNeedsColour = new Dictionary<ZGuid, (int, object)>();
		internal Dictionary<string, List<ZGuid>> ColourResult = new Dictionary<string, List<ZGuid>>();
		internal Guid VersionOfLoad;

		internal bool IsValid
		{
			get
			{
				return Queries.Count > 0 && RowsThatNeedsColour.Count > 0;
			}
		}

		internal class QueryAttrs
		{
			internal QueryAttrs(Type elementType, ZQuery query, bool parentFilterIsForceSeek, bool useHint,
				TableIndexHint hint)
			{
				ElemenType = elementType;
				Query = query;
				ParentFilterIsForceSeek = parentFilterIsForceSeek;
				UseHint = useHint;
				Hint = hint;
			}

			internal Type ElemenType;
			internal ZQuery Query;
			internal bool ParentFilterIsForceSeek;
			internal bool UseHint;
			internal TableIndexHint Hint;
		}
	}

	[Serializable]
	public class LoadColourException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message should not be localized")]
		internal LoadColourException(Guid versionOfLoad, Exception innerException) : base("Load Colour Failed", innerException)
		{
			VersionOfLoad = versionOfLoad;
		}

#if NETFRAMEWORK
		protected LoadColourException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		internal Guid VersionOfLoad;
	}
}
