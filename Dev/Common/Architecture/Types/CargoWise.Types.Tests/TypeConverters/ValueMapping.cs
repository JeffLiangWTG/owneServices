using System;
namespace CargoWise.Types.Tests
{
	public struct ValueMapping
	{
		public ValueMapping(object from, object to)
		{
			if (from == null)
			{
				throw new ArgumentNullException(nameof(from));
			}

			if (to == null)
			{
				throw new ArgumentNullException(nameof(to));
			}

			From = from;
			To = to;
			FromType = from.GetType();
			ToType = to.GetType();
		}

		public ValueMapping(object from, Type fromType, object to, Type toType)
		{
			From = from;
			To = to;
			FromType = fromType;
			ToType = toType;
		}

		public readonly object From;
		public readonly object To;

		public readonly Type FromType;
		public readonly Type ToType;
	}
}
