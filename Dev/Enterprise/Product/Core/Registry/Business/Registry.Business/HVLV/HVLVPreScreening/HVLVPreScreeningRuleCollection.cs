using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new HVLVPreScreeningRule this[int i]
		{
			get { return (HVLVPreScreeningRule)Elements[i]; }
		}

		public new HVLVPreScreeningRule AddNew()
		{
			return (HVLVPreScreeningRule)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var preScreeningRule = new HVLVPreScreeningRule();
			preScreeningRule.SetDefaultModuleType();
			preScreeningRule.SetDefaultEmailNotificationType();
			return preScreeningRule;
		}

		#region GetClone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningRuleCollection();
		}

		#endregion

		public bool IsDuplicateRule(HVLVPreScreeningRule ruleToCheck)
		{
			var result = false;
			foreach (HVLVPreScreeningRule screeningValue in this)
			{
				if (screeningValue != ruleToCheck
					&& screeningValue.TransportMode == ruleToCheck.TransportMode
					&& screeningValue.OriginCountryCode == ruleToCheck.OriginCountryCode
					&& screeningValue.DestinationCountryCode == ruleToCheck.DestinationCountryCode
					&& screeningValue.ModuleType == ruleToCheck.ModuleType
					&& screeningValue.EmailNotificationType == ruleToCheck.EmailNotificationType
					&& screeningValue.EmailNotificationGroup == ruleToCheck.EmailNotificationGroup
					)
				{
					result = true;
					break;
				}
			}

			return result;
		}
	}
}
