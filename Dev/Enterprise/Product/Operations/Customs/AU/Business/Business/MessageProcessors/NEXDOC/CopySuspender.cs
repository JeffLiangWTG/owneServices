using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business;

public sealed class CopySuspender : IDisposable
{
	public CopySuspender(IBusinessObjectInternals bizO)
	{
		internals = bizO;
		if (internals.IsCopying)
		{
			Dispose();
			throw new NotSupportedException("You can't use a CopyingOperation when IsCopying on the BizO is already true, as CopyingOperation resets the IsCopying flag when it is disposed.");
		}
		internals.IsCopying = true;
	}

	readonly IBusinessObjectInternals internals;

	public void Dispose()
	{
		if (internals != null)
		{
			internals.IsCopying = false;
		}
	}
}
