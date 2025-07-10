using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IL;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public abstract class AsycudaBaseAdditionalInfo : CusSupportingInfo
	{
		public AsycudaBaseAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ThreadSafe]
		public static new readonly AsycudaBaseAdditionalInfoTypeDecider TypeDecider = new AsycudaBaseAdditionalInfoTypeDecider();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.AdditionalInfo;
		}
	}
}
