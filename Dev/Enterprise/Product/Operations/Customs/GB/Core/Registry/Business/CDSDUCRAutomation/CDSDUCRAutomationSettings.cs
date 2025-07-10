using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CDSDUCRAutomationSettings : RegistryBusinessObjectTemplate
	{
		public abstract class Schema
		{
			public const string CDSDUCRAutomation = "CDSDUCRAutomation";
		}

		[MaxLength(5)]
		[List(nameof(CDSUCRAutomationSettingsList))]
		public ZString CDSDUCRAutomation
		{
			get { return fCDSDUCRAutomation; }
			set
			{
				if (fCDSDUCRAutomation != value)
				{
					CheckMaximumLength(CDSDUCRAutomationInfo, value);
					fCDSDUCRAutomation = value;
				}

				if (!IsValidationSuspended)
				{
					if (string.IsNullOrEmpty(fCDSDUCRAutomation))
					{
						CDSDUCRAutomationInfo.AddError("CDS DUCR Automation cannot be empty");
					}
					else
					{
						ValidateCDSUCRAutomationSettings();
					}
				}
				CDSDUCRAutomationInfo.RefreshBinding();
			}
		}
		ZString fCDSDUCRAutomation;

		public ZPropertyInfo CDSDUCRAutomationInfo
		{
			get { return GetZPropertyInfo(Schema.CDSDUCRAutomation); }
		}

		public void ValidateCDSUCRAutomationSettings()
		{
			CDSDUCRAutomationInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid CDS DUCR Automation Code", CDSDUCRAutomationInfo);
		}

		public CodeDescriptionPairList CDSUCRAutomationSettingsList
		{
			get { return CurrentFactory.GetCachedValue<CDSUCRAutomationSettingsList>(); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel,
			BusinessObjectFactory factory)
		{
			return new CDSDUCRAutomationSettings(fallbackLevel, factory);
		}

		public CDSDUCRAutomationSettings()
		{
		}

		public CDSDUCRAutomationSettings(BusinessObjectFactory factory)
	: base(factory)
		{
		}

		public CDSDUCRAutomationSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CDSDUCRAutomation, CDSDUCRAutomation.ToUpperInvariant());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			CDSDUCRAutomation = reader.ReadElementString(Schema.CDSDUCRAutomation);
		}
		#endregion
	}
}
