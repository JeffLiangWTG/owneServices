using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class CategorisedAssistWithThisTaskSetting : RegistryBusinessObject
	{
		[ChildEditable]
		public AssistWithThisTaskSettingForOneWorkflowType Setting
		{
			get => setting ?? (Setting = new AssistWithThisTaskSettingForOneWorkflowType { Code = Code });
			private set
			{
				setting = value;
				RegisterEditableChildObject(value);
			}
		}
		AssistWithThisTaskSettingForOneWorkflowType setting;

		protected override int MaxDescriptionLength => 256;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (Setting.HasErrors)
			{
				foreach (var error in Setting.GetErrors())
				{
				}
				ThrowValidationExceptionToPreventSave();
			}
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CategorisedAssistWithThisTaskSetting();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			((CategorisedAssistWithThisTaskSetting)clone).Setting = (AssistWithThisTaskSettingForOneWorkflowType)Setting.Clone(CurrentFallbackLevel, Factory);
		}

		#endregion

		#region Xml Serialization

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			Setting = (AssistWithThisTaskSettingForOneWorkflowType)SettingSerializer.Deserialize(reader);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			SettingSerializer.Serialize(writer, Setting);
		}

		ZXmlSerializer SettingSerializer => settingSerializer ?? (settingSerializer = ZXmlSerializer.New(typeof(AssistWithThisTaskSettingForOneWorkflowType)));
		ZXmlSerializer settingSerializer;

		#endregion
	}
}
