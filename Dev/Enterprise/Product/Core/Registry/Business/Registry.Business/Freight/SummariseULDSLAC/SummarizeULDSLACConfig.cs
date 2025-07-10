using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SummarizeULDSLACConfig : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string DestinationCountry = nameof(DestinationCountry);
			public const string TotalSLAC = nameof(TotalSLAC);
			public const string UseShipmentInners = nameof(UseShipmentInners);
		}

		#endregion

		public static readonly MultilingualString IdenticalConfigurationExistsMessage = ResString.GetMultilingualString("b0bc8045-3637-4a49-b78f-99e8b911a4fe", "Summarize ULD SLAC Configuration with Destination Country already exists");

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDestinationCountry();
		}

		#region DestinationCountry

		[List("CountryCodes")]
		[MaxLength(2)]
		public ZString DestinationCountry
		{
			get { return destinationCountry; }
			set
			{
				CheckMaximumLength(DestinationCountryInfo, value);
				SetNonPersistentPropertyValue<ZString>(DestinationCountryInfo, ref destinationCountry, value);
				ValidateDestinationCountry();
			}
		}
		ZString destinationCountry;

		public ZPropertyInfo DestinationCountryInfo => GetZPropertyInfo(Schema.DestinationCountry);

		void ValidateDestinationCountry()
		{
			DestinationCountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DestinationCountryInfo);
			ListValidation.ErrorIfInvalidCode(DestinationCountryInfo);
			CheckIdenticalConfigurationExists();
		}

		void CheckIdenticalConfigurationExists()
		{
			var identicalConfigurationFound = false;

			foreach (SummarizeULDSLACConfig configuration in ParentCollection)
			{
				if (PK != configuration.PK && DestinationCountry == configuration.DestinationCountry)
				{
					identicalConfigurationFound = true;
					break;
				}
			}

			if (identicalConfigurationFound)
			{
				AddRowError(IdenticalConfigurationExistsMessage);
			}
			else
			{
				RemoveRowError(IdenticalConfigurationExistsMessage);
			}
		}

		SummarizeULDSLACConfigCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (SummarizeULDSLACConfigCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new SummarizeULDSLACConfigCollection();
				}
			}
		}

		#endregion

		#region TotalSLAC

		public ZBool TotalSLAC
		{
			get { return totalSLAC; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(TotalSLACInfo, ref totalSLAC, value);
			}
		}
		ZBool totalSLAC;

		public ZPropertyInfo TotalSLACInfo => GetZPropertyInfo(Schema.TotalSLAC);

		protected bool TotalSLAC_ReadOnly
		{
			get
			{
				return UseShipmentInners;
			}
		}

		#endregion

		#region UseShipmentInners

		public ZBool UseShipmentInners
		{
			get { return useShipmentInners; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(UseShipmentInnersInfo, ref useShipmentInners, value);
				if (value)
				{
					TotalSLAC = true;
				}
			}
		}
		ZBool useShipmentInners;

		public ZPropertyInfo UseShipmentInnersInfo => GetZPropertyInfo(Schema.UseShipmentInners);

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SummarizeULDSLACConfig
			{
				DestinationCountry = DestinationCountry,
				TotalSLAC = TotalSLAC,
				UseShipmentInners = UseShipmentInners
			};
		}

		protected override void SetCustomDefaultValuesCore()
		{
			TotalSLAC = ZBool.True;
			base.SetCustomDefaultValuesCore();
		}

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DestinationCountry, DestinationCountry);
			writer.WriteElementString(Schema.TotalSLAC, TotalSLAC.ToString());
			writer.WriteElementString(Schema.UseShipmentInners, UseShipmentInners.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DestinationCountry = reader.ReadElementString(Schema.DestinationCountry);
			TotalSLAC = reader.ReadElementStringAsZBool(Schema.TotalSLAC);
			UseShipmentInners = reader.ReadElementStringAsZBool(Schema.UseShipmentInners);
		}

		#endregion

		#region Lookups

		public IBusinessObjectCollection CountryCodes
		{
			get
			{
				if (countryCodes == null)
				{
					countryCodes = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefCountryCollection>(), RegistryFactory.Instance);
				}
				return countryCodes;
			}
		}
		IBusinessObjectCollection countryCodes;

		#endregion
	}
}
