using System.Collections;
using CargoWise.Common.Testing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	[CodeAlive("I don't why it complains, this code is used.")]
	public interface IAutoCompleteField
	{
		[SuppressWeaklyTypedCollectionMessage] // We dont need to know the type of the object, and it complicates the design to include it
		IList GetList(string partialResult);

		char MagicChar { get; }
	}
}
