using System;

namespace CargoWise.Types
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public interface IZType : IComparable
	{
		bool IsEmpty { get; }
		bool IsDefault { get; }
		bool IsValid { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Default")]
		IZType Default { get; }
		ZDataType DataType { get; }
		Type BaseDataType { get; }
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public interface INumericZType : IZType
	{
		ZInt ToZInt();
	}

	public interface IZTypeInternals
	{
		object GetValueForLogicalDataLayer(bool isNullable);
	}
}
