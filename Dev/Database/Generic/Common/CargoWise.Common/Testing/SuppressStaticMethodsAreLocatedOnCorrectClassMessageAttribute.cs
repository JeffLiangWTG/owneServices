using System;

namespace CargoWise.Common.Testing
{
	/// <summary>
	/// Suppress the target class from the 'static methods are located on correct class' reflection test.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class SuppressStaticMethodsAreLocatedOnCorrectClassMessageAttribute : Attribute
	{
	}
}
