using System;

namespace CargoWise.GraphEngine.Test
{
	public class DummyEntityFactory
	{
		public DummyEntityBuilder New(params string[] keys)
		{
			return new DummyEntityBuilder(this, keyFountain++, keys);
		}

		int keyFountain;

		public class DummyEntityBuilder : DummyEntity
		{
			internal DummyEntityBuilder(DummyEntityFactory factory, int id, string[] keys)
				: base(id, keys)
			{
				Factory = factory;
			}

			public DummyEntityFactory Factory { get; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var entity = obj as DummyEntityBuilder;
				if (entity != null && Factory != entity.Factory)
				{
					throw new InvalidOperationException("Having multiple factories in the same test means order is not deterministic.");
				}

				return base.Equals(obj);
			}
		}
	}

	public static class FactoryExtensions
	{
		public static Cons<DummyEntityFactory.DummyEntityBuilder> New(this DummyEntityFactory.DummyEntityBuilder entity, params string[] keys)
		{
			return new Cons<DummyEntityFactory.DummyEntityBuilder>(entity).New(keys);
		}

		public static Cons<DummyEntityFactory.DummyEntityBuilder> New(this Cons<DummyEntityFactory.DummyEntityBuilder> entities, params string[] keys)
		{
			var factory = entities.First.Factory;
			return new Cons<DummyEntityFactory.DummyEntityBuilder>(factory.New(keys), entities);
		}
	}
}
