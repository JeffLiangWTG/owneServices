using System;

namespace CargoWise.EntityFramework.Testing
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class SuppressCollectionStateTestAttribute : Attribute { }
}
