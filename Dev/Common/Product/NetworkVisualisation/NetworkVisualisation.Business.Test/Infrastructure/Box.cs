using System;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class Box : IEquatable<Box>
	{
		public Box()
		{
		}

		public Box(int i)
		{
			BoxedInt = i;
		}

		public int BoxedInt { get; set; }

		public bool Equals(Box other)
		{
			return BoxedInt == other.BoxedInt;
		}

		public override bool Equals(object obj)
		{
			var box = obj as Box;
			return box != null && box.Equals(this);
		}

		public override int GetHashCode()
		{
			return BoxedInt.GetHashCode();
		}
	}
}
