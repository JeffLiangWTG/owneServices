using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExtraVoyageValidation : JobVoyageValidation
	{
		public ExtraVoyageValidation(JobVoyage voyage)
			: base(voyage)
		{
		}

		protected override void CheckJV_VoyageType()
		{
			base.CheckJV_VoyageType();

			if (!Parent.JV_VoyageType.IsEmpty)
			{
				switch (Parent.JV_VoyageType)
				{
					case Core.Constants.VoyageType.MainVoyage:
						if (HasExportManifestsWithAnotherType(ManifestTypeList.Codes.SlotExportSubManifest))
						{
							Parent.JV_VoyageTypeInfo.AddWarning(Res.GetString("b4cff026-ff07-41bb-baa0-a399d6093a87", "You have flagged this sailing schedule as Main, but a Slot Manifest for this vessel-voyage already exists under Shipping>Customs Export Manifest."));
						}
						break;

					case Core.Constants.VoyageType.SlotVoyage:
						if (HasExportManifestsWithAnotherType(ManifestTypeList.Codes.ExportMainManifest))
						{
							Parent.JV_VoyageTypeInfo.AddWarning(Res.GetString("3712cea9-0f0d-4871-a7e5-5cf388511eb2", "You have flagged this sailing schedule as Slot, but a Main Manifest for this vessel-voyage already exists under Shipping>Customs Export Manifest."));
						}
						break;
				}
			}
		}

		bool HasExportManifestsWithAnotherType(ZString manifestType)
		{
			if (!Parent.JV_AirSeaRoad.IsEmpty && Parent.Vessel != null && !Parent.JV_VoyageFlight.IsEmpty)
			{
				ZQuery manifestQuery = new ZQuery();
				manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_TransportMode, Parent.JV_AirSeaRoad);
				manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VesselName, Parent.Vessel.RV_Code);
				manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_VoyageNumber, Parent.JV_VoyageFlight);
				manifestQuery.AddToFilter(ExportCustomsManifestHeaderSchema.ED_ManifestType, manifestType);

				return Parent.Factory.Load<ExportCustomsManifestHeader>(manifestQuery).Length > 0;
			}

			return false;
		}
	}
}
