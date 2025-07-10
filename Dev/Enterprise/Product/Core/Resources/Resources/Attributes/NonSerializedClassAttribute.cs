using System;

namespace Enterprise.Core
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class NonSerializedClassAttribute : Attribute
	{
	}
}
