using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface IUpdateEventDateSupporter
	{
		IDisposable SetCancellingEventDatePropertyContext(IStmALog log);
	}
}
