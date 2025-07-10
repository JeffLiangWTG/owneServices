using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using AsycudaBill = Enterprise.Customs.EU.Manifest.Business.AsycudaBill;
using AsycudaContainer = Enterprise.Customs.EU.Manifest.Business.AsycudaContainer;

namespace Enterprise.Customs.GB.ICS.Business
{
	public abstract class AsycudaManifestHeaderBase : EU.Manifest.Business.AsycudaManifestHeader
		, Integration.Customs.GB.GBICS.IAsycudaManifestHeader
	{
		public AsycudaManifestHeaderBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly AsycudaManifestHeaderBaseTypeDecider TypeDecider = new AsycudaManifestHeaderBaseTypeDecider();

		public new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeaderBase> Bills => (ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeaderBase>)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeaderBase>(this);
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeaderBase> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeaderBase>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeaderBase>(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.UnitedKingdom;
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new ZString RegistrationStatusDescription => Lookups.RegistrationStatusList.GetDescriptionFromCode(RegistrationStatus);

		protected override bool SpecialMentions_ReadOnly => true;
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = ManifestType?.Code ?? ICSManifestTypes.Codes.ICS;
		}

#endif

		#region ICusCodeDataTypeSupporter

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = base.SupportedCusCodeDataTypes;
				result.Add(CusCodeDataTypeList.Codes.IcsRouteEntry, typeof(RouteEntry));
				return result;
			}
		}

		#endregion

		[ChildEditable(true)]
		public RouteEntryCollection Itinerary
		{
			get
			{
				if (itinerary == null)
				{
					itinerary = new RouteEntryCollection(this);
					itinerary.Load();
					RegisterEditableChildObject(itinerary);
				}
				return itinerary;
			}
		}
		RouteEntryCollection itinerary;

		public CommonManifestData[] FirstEntries => EUCustomsOffices.GetElementsHaving(OfficeCodes_ICS.Codes.OfficeOfFirstEntry).ToCommonManifestData();
		public IcsOfficeCode AddOfficeOfFirstEntry(ZString data) => EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfFirstEntry, data);
		public IcsOfficeCode AddOfficeOfFirstEntry(ZString data, ZDateTime datetime)
		{
			var result = EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfFirstEntry, data);
			result.CY_Date = datetime;
			return result;
		}

		public CommonManifestData[] SubsequentEntries => EUCustomsOffices.GetElementsHaving(OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry).ToCommonManifestData();
		public IcsOfficeCode AddOfficeOfSubsequentEntry(ZString data) => EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry, data);
		public IcsOfficeCode AddOfficeOfSubsequentEntry(ZString data, ZDateTime datetime)
		{
			var result = EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry, data);
			result.CY_Date = datetime;
			return result;
		}

		public bool IsLloydsNumberMandatory => IsSea || IsInlandWaterway;

		public CommonManifestData[] ActualEntryDiversions => EUCustomsOffices.GetElementsHaving(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion).ToCommonManifestData();
		public IcsOfficeCode AddOfficeOfActualEntryDiversion(ZString data) => EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion, data);
		public IcsOfficeCode AddOfficeOfActualEntryDiversion(ZString data, ZDateTime datetime)
		{
			var result = EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion, data);
			result.CY_Date = datetime;
			return result;
		}
	}
}
