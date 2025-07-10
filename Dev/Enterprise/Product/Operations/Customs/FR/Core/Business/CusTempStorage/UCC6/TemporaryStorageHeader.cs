using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using CusGoodsLocation = Enterprise.Customs.FR.Business.Declaration.CusGoodsLocation;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStorageHeader : EU.Business.CusTempStorage.TemporaryStorageHeader, Integration.Customs.FR.ITemporaryStorageHeader, ICorrelationIDProvider, IBillGenerationSupport, ICusGoodsLocationProvider
	{
		public TemporaryStorageHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static TemporaryStorageHeader New(BusinessObjectFactory factory, string manifestType = FRConstants.TemporaryStorage.AppCodeIST)
		{
			var header = factory.New<TemporaryStorageHeader>();
			header.AMA_ManifestType = manifestType;
			return header;
		}

		protected override AsycudaManifestHeaderValidation GetNewValidation() => new FRTemporaryStorageHeaderValidation(this);

		public new FRTemporaryStorageHeaderValidation Validation => (FRTemporaryStorageHeaderValidation)base.Validation;

		[ChildEditable(true)]
		public new FREDIMessageCollection Messages => (FREDIMessageCollection)base.Messages;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : AsycudaManifestHeader.Schema
		{
			public const string EntryNumber = "EntryNumber";
			public const string CorrelationID = "CorrelationID";
			public const int CorrelationMaxLength = 10;
		}

		#region CorrelationID

		[ReadOnly(true)]
		[MaxLength(Schema.CorrelationMaxLength)]
		public ZString CorrelationID
		{
			get { return CorrelationIDEntryNumber.CE_EntryNum; }
			set { CorrelationIDEntryNumber.CE_EntryNum = value; }
		}

		public ZPropertyInfo CorrelationIDInfo { get { return GetWrappedZPropertyInfo(Schema.CorrelationID, x => CorrelationIDEntryNumber.CE_EntryNumInfo); } }

		public CusEntryNumber CorrelationIDEntryNumber
		{
			get
			{
				if (correlationIDEntryNumber == null)
				{
					correlationIDEntryNumber = new CachedProperty<CusEntryNumber>(Factory, delegate
					{
						var correlationEntryNumberInternal = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.CorrelationIdentifier, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
						if (correlationEntryNumberInternal == null)
						{
							correlationEntryNumberInternal = CusEntryNumber.New(this, CusEntryNumberTypes.EU.CorrelationIdentifier, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
							correlationEntryNumberInternal.CE_EntryIsSystemGenerated = true;
							correlationEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
						}
						return correlationEntryNumberInternal;
					});
				}
				return correlationIDEntryNumber.Value;
			}
		}
		CachedProperty<CusEntryNumber> correlationIDEntryNumber;

		public CorrelationIDGenerator CorrelationIdGenerator => correlationIdGenerator ?? (correlationIdGenerator = new CorrelationIDGenerator(this, this));
		CorrelationIDGenerator correlationIdGenerator;

		bool IsUniqueCorrelationID(ZString correlationID)
		{
			return NumberGeneratorHelper.IsUniqueEntryNumber(Factory, TableName, correlationID, CusEntryNumberTypes.EU.CorrelationIdentifier, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public ZString CorrelationIDPrefix => ZString.Empty;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			CorrelationIdGenerator.InitCorrelationID(IsUniqueCorrelationID);
		}

		protected override EDIMessageCollection CreateNewEDIMessageCollection()
		{
			return new FREDIMessageCollection(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeIST;
		}

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return new TemporaryStorageHeaderValueSetStrategy(this);
		}

		protected override void DefaultAuthorizationProperties(object sender, EventArgs e)
		{
			if (sender is CusGoodsLocation cusGoodsLocation && cusGoodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace)
			{ }
			else
			{
				AuthorizationType = ZString.Empty;
				AuthorizationOwner = ZGuid.Empty;
				AuthorizationNumber = ZString.Empty;
			}
		}

		#region  IBillGenerationSupport

		BusinessObjectFactory IBillGenerationSupport.Factory => Factory;

		OrgHeader IBillGenerationSupport.CarrierPrincipal => null;

		ZString IBillGenerationSupport.TransportMode => ZString.Empty;

		ZString IBillGenerationSupport.ServiceLevel => ZString.Empty;

		RefUNLOCO IBillGenerationSupport.Origin => null;

		RefUNLOCO IBillGenerationSupport.Destination => null;

		RefUNLOCO IBillGenerationSupport.Load => null;

		RefUNLOCO IBillGenerationSupport.Discharge => null;

		ZString IBillGenerationSupport.TranshipmentIndicator => ZString.Empty;

		#endregion

		protected override Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

		ZString ICusGoodsLocationProvider.ProviderKey => "FRPNTS";

		public new TemporaryStorageBill MasterBill => (TemporaryStorageBill)base.MasterBill;

		[ChildEditable]
		public new ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader> Bills => (ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>)base.Bills;

		public new IEnumerable<TemporaryStorageBill> HouseBills => base.HouseBills.Cast<TemporaryStorageBill>();

		protected override ITemporaryStorageBillCollection<EU.Business.CusTempStorage.TemporaryStorageBill, EU.Business.CusTempStorage.TemporaryStorageHeader> CreateNewTemporaryStorageBillCollection()
		{
			return new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(this);
		}

		protected override Type GetBillTypeCore() => typeof(TemporaryStorageBill);
	}
}
