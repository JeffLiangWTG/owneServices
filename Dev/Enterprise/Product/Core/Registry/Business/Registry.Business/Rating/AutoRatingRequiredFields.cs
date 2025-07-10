using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IAutoRatingRequiredFields
	{
		ZBool RequireServiceLevel { get; }
		ZBool RequireFrequency { get; }
		ZBool RequireTransitTime { get; }
		ZBool RequireCommodityCode { get; }
		ZBool RequireIncoterm { get; }
		string RatingHeaderType { get; }
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AutoRatingRequiredFields : RegistryBusinessObjectTemplate, IAutoRatingRequiredFields
	{
		#region Schema

		public abstract class Schema
		{
			public const string RequireServiceLevel = "RequireServiceLevel";
			public const string RequireFrequency = "RequireFrequency";
			public const string RequireTransitTime = "RequireTransitTime";
			public const string RequireCommodityCode = "RequireCommodityCode";
			public const string RequireIncoterm = "RequireIncoterm";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoRatingRequiredFields();
		}

		#region Rating Header Type

		public void SetRatingHeaderType(string ratingHeaderType)
		{
			fRatingHeaderType = ratingHeaderType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Header type string")]
		public abstract class RatingHeaderTypes
		{
			public const string Quotations = "Quotations";
			public const string ClientRates = "Client Rates";
			public const string Costs = "Costs";
			public const string CompanyTariffs = "Company Tariffs";
		}

		public string RatingHeaderType
		{
			get { return fRatingHeaderType; }
		}

		string fRatingHeaderType;

		#endregion

		#region Bound Properties

		#region Require Service Level

		public ZBool RequireServiceLevel
		{
			get { return requireServiceLevel; }
			set { SetNonPersistentPropertyValue<ZBool>(RequireServiceLevelInfo, ref requireServiceLevel, value); }
		}

		public ZPropertyInfo RequireServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.RequireServiceLevel); }
		}

		ZBool requireServiceLevel;

		#endregion

		#region Require Frequency

		public ZBool RequireFrequency
		{
			get { return requireFrequency; }
			set { SetNonPersistentPropertyValue<ZBool>(RequireFrequencyInfo, ref requireFrequency, value); }
		}

		public ZPropertyInfo RequireFrequencyInfo
		{
			get { return GetZPropertyInfo(Schema.RequireFrequency); }
		}

		ZBool requireFrequency;

		#endregion

		#region Require Transit Time

		public ZBool RequireTransitTime
		{
			get { return requireTransitTime; }
			set { SetNonPersistentPropertyValue<ZBool>(RequireTransitTimeInfo, ref requireTransitTime, value); }
		}

		public ZPropertyInfo RequireTransitTimeInfo
		{
			get { return GetZPropertyInfo(Schema.RequireTransitTime); }
		}

		ZBool requireTransitTime;

		#endregion

		#region Require Commodity Code

		public ZBool RequireCommodityCode
		{
			get { return requireCommodityCode; }
			set { SetNonPersistentPropertyValue<ZBool>(RequireCommodityCodeInfo, ref requireCommodityCode, value); }
		}

		public ZPropertyInfo RequireCommodityCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RequireCommodityCode); }
		}

		ZBool requireCommodityCode;

		#endregion

		#region Require Incoterm

		public ZBool RequireIncoterm
		{
			get { return requireIncoterm; }
			set { SetNonPersistentPropertyValue<ZBool>(RequireIncotermInfo, ref requireIncoterm, value); }
		}

		public ZPropertyInfo RequireIncotermInfo
		{
			get { return GetZPropertyInfo(Schema.RequireIncoterm); }
		}

		ZBool requireIncoterm;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.RequireServiceLevel, RequireServiceLevel.ToString());
			writer.WriteElementString(Schema.RequireFrequency, RequireFrequency.ToString());
			writer.WriteElementString(Schema.RequireTransitTime, RequireTransitTime.ToString());
			writer.WriteElementString(Schema.RequireCommodityCode, RequireCommodityCode.ToString());
			writer.WriteElementString(Schema.RequireIncoterm, RequireIncoterm.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RequireServiceLevel = new ZBool(reader.ReadElementString(Schema.RequireServiceLevel));
			RequireFrequency = new ZBool(reader.ReadElementString(Schema.RequireFrequency));
			RequireTransitTime = new ZBool(reader.ReadElementString(Schema.RequireTransitTime));
			RequireCommodityCode = new ZBool(reader.ReadElementString(Schema.RequireCommodityCode));
			RequireIncoterm = new ZBool(reader.ReadElementString(Schema.RequireIncoterm));
		}

		#endregion
	}
}
