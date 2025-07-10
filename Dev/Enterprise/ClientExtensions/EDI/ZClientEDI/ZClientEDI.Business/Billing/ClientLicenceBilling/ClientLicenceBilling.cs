using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class ClientLicenceBilling : AutoClientLicenceBilling
	{
		public ClientLicenceBilling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoClientLicenceBilling.Schema
		{
			public const string PartnerEmail = "PartnerEmail";
		}

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			BillingDiscounts.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Related Business Objects

		public LicenceCompany Company
		{
			get { return Factory.Load<LicenceCompany>(L4_LC); }
		}

		[ChildEditable]
		public ClientLicenceBillingDiscountCollection BillingDiscounts
		{
			get
			{
				if (billingDiscounts == null)
				{
					billingDiscounts = new ClientLicenceBillingDiscountCollection(this);
					RegisterEditableChildObject(billingDiscounts);

					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						billingDiscounts.SetReadOnlyIncludingChildren(true);
					}
				}
				return billingDiscounts;
			}
		}
		ClientLicenceBillingDiscountCollection billingDiscounts;

		#endregion

		#region Properties

		[RelatedBusinessObject("Company")]
		public override ZGuid L4_LC
		{
			get { return base.L4_LC; }
			set { base.L4_LC = value; }
		}

		[List("Lookups.ProcessingFees")]
		public override ZString L4_ProcessingFee
		{
			get { return base.L4_ProcessingFee; }
			set
			{
				base.L4_ProcessingFee = value;
			}
		}

		public ZString L4_ProcessingFeePercentLabel
		{
			get
			{
				ZString label;
				if (!L4_ProcessingFeePercentInfo.HasErrors())
				{
					label = string.Format(CultureInfo.CurrentCulture, "This is a {0}% ", Math.Abs(L4_ProcessingFeePercent));
					if (L4_ProcessingFeePercent == 0.0m)
					{
						label = "No Fee or Discount";
					}
					else if (EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value.GetBoolFromCode(L4_ProcessingFee))
					{
						label += "Discount";
					}
					else
					{
						label += "Fee";
					}
				}
				else
				{
					label = ZString.Empty;
				}

				return label;
			}
		}

		[MaxLength(Schema.L4_NoteMaxLength)]
		public ZString PartnerEmail
		{
			get { return L4_IsPartner ? L4_Note : ZString.Empty; }
			set
			{
				L4_Note = value;
				L4_IsPartner = true;
				PartnerEmailInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PartnerEmailInfo
		{
			get { return GetZPropertyInfo(Schema.PartnerEmail); }
		}

		#endregion

		#region Calculated Properties and Methods

		public ZDecimal CalculateProcessingFeeAmount(ZDecimal amount)
		{
			ZDecimal result = 0m;

			result = amount * L4_ProcessingFeePercent / 100.0m;

			if (EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value.GetBoolFromCode(L4_ProcessingFee))
			{
				result = result * -1.0m;
			}

			if (result != 0m)
			{
				result = Utilities.Round(result, BillingConstants.RoundingDecimals);
			}

			return result;
		}

		public ZDecimal SignedProcessingFeePercent
		{
			get
			{
				ZDecimal result = L4_ProcessingFeePercent;

				if (result != 0m && EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value.GetBoolFromCode(L4_ProcessingFee))
				{
					result = result * -1.0m;
				}

				return result;
			}
		}

		public ARInvoice LastOdplMonthlyInvoice
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ARInvoice));
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, Company.LC_OH);
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);

				ZDBOnlySubQuery linesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				var chargeSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccTransactionLinesSchema.AL_AC);
				chargeSubQuery.AddToFilter(AccChargeCodeSchema.AC_Code, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
				linesSubQuery.AddSubQuery(chargeSubQuery, JoinCondition.And);

				query.AddSubQuery(AccTransactionHeaderSchema.PK, linesSubQuery, JoinCondition.And);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				query.OrderBy = AccTransactionHeaderSchema.Constants.AH_PostDate + " DESC";

				return Factory.LoadTop1<ARInvoice>(query);
			}
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
					case ClientLicenceBillingSchema.Constants.L4_IsPartner:
						result = false;
						break;

					case Schema.PartnerEmail:
						result = !L4_IsPartner;
						break;

					case ClientLicenceBillingSchema.Constants.L4_InvoiceComment:
					case ClientLicenceBillingSchema.Constants.L4_ProcessingFee:
						result = L4_IsPartner;
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
			BillingInvoicingHelper.SynchronizeChild(Company, this, ClientLicenceBillingSchema.L4_LC);
			LogChanges();
		}

		void LogChanges()
		{
			bool criticalFieldsHasChanges = IsInDatabase &&
				(L4_ProcessingFeeInfo.HasChanges);

			if (criticalFieldsHasChanges && Company != null && Company.Header != null)
			{
				ZStringBuilder builder = new ZStringBuilder("InvoicingSetting");

				if (L4_ProcessingFeeInfo.HasChanges)
				{
					builder.Append(string.Concat(" | Pr.Fee:", (ZString)L4_ProcessingFeeInfo.OriginalValue, "=>", L4_ProcessingFee));
				}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Company.Header.Logs.AddNew(Events.EditedARecord, builder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region Misc.

		public ZBool IsProcessingFeeValid => Lookups.ProcessingFees.ContainsCode(L4_ProcessingFee) && L4_ProcessingFeePercent != 0m;
		public ZBool IsProcessingFeeADiscount => Lookups.ProcessingFees.GetBoolFromCode(L4_ProcessingFee);
		public MultilingualString ProcessingFeeDescription => ((CodeDescriptionBool)Lookups.ProcessingFees.FindByCode(L4_ProcessingFee))?.Description;

		#endregion

		#region TranslatableDataField

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.L4_InvoiceComment, Schema.L4_InvoiceCommentMaxLength, Schema.L4_InvoiceComment, Type = typeof(ClientLicenceBilling), Asmid = ResString.AssemblyId)]
		public override ZString L4_InvoiceComment { get => base.L4_InvoiceComment; set => base.L4_InvoiceComment = value; }

		public MultilingualString L4_InvoiceCommentMultilingual => GetMultilingual(L4_InvoiceCommentInfo);

		#endregion
	}
}

