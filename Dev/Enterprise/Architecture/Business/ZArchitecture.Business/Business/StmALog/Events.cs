using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public sealed class Events : AutoEvents
	{
		Events()
		{
		}

		#region System Reserved

		public static ImmutableHashSet<string> ChangeLogCodes => changeLogCodes.Value;

		static readonly Lazy<ImmutableHashSet<string>> changeLogCodes = new Lazy<ImmutableHashSet<string>>(() => ChangeLogs.Select(s => s.Code).ToImmutableHashSet());

		public static Event[] ChangeLogs
		{
			get { return changeLogs.Value; }
		}

		static readonly Lazy<Event[]> changeLogs = new Lazy<Event[]>(() =>
		{
			return new Event[]
				{
					Events.AddedARecordToTheSystem,
					Events.EditedARecord,
					Events.DeletedARecordInTheSystem,
					Events.RecallDateUpdated,
					Events.SetToActive,
					Events.SetToInactive,
					Events.StaffFlaggedAsDeviceOnly,
					Events.StaffUnFlaggedAsDeviceOnly,
					Events.UserSeat,
					Events.Login,
					Events.Logout,
				};
		});

		public static Event[] SystemReservedEvents
		{
			get { return systemReservedEvents.Value; }
		}

		static readonly Lazy<Event[]> systemReservedEvents = new Lazy<Event[]>(() =>
		{
			return new Event[]
				{
					Events.ReadRelatedNotes
				};
		});

		public static Event[] EventsThatCannotBeCancelled
		{
			get { return eventsThatCannotBeCancelled.Value; }
		}

		static readonly Lazy<Event[]> eventsThatCannotBeCancelled = new Lazy<Event[]>(() =>
		{
			var list = new List<Event>();
			list.AddRange(ChangeLogs);

			list.AddRange(new Event[]
			{
				Events.JobOpen,
				Events.JobClose,
				Events.ReadRelatedNotes,
				Events.RelatedNotesNotRead,
				Events.DocumentSent,
				Events.DocumentDelivered,
				Events.DocumentNotDelivered,
				Events.AssignedUserChanged,
				Events.OceanCarrierBookingByTEU
			});

			return list.ToArray();
		});

		#endregion

		#region Former Exception Events

		#region Exceptions

		public static ICollection<Event> Exceptions
		{
			get { return exceptions.Value; }
		}

		static readonly Lazy<ICollection<Event>> exceptions = new Lazy<ICollection<Event>>(() =>
		{
			return SelectEventsFilteredBy(new ZQuery(StmEventSchema.SE_IsExceptionEvent, true));
		});

		#endregion

		#endregion

		#region Customizables

		public static ICollection<Event> Customizables
		{
			get { return customizables.Value; }
		}

		static readonly LazyOverridable<HashSet<Event>> customizables = new LazyOverridable<HashSet<Event>>(GetCustomizableEvents);

		static HashSet<Event> GetCustomizableEvents() => SelectEventsFilteredBy(new ZQuery(StmEventSchema.SE_IsCustomizable, true));

		#endregion

		#region InactiveEvents

		public static ICollection<Event> InactiveEvents
		{
			get { return inactiveEvents.Value; }
		}

		static Lazy<ICollection<Event>> inactiveEvents = GetInactiveEvents();

		static Lazy<ICollection<Event>> GetInactiveEvents()
		{
			return new Lazy<ICollection<Event>>(() =>
			{
				return SelectEventsFilteredBy(new ZQuery(StmEventSchema.SE_IsActive, false));
			});
		}

		internal static void ResetInactiveEvents()
		{
			inactiveEvents = GetInactiveEvents();
		}

		#endregion

		#region ProductivityWise

		public static ICollection<Event> ProductivityWiseEvents
		{
			get { return productivityWiseEvents.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static readonly Lazy<ICollection<Event>> productivityWiseEvents = GetProductivityWiseEvents();

		static Lazy<ICollection<Event>> GetProductivityWiseEvents()
		{
			return new Lazy<ICollection<Event>>(() =>
			{
				var query = new ZQuery(StmEventSchema.SE_IsVisibleToPW, true);
				query.AddToFilter(StmEventSchema.SE_IsActive, true);
				return SelectEventsFilteredBy(query);
			});
		}

		#endregion

		#region MultilingualDescription

		public static MultilingualString GetMultilingualDescription(string code)
		{
			var eventType = Events.All[code];

			if (eventType != null)
			{
				if (customizables.Value.TryGetValue(eventType, out Event customEvent))
				{
					return customEvent.MultilingualDescription;
				}
				else
				{
					return eventType.MultilingualDescription;
				}
			}

			return (NoResString)ZString.Empty;
		}

		#endregion

		#region Implementation

		static HashSet<Event> SelectEventsFilteredBy(ZQuery stmEventQuery)
		{
			var result = new HashSet<Event>(new EventComparer());

			foreach (var stmEvent in new BusinessObjectFactory().Load<StmEvent>(stmEventQuery))
			{
				var eventType = Events.All[stmEvent.SE_Code];
				if (eventType != null)
				{
					if (stmEvent.SE_IsCustomizable)
					{
						result.Add((Event)stmEvent);
					}
					else
					{
						result.Add(eventType);
					}
				}
			}

			return result;
		}

		public static ZString GetEventDescriptionFromCode(string code)
		{
			return (!string.IsNullOrEmpty(code) && All.Contains(code)) ? (ZString)All[code].Description : new ZString();
		}

		class EventComparer : IEqualityComparer<Event>
		{
			public bool Equals(Event x, Event y) => x.Code.Equals(y.Code);
			public int GetHashCode(Event obj) => obj.Code.GetHashCode();
		}

		#endregion

#if DEBUG
		public static void ReloadCustomizableEventsFromDB() => customizables.Value = GetCustomizableEvents();
#endif
	}
}
