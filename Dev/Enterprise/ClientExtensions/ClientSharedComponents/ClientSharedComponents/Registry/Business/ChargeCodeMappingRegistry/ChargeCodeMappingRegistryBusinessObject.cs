using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class ChargeCodeMappingRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public ChargeCodeMappingRegistryBusinessObject()
			: base()
		{
		}

		public ChargeCodeMappingRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ChargeCodeMappingRegistryBusinessObject(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public ChargeCodeMappingRegistryBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string CodePK = "CodePK";
			public const string ExternalCode = "ExternalCode";
		}

		#endregion

		#region CodePK

		public ZGuid CodePK
		{
			get { return codePK; }
			set
			{
				SetNonPersistentPropertyValue(CodePKInfo, ref codePK, value);
				if (!IsValidationSuspended)
				{
					ValidateCodePK();
				}

				CodePKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CodePKInfo
		{
			get { return GetZPropertyInfo(Schema.CodePK, "Code"); }
		}
		ZGuid codePK;

		public void ValidateCodePK()
		{
			CodePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodePKInfo);
			if (!CodePKInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(CodePKInfo, ChargeCodes);
			}
		}

		public AccChargeCode CurrentChargeCode
		{
			get
			{
				return CurrentFactory.Load<AccChargeCode>(CodePK);
			}
		}

		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				return chargeCodes ?? (chargeCodes = (CurrentFallbackLevel != null && CurrentFallbackLevel.CompanyPK(false) != Guid.Empty) ? new AccChargeCodeCollection(CurrentFactory, new ZQuery(),
						CurrentFallbackLevel.CompanyPK(false)) : new AccChargeCodeCollection(CurrentFactory));
			}
		}
		AccChargeCodeCollection chargeCodes;

		#endregion

		#region ExternalCode

		[MaxLength(100)]
		public ZString ExternalCode
		{
			get
			{
				return externalCode;
			}
			set
			{
				CheckMaximumLength(ExternalCodeInfo, value);
				SetNonPersistentPropertyValue(ExternalCodeInfo, ref externalCode, value);
				if (!IsValidationSuspended)
				{
					ValidateExternalCode();
				}

				ExternalCodeInfo.RefreshBinding();
			}
		}
		ZString externalCode;

		public ZPropertyInfo ExternalCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ExternalCode);
			}
		}

		void ValidateExternalCode()
		{
			ExternalCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ExternalCodeInfo);
		}

		#endregion

		#region Write/Read XML

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CodePK, CodePK.ToString());
			writer.WriteElementString(Schema.ExternalCode, ExternalCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CodePK = new ZGuid(reader.ReadElementString(Schema.CodePK));
			ExternalCode = reader.ReadElementString(Schema.ExternalCode);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeMappingRegistryBusinessObject(fallbackLevel);
		}

		#endregion
	}
}
