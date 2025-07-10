using Enterprise.Customs.Common.AU;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOExportCustomsManifestHeaderLookups : ExportCustomsManifestHeaderLookups
	{
		public AirCTOExportCustomsManifestHeaderLookups(AirCTOExportCustomsManifestHeader header)
			: base(header)
		{
		}

		public override CodeDescriptionPairList ManifestTypeList
		{
			get
			{
				if (manifestTypeList == null)
				{
					manifestTypeList = new AirManifestTypeList();
				}
				return manifestTypeList;
			}
		}
		CodeDescriptionPairList manifestTypeList;
	}
}
