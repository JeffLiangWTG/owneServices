using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Database.Abstractions
{
	public interface IDbValueConversion
	{
		[SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "explicitly dealing with arbitrary, potentially-boxed values of unknown types")]
		bool TryUnwrapSimpleValue(object value, out object unwrapped);

		TypeConverter TryGetConverterForValues(IEnumerable values);
	}
}
