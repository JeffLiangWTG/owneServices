using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.BufferManagement.Business
{
	public static class BMExtensionMethods
	{
		#region IWorkflow

		public static void OnComponentChanged<TWorkflow>(this TWorkflow workflow, ZGuid oldValue, ComponentChangeMode componentChangeMode, BufferStatus bufferStatusAtTimeOfTransfer = null, IBMComponentLink link = null, string reason = "")
			where TWorkflow : EnterpriseBusinessObject, IWorkflow
		{
			workflow.LastTransferDateUtc = ZDateTime.UtcNow.ToDateTime();

			if (!oldValue.IsEmpty)
			{
				workflow.LastTransferType = componentChangeMode.ToCode();
				ProcessHeader.AddChangedComponentLog(workflow, oldValue, componentChangeMode, bufferStatusAtTimeOfTransfer, link, reason);
			}
		}

		public static BMComponent GetCurrentComponent(this IWorkflow workflow, BusinessObjectFactory factory)
		{
			return factory.Load<BMComponent>(workflow.CurrentComponentPK);
		}

		public static bool AreDataVersionsLogged(this IWorkflow workflow)
		{
			return BMSRegistryProvider.IsBufferManagementEnabled && !((BusinessObject)workflow).IsDeleted;
		}

		#endregion

		#region ProcessTask

		public static bool IsCompletionTask(this IProcessTask task)
		{
			return ((ProcessTask)task).IsCompletionStatement;
		}

		#endregion

		#region ProcessHeader

		public static ViewProcessHeader GetView(this IProcessHeader workflow)
		{
			return workflow.IsInDatabase
				? workflow.Factory.LoadFromPrimaryKeysAndTableCode(new[] { workflow.PK }, new[] { workflow.FH_ParentTableCode }).Single()
				: null;
		}

		#endregion

		#region Component

		/// <summary>
		/// Throws an InvalidOperationException if the specified component is not a buffer (BUF) type.
		/// </summary>
		public static void RequireBuffer(this BMComponent component)
		{
			if (!component.IsBuffer)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Component must be a buffer - it is a {0}.", component.TypeDescription));
			}
		}

		public static int GetZone(this BMComponent buffer, IBufferedItem buffered, WorkingTimeContext context = null)
		{
			buffer.RequireBuffer();

			return ZoneCalculator.CalculateZone(buffered, context ?? WorkingTimeContext.Create(buffer), buffer.Factory);
		}

		public static BufferPenetrationResult GetSubComponentBufferPenetration(this ProcessHeader header, ConstraintStatus constraintStatusPosition)
		{
			var currentComponent = header.CurrentComponent;
			var subBuffer = currentComponent.ChildComponents.FirstOrDefault(c => c.IsBuffer && ConstrainedModeHelper.GetConstraintStatus(c) == constraintStatusPosition);

			return subBuffer != null ? BufferPenetrationCalculator.CalculatePenetrationPercentage(header, subBuffer, subBuffer.GetRelevantContext(), header.Factory) : null;
		}

		#endregion

		#region ZDateTime

		public static ZDateTime ToLocalBranchTime(this ZDateTime date, GlbBranch branch)
		{
			if (date.IsValid)
			{
				var timeZoneSet = branch?.HomePort?.TimeZoneSet;
				if (timeZoneSet != null)
				{
					var calculationTimeZone = timeZoneSet.GetCalculationTimeZone();
					return calculationTimeZone.ToLocalTime(date.ToDateTime());
				}
				else
				{
					return date.ToLocalBranchTime();
				}
			}
			else
			{
				return date;
			}
		}

		public static ZDateTime ToUniversalBranchTime(this ZDateTime date, GlbBranch branch)
		{
			if (date.IsValid)
			{
				if (branch == null)
				{
					return date.ToUniversalBranchTime();
				}

				var currentBranchZoneSet = branch.HomePort.TimeZoneSet;
				if (currentBranchZoneSet != null)
				{
					var calculationTimeZone = currentBranchZoneSet.GetCalculationTimeZone();
					return calculationTimeZone.ToUniversalTime(date.ToDateTime());
				}
			}

			return date;
		}

		public static string ToFriendlyTimeOverAgoString(this ZDateTime utcDateTimeInThePast)
		{
			if (utcDateTimeInThePast.IsValid)
			{
				var now = ZDateTime.UtcNow;
				var difference = now - utcDateTimeInThePast;

				return difference.ToFriendlyTimeOverAgoString();
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static string ToFriendlyTimeOverAgoString(this TimeSpan span)
		{
			var friendlyTimeString = span.ToFriendlyTimeString();

			if (span.TotalHours >= 1)
			{
				return Res.GetString("909b0bca-41c8-4b5d-9134-e555cd7c5a07", "over {0} ago", friendlyTimeString);
			}
			else if (span.Minutes >= 1)
			{
				return Res.GetString("7b2b0dca-98f9-4632-9075-e10a44270786", "{0} ago", friendlyTimeString);
			}
			else if (span.Minutes > -1)
			{
				return Res.GetString("f77fd3ae-6d85-4dcf-9f36-3bffbf49e74c", "just now");
			}
			else
			{
				return string.Empty;
			}
		}

		public static ZDateTime WithinSmallDateTimeRange(this ZDateTime value)
		{
			if (value < ZDateTime.MinSmallDateTimeValue)
			{
				return ZDateTime.MinSmallDateTimeValue;
			}
			else if (value > ZDateTime.MaxSmallDateTimeValue)
			{
				return ZDateTime.MaxSmallDateTimeValue;
			}
			else
			{
				return value;
			}
		}

		#endregion

		#region Color

		public static Color GetBestTextColorForBackground(this Color backgroundColor)
		{
			var average = new int[] { backgroundColor.R, backgroundColor.G, backgroundColor.B }.Average();
			return average > 230 / 2 ? Color.Black : Color.White;
		}

		public static Color FadeTowardsWhite(this Color color, float fadeFactor = 2f)
		{
			var r = FadeColorComponentTowardsWhite(color.R, fadeFactor);
			var g = FadeColorComponentTowardsWhite(color.G, fadeFactor);
			var b = FadeColorComponentTowardsWhite(color.B, fadeFactor);

			return Color.FromArgb(color.A, r, g, b);
		}

		static int FadeColorComponentTowardsWhite(int colorComponent, float fadeFactor)
		{
			return (int)(colorComponent + ((255 - colorComponent) / fadeFactor));
		}

		public static Color FadeTowardsBlack(this Color color, float fadeFactor = 2f)
		{
			var r = FadeColorComponentTowardsBlack(color.R, fadeFactor);
			var g = FadeColorComponentTowardsBlack(color.G, fadeFactor);
			var b = FadeColorComponentTowardsBlack(color.B, fadeFactor);

			return Color.FromArgb(color.A, r, g, b);
		}

		static int FadeColorComponentTowardsBlack(int colorComponent, float fadeFactor)
		{
			return (int)(colorComponent - (colorComponent / fadeFactor));
		}

		public static Color GetZoneColor(this BMBoardSection section, int? zone, bool useDefaults = true)
		{
			switch (zone)
			{
				case 0:
					return section.SectionConfiguration.BufferZone0ColorValue ?? (useDefaults ? BMConstants.Zone0DefaultColor : Color.Empty);
				case 1:
					return section.SectionConfiguration.BufferZone1ColorValue ?? (useDefaults ? BMConstants.Zone1DefaultColor : Color.Empty);
				case 2:
					return section.SectionConfiguration.BufferZone2ColorValue ?? (useDefaults ? BMConstants.Zone2DefaultColor : Color.Empty);
				case 3:
					return section.SectionConfiguration.BufferZone3ColorValue ?? (useDefaults ? BMConstants.Zone3DefaultColor : Color.Empty);
				default:
					return !section.BackgroundColor.IsEmpty ? section.BackgroundColorValue : SystemColors.Control;
			}
		}

		public static string ToHex(this Color color)
		{
			return string.Format(CultureInfo.InvariantCulture, "#{0:x6}", color.ToArgb() & 0x00FFFFFF).ToUpper(CultureInfo.InvariantCulture); // Constant string
		}

		#endregion

		#region BusinessObjectFactory

		public static bool SaveHandlingZSaveExceptions(this BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();
				return true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				return false;
			}
		}

		public static IDisposable TemporarilyDisableValidation(this BusinessObjectFactory factory)
		{
			var wasValidationSuspended = factory.IsValidationSuspended;

			if (!wasValidationSuspended)
			{
				factory.SuspendValidation();
			}

			return new DisposableAction(() =>
			{
				if (!wasValidationSuspended)
				{
					factory.ResumeValidation();
				}
			});
		}

		public static bool IsForServiceTask(this BusinessObjectFactory factory, string serviceTaskCode)
		{
			var service = factory.ServiceContainer.GetService<ServiceTaskCodeService>();

			return service != null && service.ServiceTaskCode == serviceTaskCode;
		}

		public static bool IsNotForAnyServiceTask(this BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<ServiceTaskCodeService>() == null;
		}

		public static void ReloadBusinessObjects<T>(this BusinessObjectFactory factory, ITableSchema tableSchema, params T[] bizos)
			where T : BusinessObject
		{
			if (bizos.Length > 0)
			{
				var query = new ZQuery(tableSchema.PK, bizos.Select(b => b.PK).ToArray()) { ReLoadExistingRows = true };
				factory.Load<T>(query);
			}
		}

		public static ViewProcessHeader[] LoadFromPrimaryKeysAndTableCode(this BusinessObjectFactory factory, ICollection<ZGuid> pks, ICollection<ZString> parentTableCodes)
		{
			if (pks.Count == 0)
			{
				throw new ArgumentException("No PKs were supplied");
			}

			if (parentTableCodes.Count == 0)
			{
				throw new ArgumentException("No table codes were supplied");
			}

			var query = new ZQuery(ViewProcessHeaderSchema.PK, pks);
			query.AddToFilter(ViewProcessHeaderSchema.VFH_ParentTableCode, parentTableCodes);

			return factory.Load<ViewProcessHeader>(query);
		}

		#endregion

		#region IEnumerable<IBusiness>

		public static T[] WithFetchHints<T>(this IEnumerable<T> values, params Func<T, IFetchHint>[] addFetchHints)
		where T : IBusiness
		{
			var valuesArray = values.ToArray();
			foreach (var value in valuesArray)
			{
				foreach (var hintAdder in addFetchHints)
				{
					value.Factory.AddFetchHint(hintAdder(value));
				}
			}

			return valuesArray;
		}

		#endregion

		#region IEnumerable<T>

		public static IEnumerable<T> WrapWithEnumerable<T>(this T item)
		{
			yield return item;
		}

		public static IEnumerable<TItem> FindDuplicates<TKey, TItem>(this IEnumerable<TItem> items, Func<TItem, TKey> getKey, Func<TItem, TItem, bool> isItem1TheDuplicate)
		{
			var dictionary = new Dictionary<TKey, TItem>();
			foreach (var item in items)
			{
				var key = getKey(item);
				TItem otherItem;
				if (dictionary.TryGetValue(key, out otherItem))
				{
					if (isItem1TheDuplicate(item, otherItem))
					{
						yield return item;
					}
					else
					{
						yield return otherItem;
						dictionary[key] = item;
					}
				}
				else
				{
					dictionary[key] = item;
				}
			}
		}

		public static IEnumerable<TResult> ZipWithNull<TElement, TResult>(this IEnumerable<TElement> collection1, IEnumerable<TElement> collection2, Func<TElement, TElement, TResult> selector)
			where TElement : class
		{
			var enumerator1 = collection1.GetEnumerator();
			var enumerator2 = collection2.GetEnumerator();

			var enum1IsValid = true;
			var enum2IsValid = true;

			while (enum1IsValid || enum2IsValid)
			{
				enum1IsValid = enumerator1.MoveNext();
				enum2IsValid = enumerator2.MoveNext();

				if (enum1IsValid && enum2IsValid)
				{
					yield return selector(enumerator1.Current, enumerator2.Current);
				}
				else if (enum1IsValid)
				{
					yield return selector(enumerator1.Current, null);
				}
				else if (enum2IsValid)
				{
					yield return selector(null, enumerator2.Current);
				}
			}
		}

		public delegate IEnumerable<T> RecursiveSelector<T>(T currentEntity);

		public delegate bool DepthFilter<T>(T currentEntity, IEnumerable<T> previouEntities);

		/// <summary>
		/// Fast tree traversal for identifying consecutive chains.
		/// </summary>
		public static IEnumerable<T[]> FilteredTraversalWithDepth<T>(this T root, RecursiveSelector<T> selector, DepthFilter<T> filter, DepthFilter<T> terminateBranchFilter, bool fullChainsOnly = false)
		{
			var previous = new Stack<T>();
			var items = new Stack<ItemWithDepth<T>>();
			items.Push(new ItemWithDepth<T> { Item = root, Depth = 0 });

			while (items.Count > 0)
			{
				var item = items.Pop();

				while (item.Depth < previous.Count)
				{
					previous.Pop();
				}

				var matchesTerminateBranch = terminateBranchFilter(item.Item, previous);

				previous.Push(item.Item);

				var itemCount = items.Count;
				if (!matchesTerminateBranch)
				{
					foreach (var nextItem in CreateDepth(item.Item, selector, previous.Count))
					{
						items.Push(nextItem);
					}
				}

				if (itemCount == items.Count || !fullChainsOnly)
				{
					var matchesFilter = filter(item.Item, previous);
					if (matchesFilter)
					{
						yield return previous.Reverse().ToArray();
					}
				}
			}
		}

		static IEnumerable<ItemWithDepth<T>> CreateDepth<T>(T item, RecursiveSelector<T> selector, int depth)
		{
			return selector(item).Select(i => new ItemWithDepth<T> { Item = i, Depth = depth });
		}

		struct ItemWithDepth<T>
		{
			internal T Item;
			internal int Depth;
		}

		#endregion

		#region ICollection<T>

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				collection.Add(item);
			}
		}

		#endregion

		#region List<T>

		internal static void RemoveAfterIndex<T>(this List<T> list, int index)
		{
			var numberToRemove = list.Count - index - 1;
			if (numberToRemove > 0)
			{
				list.RemoveRange(index + 1, numberToRemove);
			}
			else if (numberToRemove < 0)
			{
				throw new InvalidOperationException("Index specified is greater than list length");
			}
		}

		#endregion

		#region bool?

		public static bool IsTrue(this bool? nullable)
		{
			return nullable.HasValue && nullable.Value;
		}

		public static bool IsFalse(this bool? nullable)
		{
			return nullable.HasValue && !nullable.Value;
		}

		#endregion

		#region IBusiness

		public static bool HasErrors(this IBusiness bizo)
		{
			return ((BusinessObject)bizo).HasErrors;
		}

		public static void SetCopiedBizoNamePropertyComplyingWithMaxLength(ZPropertyInfo nameProperty)
		{
			var cloneSuffix = " " + Res.GetString("d0339665-7748-4299-964e-377390bdfb6d", "Copy");
			var newNameValue = ((ZString)nameProperty.Value).SubstringSafe(0, nameProperty.MaxLength - cloneSuffix.Length) + cloneSuffix;

			nameProperty.Value = (ZString)newNameValue;
		}

		#endregion

		#region String

		public static string AppendNextBracketedNumber(this string s, int maxLength = -1)
		{
			var match = MatchBrackets(s);

			return PreparePrefixAndSuffix(maxLength, match.Prefix, match.Num);
		}

		static string PreparePrefixAndSuffix(int maxLength, string prefix, int num)
		{
			var suffix = string.Format(CultureInfo.InvariantCulture, " [{0}]", num + 1);

			if (maxLength >= 0 && prefix.Length + suffix.Length > maxLength)
			{
				prefix = prefix.Substring(0, maxLength - suffix.Length);
			}

			return prefix + suffix;
		}

		public static string AppendNextHighestBracketedNumber(this string s, IEnumerable<string> existingValues, int maxLength = -1)
		{
			var primaryMatch = MatchBrackets(s);

			var greatestMatch = existingValues.Select(MatchBrackets).Where(m => m.Prefix == primaryMatch.Prefix).MaxBySafe(m => m.Num);

			return PreparePrefixAndSuffix(maxLength, primaryMatch.Prefix, greatestMatch != null ? Math.Max(greatestMatch.Num, primaryMatch.Num) : primaryMatch.Num);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		internal static MatchAndNumber MatchBrackets(string s)
		{
			var regex = new Regex(@" \[([0-9]+)\]$");
			var match = regex.Match(s);

			string prefix;
			int num;

			if (match.Success)
			{
				prefix = s.Substring(0, match.Index);
				num = int.Parse(match.Groups[1].Value);
			}
			else
			{
				prefix = s;
				num = 0;
			}

			return new MatchAndNumber
			{
				Prefix = prefix,
				Num = num
			};
		}

		internal class MatchAndNumber
		{
			public string Prefix { get; set; }
			public int Num { get; set; }
		}

		#endregion

		#region IVisualBoardQuery

		public static void AddGroup(this IVisualBoardQuery query, ZGuid groupPk, IEnumerable<GlbStaff> resources, IEnumerable<ZGuid> capabilityPks)
		{
			query.Groups.Add(groupPk);
			resources.ForEach(resource => query.Staffs.Add(new StaffCode(resource.PK, resource.GS_Code)));
			capabilityPks.ForEach(pk => query.Capabilities.Add(pk));
		}

		public static void AddStaff(this IVisualBoardQuery query, IStaffCode resource, IEnumerable<ICapabilityScope> capabilities)
		{
			query.Staffs.Add(resource);
			capabilities.ForEach(capability => query.StaffCapabilities.Add(capability));
		}

		public static void AddCapability(this IVisualBoardQuery query, ZGuid capabilityPK)
		{
			query.Capabilities.Add(capabilityPK);
		}

		public static void AddTagMagnitude(this IVisualBoardQuery query, TagMagnitude magnitude)
		{
			AddTagMagnitude(query, magnitude.PK);
		}

		public static void AddTagMagnitude(this IVisualBoardQuery query, ZGuid magnitudePK)
		{
			query.TagMagnitudes.Add(magnitudePK);
		}

		public static void AddBMSHolidayFetchHint(this GlbStaff staff)
		{
			var holidayQuery = new ZQuery(GlbStaffHolidaySchema.GA_GS, staff.PK);
			holidayQuery.AddToFilter(GlbStaffHolidaySchema.GA_RecordType, new[] { GlbStaffHolidayLookups.RecordTypes.Leave, StaffHolidayRecordTypeCodes.BufferManagementLeave });
			staff.Factory.AddFetchHint(typeof(GlbStaffHoliday), holidayQuery);
			staff.Factory.AddFetchHint(GlbWorkTimeSchema.GW_ParentID, staff.PK);
		}

		static IEnumerable<ZDBOnlySubQuery> GetTaskQueries(this IVisualBoardQuery queryBuilder, SchemaColumn schemaColumn)
		{
			if (queryBuilder.Staffs.Any())
			{
				var query = new ZDBOnlySubQuery(typeof(ProcessTasks), schemaColumn);
				query.AllowTableValuedParameters = true;
				query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, queryBuilder.Staffs.Select(staff => staff.Code));
				yield return query;
			}

			if (queryBuilder.Capabilities.Any())
			{
				var query = new ZDBOnlySubQuery(typeof(ProcessTasks), schemaColumn);
				query.AllowTableValuedParameters = true;
				query.AddToFilter(ProcessTasksSchema.P9_G4_RequiredCapability, queryBuilder.Capabilities);
				yield return query;
			}

			if (queryBuilder.StaffCapabilities.Any())
			{
				var staffCapabilities = queryBuilder.StaffCapabilities.Where(sc => !queryBuilder.Capabilities.Contains(sc.PK)).ToArray();
				var glbCapabilities = staffCapabilities.Where(capability => capability.Scope == GlbCapabilityScopeList.Codes.GlobalScope);
				var grpCapabilities = staffCapabilities.Where(capability => capability.Scope == GlbCapabilityScopeList.Codes.GroupScope);

				if (glbCapabilities.Any())
				{
					var query = CreateStaffCapabilityQuery(glbCapabilities.Select(g => g.PK), schemaColumn);
					yield return query;
				}

				if (grpCapabilities.Any())
				{
					var groupLinkSubQuery = CreateStaffInGroupLinkQuery(queryBuilder.Staffs.Select(staff => staff.PK));
					var workflowReleaseGroupSubQuery = CreateCapabilityGroupInWorkflowReleaseGroupQuery(groupLinkSubQuery);
					var taskGroupSubQuery = CreateCapabilityGroupInTaskGroupQuery(groupLinkSubQuery);

					var query = CreateStaffCapabilityQuery(grpCapabilities.Select(g => g.PK), schemaColumn);
					workflowReleaseGroupSubQuery.AddAsUnionQuery(taskGroupSubQuery, addAsUnionAll: true);
					query.AddSubQuery(workflowReleaseGroupSubQuery, JoinCondition.And);
					yield return query;
				}
			}

			if (queryBuilder.Groups.Count > 0)
			{
				var query = new ZDBOnlySubQuery(typeof(ProcessTasks), schemaColumn);
				query.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, queryBuilder.Groups);
				yield return query;
			}

			if (queryBuilder.TagMagnitudes.Count > 0)
			{
				var taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), schemaColumn);

				var tasksWithTagLinks = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
				tasksWithTagLinks.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, queryBuilder.TagMagnitudes);

				taskQuery.AddSubQuery(ProcessTasksSchema.PK, tasksWithTagLinks, JoinCondition.And);
				yield return taskQuery;

				var processHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessTask), schemaColumn);

				var processHeadersWithTagLinks = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
				processHeadersWithTagLinks.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, queryBuilder.TagMagnitudes);

				processHeaderQuery.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, processHeadersWithTagLinks, JoinCondition.And);
				yield return processHeaderQuery;

				var processJobHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessTask), schemaColumn);

				var processJobHeadersWithTagLinks = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				var pks = string.Join(", ", queryBuilder.TagMagnitudes.Select(s => "'" + s.ToString() + "'"));
				var sql = string.Format("FH_PK in (SELECT FH_PK from dbo.TagLink inner join dbo.ProcessHeader on TGL_ParentId = FH_FH_ParentHeader WHERE TGL_TGM_Magnitude in ({0}))", pks); // This is sql to load things

				processJobHeadersWithTagLinks.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());

				processJobHeaderQuery.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, processJobHeadersWithTagLinks, JoinCondition.And);
				yield return processJobHeaderQuery;
			}
		}

		static ZDBOnlySubQuery CreateStaffCapabilityQuery(IEnumerable<ZGuid> capabilitiesPKs, SchemaColumn schemaColumn)
		{
			var query = new ZDBOnlySubQuery(typeof(ProcessTasks), schemaColumn);
			query.AllowTableValuedParameters = true;
			query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, string.Empty);
			query.AddToFilter(ProcessTasksSchema.P9_G4_RequiredCapability, capabilitiesPKs);
			return query;
		}

		static ZDBOnlySubQuery CreateStaffInGroupLinkQuery(IEnumerable<ZGuid> staffPKs)
		{
			var groupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
			groupLinkSubQuery.AllowTableValuedParameters = true;
			groupLinkSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, staffPKs);

			return groupLinkSubQuery;
		}

		static ZDBOnlySubQuery CreateCapabilityGroupInWorkflowReleaseGroupQuery(ZDBOnlySubQuery groupLinkSubQuery)
		{
			var workflowReleaseGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.PK);
			workflowReleaseGroupSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, null);

			var processHeaderReleaseGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			processHeaderReleaseGroupSubQuery.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, null);

			var processHeaderWithReleaseGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			processHeaderWithReleaseGroupSubQuery.AddSubQuery(ProcessHeaderSchema.FH_GG_ReleaseGroup, groupLinkSubQuery, JoinCondition.And);

			processHeaderReleaseGroupSubQuery.AddAsUnionQuery(processHeaderWithReleaseGroupSubQuery, addAsUnionAll: true);
			workflowReleaseGroupSubQuery.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, processHeaderReleaseGroupSubQuery, JoinCondition.And);

			return workflowReleaseGroupSubQuery;
		}

		static ZDBOnlySubQuery CreateCapabilityGroupInTaskGroupQuery(ZDBOnlySubQuery groupLinkSubQuery)
		{
			var taskGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.PK);
			taskGroupSubQuery.AddSubQuery(ProcessTasksSchema.P9_GG_AssignedGroup, groupLinkSubQuery, JoinCondition.And);

			return taskGroupSubQuery;
		}

		public static ZDBOnlySubQuery CreateTaskQuery(this IVisualBoardQuery queryBuilder, ZQuery sharedFilter, SchemaColumn schemaColumn)
		{
			return queryBuilder.GetTaskQueries(schemaColumn).Aggregate<ZDBOnlySubQuery, ZDBOnlySubQuery>(null, (q1, q2) => CombineSubQueries(q1, q2, sharedFilter));
		}

		static ZDBOnlySubQuery CombineSubQueries(ZDBOnlySubQuery query1, ZDBOnlySubQuery query2, ZQuery sharedFilter)
		{
			query2.AddToFilter(sharedFilter);

			if (query1 == null)
			{
				return query2;
			}
			else if (query2 != null)
			{
				query1.AddAsUnionQuery(query2, addAsUnionAll: true);
			}

			return query1;
		}

		#endregion

		#region IWorkflowProvider

		public static ZString GetWorkflowType(this IWorkflowProvider workflowProvider)
		{
			var template = workflowProvider as ProcessTaskTemplate;
			return template != null ? template.P0_ProcessType : workflowProvider.WorkflowType;
		}

		#endregion

		#region IBufferedItem

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public static int CalculatePenetrationMinutesWithoutAging(this IBufferedItem item)
		{
			return item.RemainingEstimateInMinutes - item.PlannedDurationInMinutes;
		}

		#endregion

		#region ICustomisedLayoutSupportable

		public static void CloneCustomisedLayoutLayoutLinks(this ICustomisedLayoutSupportable source, ICustomisedLayoutSupportable clone)
		{
			foreach (BusinessObject link in source.CustomisedLayoutLinks)
			{
				var cloneLink = (BMControlCustomisationLink)link.Clone(new BusinessObjectCloneArgs(new[] { BMControlCustomisationLinkSchema.Constants.FML_ParentId }));
				cloneLink.FML_ParentId = clone.Identifier;
			}
		}

		public static ZDateTime GetLatestCustomisationEditTimeUTC(this ICustomisedLayoutSupportable layoutSupportable)
		{
			var links = layoutSupportable.CustomisedLayoutLinks.Cast<BMControlCustomisationLink>().ToArray();

			if (!layoutSupportable.AreCustomisedLayoutFetchHintsAdded)
			{
				AddBMControlCustomisationFetchHintsAndMarkLayoutSupportable(links, layoutSupportable);
			}

			var latestEditedLayout = (
				from BMControlCustomisationLink link in links
				let layout = link.CustomisedLayout
				where layout != null
				select layout
				).MaxBySafe(l => l.FM_SystemLastEditTimeUtc);

			return latestEditedLayout != null ? latestEditedLayout.FM_SystemLastEditTimeUtc : ZDateTime.Empty;
		}

		public static void AddBMControlCustomisationFetchHints(this ICustomisedLayoutSupportable layoutSupportable)
		{
			var links = layoutSupportable.CustomisedLayoutLinks.Cast<BMControlCustomisationLink>();
			AddBMControlCustomisationFetchHintsAndMarkLayoutSupportable(links, layoutSupportable);
		}

		static void AddBMControlCustomisationFetchHintsAndMarkLayoutSupportable(IEnumerable<BMControlCustomisationLink> links, ICustomisedLayoutSupportable layoutSupportable)
		{
			BMControlCustomisationLink.AddBMControlCustomisationFetchHints(links, layoutSupportable.Factory);
			layoutSupportable.AreCustomisedLayoutFetchHintsAdded = true;
		}

		#endregion

		#region IDisposable

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "Ensures exceptions don't prevent Dispose being called")]
		public static void DisposeOnException(this IDisposable disposable, Action actionThatMightThrowException)
		{
			try
			{
				actionThatMightThrowException();
			}
			catch
			{
				try
				{
					disposable.Dispose();
				}
				catch
				{
					// Don't hide the original exception
				}

				throw;
			}
		}

		#endregion

		#region PassDirection

		internal static NotSupportedException CreateNotSupportedException(this PassDirection direction)
		{
			return new NotSupportedException(string.Format(Culture.Invariant, "The PassDirection [{0}] was not supported by this operation.", direction));
		}

		#endregion

		#region Logs

		public static StmALog CreateLogWithReachAround_DoesntTriggerWorkflow(BusinessObjectFactory factory, ZString tableName, ZGuid parentPK, ZString staffCode, ZString eventCode, ZBool isEstimate, ZString reference)
		{
			var log = factory.New<StmALog>();

			using (((IUpdateFieldsLockReachAround)log).LockForUpdatingKeyFields(shouldNotifyOnRelease: false))
			{
				log.SL_Table = tableName;
				log.SL_Parent = parentPK;
				log.SL_GS_NKUser = staffCode;
				log.SL_SE_NKEvent = eventCode;
				log.SL_IsEstimate = isEstimate;
				log.SL_Reference = reference;
			}

			return log;
		}

		#endregion

		#region Log Information

		public static string GetLogInfo(this BMBoard board)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}({1})", board.HumanReadableName, board.PK);
		}

		public static string GetLogInfo(this BoardViewModel boardViewModel, BusinessObjectFactory factory)
		{
			var board = factory.Load<BMBoard>(boardViewModel.BoardPK);

			if (board != null)
			{
				return board.GetLogInfo();
			}
			else
			{
				return string.Format(CultureInfo.CurrentCulture, "{0}({1})", boardViewModel.BoardName, boardViewModel.BoardPK);
			}
		}

		#endregion

		#region Shapes

		public static bool IsDefaultDiagram(this IBMNCNShape shape)
		{
			return shape.BNS_ShapeType == ShapeTypeList.Codes.DefaultDiagram;
		}

		public static bool IsDefaultDiagramChild(this IBMNCNShape shape)
		{
			return shape.BNS_ShapeType == ShapeTypeList.Codes.DefaultWorkflow;
		}

		#endregion

		#region Toggle Notifications Without Triggering Events

		public static void ToggleRowWarningSafe(this BusinessObject bizo, bool shouldHaveWarning, string warning)
		{
			ToggleNotificationSafe(CargoWise.ComponentModel.NotificationType.Warning, bizo, shouldHaveWarning, warning);
		}

		public static void ToggleRowMessageErrorSafe(this BusinessObject bizo, bool shouldHaveError, string error)
		{
			ToggleNotificationSafe(CargoWise.EntityFramework.NotificationType.MessageError, bizo, shouldHaveError, error);
		}

		public static void ToggleRowErrorSafe(this BusinessObject bizo, bool shouldHaveError, string error)
		{
			ToggleNotificationSafe(CargoWise.ComponentModel.NotificationType.Error, bizo, shouldHaveError, error);
		}

		static void ToggleNotificationSafe(INotificationType type, BusinessObject bizo, bool shouldHaveNotification, string message)
		{
			// In order to not fire redundant Notifications Modified messages we have to check this. This pattern is only necessary when heavy events are attached to the NotificationsChanged event.
			var notifications = bizo.RowNotifications.Where(r => r.Type == type && r.Message == message).ToArray(); // This is much, much faster.
			var hasNotifications = notifications.Length > 0;

			if (hasNotifications && !shouldHaveNotification)
			{
				foreach (var notification in notifications)
				{
					bizo.RemoveRowNotification(notification);
				}
			}
			else if (shouldHaveNotification && !hasNotifications)
			{
				bizo.AddRowNotification(new Notification(type, message));
			}
		}

		public static void ToggleRowError(this BusinessObject bizo, bool shouldHaveNotification, string notificationToBeRemoved, string message)
		{
			var notifications = bizo.RowNotifications.Where(r => r.Type == CargoWise.ComponentModel.NotificationType.Error &&
					r.Message.StartsWith(notificationToBeRemoved, StringComparison.OrdinalIgnoreCase)).ToArray();

			foreach (var notification in notifications)
			{
				bizo.RemoveRowNotification(notification);
			}

			if (shouldHaveNotification)
			{
				bizo.AddRowError(message);
			}
		}

		#endregion

		public static T[] GetFlowDirectionedArray<T>(this BMBoardSection section, T[] objects, string defaultFlowdirection = FlowDirectionList.Codes.Down)
		{
			var flowDirection = section.SectionConfiguration.FlowDirection.ToString();

			return (defaultFlowdirection == FlowDirectionList.Codes.Down && flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Left))
				? objects.Reverse().ToArray()
				: objects;
		}

		#region JsonSerialize/Deserialize

		public static string JsonSerialize(this object o) => JsonConvert.SerializeObject(o);

		public static T JsonDeserialize<T>(this string o) => JsonConvert.DeserializeObject<T>(o);

		#endregion
	}
}
