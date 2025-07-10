using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class NoResString : MultilingualString
	{
		public static explicit operator NoResString(string value)
		{
			return new NoResString(value);
		}

		public static explicit operator NoResString(ZString value)
		{
			return new NoResString(value);
		}

		protected NoResString(string str)
		{
			this.str = str;
		}

		readonly ZString str;

		public override string ToString()
		{
			return str;
		}

		public override string ToString(string language)
		{
			return str;
		}

		public override string GetUnresolvedString()
		{
			return str;
		}

		public override bool Equals(object obj)
		{
			return obj is NoResString ? this.str.Equals(((NoResString)obj).str) : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return str.GetHashCode();
		}
	}
}
