using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class BadgeCodeSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public BadgeCodeSettingCollection()
			: base()
		{
		}

		public BadgeCodeSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BadgeCodeSetting this[int i]
		{
			get { return (BadgeCodeSetting)Elements[i]; }
		}

		public new BadgeCodeSetting AddNew()
		{
			return (BadgeCodeSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BadgeCodeSettingCollection(fallbackLevel, factory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((BadgeCodeSetting)child).IsNewBadge = true;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BadgeCodeSetting(CurrentFallbackLevel, CurrentFactory);
		}

		public BadgeCodeSetting FindByPortCode(ZString portCode, ZString importOrExport)
		{
			int highestMatch = 0;
			BadgeCodeSetting result = null;
			if (!portCode.IsEmpty && !importOrExport.IsEmpty)
			{
				foreach (BadgeCodeSetting badgeCodeSetting in this)
				{
					// Port - Matches, is blank, does not match
					// Direction - Matches, is blank, does not match

					if (!badgeCodeSetting.RL_PortCode.IsEmpty && badgeCodeSetting.RL_PortCode != portCode)
					{
						continue;   // Cannot match
					}

					if (!badgeCodeSetting.Direction.IsEmpty && badgeCodeSetting.Direction != importOrExport)
					{
						continue;   // Cannot match
					}

					int matchesCount = 0;
					if (badgeCodeSetting.Direction == importOrExport && !badgeCodeSetting.Direction.IsEmpty)
					{
						matchesCount += 2;
					}
					else if (badgeCodeSetting.Direction.IsEmpty)
					{
						matchesCount += 1;
					}

					if (badgeCodeSetting.RL_PortCode == portCode && !badgeCodeSetting.RL_PortCode.IsEmpty)
					{
						matchesCount += 3;
					}
					else if (badgeCodeSetting.RL_PortCode.IsEmpty)
					{
						matchesCount += 1;
					}

					if (badgeCodeSetting.IsPrimaryBadgeForBranch)
					{
						matchesCount += 1;
					}

					if (matchesCount > highestMatch)
					{
						highestMatch = matchesCount;
						result = badgeCodeSetting;
					}
				}
			}
			return result;
		}

		public BadgeCodeSetting FindByBadgeCode(ZString badgeCodeSought, ZString importOrExport)
		{
			BadgeCodeSetting result = null;
			foreach (BadgeCodeSetting badgeCodeSetting in this)
			{
				if (badgeCodeSetting.BadgeCode == badgeCodeSought)
				{
					if (badgeCodeSetting.Direction == importOrExport)
					{
						result = badgeCodeSetting;
						break;
					}
					if (badgeCodeSetting.Direction.IsEmpty)
					{
						result = badgeCodeSetting;
					}
				}
			}
			return result;
		}

		public BadgeCodeSetting FindByBadgeCodeOnly(ZString badgeCodeSought)
		{
			foreach (BadgeCodeSetting badgeCodeSetting in this)
			{
				if (badgeCodeSetting.BadgeCode == badgeCodeSought)
				{
					return badgeCodeSetting;
				}
			}
			return null;
		}
	}
}
