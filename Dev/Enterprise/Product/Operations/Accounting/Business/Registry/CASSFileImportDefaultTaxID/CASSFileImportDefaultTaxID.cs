using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CASSFileImportDefaultTaxID : RegistryBusinessObjectTemplate
	{
		abstract  class Schema
		{
			public const string StandardRatedTaxID = "StandardRatedTaxID";
			public const string ZeroRatedTaxID = "ZeroRatedTaxID";
		}

		#region Construction

		public CASSFileImportDefaultTaxID(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CASSFileImportDefaultTaxID()
		{
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CASSFileImportDefaultTaxID(fallbackLevel, factory) { StandardRatedTaxID = StandardRatedTaxID, ZeroRatedTaxID = ZeroRatedTaxID };
		}

		#region Properties

		ZGuid fStandardRatedTaxID;
		[List("TaxRates")]
		public ZGuid StandardRatedTaxID
		{
			get { return fStandardRatedTaxID; }
			set
			{
				SetNonPersistentPropertyValue(StandardRatedTaxIDInfo, ref fStandardRatedTaxID, value);
			}
		}

		public ZPropertyInfo StandardRatedTaxIDInfo
		{
			get { return GetZPropertyInfo(Schema.StandardRatedTaxID); }
		}

		ZGuid fZeroRatedTaxID;
		[List("TaxRates")]
		public ZGuid ZeroRatedTaxID
		{
			get { return fZeroRatedTaxID; }
			set
			{
				SetNonPersistentPropertyValue(ZeroRatedTaxIDInfo, ref fZeroRatedTaxID, value);
			}
		}

		public ZPropertyInfo ZeroRatedTaxIDInfo
		{
			get { return GetZPropertyInfo(Schema.ZeroRatedTaxID); }
		}

		#endregion

		#region Lists

		public AccTaxRateCollection TaxRates
		{
			get
			{
				if (CurrentFallbackLevel != null)
				{
					GlbCompany company = CurrentFactory.Load<GlbCompany>(CurrentFallbackLevel.CompanyPK(false));
					ZString countryCode = company == null ? ZString.Empty : company.GC_RN_NKCountryCode;
					return new AccTaxRateCollection(CurrentFactory, new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, countryCode));
				}
				else
				{
					return new AccTaxRateCollection(CurrentFactory);
				}
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			StandardRatedTaxID = new ZGuid(reader.ReadElementString(Schema.StandardRatedTaxID));
			ZeroRatedTaxID = new ZGuid(reader.ReadElementString(Schema.ZeroRatedTaxID));
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.StandardRatedTaxID, StandardRatedTaxID.ToString());
			writer.WriteElementString(Schema.ZeroRatedTaxID, ZeroRatedTaxID.ToString());
		}

		#endregion
	}
}