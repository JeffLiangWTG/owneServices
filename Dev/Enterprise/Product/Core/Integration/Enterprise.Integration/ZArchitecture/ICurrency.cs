using System;

namespace Enterprise.Integration.ZArchitecture
{
	public interface ICurrency
	{
		string Code { get; }
		int Decimals { get; }
		Guid PK { get; }
	}
}
