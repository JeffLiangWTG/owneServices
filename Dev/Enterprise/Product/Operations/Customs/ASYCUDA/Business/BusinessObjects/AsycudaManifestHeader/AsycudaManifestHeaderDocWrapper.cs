using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderDocWrapper : DocumentWrapper
	{
		public AsycudaManifestHeaderDocWrapper(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader, manifestHeader.Factory)
		{
		}

		public AsycudaManifestHeader Manifest
		{
			get { return (AsycudaManifestHeader)WrappedObject; }
		}

		public ForwardingConsol Consol
		{
			get { return Manifest.Consol; }
		}

		#region New

		public static AsycudaManifestHeaderDocWrapper New(BusinessObject bizO, ZString menuName, ZString filterList)
		{
			AsycudaManifestHeaderDocWrapper wrapper = null;
			AsycudaManifestHeader[] manifests = null;
			var manifest = bizO as AsycudaManifestHeader;
			if (manifest == null)
			{
				var consol = bizO as ForwardingConsol;
				if (consol != null)
				{
					var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
					query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, JobConsolSchema.Constants.Prefix);
					query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut);
					query.FetchOnlyFromLocalCache = !consol.IsInDatabase;
					manifests = consol.Factory.Load<AsycudaManifestHeader>(query).OrderBy(x => x.AMA_SystemCreateTimeUtc).ToArray();
				}
			}
			else
			{
				manifests = new[] { manifest };
			}

			if (manifests != null)
			{
				var matchManifest = GetManifestMatch(bizO?.Factory, menuName, filterList);
				manifest = manifests.FirstOrDefault(x => matchManifest(x));
			}
			if (manifest != null)
			{
				wrapper = manifest.GetDocWrapper();
			}
			return wrapper;
		}

		static Func<Integration.Customs.ASYCUDA.IAsycudaManifestHeader, bool> GetManifestMatch(BusinessObjectFactory factory, ZString menuName, ZString filterList)
		{
			Func<Integration.Customs.ASYCUDA.IAsycudaManifestHeader, bool> result = null;

			var valuePairs = filterList.Split('=');
			if (valuePairs.Length == 2 && Enum.TryParse(valuePairs[0], out DocumentFilters filterName) && filterName == DocumentFilters.AdditionalMatch)
			{
				result = ForwardingConsolDocumentSupporter.GetGlobalManifestMatch(valuePairs[1]);
			}

			if (result == null)
			{
				var countries = GetCountriesSpecific(factory, menuName);
				if (countries.Any())
				{
					result = new Func<Integration.Customs.ASYCUDA.IAsycudaManifestHeader, bool>(x => countries.Contains(x.AMA_RN_NKCountry));
				}
			}

			return result ?? new Func<Integration.Customs.ASYCUDA.IAsycudaManifestHeader, bool>(x => true);
		}

		static IEnumerable<ZString> GetCountriesSpecific(BusinessObjectFactory factory, ZString menuName)
		{
			switch (menuName)
			{
				case EUICS2ManifestDocument:

					yield return Core.Constants.CountryCodes.EuropeanUnion;

					foreach (var countryCode in factory?.GetEuropeanUnionForCustomsMembers() ?? Array.Empty<string>())
					{
						yield return countryCode;
					}

					break;

				case ZAManifestWithBarcode:
					yield return Core.Constants.CountryCodes.SouthAfrica;
					break;

				case TRManifestBills:
				case TRManifestBillsLines:
				case TRManifestPrint:
				case TRManifestBillsforEMANIF:
					yield return Core.Constants.CountryCodes.Turkey;
					break;
				case INManifestAirCGM:
					yield return Core.Constants.CountryCodes.India;
					break;
			}
		}

		public const string ZAManifestWithBarcode = "ZA Manifest with Barcode";
		public const string TRManifestBillsforEMANIF = "TR Manifest Bills for EMANIF";
		public const string TRManifestBills = "TR Manifest Bills";
		public const string TRManifestBillsLines = "TR Manifest Bills Lines";
		public const string TRManifestPrint = "TR Manifest Print";
		public const string EUICS2ManifestDocument = "EU Manifest";
		public const string INManifestAirCGM = "IN Manifest - AIR CGM";

		#endregion

		#region Properties

		public ZDateTime ManifestDate
		{
			get
			{
				return Manifest.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
					.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault()?.EM_MessageDateTime ?? ZDateTime.Empty;
			}
		}

		public CusPerson Driver
		{
			get
			{
				return Manifest.Persons.Count > 1
					? Manifest.Persons.Cast<CusPerson>().FirstOrDefault(x => x.IsDriver) : Manifest.Persons.Cast<CusPerson>().FirstOrDefault();
			}
		}

		public ZString TotalGrossWeightInKilograms => string.Format(DefaultCulture.Instance.NumberFormat, "{0:#,0.00}", Manifest.Bills.Cast<AsycudaBill>().SelectMany(bill => bill.Packs.Cast<AsycudaPack>().SelectMany(pack => pack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => x.PackedItem))).Sum(x => Core.Constants.Weight.ConvertSafe(x.API_GrossWeight, x.API_GrossWeightUQ, Core.Constants.Weight.Kilograms)));

		public ZInt TotalPackQuantity => Manifest.Bills.Cast<AsycudaBill>().SelectMany(bill => bill.Packs.Cast<AsycudaPack>().Select(pack => (int)pack.APA_PackQty)).Sum();

		public ZString LoadPortName => Manifest?.PortOfLoading?.RL_PortName ?? ZString.Empty;
		public virtual ZString CustomsLoadPortName => GetCustomsLoadingPortDescription(Manifest.AMA_CustomsLoadPort);

		public ZString DischargePortName => Manifest?.PortOfDischarge?.RL_PortName ?? ZString.Empty;
		public virtual ZString CustomsDischargePortName => GetCustomsDischargePortDescription(Manifest.AMA_CustomsDischargePort);

		public virtual ZString ConveyanceNationality => Manifest?.AMA_RN_NKConveyanceNationality ?? ZString.Empty;

		public ZString CompanyName => Manifest.Branch.Company.CompanyName;

		public ZString CompanyAddress => new AddressWrapper(Manifest.Branch.Company.OrgProxy.MainAddress, ContactType.All, Factory).AddressAsASingleLine;
		#endregion

		protected virtual ZString GetCustomsLoadingPortDescription(ZString customsPortCode)
		{
			if (Manifest.Lookups.CustomsLoadingPortList is ZZRefCusCodeListCombinedCollection)
			{
				return GetCustomsPortDescription(customsPortCode, (ZZRefCusCodeListCombinedCollection)Manifest.Lookups.CustomsLoadingPortList);
			}
			return ZString.Empty;
		}

		protected virtual ZString GetCustomsDischargePortDescription(ZString customsPortCode)
		{
			if (Manifest.Lookups.CustomsDischargePortList is ZZRefCusCodeListCombinedCollection)
			{
				return GetCustomsPortDescription(customsPortCode, (ZZRefCusCodeListCombinedCollection)Manifest.Lookups.CustomsDischargePortList);
			}
			return ZString.Empty;
		}

		protected ZString GetCustomsPortDescription(ZString customsPortCode, ZZRefCusCodeListCombinedCollection portList)
		{
			var filter = portList.CompleteFilter;
			filter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, customsPortCode);
			var customsPortDescription = Factory.LoadTop1<ZZRefCusCodeListCombined>(filter);
			return customsPortDescription?.ZZD_Description ?? ZString.Empty;
		}
	}
}
