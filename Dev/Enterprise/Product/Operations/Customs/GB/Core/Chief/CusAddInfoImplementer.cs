using System;
using Enterprise.Customs.Business.MultiLineAddInfos;
using static Enterprise.Integration.Customs.GB.GBChief;

namespace Enterprise.Customs.GB.Chief
{
	class CusAddInfoImplementer : ICusAddInfoImplementer
	{
		Type ICusAddInfoImplementer.GetMawbExportAddInfoType()
		{
			return typeof(CusAddInfo<ChiefExportConsolIntegration.MawbExportAddInfo>);
		}
	}
}
