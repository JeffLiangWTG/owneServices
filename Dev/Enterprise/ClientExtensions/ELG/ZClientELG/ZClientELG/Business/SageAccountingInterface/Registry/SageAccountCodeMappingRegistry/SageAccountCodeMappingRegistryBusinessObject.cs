using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.ELG
{
	[XmlSerializerAssembly("ZClientELG.XmlSerializers")]
	public class SageAccountCodeMappingRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public SageAccountCodeMappingRegistryBusinessObject()
			: base()
		{
		}

		public SageAccountCodeMappingRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string OrgHeaderPK = "OrgHeaderPK";
			public const string RefCurrencyPK = "RefCurrencyPK";
			public const string LedgerType = "LedgerType";
			public const string SageAccountCode = "SageAccountCode";
		}
		#endregion

		#region OrgHeaderPK
		public ZGuid OrgHeaderPK
		{
			get { return ordHeaderPK; }
			set
			{
				SetNonPersistentPropertyValue(OrgHeaderPKInfo, ref ordHeaderPK, value);
				if (!IsValidationSuspended)
				{
					ValidateOrgHeaderPK();
				}
				OrgHeaderPKInfo.RefreshBinding();
			}
		}
		ZGuid ordHeaderPK;

		public ZPropertyInfo OrgHeaderPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrgHeaderPK, "OrgHeader"); }
		}

		public void ValidateOrgHeaderPK()
		{
			OrgHeaderPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrgHeaderPKInfo, "Organisation");
			if (!OrgHeaderPKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(OrgHeaderPKInfo, Organisations);
			}
			if (!OrgHeaderPKInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(OrgHeaderPKInfo, RefCurrencyPKInfo, LedgerTypeInfo);
			}
		}
		#endregion

		#region RefCurrencyPK
		public ZGuid RefCurrencyPK
		{
			get { return refCurrencyPK; }
			set
			{
				SetNonPersistentPropertyValue(RefCurrencyPKInfo, ref refCurrencyPK, value);
				if (!IsValidationSuspended)
				{
					ValidateRefCurrencyPK();
				}
				RefCurrencyPKInfo.RefreshBinding();
			}
		}
		ZGuid refCurrencyPK;

		public ZPropertyInfo RefCurrencyPKInfo
		{
			get { return GetZPropertyInfo(Schema.RefCurrencyPK, "RefCurrency"); }
		}

		public void ValidateRefCurrencyPK()
		{
			RefCurrencyPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RefCurrencyPKInfo, "Currency");
			if (!RefCurrencyPKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(RefCurrencyPKInfo, Currencies);
			}
			if (!RefCurrencyPKInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(OrgHeaderPKInfo, RefCurrencyPKInfo, LedgerTypeInfo);
			}
		}
		#endregion

		#region LedgerType
		[MaxLength(2)]
		public ZString LedgerType
		{
			get { return ledgerType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(LedgerTypeInfo, ref ledgerType, value);
				if (!IsValidationSuspended)
				{
					ValidateLedgerType();
				}
				LedgerTypeInfo.RefreshBinding();
			}
		}
		ZString ledgerType;

		public ZPropertyInfo LedgerTypeInfo
		{
			get { return GetZPropertyInfo(Schema.LedgerType); }
		}

		void ValidateLedgerType()
		{
			LedgerTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LedgerTypeInfo);
			if (!LedgerTypeInfo.HasErrors() && !LedgerTypes.ContainsCode(LedgerType))
			{
				LedgerTypeInfo.AddError(ErrorInvalidSageLedgerType);
			}
			if (!RefCurrencyPKInfo.HasErrors())
			{
				UniquePropertiesValidation.CheckPropertiesAreUniqueInCollection(OrgHeaderPKInfo, RefCurrencyPKInfo, LedgerTypeInfo);
			}
		}
		#endregion

		#region SageAccountCode
		public ZString SageAccountCode
		{
			get { return sageAccountCode; }
			set
			{
				CheckMaximumLength(SageAccountCodeInfo, value);
				SetNonPersistentPropertyValue(SageAccountCodeInfo, ref sageAccountCode, value);
				if (!IsValidationSuspended)
				{
					ValidateSageAccountCode();
				}
				SageAccountCodeInfo.RefreshBinding();
			}
		}
		ZString sageAccountCode;

		public ZPropertyInfo SageAccountCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SageAccountCode); }
		}

		public int SageAccountCode_MaxLength
		{
			get { return SagInvoiceHeaderDataRow.Schema.AccountNumber.Length; }
		}

		void ValidateSageAccountCode()
		{
			SageAccountCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SageAccountCodeInfo);
		}
		#endregion

		#region Lookups
		public RefCurrency CurrentRefCurrency
		{
			get { return CurrentFactory.Load<RefCurrency>(RefCurrencyPK); }
		}

		public RefCurrencyCollection Currencies
		{
			get { return currencies ?? (currencies = new RefCurrencyCollection(CurrentFactory)); }
		}
		RefCurrencyCollection currencies;

		public OrgHeader CurrentOrgHeader
		{
			get { return CurrentFactory.Load<OrgHeader>(OrgHeaderPK); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(CurrentFactory)); }
		}
		OrgHeaderCollection organisations;

		public SageLedgerTypes LedgerTypes
		{
			get { return ledgerTypes ?? (ledgerTypes = new SageLedgerTypes()); }
		}
		SageLedgerTypes ledgerTypes;
		#endregion

		#region Write/Read XML
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrgHeaderPK, OrgHeaderPK.ToString());
			writer.WriteElementString(Schema.RefCurrencyPK, RefCurrencyPK.ToString());
			writer.WriteElementString(Schema.LedgerType, LedgerType);
			writer.WriteElementString(Schema.SageAccountCode, SageAccountCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrgHeaderPK = new ZGuid(reader.ReadElementString(Schema.OrgHeaderPK));
			RefCurrencyPK = new ZGuid(reader.ReadElementString(Schema.RefCurrencyPK));
			LedgerType = reader.ReadElementString(Schema.LedgerType);
			SageAccountCode = reader.ReadElementString(Schema.SageAccountCode);
		}
		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SageAccountCodeMappingRegistryBusinessObject(factory);
		}

		public const string ErrorInvalidSageLedgerType = "Please select a valid Sage Ledger Type.";
	}
}
