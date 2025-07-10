using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.MXManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string LastForeignPort = "LastForeignPort";
			public const int LastForeignPortMaxLength = 10;
			public const int AMA_VesselMaxLength = 28;
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Mexico;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		protected override MessageChooser GetNewMessageChooserCore(IEnumerable<ISelectionItem> items, string messageSubType, bool showStatus)
		{
			return (messageSubType != ZString.Empty)
				? new MXMessageChooser(this, items, messageSubType)
				: base.GetNewMessageChooserCore(items, messageSubType, showStatus);
		}

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

		#region LastForeignPort

		[MaxLength(Schema.LastForeignPortMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsLoadingPortList))]
		[ResourceStringData("AsycudaManifestHeader.LastForeignPort", Caption = "Last Foreign Port")]
		public ZString LastForeignPort
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.LastForeignPort);
			set
			{
				var oldValue = LastForeignPort;
				CheckMaximumLength(LastForeignPortInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.LastForeignPort, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateLastForeignPort();
				}
				LastForeignPortInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LastForeignPortInfo => GetZPropertyInfo(Schema.LastForeignPort);

		#endregion

		public override IEnumerable<RefLocoMap> GetCustomsLocalCodeList(RefUNLOCO port)
		{
			var result = new List<RefLocoMap>();
			if (port != null)
			{
				var portCode = port.RL_Code;
				var portMaps = port.RefLocoMaps.Where(x => x.RY_LocalPortCode != ZString.Empty);

				return Country != null && portCode.SubstringSafe(0, 2) == Country.Code
					? portMaps.Where(x => x.RY_SystemUsage == LocoMapSystemUsageList.Codes.CustomsPortCodeList && x.RY_RN == Core.Constants.CountryGuids.Mexico).OrderBy(x => x.RY_IsSystem)
					: portMaps.Where(x => x.RY_SystemUsage == USLocoMapSystemUsageList.Codes.SCK && x.RY_RN == Core.Constants.CountryGuids.UnitedStates).OrderBy(x => x.RY_IsSystem);
			}
			return result;
		}

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (oldValue != AMA_TransportMode)
				{
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(Schema.AMA_VesselMaxLength)]
		public override ZString AMA_VesselName { get => base.AMA_VesselName; set => base.AMA_VesselName = value; }

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = MXManifestTypes.Codes.MAN;
		}
#endif
	}
}
