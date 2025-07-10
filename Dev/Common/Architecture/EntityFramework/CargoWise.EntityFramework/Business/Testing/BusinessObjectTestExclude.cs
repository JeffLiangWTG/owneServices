using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Attribute to exclude property from being tested by BusinessObjectTestCase.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
	public sealed class BusinessObjectTestExclude : Attribute
	{
	}

	/// <summary>
	/// Attribute to exclude property MaxLength from being required by BusinessObjectTestCase.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class BusinessObjectMaxLengthTestExcludeAttribute : Attribute
	{
	}

	/// <summary>
	/// Attribute to exclude property Empty String Assertion if property always returns not empty value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class BusinessObjectEmptyStringTestExcludeAttribute : Attribute
	{
	}

	/// <summary>
	/// If applied, reflection test that makes sure IAutoRating has AutoRatingAuditLog will exclude the object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class AutoRatingAuditLogExclude : Attribute
	{
	}
}
