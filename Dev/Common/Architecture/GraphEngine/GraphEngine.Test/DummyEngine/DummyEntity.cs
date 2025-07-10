using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.GraphEngine.Test
{
	public class DummyEntity : IEquatable<DummyEntity>, IDummyEntity
	{
		protected DummyEntity(int id, string[] keys)
		{
			ID = id;
			Keys = keys.Select(k => new DummyKey(k)).ToArray();
		}

		public int ID { get; }

		public IEnumerable<DummyKey> Keys { get; }

		public bool IsProcessed { get; private set; }

		void Process()
		{
			IsProcessed = true;
		}

		internal void Process(DummyGraphEngine engine)
		{
			Process();
			engine.Dummies.Add(this);
		}

		public override string ToString()
		{
			return $"D{ID}[{string.Join(", ", Keys)}]";
		}

		public bool Equals(DummyEntity other)
		{
			return other == this;
		}
	}
}
