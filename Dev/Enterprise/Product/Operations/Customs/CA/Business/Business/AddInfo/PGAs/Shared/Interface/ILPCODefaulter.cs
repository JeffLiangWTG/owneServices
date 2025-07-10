using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface ILPCODefaulter
	{
		IEnumerable<ZString> LPCOFieldsDefaultFromURN { get; }
		ZBool ShouldDefaultLPCOFields { get; }
	}
}
