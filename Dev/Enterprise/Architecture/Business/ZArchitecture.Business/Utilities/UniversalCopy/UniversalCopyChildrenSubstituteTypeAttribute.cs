using System;
using System.Diagnostics;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyChildrenSubstituteTypeAttribute : Attribute
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug message")]
		public UniversalCopyChildrenSubstituteTypeAttribute(Type declaredType, Type actualType)
		{
			Debug.Assert(declaredType.IsAssignableFrom(actualType), "declaredType should be assignable from actualType.");

			DeclaredType = declaredType;
			ActualType = actualType;
		}

		public Type DeclaredType { get; private set; }
		public Type ActualType { get; private set; }
	}
}
