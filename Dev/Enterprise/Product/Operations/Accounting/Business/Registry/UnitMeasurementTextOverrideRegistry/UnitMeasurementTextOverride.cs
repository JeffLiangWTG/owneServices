using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class UnitMeasurementTextOverride : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string UnitMeasurement = "UnitMeasurement";
			public const string TextOverride = "TextOverride";
		}

		#endregion

		#region UnitMeasurement

		[List("UnitMeasurementList")]
		public ZString UnitMeasurement
		{
			get { return unitMeasurement; }
			set
			{
				SetNonPersistentPropertyValue(UnitMeasurementInfo, ref unitMeasurement, value);
				if (!IsValidationSuspended)
				{
					ValidateUnitMeasurement();
				}
			}
		}

		public ZPropertyInfo UnitMeasurementInfo
		{
			get { return GetZPropertyInfo(Schema.UnitMeasurement); }
		}

		public void ValidateUnitMeasurement()
		{
			UnitMeasurementInfo.ClearAllNotifications();
			Validation.ValidateUnitMeasurement();
		}

		ZString unitMeasurement;

		#endregion

		#region TextOverride

		public ZString TextOverride
		{
			get { return textOverride; }
			set
			{
				SetNonPersistentPropertyValue(TextOverrideInfo, ref textOverride, value);
				if (!IsValidationSuspended)
				{
					ValidateTextOverride();
				}
			}
		}

		public ZPropertyInfo TextOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.TextOverride); }
		}

		public void ValidateTextOverride()
		{
			TextOverrideInfo.ClearAllNotifications();
			Validation.ValidateTextOverride();
		}

		ZString textOverride;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.UnitMeasurement, UnitMeasurement.ToString());
			writer.WriteElementString(Schema.TextOverride, TextOverride.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UnitMeasurement = reader.ReadElementString(Schema.UnitMeasurement);
			TextOverride = reader.ReadElementString(Schema.TextOverride);
		}

		#endregion

		#region Lookups

		UnitMeasurementTextOverrideLookups unitMeasurementTestOverrideLookups;

		UnitMeasurementTextOverrideLookups LookUps => unitMeasurementTestOverrideLookups ?? (unitMeasurementTestOverrideLookups = new UnitMeasurementTextOverrideLookups());

		public CodeDescriptionPairList UnitMeasurementList
		{
			get { return LookUps.UnitMeasurementList; }
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UnitMeasurementTextOverride();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var unitMeasurementTextOverride = (UnitMeasurementTextOverride)clone;
			unitMeasurementTextOverride.UnitMeasurement = UnitMeasurement;
			unitMeasurementTextOverride.TextOverride = TextOverride;
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUnitMeasurement();
			ValidateTextOverride();
		}

		#endregion

		public UnitMeasurementTextOverrideValidation Validation => validation ?? (validation = new UnitMeasurementTextOverrideValidation(this));
		UnitMeasurementTextOverrideValidation validation;
	}
}
