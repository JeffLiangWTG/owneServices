using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				PopulateFCLUnitsIfRequired();
			}
		}

		void PopulateFCLUnitsIfRequired()
		{
			if (Destination.JE_TotalNoOfPieces.IsEmpty && (Source.JS_PackingMode == Core.Constants.ContainerModes.FCL || Source.JS_PackingMode == Core.Constants.ContainerModes.FCLMixedShipper))
			{
				var containerCount = Source.JS_Calc_ContainerCount;
				if (DetectEnabled)
				{
					if (Destination.JE_TotalNoOfPieces != containerCount)
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					Destination.JE_TotalNoOfPieces = containerCount;
				}
			}
		}

		protected override void HookConsolToDeclarationSynchronisers()
		{
			base.HookConsolToDeclarationSynchronisers();

			if (!SyncChangesDetected && hookedConsol != null)
			{
				HookConsolContainerMode();
			}
		}

		void HookConsolContainerMode()
		{
			IZType GetConsolContainerMode()
			{
				var containerMode = Destination.JE_ContainerMode;

				if (!Destination.IsAir
					&& hookedConsol != null && !hookedConsol.JK_ConsolMode.IsEmpty
					&& (Source.JS_TransportMode == Core.Constants.TransportModes.SeaAir || Source.JS_TransportMode == Core.Constants.TransportModes.AirSea))
				{
					containerMode = hookedConsol.JK_ConsolMode;
				}

				return containerMode;
			}

			ZPropertyInfo[] GetConsolContainerModeInfos()
			{
				var infos = new List<ZPropertyInfo>();
				infos.Add(Source.JS_TransportModeInfo);
				if (hookedConsol != null)
				{
					infos.Add(hookedConsol.JK_ConsolModeInfo);
				}

				return infos.ToArray();
			}

			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_ContainerModeInfo, GetConsolContainerMode, GetConsolContainerModeInfos));
		}

		protected override void HookContainerMode()
		{
			var packingModeSynchroniser = new FieldSynchroniser(Destination.JE_ContainerModeInfo, () => Destination.IsAir ? (ZString)Core.Constants.ContainerModes.AIR : Source.JS_PackingMode, () => new[] { Source.JS_PackingModeInfo, Source.JS_TransportModeInfo });
			packingModeSynchroniser.Format += PackingModeSynchroniser_Format;
			Synchronisers.Add(packingModeSynchroniser);
		}

		protected override Customs.Business.PackingSynchroniser GetPackingSynchroniser()
		{
			return new PackingSynchroniser(this, (JobDeclaration)Destination);
		}

		protected override ZPropertyInfo[] GetPortOfLoadingRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadForExportTransportInfo);
				infos.Add(hookedConsol.JK_RL_NKLoadForFirstImportTransportInfo);
			}
			return infos.ToArray();
		}

		protected override ZPropertyInfo[] GetDateOfArrivalRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>(base.GetDateOfArrivalRelatedInfos());
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_ATAForImportTransportInfo);
				infos.Add(hookedConsol.JK_ETAForImportTransportInfo);
			}
			return infos.ToArray();
		}

		protected override IZType GetContainerCount()
		{
			return Destination.IsAir || Destination.JE_ContainerMode == Core.Constants.ContainerModes.NonContainerised ? ZInt.Zero : base.GetContainerCount();
		}

		protected override ZPropertyInfo[] GetContainerCountInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>(base.GetContainerCountInfos());
			infos.Add(Destination.JE_ContainerModeInfo);
			return infos.ToArray();
		}
	}
}
