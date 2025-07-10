namespace Enterprise.UniversalDataBuss.Integration
{
	public struct RecipientRoleDetail
	{
		public RecipientRoleType Type;
		public ServiceCodeType? ServiceCode;

		public static bool operator ==(RecipientRoleDetail a, object b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(RecipientRoleDetail a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is RecipientRoleDetail detail)
			{
				result = detail.Type == Type && detail.ServiceCode == ServiceCode;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ ServiceCode.GetHashCode();
		}
	}
}
