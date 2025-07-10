using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class ExcessNPRExclusionEventCodeSetting : RegistryBusinessObjectTemplate
	{
		#region schema and constructors
		protected abstract class Schema
		{
			public const string EventCode = "EventCode";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExcessNPRExclusionEventCodeSetting(fallbackLevel, factory);
		}

		public ExcessNPRExclusionEventCodeSetting()
		{
		}

		public ExcessNPRExclusionEventCodeSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ExcessNPRExclusionEventCodeSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#endregion

		#region EventCode
		[MaxLength(AutoStmEvent.Schema.SE_CodeMaxLength)]
		[List(nameof(EventCodesList))]
		[ResourceStringData("ExcessNPRExclusionEventCodeControl|def9b80d-9e1b-45de-b32c-b2a89795e668", Caption = "Event Code")]
		public ZString EventCode
		{
			get { return eventCode; }
			set
			{
				SetNonPersistentPropertyValue(EventCodeInfo, ref eventCode, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateEventCode();
				}
			}
		}
		ZString eventCode;

		public ZPropertyInfo EventCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EventCode); }
		}

		public void ValidateEventCode()
		{
			EventCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(EventCodeInfo, EventCodesList);
		}

		CodeDescriptionPairList eventCodesList;
		public CodeDescriptionPairList EventCodesList
		{
			get
			{
				if (eventCodesList == null)
				{
					eventCodesList = new StmCustomizableEventCodeDescriptionPairList(new BusinessObjectFactory());
				}
				return eventCodesList;
			}
		}

		#endregion

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EventCode, EventCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EventCode = reader.ReadElementString(Schema.EventCode);
		}
		#endregion
	}
}
