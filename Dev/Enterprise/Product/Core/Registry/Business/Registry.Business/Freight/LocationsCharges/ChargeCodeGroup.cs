using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeCodeGroup : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ChargeCodePK = "ChargeCodePK";
			public const string ChargeCodeDescription = "ChargeCodeDescription";
		}

		#endregion

		public ChargeCodeGroup() { }

		public ChargeCodeGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeGroup(fallbackLevel, factory);
		}

		#region Bound Properties

		#region ChargeCode PK

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
			if (!IsValidatingChargeCodePK)
			{
				IsValidatingChargeCodePK = true;

				ChargeCodePKInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ChargeCodePKInfo);

				if (ShouldValidateChargeCodePK())
				{
					ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo, ChargeCodeList);
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

		public ZString ChargeCodeDescription
		{
			get
			{
				ZString result = "";

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

		#endregion

		#region ChargeCode List

		public BusinessObjectCollection ChargeCodeList
		{
			get
			{
				if (fChargeCodeList == null || HasFallbackLevelChanged)
				{
					if (CurrentFallbackLevel != null)
					{
						fChargeCodeList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), new object[] { CurrentFactory, new ZQuery(), CurrentFallbackLevel.CompanyPK(false) });
					}
					else
					{
						fChargeCodeList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), new object[] { CurrentFactory });
					}
				}

				return fChargeCodeList;
			}
		}

		bool HasFallbackLevelChanged
		{
			get
			{
				bool result = false;

				if (CurrentChargeCodeListFallbackLevel != CurrentFallbackLevel)
				{
					result = true;
					CurrentChargeCodeListFallbackLevel = CurrentFallbackLevel;
				}

				return result;
			}
		}

		FallbackLevel CurrentChargeCodeListFallbackLevel;
		BusinessObjectCollection fChargeCodeList;

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
