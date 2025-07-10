using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeableWeightRounding : RegistryBusinessObjectTemplate
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
		public class ShipmentChgWtSchema
		{
			public const string RoundingMode = "RoundingMode";
			public const string RoundingScale = "RoundingScale";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeableWeightRounding();
		}

		#endregion

		#region Properties

		#region RoundingMode

		[MaxLength(4)]
		public ZString RoundingMode
		{
			get { return fRoundingMode; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(RoundingModeInfo, ref fRoundingMode, value);
				if (!IsValidationSuspended)
				{
					ValidateRoundingMode();
				}
			}
		}

		ZString fRoundingMode;

		public ZPropertyInfo RoundingModeInfo
		{
			get { return GetZPropertyInfo(ShipmentChgWtSchema.RoundingMode); }
		}

		public void ValidateRoundingMode()
		{
			RoundingModeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(RoundingModeInfo, RoundingModes);
		}

		#endregion

		#region RoundingScale

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString RoundingScale
		{
			get { return fRoundingScale; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(RoundingScaleInfo, ref fRoundingScale, value);
				if (!IsValidationSuspended)
				{
					ValidateRoundingScale();
				}
			}
		}

		ZString fRoundingScale;

		public ZPropertyInfo RoundingScaleInfo
		{
			get { return GetZPropertyInfo(ShipmentChgWtSchema.RoundingScale); }
		}

		public void ValidateRoundingScale()
		{
			RoundingScaleInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(RoundingScaleInfo, RoundingScales);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRoundingMode();
			ValidateRoundingScale();
		}

		#endregion

		#region BindToLists

		public CodeDescriptionPairList RoundingModes
		{
			get
			{
				if (fRoundingModes == null)
				{
					fRoundingModes = new CodeDescriptionPairList(OLookUpEditType.ChargeableWeightRounding);
				}
				return fRoundingModes;
			}
		}

		CodeDescriptionPairList fRoundingModes;

		public CodeDescriptionPairList RoundingScales
		{
			get
			{
				if (fRoundingScales == null)
				{
					fRoundingScales = new CodeDescriptionPairList();
					fRoundingScales.AddPair(ChargeableWeightRoundingScales.Scale05, ResString.GetMultilingualString("d64be31a-11f7-4f9a-8876-7362efd1ab9e", "Round to nearest {0}", ChargeableWeightRoundingScales.Scale05));
					fRoundingScales.AddPair(ChargeableWeightRoundingScales.Scale10, ResString.GetMultilingualString("d64be31a-11f7-4f9a-8876-7362efd1ab9e", "Round to nearest {0}", ChargeableWeightRoundingScales.Scale10));
				}
				return fRoundingScales;
			}
		}

		CodeDescriptionPairList fRoundingScales;

		#endregion

		#region Methods

		public bool RoundingEnabled
		{
			get { return RoundingMode != nameof(ChargeableWeightRoundingType.None); }
		}

		public bool RoundUp
		{
			get { return RoundingMode == nameof(ChargeableWeightRoundingType.Up); }
		}

		public bool RoundToWholeNumber
		{
			get { return RoundingScale == ChargeableWeightRoundingScales.Scale10; }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			WriteElementsCore(writer);
		}

		protected virtual void WriteElementsCore(XmlWriter writer)
		{
			writer.WriteElementString(ShipmentChgWtSchema.RoundingMode, RoundingMode);
			writer.WriteElementString(ShipmentChgWtSchema.RoundingScale, RoundingScale);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReadElementsCore(reader);
		}

		protected virtual void ReadElementsCore(XmlReaderWrapper wrapper)
		{
			RoundingMode = wrapper.ReadElementString(ShipmentChgWtSchema.RoundingMode);
			RoundingScale = wrapper.ReadElementString(ShipmentChgWtSchema.RoundingScale);
		}

		#endregion
	}
}
