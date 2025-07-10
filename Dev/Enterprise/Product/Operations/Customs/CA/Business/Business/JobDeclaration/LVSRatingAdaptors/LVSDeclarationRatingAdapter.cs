using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LVSDeclarationRatingAdapter : RatingAdapter, IAutoRatingCustomsInfo, IAutoRatingFreightConditionsSupportable
	{
		public LVSDeclarationRatingAdapter(OrgHeader importer, IEnumerable<JobComInvoiceHeader> headerGroup, ZString incoTerms, ZString province, JobDeclaration declaration)
		{
			this.headerGroup = headerGroup;
			this.importer = importer;
			this.incoTerms = incoTerms;
			this.province = province;
			this.declaration = declaration;
			lvsIDs = ZString.Join(", ", headerGroup.Where(x => !x.JZ_InvoiceNumber.IsEmpty).Select(x => x.JZ_InvoiceNumber).OrderBy(x => x).ToArray());
			isDetailRequired = importer != null && OrgImpAddInfo.Get(importer).ZO_EffectiveLVSInvoiceDetailCode == LVSInvoiceDetailCodes.Codes.Detail;
		}
		readonly IEnumerable<JobComInvoiceHeader> headerGroup;
		readonly OrgHeader importer;
		readonly ZString incoTerms;
		readonly ZString province;
		readonly JobDeclaration declaration;
		readonly ZString lvsIDs;
		readonly bool isDetailRequired;

		internal static bool IsCollect(string incoTerms, string chargeGroup)
		{
			var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(chargeGroup, incoTerms);
			return string.IsNullOrEmpty(prepaidCollect) || prepaidCollect == Enterprise.Core.Constants.PaymentType.Collect;
		}

		#region RatingAdapter

		public override AdapterType AdapterType => AdapterType.LVSDeclaration;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return declaration != null ? declaration.InvoicingSupporter : null; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return declaration.RatingAdapter.JobDatesProvider; }
		}

		public override Collection<IBusiness> AutoRatedFor
		{
			get { return new Collection<IBusiness> { declaration, importer }; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[RatingDebtorOrgTypes.CNR] = importer;

				return result;
			}
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return declaration.RatingAdapter.ChargeCodeGroups; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return declaration.RatingAdapter.ConsumerType; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return declaration.RatingAdapter.MergeCharges; }
		}

		public override RateType RateTypeToUse
		{
			get { return declaration.RatingAdapter.RateTypeToUse; }
		}

		public override JobServicesCollection JobServices
		{
			get { return declaration.RatingAdapter.JobServices; }
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get { return declaration.RatingAdapter.StatusInformation; }
		}

		ZString GetExtraReference(AccChargeCode chargeCode)
		{
			ZStringBuilder extraReference;
			if (isDetailRequired)
			{
				extraReference = new ZStringBuilder(ZString.Format("{0} - {1}", declaration.DeclarationNumber, lvsIDs));
			}
			else
			{
				extraReference = new ZStringBuilder(declaration.DeclarationNumber);
				extraReference.Append(Res.GetString("f2f02e32-8cee-49f0-bad1-38a293f0bcda", "  LVS IDs: {0}", lvsIDs));
			}
			if (!IsCollect(incoTerms, chargeCode != null ? chargeCode.AC_ChargeGroup : ZString.Empty))
			{
				extraReference.Append(Res.GetString("3b6fd98a-464b-4bc6-a130-6f5d971c2b8e", "  Importer: {0}", importer.OH_Code));
			}

			extraReference.Append(Res.GetString("7eb3fd25-c523-4fdb-a910-fa83203473ec", "  Province of Clearance: {0}", province));
			return extraReference.ToStringWithNewLineBetweenAppends();
		}

		public override ILocation Destination
		{
			get { return declaration.Branch.HomePort; }
		}

		public override ILocation GetVia(CostSell costOrSell) =>
			declaration.RatingAdapter.GetVia(costOrSell);

		public override IDocAddress DeliveryAddress
		{
			get { return importer == null ? null : importer.MainAddress; }
		}

		public override Creditors Creditors
		{
			get { return declaration.RatingAdapter.Creditors; }
		}

		public override FreightMode FreightMode
		{
			get { return FreightMode.ROA; }
		}

		public override ZString ContainerMode
		{
			get { return declaration.ContainerMode; }
		}

		public override ZString HousebillReleaseType
		{
			get { return declaration.RatingAdapter.HousebillReleaseType; }
		}

		public ZDateTime JobArrivalDate
		{
			get { return declaration.JE_EntryAuthorisationDate; }
		}

		public ZDateTime JobDepartureDate
		{
			get { return declaration.JE_EntryAuthorisationDate; }
		}

		public override IRateableMeasureSet RateableMeasures => declaration.RatingAdapter.RateableMeasures;

		public override MoneyType MonetaryValues
		{
			get { return declaration.RatingAdapter.MonetaryValues; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return declaration.RatingAdapter.PaymentTerm; }
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return declaration.RatingAdapter.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell);
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get { return declaration.RatingAdapter.ServiceLevel; }
		}

		public override OrgAddress WharfCTOAddress
		{
			get { return declaration.RatingAdapter.WharfCTOAddress; }
		}

		public override Directions JobDirection
		{
			get { return Directions.Import; }
		}

		#endregion

		#region IAutoRatingCustomsInfo Members

		public ZString MessageType
		{
			get { return ((IAutoRatingCustomsInfo)declaration.RatingAdapter).MessageType; }
		}

		public ZString MessageSubType
		{
			get { return ((IAutoRatingCustomsInfo)declaration.RatingAdapter).MessageSubType; }
		}

		public EntryInfoCollection Entries
		{
			get { return ((IAutoRatingCustomsInfo)declaration.RatingAdapter).Entries; }
		}

		public InvoiceInfoCollection Invoices
		{
			get
			{
				var result = new InvoiceInfoCollection();
				foreach (var invoice in headerGroup)
				{
					result.AddNew(new Money(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency), invoice.JobComInvoiceLines.Count, invoice.Supplier);
				}
				return result;
			}
		}

		public InvoiceInfoCollection TariffsPerInvoice
		{
			get { return new InvoiceInfoCollection(); }
		}

		public InvoiceInfoCollection TariffsPerShipment
		{
			get { return new InvoiceInfoCollection(); }
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get
			{
				return 0;
			}
		}

		public override ZString JobID => declaration.JE_DeclarationReference;

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = new DeclarationRateLineConditionsSupporter(declaration)); }
		}

		RateLineConditionsSupporter conditionsSupporter;

		#endregion

		public override void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
		{
			foreach (var charge in charges)
			{
				// Override Debtor
				var chargeCode = charge.ChargeCode as AccChargeCode;
				var isCollect = IsCollect(incoTerms, chargeCode?.AC_ChargeGroup);

				var debtor = isCollect
					? importer?.DeliveryCustomsBillTo
					: declaration?.Job?.AgentCollect?.DeliveryCustomsBillTo;

				if (debtor != null)
				{
					charge.DebtorOverridePK = debtor.PK;
				}

				// Override InvoicingLineDescription
				var extraReference = GetExtraReference(chargeCode);
				if (!extraReference.IsEmpty)
				{
					var newDescription = FormattableString.Invariant($"{charge.InvoiceLineDescription} - {extraReference}");
					charge.InvoiceLineDescription = newDescription;
				}
			}
		}
	}
}
