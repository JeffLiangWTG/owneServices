using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Billing.Integration
{
#if DEBUG
	public
#endif
	class FactorySnapshot
	{
		public FactorySnapshot(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "BusinessObjectFactory factory");
			InitializeObjSnapshots(factory);
		}

#if DEBUG
		internal protected virtual
#endif
		void InitializeObjSnapshots(BusinessObjectFactory factory)
		{
			foreach (var obj in ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects)
			{
				var objSnapshot = new ObjectSnapshot(obj);
				var success = TryAdd(ObjSnapshots, objSnapshot);
				if (!success)
				{
					ErrorReporter.ReportOnce("FactorySnapshot.Constructor", string.Format("Failed to add {0} into HashSet ObjSnapshots, because the element is already present.", objSnapshot));
				}
			}
		}

#if DEBUG
		internal protected virtual
#endif
		bool TryAdd(HashSet<ObjectSnapshot> hashSet, ObjectSnapshot element)
		{
			return ObjSnapshots.Add(element);
		}

		HashSet<ObjectSnapshot> ObjSnapshots
		{
			get { return objSnapshots ?? (objSnapshots = new HashSet<ObjectSnapshot>()); }
		}

		HashSet<ObjectSnapshot> objSnapshots;

		public IEnumerable<ObjectSnapshot> Except(FactorySnapshot factorySnapshot)
		{
			var result = new HashSet<ObjectSnapshot>();
			var changedObjectSnapshotsInFactorySnapshot = new HashSet<ObjectSnapshot>();

			foreach (var objSnapshot in factorySnapshot.ObjSnapshots)
			{
				if (IsChanged(objSnapshot))
				{
					changedObjectSnapshotsInFactorySnapshot.Add(objSnapshot);
				}
			}

			foreach (var objSnapshot in ObjSnapshots)
			{
				if (IsChanged(objSnapshot) && !changedObjectSnapshotsInFactorySnapshot.Contains(objSnapshot))
				{
					result.Add(objSnapshot);
				}
			}

			return result;
		}

		bool IsChanged(ObjectSnapshot objSnapshot)
		{
			return !objSnapshot.Status.Equals(ObjectStatusEnum.Unchanged);
		}
	}

#if DEBUG
	public
#endif
	enum ObjectStatusEnum
	{
		Unchanged,
		Updated,
		New,
		Deleted,
	}

#if DEBUG
	public
#endif
	class ObjectSnapshot
	{
		public ObjectSnapshot(BusinessObject obj)
		{
			Argument.NotNull(obj, "BusinessObject obj");

			this.obj = obj;
			this.Status = obj.GetStatus();
		}

		public ZGuid PK { get { return obj.PK; } }
		public Type Type { get { return obj.GetType(); } }
		public ObjectStatusEnum Status { get; private set; }
		readonly BusinessObject obj;

		internal bool IsNotDeleted()
		{
			return !obj.IsDeleted;
		}

		internal bool IsInDatabase()
		{
			return obj.IsInDatabase;
		}

		public override bool Equals(object obj)
		{
			var snapshot = obj as ObjectSnapshot;
			return snapshot != null && this.obj.Equals(snapshot.obj);
		}

		public override int GetHashCode()
		{
			return obj.GetHashCode();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "HashCode")]
		public override string ToString()
		{
			return string.Format("{0}(HashCode:{1} PK:{2})", Type.Name, GetHashCode(), PK); // internal purpose string
		}
	}
}
