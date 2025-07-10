using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientInvoiceDelivery : AutoClientInvoiceDelivery
	{
		public ClientInvoiceDelivery(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoClientInvoiceDelivery.Schema
		{
			public const string ServerCodeForDisplay = "ServerCodeForDisplay";
			public const string DoNotBillReason = "DoNotBillReason";
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			L9_SystemCode = BillingConstants.BillingSystem.All;
		}

		#endregion

		#region Related Business Objects

		public new EDIOrgHeader InvoiceTo
		{
			get { return Factory.Load<EDIOrgHeader>(L9_OH_InvoiceTo); }
		}

		public LicenceCompany Company
		{
			get { return Factory.Load<LicenceCompany>(L9_LC); }
		}

		#endregion

		#region Properties

		public override ZGuid L9_GB_InvoicingBranch
		{
			get
			{
				return base.L9_GB_InvoicingBranch;
			}
			set
			{
				if (base.L9_GB_InvoicingBranch != value)
				{
					base.L9_GB_InvoicingBranch = value;
					UpdateTaxForNewBranch();
				}
			}
		}

		void UpdateTaxForNewBranch()
		{
			if (!L9_AT_TaxId.IsEmpty && TaxId != null)
			{
				var items = Lookups.TaxIds;
				items.Load();
				var newItem = items.Find(new ZQuery(AccTaxRateSchema.AT_Code, TaxId.AT_Code));
				L9_AT_TaxId = newItem.Length != 0 ? newItem[0].PK : ZGuid.Empty;
			}

			if (!L9_AC_SalesTaxChargeCode.IsEmpty && SalesTaxChargeCode != null)
			{
				var items = Lookups.SalesTaxChargeCodes;
				items.Load();
				var newItem = items.Find(new ZQuery(AccChargeCodeSchema.AC_Code, SalesTaxChargeCode.AC_Code));
				L9_AC_SalesTaxChargeCode = newItem.Length != 0 ? newItem[0].PK : ZGuid.Empty;
			}
		}

		[RelatedBusinessObject("Company")]
		public override ZGuid L9_LC
		{
			get { return base.L9_LC; }
			set { base.L9_LC = value; }
		}

		static bool CalcIsInvoiceToAnother(ZGuid invoiceToPk, LicenceCompany licCompany)
		{
			return !invoiceToPk.IsEmpty && licCompany != null && invoiceToPk != licCompany.LC_OH;
		}

		[List("Lookups.SystemCodes")]
		public override ZString L9_SystemCode
		{
			get { return base.L9_SystemCode; }
			set
			{
				base.L9_SystemCode = value;
			}
		}

		public ZBool IsAllSystemCode
		{
			get { return L9_SystemCode == BillingConstants.BillingSystem.All; }
		}

		public const string AllServerCodeForDisplay = "<ALL>";

		// ServerCode with blank converted to "<ALL>"
		// We don't store "ALL" in L9_ServerCode since
		// it a valid choice for an individual server code.
		[List("Lookups.ServerCodes")]
		public ZString ServerCodeForDisplay
		{
			get { return !L9_ServerCode.IsEmpty ? L9_ServerCode : new ZString(AllServerCodeForDisplay); }
			set { L9_ServerCode = (value != AllServerCodeForDisplay) ? value : ZString.Empty; }
		}

		public ZPropertyInfo ServerCodeForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ServerCodeForDisplay, x => L9_ServerCodeInfo); }
		}

		public ZBool IsAllServerCode
		{
			get { return L9_ServerCode.IsEmpty; }
		}

		public CodeDescriptionPairList GetServerCodes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(AllServerCodeForDisplay);
			if (Company != null)
			{
				foreach (LicenceDatabase db in Company.LicDatabases)
				{
					result.AddPair(db.LD_ServerCode);
				}
			}
			return result;
		}

		public EDIOrgHeader Partner
		{
			get
			{
				EDIOrgHeader org = InvoiceTo;
				return org != null
					&& org.LicCompany != null
					&& org.LicCompany.SelfBilling.L4_IsPartner
					? org
					: null;
			}
		}

		public ZBool IsInvoicedByPartner
		{
			get { return Partner != null; }
		}

		const string InvoiceToSelfText = "Self";
		const string InvoiceToAnotherText = "Other";
		const string DoNotInvoiceText = "None";

		enum InvoiceDeliveryType
		{
			InvoiceToSelf,
			InvoiceToAnother,
			DoNotInvoice
		}

		InvoiceDeliveryType InvoiceDelivery
		{
			get
			{
				if (L9_IsBilled)
				{
					return L9_OH_InvoiceTo.IsEmpty ? InvoiceDeliveryType.InvoiceToSelf : InvoiceDeliveryType.InvoiceToAnother;
				}
				else
				{
					return InvoiceDeliveryType.DoNotInvoice;
				}
			}
		}

		[MaxLength(5)]
		public ZString InvoiceDeliveryText
		{
			get
			{
				ZString result = ZString.Empty;
				switch (InvoiceDelivery)
				{
					case InvoiceDeliveryType.InvoiceToSelf: result = InvoiceToSelfText; break;
					case InvoiceDeliveryType.DoNotInvoice: result = DoNotInvoiceText; break;
					case InvoiceDeliveryType.InvoiceToAnother: result = InvoiceToAnotherText; break;
				}

				return result;
			}
		}

		public ZPropertyInfo InvoiceDeliveryTextInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceDeliveryText)); }
		}

		public ZString InvoiceToText
		{
			get { return CalcInvoiceToText(Factory, InvoiceDelivery, L9_OH_InvoiceTo); }
		}

		static ZString CalcInvoiceToText(BusinessObjectFactory factory, InvoiceDeliveryType deliveryType, ZGuid orgInvoiceToPk)
		{
			var result = ZString.Empty;
			switch (deliveryType)
			{
				case InvoiceDeliveryType.InvoiceToSelf:
					result = InvoiceToSelfText;
					break;
				case InvoiceDeliveryType.DoNotInvoice:
					result = DoNotInvoiceText;
					break;
				case InvoiceDeliveryType.InvoiceToAnother:
					result = factory.Load<EDIOrgHeader>(orgInvoiceToPk).OH_Code;
					break;
			}
			return result;
		}

		static InvoiceDeliveryType CalcInvoiceDelivery(ZBool isBilled, ZGuid orgInvoiceToPk, LicenceCompany licCompany)
		{
			InvoiceDeliveryType result;
			if (!isBilled)
			{
				result = InvoiceDeliveryType.DoNotInvoice;
			}
			else if (CalcIsInvoiceToAnother(orgInvoiceToPk, licCompany))
			{
				result = InvoiceDeliveryType.InvoiceToAnother;
			}
			else
			{
				result = InvoiceDeliveryType.InvoiceToSelf;
			}
			return result;
		}

		public ZString DoNotBillReason
		{
			get { return !L9_IsBilled ? L9_Note : ZString.Empty; }
			set { L9_Note = value; }
		}

		public ZPropertyInfo DoNotBillReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DoNotBillReason, s => L9_NoteInfo); }
		}

		public static InvoiceGroup CreateGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceHeader licHeader, ZDateTime dueDate, int renewalMonths)
		{
			return new InvoiceGroup(delivery, orgPk, licHeader, dueDate, renewalMonths);
		}

		public static InvoiceGroup CreateGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceCompany licCompany, ZDateTime dueDate, int renewalMonths)
		{
			return new InvoiceGroup(delivery, orgPk, licCompany, dueDate, renewalMonths);
		}

		public static InvoiceGroup CreateGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceCompany licCompany, ZString serverCode, bool isBillable = true)
		{
			return new InvoiceGroup(delivery, orgPk, licCompany, serverCode, isBillable);
		}

		public static InvoiceGroup CreateGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceCompany licCompany, IBilledDatabase db)
		{
			return new InvoiceGroup(delivery, orgPk, licCompany, db);
		}

		[List("Lookups.GroupByCodes")]
		public override ZString L9_GroupBy
		{
			get { return base.L9_GroupBy; }
			set { base.L9_GroupBy = value; }
		}

		public static class GroupBy
		{
			public const string All = "ALL";
			public const string Db = "DAT";
			public const string Ent = "ENT";
			public const string Lic = "LIC";
			public const string Org = "ORG";
		}

		ZGuid GetIdForGroupBy(LicenceHeader licHeader)
		{
			switch (L9_GroupBy)
			{
				case ClientInvoiceDelivery.GroupBy.Db: return licHeader.LA_LD;
				case ClientInvoiceDelivery.GroupBy.Ent: return licHeader.Company.LC_LE;
				case ClientInvoiceDelivery.GroupBy.Lic: return licHeader.PK;
				case ClientInvoiceDelivery.GroupBy.Org: return licHeader.LA_LC;
				default:
					return ZGuid.Empty;
			}
		}

		ZGuid GetIdForGroupBy(LicenceCompany licCompany)
		{
			switch (L9_GroupBy)
			{
				case ClientInvoiceDelivery.GroupBy.Db: return licCompany.PK;
				case ClientInvoiceDelivery.GroupBy.Ent: return licCompany.LC_LE;
				case ClientInvoiceDelivery.GroupBy.Lic: return licCompany.PK;
				case ClientInvoiceDelivery.GroupBy.Org: return licCompany.PK;
				default:
					return ZGuid.Empty;
			}
		}

		public ZGuid GetIdForGroupBy(LicenceCompany licCompany, ZString serverCode)
		{
			ZGuid result = ZGuid.Empty;

			if (L9_GroupBy != ClientInvoiceDelivery.GroupBy.All)
			{
				LicenceHeader licHeader = !serverCode.IsEmpty ? licCompany.LicHeadersForAllDatabases.FindById(serverCode, licCompany.LC_CompanyCode) : null;

				if (licHeader != null)
				{
					result = GetIdForGroupBy(licHeader);
				}
				else
				{
					result = GetIdForGroupBy(licCompany);
				}
			}

			return result;
		}

		ZGuid GetIdForGroupBy(LicenceCompany licCompany, IBilledDatabase db)
		{
			switch (L9_GroupBy)
			{
				case ClientInvoiceDelivery.GroupBy.Db: return db?.PK ?? licCompany.PK;
				case ClientInvoiceDelivery.GroupBy.Ent: return licCompany.LC_LE;
				case ClientInvoiceDelivery.GroupBy.Lic: return licCompany.PK;
				case ClientInvoiceDelivery.GroupBy.Org: return licCompany.PK;
				default:
					return ZGuid.Empty;
			}
		}

		public static ZGuid SafeGetIdForGroupBy(ClientInvoiceDelivery delivery, LicenceHeader licHeader)
		{
			return delivery != null ? delivery.GetIdForGroupBy(licHeader) : ZGuid.Empty;
		}

		public static ZGuid SafeGetIdForGroupBy(ClientInvoiceDelivery delivery, LicenceCompany licCompany, ZString serverCode)
		{
			return delivery != null ? delivery.GetIdForGroupBy(licCompany, serverCode) : ZGuid.Empty;
		}

		public static ZGuid SafeGetIdForGroupBy(ClientInvoiceDelivery delivery, LicenceCompany licCompany, IBilledDatabase db)
		{
			return delivery != null ? delivery.GetIdForGroupBy(licCompany, db) : ZGuid.Empty;
		}

		#endregion

		#region Calculated

		static public ZGuid CalcInvoicedOrganisationPK(ClientInvoiceDelivery invoiceDelivery, ZGuid defaultPk)
		{
			return invoiceDelivery != null ? invoiceDelivery.CalcInvoicedOrganisationPK(defaultPk) : defaultPk;
		}

		public ZGuid CalcInvoicedOrganisationPK(ZGuid defaultPk)
		{
			return L9_IsBilled && L9_OH_InvoiceTo.IsValid && !IsInvoicedByPartner ? L9_OH_InvoiceTo : defaultPk;
		}

		static public ZGuid GetInvoicingBranchPk(ClientInvoiceDelivery invoiceDelivery)
		{
			return (invoiceDelivery != null) ? invoiceDelivery.L9_GB_InvoicingBranch : ZGuid.Empty;
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			bool result = false;

			if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed || ReadOnly)
			{
				result = true;
			}
			else
			{
				switch (property.Name)
				{
					case ClientInvoiceDeliverySchema.Constants.L9_OH_InvoiceTo:
					case ClientInvoiceDeliverySchema.Constants.L9_AC_SalesTaxChargeCode:
					case ClientInvoiceDeliverySchema.Constants.L9_AT_TaxId:
					case ClientInvoiceDeliverySchema.Constants.L9_GB_InvoicingBranch:
					case ClientInvoiceDeliverySchema.Constants.L9_GroupBy:
					case ClientInvoiceDeliverySchema.Constants.L9_RX_NKInvoiceCurrency:
						result = !L9_IsBilled;
						break;

					case Schema.DoNotBillReason:
						result = L9_IsBilled;
						break;

					default:
						break;
				}
			}

			return result;
		}

		#endregion

		#region Log Changes

		public override void OnSaving()
		{
			base.OnSaving();
			LogChanges();
		}

		void LogChanges()
		{
			bool criticalFieldsHasChanges = IsInDatabase &&
				(L9_OH_InvoiceToInfo.HasChanges || L9_IsBilledInfo.HasChanges || L9_ServerCodeInfo.HasChanges || L9_SystemCodeInfo.HasChanges);

			if (criticalFieldsHasChanges && Company != null && Company.Header != null)
			{
				ZStringBuilder builder = new ZStringBuilder("InvoiceTo");

				builder.Append(" Server:");
				if (L9_ServerCodeInfo.HasChanges)
				{
					builder.Append(string.Concat(L9_ServerCodeInfo.OriginalValue, "=>"));
				}
				builder.Append(ServerCodeForDisplay);

				builder.Append(" System:");
				if (L9_SystemCodeInfo.HasChanges)
				{
					builder.Append(string.Concat(L9_SystemCodeInfo.OriginalValue, "=>"));
				}
				builder.Append(L9_SystemCode);

				if (L9_OH_InvoiceToInfo.HasChanges || L9_IsBilledInfo.HasChanges)
				{
					ZGuid originalOrgToPk = (ZGuid)L9_OH_InvoiceToInfo.OriginalValue;
					ZBool originalIsBilled = (ZBool)L9_IsBilledInfo.OriginalValue;
					ZString originalInvoiceToText = CalcInvoiceToText(Factory, CalcInvoiceDelivery(originalIsBilled, originalOrgToPk, Company), originalOrgToPk);
					builder.Append(string.Concat(" | To:", originalInvoiceToText, "=>", InvoiceToText));
				}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Company.Header.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion
	}

	public class InvoiceGroup : IEquatable<InvoiceGroup>
	{
		public InvoiceGroup(ClientInvoiceDelivery delivery, ZGuid orgPk)
		{
			BranchPK = ClientInvoiceDelivery.GetInvoicingBranchPk(delivery);
			InvoicedOrganisationPK = orgPk;
			Currency = delivery != null ? delivery.L9_RX_NKInvoiceCurrency : ZString.Empty;
			IsBilled = delivery != null ? delivery.L9_IsBilled : ZBool.False;
		}

		public InvoiceGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceHeader licHeader, ZDateTime dueDate, int renewalMonths)
			: this(delivery, orgPk)
		{
			IdForGroupBy = ClientInvoiceDelivery.SafeGetIdForGroupBy(delivery, licHeader);
			DueDate = dueDate;
			RenewalMonths = renewalMonths;
		}

		public InvoiceGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceCompany licCompany, ZDateTime dueDate, int renewalMonths)
			: this(delivery, orgPk)
		{
			IdForGroupBy = ClientInvoiceDelivery.SafeGetIdForGroupBy(delivery, licCompany, ZString.Empty);
			DueDate = dueDate;
			RenewalMonths = renewalMonths;
		}

		public InvoiceGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceCompany licCompany, ZString serverCode, bool isBillable = true)
			: this(delivery, orgPk)
		{
			IdForGroupBy = ClientInvoiceDelivery.SafeGetIdForGroupBy(delivery, licCompany, serverCode);
			if (!isBillable)
			{
				IsBilled = false;
			}
		}

		public InvoiceGroup(ClientInvoiceDelivery delivery, ZGuid orgPk, LicenceCompany licCompany, IBilledDatabase db)
			: this(delivery, orgPk)
		{
			IdForGroupBy = ClientInvoiceDelivery.SafeGetIdForGroupBy(delivery, licCompany, db);
			if (db != null && !db.IsBillable)
			{
				IsBilled = false;
			}
		}

		public ZGuid BranchPK;
		public ZGuid InvoicedOrganisationPK;
		public ZGuid IdForGroupBy;
		public ZString Currency;
		public ZBool IsBilled;
		public ZDateTime DueDate;
		public int RenewalMonths;

		public override bool Equals(object obj)
		{
			return Equals((InvoiceGroup)obj);
		}

		public bool Equals(InvoiceGroup other)
		{
			return BranchPK == other.BranchPK
				&& InvoicedOrganisationPK == other.InvoicedOrganisationPK
				&& IdForGroupBy == other.IdForGroupBy
				&& Currency == other.Currency
				&& IsBilled == other.IsBilled
				&& DueDate == other.DueDate
				&& RenewalMonths == other.RenewalMonths;
		}

		public override int GetHashCode()
		{
			return BranchPK.GetHashCode()
				^ InvoicedOrganisationPK.GetHashCode()
				^ IdForGroupBy.GetHashCode()
				^ Currency.GetHashCode()
				^ IsBilled.GetHashCode()
				^ DueDate.GetHashCode()
				^ RenewalMonths.GetHashCode();
		}
	}
}

