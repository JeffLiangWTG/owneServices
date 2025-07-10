namespace Enterprise.ZArchitecture.Modules
{
	public class TableRegistrationInfo
	{
		public TableRegistrationInfo(string name)
		{
			this.Name = name;
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			TableRegistrationInfo info = obj as TableRegistrationInfo;
			return info != null && Name == info.Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public readonly string Name;
	}
}
