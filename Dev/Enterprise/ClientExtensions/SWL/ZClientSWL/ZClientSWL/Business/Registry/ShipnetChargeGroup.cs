using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.SWL.Business
{
	[XmlSerializerAssembly("ZClientSWL.XmlSerializers")]
	public class ShipnetChargeGroup : RegistryBusinessObjectTemplate, IDisposable
	{
		public ShipnetChargeGroup()
		{
		}

		public ShipnetChargeGroup(ShipnetSetupBusinessObject parent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = parent;
		}

		#region Schema

		public static class Schema
		{
			public const string ChargeGroupCode = "ChargeGroupCode";
			public const string ChargeGroupDescription = "ChargeGroupDescription";
			public const string Charges = "Charges";
		}

		#endregion

		#region Bound Properties

		#region ChargeGroupCode

		ZString fChargeGroupCode;
		[MaxLength(15)]
		public ZString ChargeGroupCode
		{
			get { return fChargeGroupCode; }
			set
			{
				value = value.TrimEnd(' ');
				if (value != fChargeGroupCode)
				{
					CheckMaximumLength(ChargeGroupCodeInfo, value);
					SetNonPersistentPropertyValue<ZString>(ChargeGroupCodeInfo, ref fChargeGroupCode, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateChargeGroupCode();
					}
				}
			}
		}

		public ZPropertyInfo ChargeGroupCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroupCode); }
		}

		void ValidateChargeGroupCode()
		{
			ChargeGroupCodeInfo.ClearAllNotifications();
			if (Parent != null && Parent.IsShipnetCarrier)
			{
				MandatoryValidation.CheckEntered(ChargeGroupCodeInfo);
				if (!ChargeGroupCodeInfo.HasErrors())
				{
					CheckDuplicateCodes();
				}
			}
		}

		#endregion

		#region ChargeGroupDescription

		ZString fChargeGroupDescription;
		[MaxLength(50)]
		public ZString ChargeGroupDescription
		{
			get { return fChargeGroupDescription; }
			set
			{
				value = value.TrimEnd(' ');
				if (value != fChargeGroupDescription)
				{
					CheckMaximumLength(ChargeGroupDescriptionInfo, value);
					SetNonPersistentPropertyValue<ZString>(ChargeGroupDescriptionInfo, ref fChargeGroupDescription, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateChargeGroupDescription();
					}
				}
			}
		}

		public ZPropertyInfo ChargeGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroupDescription); }
		}

		void ValidateChargeGroupDescription()
		{
			ChargeGroupDescriptionInfo.ClearAllNotifications();
			if (Parent != null && Parent.IsShipnetCarrier)
			{
				MandatoryValidation.CheckEntered(ChargeGroupDescriptionInfo);
			}
		}

		#endregion

		#endregion

		#region Charges

		ShipnetChargeCollection fCharges;
		public ShipnetChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ShipnetChargeCollection(Parent, CurrentFactory);
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}

		#endregion

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				bool oldHasChange = HasChanges;
				base.HasChanges = value;
				if (!IsCopying)
				{
					if (oldHasChange != value && oldHasChange)
					{
						SetAllChildHasChange(value);
					}
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (fCharges != null)
			{
				UnRegisterEditableChildObject(fCharges);
				fCharges = null;
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ShipnetChargeGroup result = new ShipnetChargeGroup(Parent, factory);
			try
			{
				((IBusinessObjectInternals)result).IsCopying = true;
				result.ChargeGroupCode = ChargeGroupCode;
				result.ChargeGroupDescription = ChargeGroupDescription;
				result.Charges.AddRange((BusinessObjectCollection)Charges.Clone(fallbackLevel, factory));
			}
			finally
			{
				result.HasChanges = false;
				((IBusinessObjectInternals)result).IsCopying = false;
			}
			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ChargeGroupCode, ChargeGroupCode);
			writer.WriteElementString(Schema.ChargeGroupDescription, ChargeGroupDescription);
			ChargeCollectionSerializer.Serialize(writer, Charges);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			try
			{
				((IBusinessObjectInternals)this).IsCopying = true;
				ChargeGroupCode = reader.ReadElementString(Schema.ChargeGroupCode);
				ChargeGroupDescription = reader.ReadElementString(Schema.ChargeGroupDescription);
				Charges.RemoveAndDeleteAll();
				Charges.AddRange((ShipnetChargeCollection)ChargeCollectionSerializer.Deserialize(reader));
			}
			finally
			{
				HasChanges = false;
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateChargeGroupCode();
			ValidateChargeGroupDescription();
			CheckChargeCodes();
		}

		protected void SetAllChildHasChange(bool changed)
		{
			if (fCharges != null)
			{
				foreach (ShipnetCharge charge in fCharges)
				{
					charge.HasChanges = changed;
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			ChargeGroupCode = "TestCode";
			ChargeGroupDescription = "TestDescription";
			ShipnetCharge charge = Charges.AddNew();
			charge.FillWithValidTestData(kind, propertyPath);
		}
#endif
#endregion

		#region CheckDuplicateCodes

		void CheckDuplicateCodes()
		{
			if (ParentCollections.Count > 0 && ((ShipnetChargeGroupCollection)ParentCollections.First()).IsDuplicateEntry(this))
			{
				ChargeGroupCodeInfo.AddError(Res.GetString("be55ce00-413e-40f9-89db-e5ccf83b2487", "Duplicate Codes are entered."));
			}
		}

		#endregion

		#region CheckChargeCodes

		void CheckChargeCodes()
		{
			if (Parent != null && Parent.IsShipnetCarrier)
			{
				if (!ChargeGroupCodeInfo.HasErrors() && Charges.Count == 0)
				{
					ChargeGroupCodeInfo.AddError(Res.GetString("5c7f468d-03e5-4ec5-ba6c-d67e9264a93d", "No charge code is entered for this Group '{0}'.", ChargeGroupCode));
				}
			}
		}

		#endregion

		#region ChargeCollectionSerializer

		ZXmlSerializer fChargeCollectionSerializer;
		ZXmlSerializer ChargeCollectionSerializer
		{
			get
			{
				if (fChargeCollectionSerializer == null)
				{
					fChargeCollectionSerializer = ZXmlSerializer.New(typeof(ShipnetChargeCollection));
				}

				return fChargeCollectionSerializer;
			}
		}

		#endregion

		internal ShipnetSetupBusinessObject Parent;

		#endregion
	}
}
