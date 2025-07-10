using System.Xml;
using System.Xml.Serialization;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class InternalIncidentLicenceSettings : AutoInternalIncidentLicenceSettings
	{
		public InternalIncidentLicenceSettings() { }

		public InternalIncidentLicenceSettings(BusinessObjectFactory factory)
			: base(factory) { }

		public InternalIncidentLicenceSettings(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public InternalIncidentLicenceSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new BusinessObjectFactory CurrentFactory
		{
			get { return base.CurrentFactory; }
		}

		#region Properties

		[List("Lookups.LicenceList")]
		public override ZGuid EdiProd_LicencePK
		{
			get { return base.EdiProd_LicencePK; }
			set { base.EdiProd_LicencePK = value; }
		}

		public LicenceHeader EdiProd_Licence
		{
			get { return CurrentFactory.Load<LicenceHeader>(EdiProd_LicencePK); }
		}

		[List("Lookups.LicenceList")]
		public override ZGuid UAT_ALP_LicencePK
		{
			get { return base.UAT_ALP_LicencePK; }
			set { base.UAT_ALP_LicencePK = value; }
		}

		public LicenceHeader UAT_ALP_Licence
		{
			get { return CurrentFactory.Load<LicenceHeader>(UAT_ALP_LicencePK); }
		}

		[List("Lookups.LicenceList")]
		public override ZGuid UAT_DPR_LicencePK
		{
			get { return base.UAT_DPR_LicencePK; }
			set { base.UAT_DPR_LicencePK = value; }
		}

		public LicenceHeader UAT_DPR_Licence
		{
			get { return CurrentFactory.Load<LicenceHeader>(UAT_DPR_LicencePK); }
		}

		[List("Lookups.LicenceList")]
		public override ZGuid UAT_STD_LicencePK
		{
			get { return base.UAT_STD_LicencePK; }
			set { base.UAT_STD_LicencePK = value; }
		}

		public LicenceHeader UAT_STD_Licence
		{
			get { return CurrentFactory.Load<LicenceHeader>(UAT_STD_LicencePK); }
		}

		[List("Lookups.LicenceList")]
		public override ZGuid UAT_GPC_LicencePK
		{
			get { return base.UAT_GPC_LicencePK; }
			set { base.UAT_GPC_LicencePK = value; }
		}

		public LicenceHeader UAT_GPC_Licence
		{
			get { return CurrentFactory.Load<LicenceHeader>(UAT_GPC_LicencePK); }
		}

		[List("Lookups.LicenceList")]
		public override ZGuid UAT_GPR_LicencePK
		{
			get { return base.UAT_GPR_LicencePK; }
			set { base.UAT_GPR_LicencePK = value; }
		}

		public LicenceHeader UAT_GPR_Licence
		{
			get { return CurrentFactory.Load<LicenceHeader>(UAT_GPR_LicencePK); }
		}

		#endregion

		#region Licence Enterprise Keys

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public LicenceEnterpriseKeyCollection LicenceEnterpriseKeys
		{
			get
			{
				if (licenceEnterpriseKeys == null)
				{
					licenceEnterpriseKeys = new LicenceEnterpriseKeyCollection(CurrentFactory);
				}
				return licenceEnterpriseKeys;
			}
			private set { licenceEnterpriseKeys = value; }
		}
		LicenceEnterpriseKeyCollection licenceEnterpriseKeys;

		protected override void ReadLicenceEnterpriseKeys(XmlReader reader)
		{
			LicenceEnterpriseKeys.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("LicenceEnterpriseKey"))
				{
					LicenceEnterpriseKey enterprise = LicenceEnterpriseKeys.AddNew();
					((IXmlSerializable)enterprise).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WriteLicenceEnterpriseKeys(XmlWriter writer)
		{
			foreach (LicenceEnterpriseKey enterprise in LicenceEnterpriseKeys)
			{
				writer.WriteStartElement("LicenceEnterpriseKey");
				((IXmlSerializable)enterprise).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		#region Lookups

		public InternalIncidentLicenceSettingsLookups Lookups
		{
			get { return lookups ?? (lookups = new InternalIncidentLicenceSettingsLookups(this)); }
		}
		InternalIncidentLicenceSettingsLookups lookups;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			InternalIncidentLicenceSettings clone = new InternalIncidentLicenceSettings(fallbackLevel, factory);
			clone.LicenceEnterpriseKeys = (LicenceEnterpriseKeyCollection)this.LicenceEnterpriseKeys.Clone(fallbackLevel, factory);
			return clone;
		}

		#endregion

		#region Validation

		public override void ValidateEdiProd_LicencePK()
		{
			base.ValidateEdiProd_LicencePK();
			MandatoryValidation.CheckEntered(EdiProd_LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(EdiProd_LicencePKInfo);
		}

		public override void ValidateUAT_ALP_LicencePK()
		{
			base.ValidateUAT_ALP_LicencePK();
			MandatoryValidation.CheckEntered(UAT_ALP_LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(UAT_ALP_LicencePKInfo);
		}

		public override void ValidateUAT_DPR_LicencePK()
		{
			base.ValidateUAT_DPR_LicencePK();
			MandatoryValidation.CheckEntered(UAT_DPR_LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(UAT_DPR_LicencePKInfo);
		}

		public override void ValidateUAT_STD_LicencePK()
		{
			base.ValidateUAT_STD_LicencePK();
			MandatoryValidation.CheckEntered(UAT_STD_LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(UAT_STD_LicencePKInfo);
		}

		public override void ValidateUAT_GPC_LicencePK()
		{
			base.ValidateUAT_GPC_LicencePK();
			MandatoryValidation.CheckEntered(UAT_GPC_LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(UAT_GPC_LicencePKInfo);
		}

		public override void ValidateUAT_GPR_LicencePK()
		{
			base.ValidateUAT_GPR_LicencePK();
			MandatoryValidation.CheckEntered(UAT_GPR_LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(UAT_GPR_LicencePKInfo);
		}

		#endregion

		#region Default Value

		public static InternalIncidentLicenceSettings GetDefaultValue()
		{
			return new InternalIncidentLicenceSettings();
		}

		#endregion
	}
}

