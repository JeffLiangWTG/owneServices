using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				var euDeclaration = Destination as JobDeclaration;
				if (euDeclaration != null)
				{
					Synchronisers.Add(new FieldSynchroniser(euDeclaration.ZG_CTStatusIDInfo, GetCTStatusID, () => new[] { Source.JS_CommunityTransitStatusInfo }));
					Synchronisers.Add(new FieldSynchroniser(euDeclaration.JE_OA_DeclarantAddressInfo, GetBrokerOrgAddressPk, GetBrokerOrgAddressPkInfos));
					Synchronisers.Add(new FieldSynchroniser(() => euDeclaration.CustomsEntryInstructions.OrderBy(cei => cei.CEI_SystemCreateTimeUtc).FirstOrDefault()?.CEI_TotalInnerPackagesInfo, () => Source.JS_TotalPackageCount, () => new[] { Source.JS_TotalPackageCountInfo }, () => false));
					Synchronisers.Add(new FieldSynchroniser(euDeclaration.UniqueVoyageIdentifierInfo, GetSourceTransportsIncludingRelated, GetSourceTransportsIncludingRelatedInfos));
				}
			}
		}

		protected override PackingSynchroniser GetPackingSynchroniser()
		{
			return new EuPackingSynchroniser(this, Destination);
		}

		protected virtual IZType GetBrokerOrgAddressPk()
		{
			var brokerOrgHeader = Destination.IsImport ? Source.ImportBroker : Destination.IsExport ? Source.ExportBroker : null;
			if (brokerOrgHeader == null && Destination.Branch != null && Destination.Branch.OrgProxy != null)
			{
				brokerOrgHeader = Destination.Branch.OrgProxy;
			}
			return brokerOrgHeader != null ? brokerOrgHeader.MainAddress.PK : ZGuid.Empty;
		}

		protected virtual ZPropertyInfo[] GetBrokerOrgAddressPkInfos()
		{
			var brokerInfo = Destination.IsImport ? Source.JS_OH_ImportBrokerInfo : Destination.IsExport ? Source.JS_OH_ExportBrokerInfo : null;
			return brokerInfo == null ? new ZPropertyInfo[] { Destination.JE_MessageTypeInfo } : new ZPropertyInfo[] { Destination.JE_MessageTypeInfo, brokerInfo };
		}

		protected virtual IZType GetCTStatusID() => Source.JS_CommunityTransitStatus;

		protected override void HookConsolToDeclarationSynchronisers()
		{
			base.HookConsolToDeclarationSynchronisers();
			var consol = Destination.RelevantConsol;
			if (consol != null)
			{
				var euDeclaration = Destination as JobDeclaration;
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(euDeclaration.ZG_ShipmentTypeInfo, GetNewShipmentTypeFromConsol, GetNewShipmentTypeFromConsolInfos, true));
			}
		}

		ZPropertyInfo[] GetNewShipmentTypeFromConsolInfos()
		{
			var infos = new List<ZPropertyInfo>();
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_AgentTypeInfo);
			}
			return infos.ToArray();
		}

		IZType GetNewShipmentTypeFromConsol()
		{
			var result = ZString.Empty;
			if (hookedConsol != null)
			{
				result = hookedConsol.IsDirect ? ShipmentTypeList.Codes.BasicDirect : ShipmentTypeList.Codes.HouseConsignment;
			}
			return result;
		}

		protected override IZType GetFirstArrivalPort()
		{
			var result = ZString.Empty;

			if (!hookedConsol.JK_RL_NKPortOfFirstArrival.IsEmpty && Destination.Factory.IsInEuropeanCustomsUnion(hookedConsol.JK_RL_NKPortOfFirstArrival.Left(2)))
			{
				result = (ZString)base.GetFirstArrivalPort();
			}
			else if (Destination.IsFirstArrivalDateAndPortUsed && hookedConsol != null)
			{
				var orderedLegs = hookedConsol.Transports.OfType<Transport>().OrderBy(x => x.JW_LegOrder).ToArray();

				result = orderedLegs.Where(x => Destination.Factory.IsInEuropeanCustomsUnion(x.JW_RL_NKDiscPort.Left(2)) &&
											!Destination.Factory.IsInEuropeanCustomsUnion(x.JW_RL_NKLoadPort.Left(2))).
											Select(x => x.JW_RL_NKDiscPort).FirstOrDefault();
			}

			return result;
		}

		protected override IZType GetPortOfLoading() => Destination.IsImport && hookedConsol != null ? hookedConsol.JK_RL_NKLoadPort : base.GetPortOfLoading();

		protected override ZPropertyInfo[] GetPortOfLoadingRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>(base.GetPortOfLoadingRelatedInfos());
			if (Destination.IsImport && hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadPortInfo);
			}
			return infos.ToArray();
		}

		IZType GetSourceTransportsIncludingRelated()
		{
			var legs = new List<Transport>();
			foreach (Transport leg in Source.TransportsIncludingRelated)
			{
				legs.Add(leg);
			}

			var countriesOfRouting = GetCountriesOfRoutingFromTransportLegs(legs).ToArray();

			return ZString.Join(countriesOfRouting);
		}

		public static List<ZString> GetCountriesOfRoutingFromTransportLegs(IEnumerable<Transport> legs)
		{
			List<ZString> countries = new List<ZString>();
			foreach (Transport trans in legs)
			{
				var depCountry = ZString.Empty;
				var arrCountry = ZString.Empty;
				if (trans.LoadPort != null && trans.LoadPort.Country != null)
				{
					depCountry = trans.LoadPort.RL_RN_NKCountryCode;
				}
				if (trans.DiscPort != null && trans.DiscPort.Country != null)
				{
					arrCountry = trans.DiscPort.RL_RN_NKCountryCode;
				}

				if (!depCountry.IsEmpty && !countries.Contains(depCountry))
				{
					countries.Add(depCountry);
				}
				if (!arrCountry.IsEmpty && !countries.Contains(arrCountry))
				{
					countries.Add(arrCountry);
				}
			}
			return countries;
		}

		IEnumerable<ZPropertyInfo> GetSourceTransportsIncludingRelatedInfos()
		{
			foreach (Transport leg in Source.TransportsIncludingRelated)
			{
				yield return leg.JW_RL_NKLoadPortInfo;
				yield return leg.JW_RL_NKDiscPortInfo;
			}
		}

		public JobDeclarationSynchroniser(BaseJobDeclaration destination)
			: base(destination)
		{
		}
	}
}
