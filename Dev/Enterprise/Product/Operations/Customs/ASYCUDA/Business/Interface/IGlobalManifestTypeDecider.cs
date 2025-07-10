using System;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IGlobalManifestTypeDecider
	{
		Type GetType(ZString manifestType);
	}
}
