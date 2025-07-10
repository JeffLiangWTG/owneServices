using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class KoreaSouthEInvoicingDSBSummaryAppendingRule : RegistryBusinessObjectTemplate
	{
		public KoreaSouthEInvoicingDSBSummaryAppendingRule()
		{
		}

		public KoreaSouthEInvoicingDSBSummaryAppendingRule(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			this.CurrentFallbackLevel = this.CurrentFallbackLevel ?? clone.CurrentFallbackLevel;
		}

		#region Schema

		public abstract class Schema
		{
			public const string TaxIdPK = "TaxIdPK";
			public const string PostingGroup = "PostingGroup";
			public const string Order = "Order";
		}

		#endregion

		KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRule(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateOrder();
			ValidateTaxId();
			base.RunPreSaveValidationCore();
		}

		void ValidateTaxId()
		{
			ValidateValidTaxId();
			ValidateDuplicateTaxId();
		}

		void ValidateValidTaxId()
		{
			var errorMessage = Res.GetString("6A935A77-1014-4934-978E-6A4A27D95F98", "Enter a valid selection.");

			CleanAndAddErrorIfNecessary(errorMessage, CheckValidTaxId);

			bool CheckValidTaxId()
			{
				return !TaxIdPK.IsValid || TaxId == null || !CanReloadWithFilter();
			}
		}

		bool CanReloadWithFilter()
		{
			var newFactory = new BusinessObjectFactory();
			var filter = TaxIds.CompleteFilter;
			filter.AddToFilter(AccTaxRateSchema.PK, TaxIdPK);

			return newFactory.LoadTop1(TaxId.GetType(), filter) != null;
		}

		void ValidateDuplicateTaxId()
		{
			var errorMessage = Res.GetString("60E4B3A0-D2E1-4CB8-AA4E-D23EE617EF9D", "Duplicated Tax ID is not allowed.");

			CleanAndAddErrorIfNecessary(errorMessage, CheckDuplicatedTaxId);

			bool CheckDuplicatedTaxId()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<KoreaSouthEInvoicingDSBSummaryAppendingRule>()
					.Except(new[] { this })
					.Any(item => item.TaxIdPK != ZGuid.Empty && item.TaxIdPK == TaxIdPK);
			}
		}

		void ValidateOrder()
		{
			OrderInfo.ClearAllNotifications();
			CompareValidation.CheckNumberNotNegative(OrderInfo);

			ValidateDuplicatedOrder();
			ValidateExceededOrder();
			ValidatePostGourpWithInconsistentOrder();
		}

		void ValidateDuplicatedOrder()
		{
			var errorMessage = Res.GetString("84AD45B4-21A9-4800-A984-1E557F63AE74", "Duplicated Order Number is not allowed.");

			CleanAndAddErrorIfNecessary(errorMessage, CheckDuplicatedOrder);

			bool CheckDuplicatedOrder()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<KoreaSouthEInvoicingDSBSummaryAppendingRule>()
					.Except(new[] { this })
					.Any(item => item.Order != ZInt.Zero && item.PostingGroup != PostingGroup && item.Order == Order);
			}
		}

		void ValidatePostGourpWithInconsistentOrder()
		{
			var errorMessage = Res.GetString("E4B8EB7C-4C4D-455A-A017-2A9498D1B504", "The Tax ID with the same posting group number must correspond to the same Order Number.");

			CleanAndAddErrorIfNecessary(errorMessage, CheckPostGourpWithInconsistentOrder);

			bool CheckPostGourpWithInconsistentOrder()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<KoreaSouthEInvoicingDSBSummaryAppendingRule>()
					.Except(new[] { this })
					.Any(item => item.PostingGroup == PostingGroup && item.Order != Order);
			}
		}

		void ValidateExceededOrder()
		{
			var errorMessage = GetExceededOrderErrorMessage(Order);

			CleanAndAddErrorIfNecessary(errorMessage, CheckExceededOrder);

			bool CheckExceededOrder()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return Order != ZInt.Zero && Order > ParentCollection.Count;
			}
		}
		string GetExceededOrderErrorMessage(ZInt number) => Res.GetString("6EF56F19-A8E9-4B50-B09B-5E77C45A853A", "Number '{0}' is exceeded the total number of items.", number);

		void CleanAndAddErrorIfNecessary(string errorMessage, Func<bool> validate)
		{
			if (validate())
			{
				AddRowError(errorMessage);
			}
			else
			{
				ClearRowNotificationsContaining(errorMessage);
			}
		}

		#region Properties

		#region PostingGroup

		[ResourceStringData("KoreaSouthEInvoicingDSBSummaryAppendingRule|PostingGroup", Caption = "Posting Group")]
		public ZShort PostingGroup => TaxId?.AT_PostingGroupId ?? ZShort.Zero;

		public ZPropertyInfo PostingGroupInfo
		{
			get { return GetZPropertyInfo(Schema.PostingGroup); }
		}

		#endregion

		#region TaxIdPK

		[List("TaxIds")]
		[ResourceStringData("KoreaSouthEInvoicingDSBSummaryAppendingRule|TaxId", Caption = "Tax Id")]
		public ZGuid TaxIdPK
		{
			get { return taxIdPK; }
			set
			{
				SetNonPersistentPropertyValue(TaxIdPKInfo, ref taxIdPK, value);
			}
		}
		ZGuid taxIdPK;

		public ZPropertyInfo TaxIdPKInfo
		{
			get { return GetZPropertyInfo(Schema.TaxIdPK); }
		}

		public AccTaxRate TaxId
		{
			get { return CurrentFactory.Load<AccTaxRate>(TaxIdPK); }
		}

		public AccTaxRateCollection TaxIds
		{
			get
			{
				if (taxIds == null)
				{
					var filter = GetZQuery();
					var company = CurrentFactory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, CurrentFallbackLevel?.CompanyPK(false) ?? GlbCompany.CurrentCompany.PK));
					taxIds = new AccTaxRateCollection(CurrentFactory, filter, company);
				}

				return taxIds;
			}
		}
		AccTaxRateCollection taxIds;

		internal static class ExludeTaxCodes
		{
			internal const string EXCLUDE = "EXCLUDE";
			internal const string NOTREPORT = "NOTREPORT";
		}

		ZQuery GetZQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.And, AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_Code, SQLComparisonOperator.NotEqual, ExludeTaxCodes.EXCLUDE);
			query.AddToFilter(AccTaxRateSchema.AT_Code, SQLComparisonOperator.NotEqual, ExludeTaxCodes.NOTREPORT);
			return query;
		}

		#endregion

		#region Order

		[MaxLength(1024)]
		[ResourceStringData("KoreaSouthEInvoicingDSBSummaryAppendingRule|Order", Caption = "Order")]
		public ZInt Order
		{
			get { return order; }
			set
			{
				if (order != value)
				{
					var errorMessage = GetExceededOrderErrorMessage(order);
					SetNonPersistentPropertyValue(OrderInfo, ref order, value);
					ClearRowNotificationsContaining(errorMessage);
				}
			}
		}
		ZInt order;

		public ZPropertyInfo OrderInfo
		{
			get { return GetZPropertyInfo(Schema.Order); }
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.TaxIdPK, TaxIdPK.ToString());
			writer.WriteElementString(Schema.Order, Order.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TaxIdPK = new ZGuid(reader.ReadElementString(Schema.TaxIdPK));
			Order = reader.ReadElementStringAsZInt(Schema.Order);
		}

		#endregion
	}
}
