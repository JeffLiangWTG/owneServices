using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface ICompositeCollection
	{
		Type TypeOfElementFromPK(ZGuid pK);
		Type TypeOfElementFromCode(ZString code);
		int MaxLength { get; }
	}
}
