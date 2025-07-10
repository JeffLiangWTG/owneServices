using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class AllowAllObjectsToBeLoaded : Attribute
	{
	}
}
