using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.Common
{
	public class EntityCollection : IEnumerable<IEntity>
	{
		List<IEntity> entities;

		public void Add(IEntity entity)
		{
			entities = entities ?? new List<IEntity>();
			entities.Add(entity);
		}

		public void AddAtStart(IEntity entity)
		{
			entities = entities ?? new List<IEntity>();
			entities.Insert(0, entity);
		}

		public void Add(IEnumerable<IEntity> others)
		{
			entities = entities ?? new List<IEntity>();
			entities.AddRange(others);
		}

		public bool Remove(IEntity entity)
		{
			return entities?.Remove(entity) ?? false;
		}

		public void RemoveAll()
		{
			entities = null;
		}

		public int FindIndex(Predicate<IEntity> predicate)
		{
			return entities?.FindIndex(predicate) ?? -1;
		}

		public void RemoveAt(int index)
		{
			entities?.RemoveAt(index);
		}

		public void AddRange(IEnumerable<IEntity> range)
		{
			entities = entities ?? new List<IEntity>();
			entities?.AddRange(range);
		}

		public void ForEach(Action<IEntity> action)
		{
			entities?.ForEach(action);
		}

		#region Implementation of IEnumerable

		public IEnumerator<IEntity> GetEnumerator()
		{
			if (entities == null)
			{
				return Enumerable.Empty<IEntity>().GetEnumerator();
			}

			return entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
