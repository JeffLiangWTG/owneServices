using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class StmALogCollectionView : BusinessObjectCollectionView<StmALog>
	{
		internal static string ThisPrefix(string name, bool useResString = false)
		{
			if (useResString)
			{
				return Res.GetString("82e43204-fa63-47fa-be76-2edfdbfbb163", "This {0}", name);
			}
			else
			{
				return string.Format((NoResString)"This {0}", name);
			}
		}

		public StmALogCollectionView(IStmALogParent parent)
			: base(parent.Logs.AllElements)
		{
			this.Parent = parent;
			SetReadOnlyIncludingChildren(true);
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// don't rebuild yet as we have not set the Parent
		}

		public readonly IStmALogParent Parent;

		#region Logs to Show

		public LogsToShow LogsToShow
		{
			get { return logsToShow; }
			set
			{
				if (logsToShow != value)
				{
					logsToShow = value;
					Rebuild();
				}
			}
		}
		LogsToShow logsToShow;

		internal bool IncludeCancelled
		{
			get { return includeCancelled; }
			set
			{
				includeCancelled = value;
				Rebuild();
			}
		}
		bool includeCancelled;

		internal bool IncludeEstimates
		{
			get { return includeEstimates; }
			set
			{
				includeEstimates = value;
				Rebuild();
			}
		}
		bool includeEstimates;

		#endregion

		#region Rebuilding the View

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = true;

			StmALog log = (StmALog)element;

			if (!IncludeCancelled && log.SL_IsCancelled)
			{
				result = false;
			}
			else if (!IncludeEstimates && log.SL_IsEstimate)
			{
				result = false;
			}
			else if (IncludeChangeLogsOnly && !IsChangeLog(log))
			{
				result = false;
			}
			else if (IncludeOperationsOnly && !IsOperationsLog(log))
			{
				result = false;
			}
			else if (!IsLogChildOfBizObjToShowEventsFor(log))
			{
				result = false;
			}
			else
			{
				result = true;
			}

			return result;
		}

		bool IncludeChangeLogsOnly
		{
			get { return (LogsToShow & LogsToShow.All) == LogsToShow.ChangeLogs; }
		}

		bool IncludeOperationsOnly
		{
			get { return (LogsToShow & LogsToShow.All) == LogsToShow.Operations; }
		}

		bool IsChangeLog(StmALog log)
		{
			return Events.ChangeLogs.Contains(Events.All[log.SL_SE_NKEvent]);
		}

		bool IsOperationsLog(StmALog log)
		{
			return !IsChangeLog(log);
		}

		bool IsLogChildOfBizObjToShowEventsFor(StmALog log)
		{
			string showEventsFor = BizObjNameToShowEventsFor_List.GetDescriptionFromCode(BizObjNameToShowEventsFor);

			if (!string.IsNullOrEmpty(showEventsFor))
			{
				if (showEventsFor == BizObjNameToShowAllCurrentLogsToShow)
				{
					return true;
				}

				if (log.SL_Parent.ToStringKey() == showEventsFor)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region BizObjNameToShowEventsFor

		public ZString BizObjNameToShowEventsFor
		{
			get { return fBizObjNameToShowEventsFor.IsEmpty ? (ZString)BizObjNameToShowAllCurrentLogsToShow : fBizObjNameToShowEventsFor; }
			set
			{
				if (BizObjNameToShowEventsFor != value)
				{
					fBizObjNameToShowEventsFor = value;
					Rebuild();
				}
			}
		}
		ZString fBizObjNameToShowEventsFor;

		internal CodeDescriptionPairList BizObjNameToShowEventsFor_List
		{
			get
			{
				return BizObjNameToShowEventsFor_ListCachedProperty.Value;
			}
		}

		public static CodeDescriptionPairList GetBizObjNameToShowEventsFor_List(IStmALogParent master, MultilingualString logsToShow)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.Add(new CodeDescriptionPair(logsToShow, logsToShow));
			result.Add(new CodeDescriptionPair(StmALogCollectionView.ThisPrefix(master.HumanReadableName), master.LogsParentPK.ToStringKey()));

			if (master.BusinessObjectsWithRelatedEvents != null)
			{
				Dictionary<string, int> duplicateCodeCountDict = new Dictionary<string, int>();

				foreach (BusinessObject bizO in master.BusinessObjectsWithRelatedEvents)
				{
					if (bizO != null)
					{
						string code = bizO.HumanReadableName;
						string description = bizO.PK.ToStringKey();

						// If we have more than one of the same code, start numbering them by appending [n] to distinguish them.
						if (!duplicateCodeCountDict.ContainsKey(code))
						{
							duplicateCodeCountDict[code] = 0;
						}
						duplicateCodeCountDict[code]++;
						if (duplicateCodeCountDict[code] > 1)
						{
							code = string.Format("{0} [{1}]", code, duplicateCodeCountDict[code]);
						}

						result.Add(new CodeDescriptionPair(code, description));
					}
				}
			}

			return result;
		}

		CachedProperty<CodeDescriptionPairList> BizObjNameToShowEventsFor_ListCachedProperty
		{
			get
			{
				if (bizObjNameToShowEventsFor_ListCachedProperty == null)
				{
					bizObjNameToShowEventsFor_ListCachedProperty = new CachedProperty<CodeDescriptionPairList>(Factory, delegate
					{
						return GetBizObjNameToShowEventsFor_List(Parent, BizObjNameToShowAllCurrentLogsToShow);
					});
				}
				return bizObjNameToShowEventsFor_ListCachedProperty;
			}
		}
		CachedProperty<CodeDescriptionPairList> bizObjNameToShowEventsFor_ListCachedProperty;

		public static MultilingualString BizObjNameToShowAllCurrentLogsToShow = ResString.GetMultilingualString("BizObjNameToShow|All", "All");

		#endregion
	}
}
