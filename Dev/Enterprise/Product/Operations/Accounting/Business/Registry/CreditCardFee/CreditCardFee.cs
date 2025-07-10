using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CreditCardFee : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ChargeCodePK = "ChargeCodePK";
			public const string Percentage = "Percentage";
		}

		#endregion

		#region Construction

		public CreditCardFee()
		{
		}

		public CreditCardFee(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CreditCardFee(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Bound Properties

		#region Charge Code

		ZGuid chargeCodePK;

		[List("ChargeCodeList")]
		public ZGuid ChargeCodePK
		{
			get { return chargeCodePK; }
			set
			{
				if (chargeCodePK != value)
				{
					SetNonPersistentPropertyValue(ChargeCodePKInfo, ref chargeCodePK, value);
					BusinessObject accChargeCode = CurrentFactory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, chargeCodePK));
					fChargeCodeDescription = (accChargeCode == null) ? ZString.Empty : (ZString)accChargeCode[AccChargeCodeSchema.Constants.AC_Desc];
				}
				if (!IsValidationSuspended)
				{
					ValidateChargeCodePK();
				}
			}
		}

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodePK); }
		}

		public void ValidateChargeCodePK()
		{
			ChargeCodePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChargeCodePKInfo);
			ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo, ChargeCodeList);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ChargeCodePKInfo, Res.GetString("9ff803c9-32aa-4ed0-99b5-05689c3e85af", "There must be only one line for each country/region."));
			}
		}

		#endregion

		#region Charge Code Description

		ZString fChargeCodeDescription;
		public ZString ChargeCodeDescription
		{
			get { return fChargeCodeDescription; }
		}

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodePK); }
		}

		public void ValidateChargeCodeDescription()
		{
			ValidateChargeCodePK();
		}

		#endregion

		#region Percentage

		ZDecimal percentage;
		public ZDecimal Percentage
		{
			get { return percentage; }
			set
			{
				if (percentage != value)
				{
					SetNonPersistentPropertyValue(PercentageInfo, ref percentage, value);
					if (!IsValidationSuspended)
					{
						ValidatePercentage();
					}
				}
			}
		}

		public ZPropertyInfo PercentageInfo
		{
			get { return GetZPropertyInfo(Schema.Percentage); }
		}

		public void ValidatePercentage()
		{
			PercentageInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PercentageInfo);
		}

		#endregion

		#endregion

		#region Overridded

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateChargeCodePK();
			ValidatePercentage();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditCardFee(fallbackLevel);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ChargeCodePK, ChargeCodePK.ToString());
			writer.WriteElementString(Schema.Percentage, Percentage.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeCodePK = new ZGuid(reader.ReadElementString(Schema.ChargeCodePK));
			Percentage = new ZDecimal(reader.ReadElementString(Schema.Percentage));
		}

		#endregion

		#region ChargeCode List

		BusinessObjectCollection fChargeCodeList;
		public BusinessObjectCollection ChargeCodeList
		{
			get
			{
				if (fChargeCodeList == null)
				{
					fChargeCodeList = new AccChargeCodeCollectionForRegistry(CurrentFactory);
				}

				return fChargeCodeList;
			}
		}

		#endregion
	}
}