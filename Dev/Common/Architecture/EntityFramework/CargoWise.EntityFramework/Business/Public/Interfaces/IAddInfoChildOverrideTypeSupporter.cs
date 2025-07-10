using System;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoChildOverrideTypeSupporter : IAddInfoChildSupporter
	{
		Type AddInfoChildType { get; }
	}
}
