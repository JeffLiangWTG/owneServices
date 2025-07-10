using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public struct PKData
	{
		public ZGuid PK;
		public object Data;

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is PKData)
			{
				var pkData = (PKData)obj;
				result = pkData.PK == PK && (pkData.Data?.Equals(Data) ?? Data == null);
			}
			return result;
		}

		public override int GetHashCode()
		{
			return PK.IsEmpty || Data == null ? 0 : PK.GetHashCode() ^ Data.GetHashCode();
		}

		public static bool operator ==(PKData obj1, PKData obj2)
		{
			return obj1.Equals(obj2);
		}

		public static bool operator !=(PKData obj1, PKData obj2)
		{
			return !obj1.Equals(obj2);
		}
	}
}
