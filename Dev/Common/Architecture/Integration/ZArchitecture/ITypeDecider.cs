using System;
using System.Data;

namespace CargoWise.Integration
{
	public interface ITypeDecider
	{
		Type GetTypeForNew();
		Type GetTypeForLoad(DataRow row, object factory);
		Type GetTypeForBinding();
	}
}
