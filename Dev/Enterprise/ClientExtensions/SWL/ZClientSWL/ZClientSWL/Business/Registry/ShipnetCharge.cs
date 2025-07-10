using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWL.Business
{
	[XmlSerializerAssembly("ZClientSWL.XmlSerializers")]
	public class ShipnetCharge : RegistryBusinessObjectTemplate
	{
		public ShipnetCharge()
		{
		}

		public ShipnetCharge(ShipnetSetupBusinessObject parent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = parent;
		}

		#region Schema

		public static class Schema
		{
			public const string ChargePK = "ChargePK";
			public const string ChargeCodeDesc = "ChargeCodeDesc";
		}

		#endregion

		#region Bound Properties

		#region ChargePK

		[RelatedBusinessObject("AccChargeCode")]
		public ZGuid ChargePK
		{
			get
			{
				return fChargePK;
			}
			set
			{
				if (value != fChargePK)
				{
					SetNonPersistentPropertyValue<ZGuid>(ChargePKInfo, ref fChargePK, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateChargePK();
					}
				}
			}
		}

		ZGuid fChargePK;

		public ZPropertyInfo ChargePKInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ChargePK);
			}
		}

		public AccChargeCode AccChargeCode
		{
			get
			{
				return CurrentFactory.Load<AccChargeCode>(ChargePK);
			}
		}

		void ValidateChargePK()
		{
			ChargePKInfo.ClearAllNotifications();
			if (Parent != null && Parent.IsShipnetCarrier)
			{
				if (!ChargePK.IsValid)
				{
					ChargePKInfo.AddError(Res.GetString("d6c79e12-ea60-44d2-86c4-f1a46b28f5e9", "Please select a valid charge code."));
				}
				else
				{
					CheckDuplicateChargeCodes();
				}
			}
		}

		#endregion

		#region ChargeCodeDesc

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString ChargeCodeDesc
		{
			get
			{
				return (AccChargeCode == null) ? ZString.Empty : AccChargeCode.AC_Desc;
			}
		}

		public ZPropertyInfo ChargeCodeDescInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ChargeCodeDesc);
			}
		}

		#endregion

		#region ChargeCodeList

		public AccChargeCodeCollection ChargeCodeList
		{
			get
			{
				if (fChargeCodeList == null)
				{
					fChargeCodeList = new AccChargeCodeCollection(CurrentFactory);
					fChargeCodeList.Load();
				}
				return fChargeCodeList;
			}
		}

		AccChargeCodeCollection fChargeCodeList;

		#endregion

		#endregion

		#region Implementation

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ShipnetCharge result = new ShipnetCharge(Parent, factory);
			try
			{
				((IBusinessObjectInternals)result).IsCopying = true;
				result.ChargePK = ChargePK;
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
			writer.WriteElementString(Schema.ChargePK, ChargePK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargePK = new ZGuid(reader.ReadElementString(Schema.ChargePK));
			HasChanges = false;
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateChargePK();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			ChargePK = new ZGuid("11111111-1111-1111-1111-111111111111");
		}
#endif
#endregion

		#region CheckDuplicateChargeCodes

		void CheckDuplicateChargeCodes()
		{
			if (ParentCollections.Count > 0 && ((ShipnetChargeCollection)ParentCollections.First()).IsDuplicateCharge(this))
			{
				ChargePKInfo.AddError(Res.GetString("0ab2c0bc-6166-4baa-9795-13c62fb4dec5", "Duplicate charge codes are entered."));
			}
		}

		#endregion

		internal ShipnetSetupBusinessObject Parent;

		#endregion
	}
}
