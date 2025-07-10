
using CargoWise.Types;

namespace Enterprise.UniversalCopy.Business
{
	interface ICopyable
	{
		ZString CopyMethod { get; }
	}
}
