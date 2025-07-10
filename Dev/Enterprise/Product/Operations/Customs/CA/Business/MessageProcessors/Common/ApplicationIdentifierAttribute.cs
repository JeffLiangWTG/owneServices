using System;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ApplicationIdentifierAttribute : Attribute
	{
		public ApplicationIdentifierAttribute(string applicationIdentifier)
		{
			if (string.IsNullOrEmpty(applicationIdentifier))
			{
				throw new ArgumentException("applicationIdentifier must have a non empty value");
			}
			ApplicationIdentifier = applicationIdentifier;
		}

		public readonly string ApplicationIdentifier;
	}
}
