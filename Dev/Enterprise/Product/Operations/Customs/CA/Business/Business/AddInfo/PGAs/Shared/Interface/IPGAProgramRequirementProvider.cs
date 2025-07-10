using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public interface IPGAProgramRequirementProvider : IPGAHeader
	{
		ZPropertyInfo GetProgramIndicatorInfo(ZString programCode);
		CodeDescriptionPairList GetProgramCodesList();

		void ValidateProgramIndicator(ZString programCode);
		IDisposable SuspendSettingDefaultValues();

		SetterSuspender SetterSuspender { get; }
	}
}
