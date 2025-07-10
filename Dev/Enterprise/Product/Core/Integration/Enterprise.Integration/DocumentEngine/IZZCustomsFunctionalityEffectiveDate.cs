using System;
using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IZZCustomsFunctionalityEffectiveDate
	{
		bool IsFunctionalityValid(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate);
		bool IsFunctionalityValid(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, ZString companyKey);
#if DEBUG
		IDisposable TemporarilySetFunctionality(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, bool value);
#endif
	}
}
