
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Modules
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class RegistrationIdentifier
	{
		protected internal RegistrationIdentifier(string name)
		{
			Argument.NotNull(name, nameof(name));
			this.Name = name;
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			RegistrationIdentifier rhs = obj as RegistrationIdentifier;
			return rhs != null && Name == rhs.Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public readonly string Name;
	}
}
