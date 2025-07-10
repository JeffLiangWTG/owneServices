using System;

namespace CargoWise.EntityFramework
{
	public interface IImmediateHint : IFetchHint
	{
		Type BusinessObjectType { get; }
	}
}
