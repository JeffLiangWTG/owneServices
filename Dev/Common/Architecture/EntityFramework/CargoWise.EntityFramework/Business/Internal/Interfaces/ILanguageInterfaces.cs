using System;

namespace CargoWise.EntityFramework
{
	public interface ICharacterSetValidationSupport
	{
		Action<ZPropertyInfo> ValidateCharacterSet { get; }
	}
}
