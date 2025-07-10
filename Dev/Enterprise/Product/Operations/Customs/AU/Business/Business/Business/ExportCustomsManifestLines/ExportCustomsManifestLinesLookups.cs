using CargoWise.Integration;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestLinesLookups : Customs.Business.ExportCustomsManifestLinesLookups
	{
		public ExportCustomsManifestLinesLookups(ExportCustomsManifestLines parent)
			: base(parent)
		{
		}

		public override ICodeDescriptionPairList TypeOfCANs
		{
			get { return Factory.GetCachedValue<CANTypeList>(); }
		}

		public override ICodeDescriptionPairList ValidExemptionCodes
		{
			get { return Factory.GetCachedValue<CMRExportExemptionCodesList>(); }
		}

		public override OrgHeaderCollection Owners
		{
			get
			{
				if (fOwners == null)
				{
					fOwners = new ConsignorCollection(Factory);
				}
				return fOwners;
			}
		}
		OrgHeaderCollection fOwners;
	}
}
