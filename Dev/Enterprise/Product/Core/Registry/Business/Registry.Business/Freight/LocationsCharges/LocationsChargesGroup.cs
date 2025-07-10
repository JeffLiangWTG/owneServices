using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LocationsChargesGroup : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Location = "Location";
			public const string Charges = "Charges";
		}

		#endregion

		public LocationsChargesGroup()
		{
		}

		public LocationsChargesGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateLocation();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LocationsChargesGroup(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			if (Charges != null)
			{
				LocationsChargesGroup locationsChargesGroupClone = (LocationsChargesGroup)clone;
				locationsChargesGroupClone.CloneChargesFrom(locationsChargesGroupClone, Charges);
			}
		}

		#region Location

		[MaxLength(5)]
		public ZString Location
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return location; }
			set
			{
				CheckMaximumLength(LocationInfo, value);
				SetNonPersistentPropertyValue<ZString>(LocationInfo, ref location, value);
				if (!IsValidationSuspended)
				{
					ValidateLocation();
				}
			}
		}

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(Schema.Location); }
		}

		public void ValidateLocation()
		{
			LocationInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LocationInfo);
			if (Location.Length == 2)
			{
				ListValidation.ErrorIfInvalidCode(LocationInfo, CountryList);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(LocationInfo, LocationList);
			}

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(LocationInfo, Res.GetString("1beee1fa-f3f6-429b-a38e-f00105cb1b69", "A location may only appear once in this list."));
			}
		}

		ZString location;

		#endregion

		#region LocationList

		public IBusinessObjectCollection LocationList
		{
			get
			{
				if (locationList == null)
				{
					locationList = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefUNLOCOCollection>(), new object[] { CurrentFactory });
				}
				return locationList;
			}
		}

		IBusinessObjectCollection locationList;

		public IBusinessObjectCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), new object[] { CurrentFactory });
				}
				return countryList;
			}
		}

		IBusinessObjectCollection countryList;

		#endregion

		#region Charges

		public ChargeCodeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ChargeCodeCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(fCharges);
				}

				return fCharges;
			}
		}

		void CloneChargesFrom(LocationsChargesGroup parentLocationsChargesGroup, ChargeCodeCollection existingCharges)
		{
			fCharges = existingCharges.Clone(parentLocationsChargesGroup, CurrentFallbackLevel, CurrentFactory);
			RegisterEditableChildObject(fCharges);
		}

		ZXmlSerializer ChargeCodeCollectionSerialiser
		{
			get
			{
				if (fChargeCodeCollectionSerialiser == null)
				{
					fChargeCodeCollectionSerialiser = ZXmlSerializer.New(typeof(ChargeCodeCollection));
				}
				return fChargeCodeCollectionSerialiser;
			}
		}

		ChargeCodeCollection fCharges;
		ZXmlSerializer fChargeCodeCollectionSerialiser;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Location, Location);
			ChargeCodeCollectionSerialiser.Serialize(writer, Charges);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Location = reader.ReadElementString(Schema.Location);

			ChargeCodeCollection charges = (ChargeCodeCollection)ChargeCodeCollectionSerialiser.Deserialize(new XmlRootChangeCompatibilyReader(reader));
			CloneChargesFrom(this, charges);
		}

		class XmlRootChangeCompatibilyReader : XmlReader
		{
			readonly XmlReader inner;

			public XmlRootChangeCompatibilyReader(XmlReader inner)
			{
				this.inner = inner;
			}

			public override int AttributeCount
			{
				get { return inner.AttributeCount; }
			}

			public override string BaseURI
			{
				get { return inner.BaseURI; }
			}

			public override void Close()
			{
				inner.Close();
			}

			public override int Depth
			{
				get { return inner.Depth; }
			}

			public override bool EOF
			{
				get { return inner.EOF; }
			}

			public override string GetAttribute(int i)
			{
				return inner.GetAttribute(i);
			}

			public override string GetAttribute(string name, string namespaceURI)
			{
				return inner.GetAttribute(name, namespaceURI);
			}

			public override string GetAttribute(string name)
			{
				return inner.GetAttribute(name);
			}

			public override bool HasValue
			{
				get { return inner.HasValue; }
			}

			public override bool IsEmptyElement
			{
				get { return inner.IsEmptyElement; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML Element Name")]
			public override string LocalName
			{
				get { return inner.LocalName == "Charges" ? "ChargeCodes" : inner.LocalName; }
			}

			public override string LookupNamespace(string prefix)
			{
				return inner.LookupNamespace(prefix);
			}

			public override bool MoveToAttribute(string name, string ns)
			{
				return inner.MoveToAttribute(name, ns);
			}

			public override bool MoveToAttribute(string name)
			{
				return inner.MoveToAttribute(name);
			}

			public override bool MoveToElement()
			{
				return inner.MoveToElement();
			}

			public override bool MoveToFirstAttribute()
			{
				return inner.MoveToFirstAttribute();
			}

			public override bool MoveToNextAttribute()
			{
				return inner.MoveToNextAttribute();
			}

			public override XmlNameTable NameTable
			{
				get { return inner.NameTable; }
			}

			public override string NamespaceURI
			{
				get { return inner.NamespaceURI; }
			}

			public override XmlNodeType NodeType
			{
				get { return inner.NodeType; }
			}

			public override string Prefix
			{
				get { return inner.Prefix; }
			}

			public override bool Read()
			{
				return inner.Read();
			}

			public override bool ReadAttributeValue()
			{
				return inner.ReadAttributeValue();
			}

			public override ReadState ReadState
			{
				get { return inner.ReadState; }
			}

			public override void ResolveEntity()
			{
				inner.ResolveEntity();
			}

			public override string Value
			{
				get { return inner.Value; }
			}
		}

		#endregion
	}
}
