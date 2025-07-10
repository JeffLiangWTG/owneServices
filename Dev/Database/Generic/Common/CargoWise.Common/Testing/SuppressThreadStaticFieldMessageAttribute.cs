using System;

namespace CargoWise.Common.Testing
{
	/// <summary>
	/// ** DO NOT APPLY THIS ATTRIBUTE WITHOUT SPEAKING TO THE ARCHITECTURE TEAM **
	/// Static fields can cause threading issues on the web and should be treated with care.
	///
	/// Apply this attribute to:
	/// - A static field that you are certain will not cause any threading problems.
	/// - A class that BOTH:
	///   - Has no static fields that will cause any threading problems.
	///   - Whose INSTANCE members are 100% thread safe.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class SuppressThreadStaticFieldMessageAttribute : Attribute
	{
	}
}
