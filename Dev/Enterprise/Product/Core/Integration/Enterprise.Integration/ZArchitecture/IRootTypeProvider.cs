using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IRootTypeProvider
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		Type[] RootTypes { get; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		BusinessObject[] Roots { get; }
	}
}
