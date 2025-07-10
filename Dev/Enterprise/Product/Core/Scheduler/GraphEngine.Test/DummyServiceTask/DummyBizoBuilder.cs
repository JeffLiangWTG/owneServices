using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GraphEngine.Test;
using CargoWise.Types;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	public static class DummyToDummyBizoBuilder
	{
		const char KeySeperator = ',';

		public static DummyBusinessObject[] MakeBizos<T>(this Cons<T> dummies, BusinessObjectFactory factory)
		where T : DummyEntity
		{
			return dummies.Reverse().MakeBizos<DummyBusinessObject>(factory);
		}

		public static DummyBusinessObject[] MakeBizos(this IEnumerable<DummyEntity> dummies, BusinessObjectFactory factory)
		{
			return dummies.MakeBizos<DummyBusinessObject>(factory);
		}

		public static DummyBusinessObject MakeBizo(this DummyEntity dummy, BusinessObjectFactory factory)
		{
			return dummy.MakeBizo<DummyBusinessObject>(factory);
		}

		public static T[] MakeBizos<T>(this IEnumerable<DummyEntity> dummies, BusinessObjectFactory factory)
			where T : DummyBusinessObject
		{
			return dummies.Select(d => d.MakeBizo<T>(factory)).ToArray();
		}

		public static T MakeBizo<T>(this DummyEntity dummy, BusinessObjectFactory factory)
			where T : DummyBusinessObject
		{
			var bizo = factory.NewWithValidTestData<T>();

			bizo.Z0_Number = dummy.ID + 1;
			bizo.Z0_Date = ZDateTime.UtcNow.AddSeconds(bizo.Z0_Number);
			bizo.Z0_Bool = dummy.IsProcessed;
			bizo.Z0_VarCharMax = string.Join(KeySeperator.ToString(), dummy.Keys);

			return bizo;
		}

		public static ZString[] GetKeys(this DummyBusinessObject bizo)
		{
			return bizo.Z0_VarCharMax.Split(KeySeperator).Where(s => !s.IsEmpty).ToArray();
		}

		public static string[] GetStringKeys(this DummyBusinessObject bizo)
		{
			return ((string)bizo.Z0_VarCharMax).Split(KeySeperator).Where(s => !string.IsNullOrEmpty(s)).ToArray();
		}
	}
}
