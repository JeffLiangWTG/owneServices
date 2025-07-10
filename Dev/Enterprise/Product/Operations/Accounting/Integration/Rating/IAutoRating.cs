using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IAutoRatingPackageTypeInfo
	{
		List<PackageInformation> PackageInformation { get; }
	}

	public class PackageInformation
	{
		public PackageInformation(Guid identifier, string description, int count, decimal volume, string volumeUnit, string commodityCode)
		{
			this.Identifier = identifier;
			this.Description = description;
			this.Count = count;
			this.Volume = volume;
			this.VolumeUnit = volumeUnit;
			this.CommodityCode = commodityCode;
		}

		public readonly Guid Identifier;
		public readonly string Description;
		public readonly string CommodityCode;
		public readonly int Count;

		public decimal Volume
		{
			get { return fVolume; }
			set { fVolume = value; }
		}

		decimal fVolume;

		public string VolumeUnit
		{
			get { return fVolumeUnit; }
			set { fVolumeUnit = value; }
		}

		string fVolumeUnit;
	}

	#region Customs

	public interface IAutoRatingCustomsInfo
	{
		ZString MessageType { get; }
		ZString MessageSubType { get; }
		EntryInfoCollection Entries { get; }
		InvoiceInfoCollection Invoices { get; }
		InvoiceInfoCollection TariffsPerInvoice { get; }
		InvoiceInfoCollection TariffsPerShipment { get; }
		ZInt SubHeaderCount { get; }
	}

	#endregion

	#region Accounting

	public interface IAutoRatingAccountingInfo
	{
		ZString QuoteNumber { get; }
		IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false);
	}

	public interface IAutoRatingAccountingUtils
	{
		void ReloadChargesFromAdditionalJobs();
		void UpdateChargesDescription();
	}

	#endregion

	#region Autorating GlbCompany

	public interface IAutoRatingGlbCompany
	{
		GlbCompany Company { get; }
	}

	#endregion

	#region IAutoRatingStandardFreightCost

	public interface IAutoRatingStandardFreightCost
	{
		bool Enabled { get; }
		void Set(Money cost);
		BusinessObject GetHost();
	}

	public interface IAutoRatingStandardFreightCostProvider
	{
		IAutoRatingStandardFreightCost StandardFreightCost { get; }
	}

	#endregion

	#region IAutoRatingWeightBreakOverrideProvider

	public interface IAutoRatingWeightBreakOverrideProvider
	{
		decimal? WeightBreakOverride { get; }
		string WeightBreakOverrideUnit { get; }
	}

	#endregion

	#region IAutoRatingChargeApplicabilityDecider

	public interface IAutoRatingChargeApplicabilityDecider
	{
		bool ShouldRemoveCharge(AccChargeCode chargeCode);
	}

	#endregion

	#region EntryInfo & EntryInfoCollection

	public interface ILineCountInfo
	{
		ZInt EntryLines { get; }
		ZInt InvoiceLines { get; }
	}

	public class EntryInfo : ILineCountInfo
	{
		public EntryInfo(ZInt entryLines, ZInt invoiceLines, ZDecimal customsValue)
		{
			this.EntryLines = entryLines;
			this.InvoiceLines = invoiceLines;
			this.CustomsValue = customsValue;
		}

		public readonly ZInt EntryLines;
		public readonly ZInt InvoiceLines;
		public readonly ZDecimal CustomsValue;

		#region ILineCountInfo Members

		ZInt ILineCountInfo.EntryLines
		{
			get { return EntryLines; }
		}

		ZInt ILineCountInfo.InvoiceLines
		{
			get { return InvoiceLines; }
		}

		#endregion
	}

	public class EntryInfoCollection : List<EntryInfo>
	{
		public EntryInfo AddNew(ZInt entryLines, ZInt invoiceLines, ZDecimal customsValue)
		{
			EntryInfo newEntryInfo = new EntryInfo(entryLines, invoiceLines, customsValue);
			Add(newEntryInfo);

			return newEntryInfo;
		}
	}

	#endregion

	#region InvoiceInfo & InvoiceInfoCollection

	public class InvoiceInfo : ILineCountInfo
	{
		public InvoiceInfo(Money invoiceValue, ZInt invoiceLines, OrgHeader supplier, ZInt entryLines)
		{
			this.InvoiceValue = invoiceValue;
			this.InvoiceLines = invoiceLines;
			this.Supplier = supplier;
			this.EntryLines = entryLines;
		}

		public readonly Money InvoiceValue;
		public readonly ZInt InvoiceLines;
		public readonly OrgHeader Supplier;

		/// <summary>
		/// US does not merge two invoice lines from two different invoices into one entry line.
		/// If your Customs system does not behave like US, 'EntryLines' is meaningless.
		/// </summary>
		public readonly ZInt EntryLines;

		#region ILineCountInfo Members

		ZInt ILineCountInfo.EntryLines
		{
			get { return EntryLines; }
		}

		ZInt ILineCountInfo.InvoiceLines
		{
			get { return InvoiceLines; }
		}

		#endregion
	}

	public class InvoiceInfoCollection : List<InvoiceInfo>
	{
		public InvoiceInfo AddNew(Money invoiceValue, ZInt invoiceLines, OrgHeader supplier, ZInt entryLines)
		{
			InvoiceInfo newInvoiceInfo = new InvoiceInfo(invoiceValue, invoiceLines, supplier, entryLines);
			Add(newInvoiceInfo);
			return newInvoiceInfo;
		}

		public InvoiceInfo AddNew(Money invoiceValue, ZInt invoiceLines, OrgHeader supplier)
		{
			return AddNew(invoiceValue, invoiceLines, supplier, 0);
		}

		public InvoiceInfo AddNew(Money invoiceValue, ZInt invoiceLines)
		{
			return AddNew(invoiceValue, invoiceLines, null);
		}

		public List<OrgHeader> UniqueSuppliers
		{
			get
			{
				List<OrgHeader> suppliers = new List<OrgHeader>();

				foreach (InvoiceInfo invoice in this)
				{
					if (!suppliers.Contains(invoice.Supplier))
					{
						suppliers.Add(invoice.Supplier);
					}
				}

				return suppliers;
			}
		}
	}

	#endregion

	#region ProductAttributesMeasure

	public class ProductAttributesMeasure :
		IEquatable<ProductAttributesMeasure>,
		IComparable<ProductAttributesMeasure>,
		IComparable
	{
		public static ProductAttributesMeasure Empty
		{
			get { return fEmpty ?? (fEmpty = new ProductAttributesMeasure()); }
		}

		[ThreadStatic]
		static ProductAttributesMeasure fEmpty;

		public ProductAttributesMeasure(params string[] attributes)
		{
			this.attributes = (string[])attributes.Clone();
		}

		public int Length
		{
			get { return attributes.Length; }
		}

		public string this[int index]
		{
			get { return attributes[index]; }
		}

		readonly string[] attributes;

		public bool IsEmpty
		{
			get { return Length == 0; }
		}

		#region GetSimilarity

		public int GetSimilarity(ProductAttributesMeasure other)
		{
			if (CompareTo(other) == 0)
			{
				return 9;
			}
			else if (IsEmpty)
			{
				return 0;
			}
			else
			{
				int result = 0;
				for (int i = 0; i < Length && i < other.Length; i++)
				{
					if (this[i] == other[i])
					{
						if (i <= 2)
						{
							result += 1 << (2 - i);
						}
					}
					else if (!string.IsNullOrEmpty(this[i]))
					{
						return -1;
					}
				}

				for (int i = other.Length; i < Length; i++)
				{
					if (!string.IsNullOrEmpty(this[i]))
					{
						return -1;
					}
				}

				return result;
			}
		}

		#endregion

		#region Object overrides

		public override bool Equals(object obj)
		{
			return Equals((ProductAttributesMeasure)obj);
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			if (Length > 0)
			{
				hashCode = this[0] == null ? 0 : this[0].GetHashCode();
				for (int i = 1; i < Length; i++)
				{
					hashCode ^= this[i] == null ? 0 : this[i].GetHashCode();
				}
			}

			return hashCode;
		}

		#endregion

		#region IEquatable<ProductAttributesMeasure> Members

		public bool Equals(ProductAttributesMeasure other)
		{
			if (other == null)
			{
				return false;
			}

			if (Length != other.Length)
			{
				return false;
			}

			for (int i = 0; i < Length; i++)
			{
				if (this[i] != other[i])
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region IComparable<ProductAttributesMeasure> Members

		public int CompareTo(ProductAttributesMeasure other)
		{
			if (other == null)
			{
				return 1;
			}

			int result = Length.CompareTo(other.Length);
			if (result == 0)
			{
				for (int i = 0; i < Length; i++)
				{
					if (this[i] == null)
					{
						if (other[i] != null)
						{
							return 1;
						}
					}
					else
					{
						result = this[i].CompareTo(other[i]);
						if (result != 0)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			return CompareTo((ProductAttributesMeasure)obj);
		}

		#endregion
	}

	#endregion

	#region LocationMeasure

	public class LocationMeasure :
		IEquatable<LocationMeasure>,
		IComparable<LocationMeasure>,
		IComparable
	{
		public static LocationMeasure Empty
		{
			get { return fEmpty ?? (fEmpty = new LocationMeasure(ZGuid.Empty, ZString.Empty, ZString.Empty)); }
		}

		[ThreadStatic]
		static LocationMeasure fEmpty;

		public LocationMeasure(ZGuid locationPK, ZString locationType, ZString locationDescription)
		{
			pk = locationPK;
			this.locationType = locationType;
			description = locationDescription;
		}

		#region Properties

		public ZGuid PK
		{
			get { return pk; }
		}

		readonly ZGuid pk;

		public ZString LocationType
		{
			get { return locationType; }
		}

		readonly ZString locationType;

		public ZString Description
		{
			get { return description; }
		}

		readonly ZString description;

		#endregion

		public bool IsEmpty
		{
			get { return pk.IsEmpty; }
		}

		#region Object overrides

		public override bool Equals(object obj)
		{
			return Equals((LocationMeasure)obj);
		}

		public override int GetHashCode()
		{
			return pk.GetHashCode();
		}

		#endregion

		#region IEquatable<LocationMeasure> Members

		public bool Equals(LocationMeasure other)
		{
			if (other == null)
			{
				return false;
			}

			return pk.Equals(other.pk);
		}

		#endregion

		#region IComparable<LocationMeasure> Members

		public int CompareTo(LocationMeasure other)
		{
			if (other == null)
			{
				return 1;
			}

			return pk.CompareTo(other.pk);
		}

		#endregion

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			return CompareTo((LocationMeasure)obj);
		}

		#endregion
	}

	#endregion
}
