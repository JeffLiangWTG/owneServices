using System;

namespace Enterprise.Customs.KR.Messaging
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class OrganisationIdentificationTypeAttribute : Attribute
	{
		public OrganisationIdentificationTypeAttribute(string idNumberType)
		{
			IDNumberType = idNumberType;
		}
		public readonly string IDNumberType;
	}
}
