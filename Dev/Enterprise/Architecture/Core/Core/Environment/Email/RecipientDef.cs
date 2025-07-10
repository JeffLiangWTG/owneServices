namespace Enterprise.ZArchitecture.Environment
{
	public class RecipientDef
	{
		public enum RecipientTypes
		{
			TO, CC, BCC
		}

		public RecipientDef(string email)
			: this(email, false)
		{ }

		public RecipientDef(string email, bool isForSystemCommunication, bool ignoreSystemEmailDestinationOverride = false)
		{
			this.email = email;
			this.isForSystemCommunication = isForSystemCommunication;
			this.ignoreSystemEmailDestinationOverride = ignoreSystemEmailDestinationOverride;
		}

		bool isForSystemCommunication;
		public bool IsForSystemCommunication
		{
			get { return isForSystemCommunication; }
			set { isForSystemCommunication = value; }
		}

		bool ignoreSystemEmailDestinationOverride;
		public bool IgnoreSystemEmailDestinationOverride
		{
			get { return IsForSystemCommunication && ignoreSystemEmailDestinationOverride; }
			set { ignoreSystemEmailDestinationOverride = value; }
		}

		readonly string email;
		public string Email
		{
			get { return email; }
		}

		public override bool Equals(object obj)
		{
			if (obj is string || obj is CargoWise.Types.ZString)
			{
				return obj.ToString() == Email;
			}
			return base.Equals(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
