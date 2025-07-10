using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CASSChargeCode : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string CASSType = "CASSType";
			public const string CASSComponentCode = "CASSComponentCode";
			public const string CASSComponentDescription = "CASSComponentDescription";
			public const string ChargeCodePK = "ChargeCodePK";
			public const string ChargeDescription = "ChargeDescription";
		}

		#endregion

		#region Constructor
		public CASSChargeCode(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CASSChargeCode()
			: base()
		{
		}
		#endregion

		#region ParentCollection
		public CASSChargeCodeCollection CASSChargeCodeCollection
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(CASSChargeCodeCollection)) as CASSChargeCodeCollection;
				return parentCollection ?? new CASSChargeCodeCollection(this.CurrentFallbackLevel, Factory);
			}
		}
		#endregion

		#region CASSType

		ZString cASSType;

		[List("Lookups.CASSTypesList")]
		public ZString CASSType
		{
			get { return cASSType; }
			set
			{
				cASSType = value;
				if (!IsValidationSuspended)
				{
					ValidateCASSType();
					ValidateCASSComponent();
				}
				CASSTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CASSTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CASSType); }
		}

		#endregion

		#region CASSComponent

		ZString cASSComponentCode;

		[List("Lookups.CASSLineComponentList")]
		public ZString CASSComponentCode
		{
			get { return cASSComponentCode; }
			set
			{
				cASSComponentCode = value;
				if (!IsValidationSuspended)
				{
					ValidateCASSComponent();
				}
				CASSComponentCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CASSComponentCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CASSComponentCode); }
		}

		public ZString CASSComponentDescription
		{
			get
			{
				return Lookups.CASSLineComponentList.GetDescriptionFromCode(CASSComponentCode);
			}
		}

		public ZPropertyInfo CASSComponentDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CASSComponentDescription)); }
		}
		#endregion

		#region Charge Code

		ZGuid chargeCodePK;

		[List("Lookups.ChargeCodeList")]
		public ZGuid ChargeCodePK
		{
			get { return chargeCodePK; }
			set
			{
				if (chargeCodePK != value)
				{
					SetNonPersistentPropertyValue(ChargeCodePKInfo, ref chargeCodePK, value);
					chargeCode = null;
					if (!IsValidationSuspended)
					{
						ValidateChargeCode();
					}
					ChargeCodePKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodePK); }
		}

		#endregion

		#region Charge Description

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString ChargeDescription
		{
			get
			{
				if (chargeCode == null)
				{
					chargeCode = Factory.Load<AccChargeCode>(ChargeCodePK);
				}
				return chargeCode != null ? chargeCode.AC_Desc : ZString.Empty;
			}
		}
		AccChargeCode chargeCode;

		public ZPropertyInfo ChargeDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ChargeDescription); }
		}

		#endregion

		#region Validation
		void ValidateCASSType()
		{
			CASSTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CASSTypeInfo);
			ListValidation.ErrorIfInvalidCode(CASSTypeInfo);

			if (!CASSTypeInfo.HasErrors())
			{
				CheckForDuplicateRow(CASSTypeInfo);
			}
			if (!CASSTypeInfo.HasErrors())
			{
				CheckIntegrity();
			}
		}

		void ValidateCASSComponent()
		{
			CASSComponentCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CASSComponentCodeInfo);
			ListValidation.ErrorIfInvalidCode(CASSComponentCodeInfo);

			if (!CASSComponentCodeInfo.HasErrors())
			{
				CheckForDuplicateRow(CASSComponentCodeInfo);
			}
			if (!CASSComponentCodeInfo.HasErrors())
			{
				CheckIntegrity();
			}
		}

		void ValidateChargeCode()
		{
			ChargeCodePKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChargeCodePKInfo);
			ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo);
			if (!ChargeCodePKInfo.HasErrors())
			{
				CheckForDuplicateRow(ChargeCodePKInfo);
			}
		}

		void CheckForDuplicateRow(ZPropertyInfo propertyInfo)
		{
			foreach (CASSChargeCode chargeCodeListElement in CASSChargeCodeCollection)
			{
				if (chargeCodeListElement.PK != PK && chargeCodeListElement.ChargeCodePK == ChargeCodePK && chargeCodeListElement.CASSComponentCode == CASSComponentCode && chargeCodeListElement.CASSType == CASSType)
				{
					propertyInfo.AddError(Res.GetString("bc81c187-459a-4371-95ca-4cc0b912dc5b", "Another row already exists with same Charge Code, CASS Component and CASS Type."));
					break;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void CheckIntegrity(bool showInvalidRowErrorOnly = true)
		{
			var cASSChargeCodes = CASSChargeCodeCollection.Cast<CASSChargeCode>();

			if (cASSChargeCodes.Any(x => x.CASSType == CASSChargeCodeLookups.ALL) && !cASSChargeCodes.All(x => x.CASSType == CASSChargeCodeLookups.ALL))
			{
				CASSTypeInfo.AddError(Res.GetString("ee382616-e592-4c7a-901e-77344d18bbbf", "Settings for a specific CASS Type is not allowed, as there is a settings for 'ALL' CASS Type."));
			}
			else if (!cASSChargeCodes.Any(x => x.CASSType == CASSChargeCodeLookups.ALL))
			{
				var missingCASSType = new List<string>();
				var missingCASSComponentCode = new List<string>();

				var cASSTypeList = MetaData.GetListDataSource(this, CASSTypeInfo.PropertyDescriptor).Cast<CodeDescriptionPair>().Where(x => x.Code != CASSChargeCodeLookups.ALL);
				foreach (CodeDescriptionPair cASSType in cASSTypeList)
				{
					var cassChargeCodesByCASSType = cASSChargeCodes.Where(x => x.CASSType == cASSType.Code).ToArray();

					if (cassChargeCodesByCASSType.Length > 0)
					{
						if (cassChargeCodesByCASSType.Any(x => x.CASSComponentCode == CASSChargeCodeLookups.ALL) && !cassChargeCodesByCASSType.All(x => x.CASSComponentCode == CASSChargeCodeLookups.ALL))
						{
							CASSComponentCodeInfo.AddError(Res.GetString("1f65a852-5692-4af3-84b0-cda9c630e6c2", "Settings for a specific CASS Component with CASS Type '{0}' is not allowed, as there is a settings for 'ALL' CASS Component.", cASSType.Code));
						}
						else if (!cassChargeCodesByCASSType.Any(x => x.CASSComponentCode == CASSChargeCodeLookups.ALL))
						{
							var sampleCASSChargeCode = cassChargeCodesByCASSType.Where(x => x.CASSType == cASSType.Code).First();
							var cassComponentListWithoutALL = MetaData.GetListDataSource(sampleCASSChargeCode, sampleCASSChargeCode.CASSComponentCodeInfo.PropertyDescriptor).Cast<CodeDescriptionPair>().Where(x => x.Code != CASSChargeCodeLookups.ALL).Select(x => x.Code);
							var cassComponentsByCASSType = cassChargeCodesByCASSType.Select(x => x.CASSComponentCode.ToString());
							missingCASSComponentCode.AddRange(cassComponentListWithoutALL.Except(cassComponentsByCASSType));
						}
					}
					else
					{
						missingCASSType.Add(cASSType.Description);
					}
				}

				if (!showInvalidRowErrorOnly && missingCASSType.Count > 0)
				{
					CASSTypeInfo.AddError(Res.GetString("5055252c-0178-4282-a8e0-9e72facc85d6", "You must add settings for CASS Type: {0}.", String.Join(", ", missingCASSType.ToArray())));
				}
				if (!showInvalidRowErrorOnly && missingCASSComponentCode.Count > 0)
				{
					CASSComponentCodeInfo.AddError(Res.GetString("5f937161-4896-48c5-8546-f753f06736c4", "You must add settings for CASS Component Code: {0}.", String.Join(", ", missingCASSComponentCode.ToArray())));
				}
			}
		}
		#endregion

		#region Overrides

		public new BusinessObjectFactory Factory
		{
			get { return CurrentFactory; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			this.CurrentFallbackLevel = fallbackLevel;
			return new CASSChargeCode(fallbackLevel, factory);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CASSType, CASSType);
			writer.WriteElementString(Schema.CASSComponentCode, CASSComponentCode);
			writer.WriteElementString(Schema.ChargeCodePK, ChargeCodePK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CASSType = reader.ReadElementString(Schema.CASSType);
			CASSComponentCode = reader.ReadElementString(Schema.CASSComponentCode);
			ChargeCodePK = new ZGuid(reader.ReadElementString(Schema.ChargeCodePK));
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				base.RunPreSaveValidationCore();
				ValidateCASSType();
				ValidateCASSComponent();
				ValidateChargeCode();
				CheckIntegrity(false);
			}
		}

		#endregion

		public CASSChargeCodeLookups Lookups
		{
			get
			{
				return new CASSChargeCodeLookups(this);
			}
		}
	}
}
