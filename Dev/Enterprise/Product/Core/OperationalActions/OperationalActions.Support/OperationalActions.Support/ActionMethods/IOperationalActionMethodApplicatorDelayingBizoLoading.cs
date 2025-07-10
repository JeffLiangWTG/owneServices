using System;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Support
{
	public interface IOperationalActionMethodApplicatorDelayingBizoLoading
	{
		void Apply(IOperationalActionSectionLog log, ZGuid[] targetPks, Type bizObjType);
	}
}
