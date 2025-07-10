using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.CA.Business.XmlSerializers")]
	public class DefaultFreightPercentage : RegistryBusinessObjectTemplate
	{
		public DefaultFreightPercentage()
		{
		}

		public DefaultFreightPercentage(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string ModeofTransport = "ModeofTransport";
			public const string FreightPercentage = "FreightPercentage";
		}

		#endregion

		#region Overrides

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultFreightPercentage(factory);
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ModeofTransport, ModeofTransport);
			writer.WriteElementString(Schema.FreightPercentage, FreightPercentage.ToString());
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ModeofTransport = reader.ReadElementString(Schema.ModeofTransport);
			FreightPercentage = reader.ReadElementStringAsZDecimal(Schema.FreightPercentage);
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateModeofTransport();
		}

		#endregion

		#endregion

		#region Bound Properties

		#region ModeofTransport

		ZString modeofTransport;
		[MaxLength(3)]
		public ZString ModeofTransport
		{
			get { return modeofTransport; }
			set
			{
				CheckMaximumLength(ModeofTransportInfo, value);
				SetNonPersistentPropertyValue(ModeofTransportInfo, ref modeofTransport, value);
				if (!IsValidationSuspended)
				{
					ValidateModeofTransport();
				}
			}
		}

		public ZPropertyInfo ModeofTransportInfo
		{
			get { return GetZPropertyInfo(Schema.ModeofTransport); }
		}

		void ValidateModeofTransport()
		{
			ModeofTransportInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ModeofTransportInfo);
			if (!ModeofTransportInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ModeofTransportInfo, TransportTypeList);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ModeofTransportInfo);
			}
		}

		#endregion

		#region FreightPercentage

		ZDecimal freightPercentage;
		[DecimalPlaces(2)]
		public ZDecimal FreightPercentage
		{
			get { return freightPercentage; }
			set
			{
				SetNonPersistentPropertyValue(FreightPercentageInfo, ref freightPercentage, value);
			}
		}

		public ZPropertyInfo FreightPercentageInfo
		{
			get { return GetZPropertyInfo(Schema.FreightPercentage); }
		}

		#endregion

		#endregion

		#region TransportTypeList

		public CodeDescriptionPairList TransportTypeList
		{
			get
			{
				return CurrentFactory.GetCachedValue<TransportTypeList>();
			}
		}

		#endregion

	}
}
