namespace CargoWise.NetworkVisualisation.Integration
{
	public struct Location
	{
		public Location(double x, double y)
		{
			X = x;
			Y = y;
		}

		public double X { get; set; }
		public double Y { get; set; }

		public static bool operator ==(Location point1, Location point2)
		{
			if (point1.X == point2.X)
			{
				return point1.Y == point2.Y;
			}

			return false;
		}

		public static bool operator !=(Location point1, Location point2)
		{
			return !(point1 == point2);
		}

		public static bool Equals(Location point1, Location point2)
		{
			if (point1.X.Equals(point2.X))
			{
				return point1.Y.Equals(point2.Y);
			}

			return false;
		}

		public override bool Equals(object o)
		{
			if (o == null || !(o is Location))
			{
				return false;
			}

			Location point = (Location)o;
			return Equals(this, point);
		}

		public bool Equals(Location value)
		{
			return Equals(this, value);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}
}
