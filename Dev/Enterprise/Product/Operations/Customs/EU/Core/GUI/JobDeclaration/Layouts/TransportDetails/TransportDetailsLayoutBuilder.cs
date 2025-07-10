using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Declaration
{
	public class TransportDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, Customs.GUI.TransportDetailsControlBag>
		where T : JobDeclaration
	{
		public override Customs.GUI.TransportDetailsControlBag CommonBag => Customs.GUI.TransportDetailsControlBag.Instance;

		public TransportDetailsControlBag EUBag => TransportDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.OverrideValuesCheckBox, x => !x.IsStandAlone, x => x.JE_JSInfo);
			SetVisibility(CommonBag.MasterBillTextBox, x => x.IsAir, x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.OceanBillTextBox, x => x.IsSea, x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.VesselCodeFindBox, x => x.IsSea, x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.FlightAndNationalityUserControl, x => x.IsAir, x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.VoyageAndNationalityUserControl, x => x.IsSea, x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.TransportIDAndNationalityUserControl, x => !(x.IsAir || x.IsSea), x => x.JE_TransportModeInfo);
			SetVisibility(CommonBag.PortOfLoadingUserControl, x => !(x.IsImport && x.IsAir), dependsOnTransportModeAndMessageType);
			SetVisibility(CommonBag.TransportDetailsPortOfLoadingWithIATAUserControl, x => x.IsImport && x.IsAir, dependsOnTransportModeAndMessageType);
			SetVisibility(CommonBag.TransportInlandRoadUserControl, x => x.IsUCC6 && x.IsRoadInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandAirUserControl, x => x.IsUCC6 && x.IsAirInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandInlandWaterwaysUserControl, x => x.IsUCC6 && x.IsWaterwayTransportsInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandOwnPropulsionUserControl, x => x.IsUCC6 && (x.IsOwnPropulsionInland || x.IsFixedInstallationInland || x.IsMailInland), dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandRailUserControl, x => x.IsUCC6 && x.IsRailInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandSeaUserControl, x => x.IsUCC6 && x.IsSeaInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(EUBag.InlandTransportDetailsUserControl, InlandTransportDetailsUserControlVisible, dependsOnTransportModeAndMessageType);
			SetVisibility(CommonBag.InlandModeOfTransportDropEdit, InlandModeOfTransportDropEditVisibility, dependsOnTransportModeAndMessageType);
			SetVisibility(EUBag.AdditionalWagonNumbersUserControl, AdditionalWagonNumbersUserControlVisibility, dependsOnTransportModeInlandAndMessageType);
		}

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();

			SetCaptions(
				CommonBag.TransportInlandSeaUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox),
				j => j.JE_TransportMeansInfo
			);
		}

		protected virtual bool InlandModeOfTransportDropEditVisibility(T declaration) => !declaration.IsExport;

		protected readonly Func<JobDeclaration, ZPropertyInfo>[] dependsOnTransportModeInlandAndMessageType = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInlandInfo, x => x.JE_MessageTypeInfo };
		protected readonly Func<JobDeclaration, ZPropertyInfo>[] dependsOnTransportModeAndMessageType = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInfo, x => x.JE_MessageTypeInfo };

		protected virtual bool AdditionalWagonNumbersUserControlVisibility(T declration) => declration.IsUCC6 && declration.IsRailInland && !declration.IsTransitionPeriodAES30;
		protected virtual bool InlandTransportDetailsUserControlVisible(T declaration) => !declaration.IsUCC6 || declaration.JE_TransportModeInland.IsEmpty;

		Dictionary<string, ResourceStringData> GetInlandCaptionDictionary(
			JobDeclaration declaration,
			ZString idControlName
		)
		{
			var result = new Dictionary<string, ResourceStringData>();
			if (!idControlName.IsEmpty)
			{
				result.Add(idControlName, JobDeclarationInlandTransportResDataHelper.GetInlandTransactionRes(declaration.JE_TransportModeInland, declaration.JE_TransportMeans));
			}

			return result;
		}
	}
}
