using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ChargeCodeWithType : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string PartyType = "PartyType";
			public const string UseDefaultProfitShareChargeCode = "UseDefaultProfitShareChargeCode";
			public const string ChargeCode = "ChargeCode";
			public const string ChargeCodeDescription = "ChargeCodeDescription";
		}

		#endregion

		#region Construction

		public ChargeCodeWithType()
		{
		}

		public ChargeCodeWithType(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public ChargeCodeWithType(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Properties

		#region PartyType

		[MaxLength(100)]
		[ReadOnly(true)]
		public MultilingualString PartyType
		{
			get { return partyType ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				SetNonPersistentPropertyValue(PartyTypeInfo, ref partyType, value, false);
			}
		}
		MultilingualString partyType;

		public ZPropertyInfo PartyTypeInfo
		{
			get { return GetZPropertyInfo(Schema.PartyType); }
		}

		#endregion

		#region UseDefaultProfitShareChargeCode

		public ZBool UseDefaultProfitShareChargeCode
		{
			get { return fUseDefaultProfitShareChargeCode; }
			set
			{
				SetNonPersistentPropertyValue(UseDefaultProfitShareChargeCodeInfo, ref fUseDefaultProfitShareChargeCode, value);

				ChargeCode = ZGuid.Empty;
				ChargeDescriptionInfo.RefreshBinding();
			}
		}
		ZBool fUseDefaultProfitShareChargeCode;

		public ZPropertyInfo UseDefaultProfitShareChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.UseDefaultProfitShareChargeCode); }
		}

		#endregion

		#region ChargeCode

		[List("ChargeCodeList")]
		[ReadOnlyMember(nameof(UseDefaultProfitShareChargeCode))]
		public ZGuid ChargeCode
		{
			get { return fChargeCode; }
			set
			{
				if (fChargeCode != value)
				{
					SetNonPersistentPropertyValue(ChargeCodeInfo, ref fChargeCode, value);

					ChargeDescriptionInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateChargeCode();
				}
			}
		}
		ZGuid fChargeCode;

		public ZPropertyInfo ChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCode); }
		}

		public void ValidateChargeCode()
		{
			ChargeCodeInfo.ClearAllNotifications();
			if (!UseDefaultProfitShareChargeCode)
			{
				MandatoryValidation.CheckEntered(ChargeCodeInfo);
			}
			ListValidation.ErrorIfInvalidPK(ChargeCodeInfo, ChargeCodeList);
		}

		#endregion

		#region ChargeCodeDescription

		public ZString ChargeCodeDescription
		{
			get
			{
				ZString result = ZString.Empty;
				BusinessObject accChargeCode = CurrentFactory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, ChargeCode));
				if (accChargeCode != null)
				{
					result = (ZString)accChargeCode[AccChargeCodeSchema.Constants.AC_Desc];
				}
				else if (UseDefaultProfitShareChargeCode)
				{
					result = Res.GetString("427760db-7b0f-4f9f-9b7a-1527a84a6943", "Default from Profit Share Charge Code Registry Item");
				}

				return result;
			}
		}

		public ZPropertyInfo ChargeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeDescription); }
		}

		#endregion

		#endregion

		#region Lookups

		public IBusinessObjectCollection ChargeCodeList
		{
			get
			{
				if (fChargeCodeList == null)
				{
					var result = new AccChargeCodeCollectionForRegistry(CurrentFactory);
					if (CurrentFallbackLevel != null)
					{
						result.CompanyPK = CurrentFallbackLevel.CompanyPK(false);
					}
					fChargeCodeList = result;
				}
				return fChargeCodeList;
			}
		}
		IBusinessObjectCollection fChargeCodeList;

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateChargeCode();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeWithType(fallbackLevel);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.PartyType, PartyType);
			writer.WriteElementString(Schema.UseDefaultProfitShareChargeCode, UseDefaultProfitShareChargeCode.ToString());
			writer.WriteElementString(Schema.ChargeCode, ChargeCode.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			PartyType = (NoResString)reader.ReadElementString(Schema.PartyType);
			UseDefaultProfitShareChargeCode = new ZBool(reader.ReadElementString(Schema.UseDefaultProfitShareChargeCode));
			ChargeCode = new ZGuid(reader.ReadElementString(Schema.ChargeCode));
		}

		#endregion

		#endregion
	}
}
