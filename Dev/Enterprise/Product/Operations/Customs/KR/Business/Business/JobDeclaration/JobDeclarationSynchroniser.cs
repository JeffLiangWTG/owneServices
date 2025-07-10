using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Destination
		{
			get => (JobDeclaration)base.Destination;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksPackTypeInfo, GetCustomsUnitForThisPackType, GetJE_TotalNoOfPacksPackTypeInfo));
		}

		IZType GetCustomsUnitForThisPackType()
		{
			return PackageTypeConverter.GetCustomsPackageType(Source.JS_F3_NKPackType);
		}
		IEnumerable<ZPropertyInfo> GetJE_TotalNoOfPacksPackTypeInfo()
		{
			yield return Destination.JE_TotalNoOfPacksPackTypeInfo;
		}

		protected override ZString GetCustomsUnitForThisPackType(ZString freightPackType)
		{
			return PackageTypeConverter.GetCustomsPackageType(freightPackType);
		}
	}
}

