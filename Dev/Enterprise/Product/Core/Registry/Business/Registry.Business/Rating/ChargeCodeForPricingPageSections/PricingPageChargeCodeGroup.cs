using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PricingPageChargeCodeGroup : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ChargeCodePK = nameof(ChargeCodePK);
			public const string ChargeCodeDescription = nameof(ChargeCodeDescription);
		}

		#endregion

		public PricingPageChargeCodeGroup() { }

		public PricingPageChargeCodeGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory,
			PricingPageChargeCodeCollection parentCollection) : base(fallbackLevel, factory)
		{
			SetParentCollection(parentCollection);
		}

		[BusinessObjectTestExclude]
		public PricingPageChargeCodeCollection ParentCollection { get; private set; }

		public void SetParentCollection(PricingPageChargeCodeCollection parentCollection)
		{
			ParentCollection = parentCollection;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PricingPageChargeCodeGroup(fallbackLevel, factory, null);
		}

		#region Bound Properties

		#region ChargeCode PK

		[List("ChargeCodeList")]
		[ResourceStringData("PricingPageChargeCodeGroup|ChargeCodePK", Caption = "Charge Code")]
		public ZGuid ChargeCodePK
		{
			get { return chargeCodePK; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(ChargeCodePKInfo, ref chargeCodePK, value);
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
			if (!IsValidatingChargeCodePK && CurrentFallbackLevel != null)
			{
				IsValidatingChargeCodePK = true;

				ChargeCodePKInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ChargeCodePKInfo);

				// Only validate it when we have a specific fallback information
				if (ShouldValidateChargeCodePK())
				{
					ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo, ChargeCodeList, ParentCollection.ParentConfiguration.GetInvalidChargeCodeMessage());

					if (ParentCollections.Count > 0)
					{
						PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ChargeCodePKInfo, ErrorDuplicateChargeCode);
					}
				}

				IsValidatingChargeCodePK = false;
			}
		}

		protected virtual bool ShouldValidateChargeCodePK()
		{
			return CurrentFallbackLevel != null;
		}

		ZGuid chargeCodePK;
		bool IsValidatingChargeCodePK;

		#endregion

		#region ChargeCode Description

		[ResourceStringData("PricingPageChargeCodeGroup|ChargeCodeDescription", Caption = "Charge Code Description")]
		public ZString ChargeCodeDescription
		{
			get
			{
				var result = "";

				if (ChargeCode != null)
				{
					result = (ZString)ChargeCode[AccChargeCodeSchema.Constants.AC_Desc];
				}

				return result;
			}
		}

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeDescription); }
		}

		#endregion

		public BusinessObjectCollection ChargeCodeList => ParentCollection?.ParentConfiguration?.ChargeCodeList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateChargeCodePK();
		}

		#endregion

		#region ChargeCode BusinessObject

		BusinessObject ChargeCode
		{
			get
			{
				if (fChargeCode == null || fChargeCode.PK != ChargeCodePK)
				{
					fChargeCode = (BusinessObject)CurrentFactory.Load<Enterprise.MasterFiles.Integration.IAccChargeCode>(ChargeCodePK);
				}

				return fChargeCode;
			}
		}

		BusinessObject fChargeCode;

		#endregion

		public static string ErrorDuplicateChargeCode
		{
			get { return Res.GetString("fb6cef2a-062d-459b-a138-e84baeb5c436", "There must be only one the same charge code in the list."); }
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ChargeCodePK, ChargeCodePK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeCodePK = new ZGuid(reader.ReadElementString(Schema.ChargeCodePK));
		}

		#endregion
	}
}
