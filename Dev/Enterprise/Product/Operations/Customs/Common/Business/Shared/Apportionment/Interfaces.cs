using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public interface ICommonInvoice
	{
		BusinessObjectFactory Factory { get; }
		RefCurrencyCurrencyConverter CurrencyConverter { get; }
		ZString IncoTerm { get; }
		bool IsDeleted { get; }
		ZGuid PK { get; }

		/// <summary>
		/// ImmediateCommonInvoiceParent can be null eg. Top Group Header as JobDeclaration does not have charges.
		/// </summary>
		ICommonInvoice ImmediateCommonInvoiceParent { get; }

		IApportionInvoiceHolder InvoicesHolder { get; }
		AllChargesCollection AllCharges { get; }

		ZString LocalCurrencyCode { get; }
		bool HasMultipleInvoiceUQs { get; }
	}

	public static class ICommonInvoiceExtensionMethods
	{
		public static ICommonInvoice GetParentWithThisChargeType(this ICommonInvoice currentCommonInvoice, ChargeCodeChargeKey chargeKey)
		{
			ICommonInvoice parent = currentCommonInvoice.ImmediateCommonInvoiceParent;
			if (parent == null)
			{
				return null;
			}
			else if (parent.HasChargeWithThisKey(chargeKey))
			{
				return parent;
			}
			return GetParentWithThisChargeType(parent, chargeKey);
		}

		public static ICommonInvoice GetParentWithThisChargeType(this ICommonInvoice currentCommonInvoice, ApportionChargeKey chargeKey)
		{
			ICommonInvoice parent = currentCommonInvoice.ImmediateCommonInvoiceParent;
			if (parent == null)
			{
				return null;
			}
			else if (parent.HasChargeWithThisKey(chargeKey))
			{
				return parent;
			}
			return GetParentWithThisChargeType(parent, chargeKey);
		}

		public static bool HasParentChargeDistributedByThisField(this ICommonInvoice currentCommonInvoice, string distributedBy)
		{
			ICommonInvoice parent = currentCommonInvoice.ImmediateCommonInvoiceParent;
			if (parent == null)
			{
				return false;
			}
			else if (parent.HasChargeDistributedBy(distributedBy))
			{
				return true;
			}
			return HasParentChargeDistributedByThisField(parent, distributedBy);
		}

		public static bool HasChargeWithThisKey(this ICommonInvoice currentCommonInvoice, ChargeCodeChargeKey chargeCode)
		{
			bool result = false;

			IChargeHolder chargeHolder = currentCommonInvoice as IChargeHolder;

			if (chargeHolder != null)
			{
				result = chargeHolder.Charges.HasChargeWithThisKey(chargeCode);
			}

			if (!result)
			{
				IChargeApportionee apportionee = currentCommonInvoice as IChargeApportionee;

				result = apportionee != null && apportionee.ApportionedCharges.HasChargeWithThisKey(chargeCode);
			}

			return result;
		}

		public static bool HasChargeWithThisKey(this ICommonInvoice currentCommonInvoice, ApportionChargeKey chargeCode)
		{
			bool result = false;

			IChargeHolder chargeHolder = currentCommonInvoice as IChargeHolder;

			if (chargeHolder != null)
			{
				result = chargeHolder.Charges.HasChargeWithThisKey(chargeCode);
			}

			if (!result)
			{
				IChargeApportionee apportionee = currentCommonInvoice as IChargeApportionee;

				result = apportionee != null && apportionee.ApportionedCharges.HasChargeWithThisKey(chargeCode);
			}

			return result;
		}

		public static bool HasChargeDistributedBy(this ICommonInvoice currentCommonInvoice, string chargeCode)
		{
			bool result = false;

			IChargeHolder chargeHolder = currentCommonInvoice as IChargeHolder;

			if (chargeHolder != null)
			{
				result = chargeHolder.Charges.HasChargeDistributedBy(chargeCode);
			}

			if (!result)
			{
				IChargeApportionee apportionee = currentCommonInvoice as IChargeApportionee;

				result = apportionee != null && apportionee.ApportionedCharges.HasChargeDistributedBy(chargeCode);
			}

			return result;
		}

		public static bool HasChargesExcludedInITOT(this ICommonInvoice currentCommonInvoice, string chargeCode)
		{
			bool result = false;

			IChargeHolder chargeHolder = currentCommonInvoice as IChargeHolder;

			if (chargeHolder != null)
			{
				result = chargeHolder.Charges.HasChargesExcludedInITOT(chargeCode);
			}

			if (!result)
			{
				IChargeApportionee apportionee = currentCommonInvoice as IChargeApportionee;

				result = apportionee != null && apportionee.ApportionedCharges.HasChargesExcludedInITOT(chargeCode);
			}

			return result;
		}

		public static bool HasChargesIncludedInITOT(this ICommonInvoice currentCommonInvoice, string chargeCode)
		{
			bool result = false;

			IChargeHolder chargeHolder = currentCommonInvoice as IChargeHolder;

			if (chargeHolder != null)
			{
				result = chargeHolder.Charges.HasChargesIncludedInITOT(chargeCode);
			}

			if (!result)
			{
				IChargeApportionee apportionee = currentCommonInvoice as IChargeApportionee;

				result = apportionee != null && apportionee.ApportionedCharges.HasChargesIncludedInITOT(chargeCode);
			}

			return result;
		}

		public static bool HasChargesWithCurrency(this ICommonInvoice invoice, string chargeType)
		{
			var result = false;

			var chargeHolder = invoice as IChargeHolder;

			var chargeCode = invoice.InvoicesHolder?.IncoTermAndChargeFactory.GetCharge(chargeType);
			var chargeKey = chargeCode != null ? chargeCode.ChargeCodeChargeKey : null;

			if (chargeKey != null)
			{
				if (chargeHolder != null)
				{
					result = chargeHolder.Charges.HasChargeWithCurrency(chargeKey);
				}

				if (!result)
				{
					var apportionee = invoice as IChargeApportionee;

					result = apportionee != null && apportionee.ApportionedCharges.HasChargeWithCurrency(chargeKey);
				}
			}

			return result;
		}
	}

	public interface IApportionStrategy
	{
		decimal Round(decimal amount);
		bool ShouldBackApportion(ApportionChargeKey chargeKey);
		decimal UnitOfAmountToBackApportion { get; }
	}

	public interface IApportionInvoiceHolder
	{
		IChargeHolder[] ChargeHolders { get; }
		IChargeApportionee[] Invoices { get; }

		IComparer ChargeComparer { get; }

		IComparer<IChargeApportionee> LineChargeApportioneeComparer { get; }
		IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory { get; }

		void MarkApportionmentDirty();
		void UpdateApportionmentProgress();
		void ValidateIncoTerms();
		string CountryContext { get; }
	}

	public interface IChargeHolder
	{
		BusinessObjectFactory Factory { get; }
		CurrencyConverter CurrencyConverter { get; }
		IJobComInvChargeCollection<JobComInvCharge> Charges { get; }

		/// <summary>
		/// Should return all possible apportionees even if IsValidToApportionTo is false to clear apportionment
		/// </summary>
		IChargeApportionee[] AllApportionees { get; }
		IChargeHolder[] ImmediateChargeHolderChildren { get; }
		IChargeHolder ImmediateChargeHolderParent { get; }
		ZGuid PK { get; }
		bool IsGroupInvoice { get; }

		ZString GetDefaultCurrencyCode(ICustomsChargeCode chargeCode, JobComInvCharge charge);

		ZString GetDefaultDistributeBy();
	}

	public interface IChargeApportionee : IChargeHolder
	{
		bool IsValidToApportionTo { get; }
		IJobComInvApportionedChargeCollection<JobComInvCharge> ApportionedCharges { get; }
		ZDecimal GetBaseValueToApportionOn(CurrencyConverter currencyConverter, string distributeBy);
		bool CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionChargeKey);

		void CalculateAmountBasedOnPercentage(JobComInvCharge charge);
	}
}

#region Test Objects
#if DEBUG
namespace Enterprise.Customs.Common.Testing
{
	using System.Data;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Integration;
	using ZArchitecture.Schema;

	public class TestDeclaration : DummyBaseBusinessObject, ICommonInvoice, IApportionInvoiceHolder, ICurrencyConverterDataProvider, IChargeHolder
	{
		public TestDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCode => GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;

		public TestInvoiceCollection Invoices
		{
			get
			{
				if (invoices == null)
				{
					invoices = new TestInvoiceCollection(this);
					invoices.Load();
				}
				return invoices;
			}
		}
		TestInvoiceCollection invoices;

		public TestGroupHeaderCollection AllGroupHeaders => allGroupHeaders ?? (allGroupHeaders = new TestGroupHeaderCollection(this));
		TestGroupHeaderCollection allGroupHeaders;

		public TestChargeCollection Charges => charges ?? (charges = new TestChargeCollection(this));
		TestChargeCollection charges;

		public CurrencyConverterWithDataProvider CurrencyConverter => currencyConverter ?? (currencyConverter = new CurrencyConverterWithDataProvider(Factory, this));
		CurrencyConverterWithDataProvider currencyConverter;

		#region ICommonInvoice Members

		AllChargesCollection ICommonInvoice.AllCharges
		{
			get
			{
				if (allCharges == null)
				{
					allCharges = new AllChargesCollection(this);
					allCharges.Load();
				}
				return allCharges;
			}
		}
		AllChargesCollection allCharges;

		BusinessObjectFactory ICommonInvoice.Factory => Factory;

		ICommonInvoice ICommonInvoice.ImmediateCommonInvoiceParent => null;

		RefCurrencyCurrencyConverter ICommonInvoice.CurrencyConverter => CurrencyConverter;

		public ZString IncoTerm { get; set; }

		bool ICommonInvoice.IsDeleted => IsDeleted;

		ZGuid ICommonInvoice.PK => PK;

		IApportionInvoiceHolder ICommonInvoice.InvoicesHolder => this;

		string IApportionInvoiceHolder.CountryContext => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		bool ICommonInvoice.HasMultipleInvoiceUQs => false;

		#endregion

		#region IApportionInvoiceHolder Members

		public IChargeHolder[] ChargeHolders
		{
			get
			{
				ArrayList result = new ArrayList(Invoices);
				result.Add(this);
				result.AddRange(AllGroupHeaders);
				return (IChargeHolder[])result.ToArray(typeof(IChargeHolder));
			}
		}

		public IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory
		{
			get => incoTermAndChargeFactory ?? (incoTermAndChargeFactory = IncoTermAndCustomsChargeFactory.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			set => incoTermAndChargeFactory = value;
		}
		IncoTermAndCustomsChargeFactory incoTermAndChargeFactory;

		IChargeApportionee[] IApportionInvoiceHolder.Invoices => (IChargeApportionee[])new ArrayList(Invoices).ToArray(typeof(IChargeApportionee));

		public IComparer ChargeComparer => null;

		public bool ApportionmentDirty { get; set; }

		public void MarkApportionmentDirty()
		{
			ApportionmentDirty = true;
		}

		public void UpdateApportionmentProgress()
		{
		}

		void IApportionInvoiceHolder.ValidateIncoTerms()
		{
		}

		IComparer<IChargeApportionee> IApportionInvoiceHolder.LineChargeApportioneeComparer => new LineChargeApportioneeComparer();

		class LineChargeApportioneeComparer : IComparer<IChargeApportionee>
		{
			int IComparer<IChargeApportionee>.Compare(IChargeApportionee x, IChargeApportionee y) => 0;
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		public ZDateTime DateOfValuation
		{
			get;
			set;
		}

		public int MaximumDaysToFallback
		{
			get;
			set;
		}

		ZArchitecture.Core.ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return Enterprise.ZArchitecture.Core.ExchangeRateType.Customs; }
		}

		public GlbCompany CompanyExposed;
		public GlbCompany Company
		{
			get { return CompanyExposed ?? GlbCompany.CurrentCompany; }
		}

		public ZBool? IsReciprocalOverride
		{
			get { return null; }
		}

		public ZString LocalCurrencyCodeOverride
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IChargeHolder Members

		CurrencyConverter IChargeHolder.CurrencyConverter => CurrencyConverter;

		IJobComInvChargeCollection<JobComInvCharge> IChargeHolder.Charges => Charges;

		IChargeApportionee[] IChargeHolder.AllApportionees
		{
			get
			{
				ArrayList result = new ArrayList();

				foreach (TestInvoice invoice in Invoices)
				{
					result.Add(invoice);
					result.AddRange(invoice.InvoiceLines);
				}

				return (IChargeApportionee[])result.ToArray(typeof(IChargeApportionee));
			}
		}

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren => (IChargeHolder[])new ArrayList(Invoices).ToArray(typeof(IChargeHolder));

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent => null;

		bool IChargeHolder.IsGroupInvoice => false;

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge) => ZString.Empty;

		ZString IChargeHolder.GetDefaultDistributeBy() => ZString.Empty;

		#endregion
	}

	public class TestInvoiceCollection : DummyBusinessObjectCollection
	{
		public TestInvoiceCollection(TestDeclaration declaration)
			: base(declaration.Factory, new ZQuery(DummyBizoSchema.Z0_Guid, declaration.PK))
		{
			this.declaration = declaration;
		}

		readonly TestDeclaration declaration;

		public new TestInvoice AddNew() => (TestInvoice)base.AddNew();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(TestInvoice);

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			((TestInvoice)child).Z0_Guid = declaration.PK;
		}
	}

	public class TestGroupHeaderCollection : DummyBusinessObjectCollection
	{
		readonly TestDeclaration declaration;
		public TestGroupHeaderCollection(TestDeclaration declaration)
			: base(declaration.Factory, new ZQuery(DummyBizoSchema.Z0_Guid, declaration.PK))
		{
			this.declaration = declaration;
		}

		public new TestGroupHeader AddNew() => (TestGroupHeader)base.AddNew();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(TestGroupHeader);

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			((TestGroupHeader)child).Z0_Guid = declaration.PK;
		}
	}

	public class TestInvoice : DummyBusinessObject
		, ICommonInvoice
		, IChargeHolder
		, ICurrencyConverterDataProvider
		, IChargeApportionee
		, ITypeDeciderContext
	{
		public TestInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCode => GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;

		public RefCurrency LocalCurrency => RefCurrency.LoadFromCurrencyCode(Factory, LocalCurrencyCode);

		public CurrencyConverterWithDataProvider CurrencyConverter => currencyConverter ?? (currencyConverter = new CurrencyConverterWithDataProvider(Factory, this));
		CurrencyConverterWithDataProvider currencyConverter;

		public ZDecimal InvoicePrice { get; set; }

		public ZString InvoiceCurrencyCode { get; set; }

		public TestDeclaration Declaration => Factory.Load<TestDeclaration>(Z0_Guid);

		public RefCurrency InvoiceCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, InvoiceCurrencyCode);

		public Money InvoicePriceMoney => new Money(InvoicePrice, InvoiceCurrency);

		public IJobComInvChargeCollection<TestCharge> Charges => charges ?? (charges = new TestChargeCollection(this));
		IJobComInvChargeCollection<TestCharge> charges;

		public TestApportionedChargeCollection ApportionedCharges => apportionedCharges ?? (apportionedCharges = new TestApportionedChargeCollection(this));
		TestApportionedChargeCollection apportionedCharges;

		public TestInvoiceLineCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new TestInvoiceLineCollection(this);
					invoiceLines.Load();
				}
				return invoiceLines;
			}
		}
		TestInvoiceLineCollection invoiceLines;

		#region ICommonInvoice Members

		RefCurrencyCurrencyConverter ICommonInvoice.CurrencyConverter => CurrencyConverter;

		AllChargesCollection ICommonInvoice.AllCharges
		{
			get
			{
				if (allCharges == null)
				{
					allCharges = new AllChargesCollection(this);
					allCharges.Load();
				}
				return allCharges;
			}
		}
		AllChargesCollection allCharges;

		public ZString IncoTerm { get; set; }

		ICommonInvoice ICommonInvoice.ImmediateCommonInvoiceParent => Declaration;

		public IApportionInvoiceHolder InvoicesHolder => Declaration;

		bool ICommonInvoice.HasMultipleInvoiceUQs => false;

		#endregion

		#region IChargeHolder Members

		CurrencyConverter IChargeHolder.CurrencyConverter => CurrencyConverter;

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge) => InvoiceCurrencyCode;

		ZString IChargeHolder.GetDefaultDistributeBy() => ZString.Empty;

		IJobComInvChargeCollection<JobComInvCharge> IChargeHolder.Charges => Charges;

		public IChargeApportionee[] AllApportionees => (IChargeApportionee[])InvoiceLines.ToArray();

		public IChargeHolder[] ImmediateChargeHolderChildren => (IChargeHolder[])InvoiceLines.ToArray();

		public IChargeHolder ImmediateChargeHolderParent => Declaration;

		public bool IsGroupInvoice => false;

		#endregion

		#region ICurrencyConverterDataProvider Members

		public ZDateTime DateOfValuation { get; set; }

		public int MaximumDaysToFallback { get; set; }

		public ZArchitecture.Core.ExchangeRateType RateType => Enterprise.ZArchitecture.Core.ExchangeRateType.Customs;

		public GlbCompany CompanyExposed;
		public GlbCompany Company => CompanyExposed ?? GlbCompany.CurrentCompany;

		public ZBool? IsReciprocalOverride => null;

		public ZString LocalCurrencyCodeOverride => ZString.Empty;

		#endregion

		#region IChargeHolder Members

		BusinessObjectFactory IChargeHolder.Factory
		{
			get { return Factory; }
		}

		IChargeApportionee[] IChargeHolder.AllApportionees
		{
			get { return (IChargeApportionee[])new ArrayList(InvoiceLines).ToArray(typeof(IChargeApportionee)); }
		}

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren
		{
			get { return Array.Empty<IChargeHolder>(); }
		}

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent
		{
			get { return Declaration; }
		}

		ZGuid IChargeHolder.PK
		{
			get { return PK; }
		}

		bool IChargeHolder.IsGroupInvoice
		{
			get { return false; }
		}

		#endregion

		#region IChargeApportionee Members

		bool IChargeApportionee.IsValidToApportionTo
		{
			get { return true; }
		}

		IJobComInvApportionedChargeCollection<JobComInvCharge> IChargeApportionee.ApportionedCharges
		{
			get { return ApportionedCharges; }
		}

		ZDecimal IChargeApportionee.GetBaseValueToApportionOn(CurrencyConverter currencyConverter, string distributeBy)
		{
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					return currencyConverter.ConvertExact(InvoicePriceMoney, currencyConverter.LocalCurrency).Amount;

				default:
					return 0m;
			}
		}

		bool IChargeApportionee.CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionChargeKey)
		{
			var incoTermAndChargeFactory = InvoicesHolder?.IncoTermAndChargeFactory;
			return incoTermAndChargeFactory != null && incoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(IncoTerm, incoTermAndChargeFactory.GetCharge(apportionChargeKey.ChargeKey.ChargeCode));
		}

		void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
		{
		}

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => "XX";

		#endregion
	}

	public class TestInvoiceLine : DummyDependantBusinessObject, IChargeApportionee, ICommonInvoice
	{
		public TestInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TestInvoice Invoice { get; set; }

		public TestChargeCollection Charges => charges ?? (charges = new TestChargeCollection(this));
		TestChargeCollection charges;

		public TestApportionedChargeCollection ApportionedCharges => apportionedCharges ?? (apportionedCharges = new TestApportionedChargeCollection(this));
		TestApportionedChargeCollection apportionedCharges;

		public ZDecimal Price { get; set; }

		public ZString CurrencyCode => Invoice.InvoiceCurrencyCode;

		public RefCurrency Currency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);

		public Money PriceMoney => new Money(Price, Currency);

		public ZDecimal Weight { get; set; }

		public ZString WeightUQ { get; set; }

		public ZDecimal Volume { get; set; }

		public ZString VolumeUQ { get; set; }

		#region IChargeApportionee Members

		public bool IsValidToApportionTo => true;

		IJobComInvApportionedChargeCollection<JobComInvCharge> IChargeApportionee.ApportionedCharges => ApportionedCharges;

		ZDecimal IChargeApportionee.GetBaseValueToApportionOn(CurrencyConverter currencyConverter, string distributeBy)
		{
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					return currencyConverter.ConvertExact(PriceMoney, currencyConverter.LocalCurrency).Amount;

				case ChargeDistributeByList.Codes.Weight:
					return Core.Constants.Weight.Convert(Weight, WeightUQ, Core.Constants.Weight.Kilograms);

				case ChargeDistributeByList.Codes.Volume:
					return Core.Constants.Volume.Convert(Volume, VolumeUQ, Core.Constants.Volume.CubicMetres);

				default:
					return 0m;
			}
		}

		bool IChargeApportionee.CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionChargeKey) => ((IChargeApportionee)Invoice).CanThisChargeBeApportionedBasedOnIncoterm(apportionChargeKey);

		void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
		{
			charge.J7_Amount = charge.J7_Percentage * Price / 100;
			charge.J7_RX_NKCurrency = CurrencyCode;
		}

		#endregion

		#region IChargeHolder Members

		BusinessObjectFactory IChargeHolder.Factory => Factory;

		CurrencyConverter IChargeHolder.CurrencyConverter => Invoice.CurrencyConverter;

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge) => Invoice.InvoiceCurrencyCode;

		ZString IChargeHolder.GetDefaultDistributeBy() => ZString.Empty;

		IJobComInvChargeCollection<JobComInvCharge> IChargeHolder.Charges => Charges;

		IChargeApportionee[] IChargeHolder.AllApportionees => Array.Empty<IChargeApportionee>();

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren => Array.Empty<IChargeHolder>();

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent => Invoice;

		ZGuid IChargeHolder.PK => PK;

		bool IChargeHolder.IsGroupInvoice => false;

		#endregion

		#region ICommonInvoice Members

		AllChargesCollection ICommonInvoice.AllCharges
		{
			get
			{
				if (allCharges == null)
				{
					allCharges = new AllChargesCollection(this);
					allCharges.Load();
				}
				return allCharges;
			}
		}
		AllChargesCollection allCharges;

		BusinessObjectFactory ICommonInvoice.Factory => Factory;

		RefCurrencyCurrencyConverter ICommonInvoice.CurrencyConverter => Invoice.CurrencyConverter;

		ZString ICommonInvoice.IncoTerm => ((ICommonInvoice)Invoice).IncoTerm;

		ICommonInvoice ICommonInvoice.ImmediateCommonInvoiceParent => Invoice;

		bool ICommonInvoice.IsDeleted => IsDeleted;

		ZGuid ICommonInvoice.PK => PK;

		IApportionInvoiceHolder ICommonInvoice.InvoicesHolder => ((ICommonInvoice)Invoice).InvoicesHolder;

		ZString ICommonInvoice.LocalCurrencyCode => Invoice.LocalCurrencyCode;

		bool ICommonInvoice.HasMultipleInvoiceUQs => false;

		#endregion
	}

	public class TestInvoiceLineCollection : DummyDependentBusinessObjectCollection
	{
		public TestInvoiceLineCollection(TestInvoice invoice)
			: base(invoice, invoice.Factory)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			((TestInvoiceLine)bizOAdded).Invoice = (TestInvoice)Master;
		}

		public new TestInvoiceLine AddNew() => (TestInvoiceLine)base.AddNew();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(TestInvoiceLine);
	}

	public class TestGroupHeader : TestInvoice
	{
		public TestGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	public class TestChargeCollection : JobComInvChargeCollection<TestCharge>
	{
		public TestChargeCollection(ICommonInvoice invoice)
			: base(invoice)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(TestCharge);
	}

	public class TestCharge : JobComInvCharge, Integration.Customs.IBaseJobComInvHeaderCharge
	{
		public TestCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J7_IsApportionedCharge = false;
		}

		public override ApportionChargeKey ApportionChargeKey => Parent is TestDeclaration
			? new ApportionChargeKey(ChargeKey, J7_FullOrPartialApportionment, J7_Calc_IsIncludedInITOT, J7_Calc_IsIncludedInInvoiceString, J7_DistributeBy, J7_Percentage, J7_AdjustedCharge, J7_ChargeDescription, J7_IsSystem)
			: base.ApportionChargeKey;

		bool IsJ7_IsIncludedInITOTCalculated => IncoTermAndChargeFactory?.IsIncludedInITOTReadOnlyForGroupCharge(J7_ChargeType) ?? false;

		public ZString J7_Calc_IsIncludedInITOT => IsJ7_IsIncludedInITOTCalculated
			? GroupIsIncludedInLinesOptionList.Codes.NotApplicable
			: J7_IsIncludedInITOT ? GroupIsIncludedInLinesOptionList.Codes.Yes : GroupIsIncludedInLinesOptionList.Codes.No;

		public ZString J7_Calc_IsIncludedInInvoiceString => IsJ7_IsIncludedInITOTCalculated
			? GroupIsIncludedInLinesOptionList.Codes.NotApplicable
			: J7_Calc_IsIncludedInInvoiceAmount ? GroupIsIncludedInLinesOptionList.Codes.Yes : GroupIsIncludedInLinesOptionList.Codes.No;

		ZBool Integration.Customs.IBaseJobComInvHeaderCharge.NeedCheckChargeType => IsInterface;

		public ZBool IsInterface { get; set; }
	}

	public class TestApportionedCharge : JobComInvCharge
	{
		public TestApportionedCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J7_IsApportionedCharge = true;
		}
	}

	public class TestApportionedChargeCollection : JobComInvApportionedChargeCollection<TestApportionedCharge>
	{
		public TestApportionedChargeCollection(ICommonInvoice parent)
			: base(parent)
		{
		}

		public new TestApportionedCharge AddNew() => base.AddNew();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(TestApportionedCharge);
	}
}
#endif
#endregion
