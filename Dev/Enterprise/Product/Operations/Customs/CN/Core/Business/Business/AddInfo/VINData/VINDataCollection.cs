using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CN.Business
{
	public class VINDataCollection : DependentCusAddInfoCollection<VINData, BusinessObject>
	{
		public VINDataCollection(BusinessObject master) : base(master, CusAddInfoTypeAttribute.Codes.CNVINData)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var vinData = child as VINData;
			if (vinData != null)
			{
				vinData.XC_ProductNameCN = vinData.GetDefaultCNProductName();
				vinData.XC_ProductNameEN = vinData.GetDefaultENProductName();
			}
		}
	}
}
