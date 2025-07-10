using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogDownloadObject : NonPersistentBusinessObject, IMessageSendingObject
	{
		public GoodsCatalogDownloadObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string OwnerCode = nameof(OwnerCode);
			public const string BrokerCode = nameof(BrokerCode);
			public const string DownloadCatalog = nameof(DownloadCatalog);
			public const string DownloadForeignOperator = nameof(DownloadForeignOperator);
			public const string DownloadDeactivated = nameof(DownloadDeactivated);
		}

		#region OwnerCode

		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(GoodsCatalogDownloadObjectLookups.ConsigneeOrConsignorList))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject|OwnerCode", Caption = "Owner")]
		public ZString OwnerCode
		{
			get { return ownerCode; }
			set
			{
				SetNonPersistentPropertyValue(OwnerCodeInfo, ref ownerCode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOwnerCode();
				}
			}
		}

		ZString ownerCode;

		public ZPropertyInfo OwnerCodeInfo => GetZPropertyInfo(Schema.OwnerCode);

		public ZString OwnerRootCNPJ => Owner.GetRootCNPJ();

		public OrgHeader Owner => OrgHeader.LoadFromCode(Factory, OwnerCode);

		#endregion

		#region DownloadCatalog

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject|DownloadCatalog", Caption = "Download Catalog", FullDescription = "Check this box to download all active Catalogs.")]
		public ZBool DownloadCatalog
		{
			get { return downloadCatalog; }
			set
			{
				var oldValue = DownloadCatalog;
				SetNonPersistentPropertyValue(DownloadCatalogInfo, ref downloadCatalog, value);

				if (!IsCopying && oldValue != DownloadCatalog)
				{
					DownloadDeactivated = false;
					DownloadForeignOperator = true;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateDownloadCatalog();
				}
			}
		}

		ZBool downloadCatalog;

		public ZPropertyInfo DownloadCatalogInfo => GetZPropertyInfo(Schema.DownloadCatalog);

		#endregion

		#region DownloadForeignOperator

		[ReadOnlyMember(nameof(DownloadCatalog))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject|DownloadForeignOperator", Caption = "Download Foreign Operator", FullDescription = "Check this box to download all Foreign Operators.")]
		public ZBool DownloadForeignOperator
		{
			get { return downloadForeignOperator; }
			set
			{
				SetNonPersistentPropertyValue(DownloadForeignOperatorInfo, ref downloadForeignOperator, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDownloadForeignOperator();
				}
			}
		}

		ZBool downloadForeignOperator;

		public ZPropertyInfo DownloadForeignOperatorInfo => GetZPropertyInfo(Schema.DownloadForeignOperator);

		#endregion

		#region DownloadDeactivated

		[ReadOnlyMember(nameof(DownloadDeactivated_ReadOnly))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject|DownloadDeactivated", Caption = "Download Deactivated", FullDescription = "Check this box to also download all inactive Catalogs.")]
		public ZBool DownloadDeactivated
		{
			get { return downloadDeactivated; }
			set { SetNonPersistentPropertyValue(DownloadDeactivatedInfo, ref downloadDeactivated, value); }
		}

		ZBool downloadDeactivated;

		public ZPropertyInfo DownloadDeactivatedInfo => GetZPropertyInfo(Schema.DownloadDeactivated);

		ZBool DownloadDeactivated_ReadOnly => !DownloadCatalog;

		#endregion

		#region BrokerCode

		[List(nameof(Lookups) + "." + nameof(GoodsCatalogDownloadObjectLookups.BrokerList))]
		[MaxLength(GlbStaff.Schema.GS_CodeMaxLength)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject|BrokerCode", Caption = "Broker")]
		public ZString BrokerCode
		{
			get { return fBrokerCode; }
			set
			{
				SetNonPersistentPropertyValue(BrokerCodeInfo, ref fBrokerCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBrokerCode();
				}
			}
		}
		ZString fBrokerCode;

		public ZPropertyInfo BrokerCodeInfo => GetZPropertyInfo(Schema.BrokerCode);

		public GlbStaff Broker => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, fBrokerCode);

		public GlbExternalPassword_CCT BrokerCertificate => BRGlbStaffWrapper.Get(Broker)?.GetCCTPassword();

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#region IMessageSendingObject

		public ZString MessageType { get; set; }

		public BusinessObject MessageAttachee => Owner;

		public ZString GetMessageOwner() => ZString.Empty;

		public ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.CAT;

		public ZString GetApplicationReference() => MessageType == EDIMessageSubTypeList.Codes.ManufacturerZipFile ? OwnerRootCNPJ : $"{OwnerRootCNPJ}|{(bool)DownloadDeactivated}".ToLower();

		public ZGuid GetGlbExternalPasswordPK() => BrokerCertificate?.PK ?? ZGuid.Empty;

		public ZString GetMessageText() => ZString.Empty;

		#endregion

		public GoodsCatalogDownloadObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new GoodsCatalogDownloadObjectLookups(this);
				}
				return fLookups;
			}
		}

		GoodsCatalogDownloadObjectLookups fLookups;

		public GoodsCatalogDownloadObjectValidation Validation => new GoodsCatalogDownloadObjectValidation(this);
	}
}
