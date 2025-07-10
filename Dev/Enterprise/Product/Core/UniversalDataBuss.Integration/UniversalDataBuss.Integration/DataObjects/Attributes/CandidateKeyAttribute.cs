using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CandidateKeyAttribute : Attribute
	{
	}
}
