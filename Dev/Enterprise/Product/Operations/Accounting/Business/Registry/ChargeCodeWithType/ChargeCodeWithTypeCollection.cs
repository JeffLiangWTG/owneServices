using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ChargeCodeWithTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ChargeCodeWithTypeCollection()
		{
		}

		public ChargeCodeWithTypeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Implementation

		public new ChargeCodeWithType AddNew()
		{
			return (ChargeCodeWithType)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeWithTypeCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChargeCodeWithType(CurrentFallbackLevel);
		}

		#endregion

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public new ChargeCodeWithType this[int index]
		{
			get { return (ChargeCodeWithType)Elements[index]; }
		}

		public ChargeCodeWithType this[string key]
		{
			get
			{
				foreach (ChargeCodeWithType chargeCodeWithType in this)
				{
					if (chargeCodeWithType.PartyType == key)
					{
						return chargeCodeWithType;
					}
				}
				return null;
			}
		}

		public static ChargeCodeWithTypeCollection GetDefaultCollection()
		{
			var result = new ChargeCodeWithTypeCollection();
			foreach (CodeDescriptionPair pair in GetPartyTypes())
			{
				AddNewDefaultItem(result, pair.MultilingualDescription);
			}
			return result;
		}

		public ZGuid GetCode(string partyTypeCode)
		{
			var result = ZGuid.Empty;
			ZString partyType = GetPartyTypes().GetDescriptionFromCode(partyTypeCode);
			if (!partyType.IsEmpty)
			{
				result = GetGuidFromChargeCode(this[partyType]);
			}

			return result;
		}

		ZGuid GetGuidFromChargeCode(ChargeCodeWithType chargeCodeWithType)
		{
			ZGuid result = ZGuid.Empty;
			if (chargeCodeWithType != null)
			{
				if (chargeCodeWithType.UseDefaultProfitShareChargeCode)
				{
					result = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
				}
				else
				{
					result = chargeCodeWithType.ChargeCode;
				}
			}

			return result;
		}

		static CodeDescriptionPairList GetPartyTypes()
		{
			return RegistryFactory.Instance.GetCachedValue("ChargeCodeWithTypeCollection.GetPartyTypes", () => new OrgProfitSharePartyLookups().PartyTypes);
		}

		/// <summary>
		/// Ensure all the default charge codes with types are added to the collection.
		/// This does not check if existing items in the collection are valid or not.
		/// </summary>
		public void EnsureDefaultPartyTypesExist()
		{
			foreach (CodeDescriptionPair pair in GetPartyTypes())
			{
				var partyType = pair.MultilingualDescription;
				if (this[partyType] == null)
				{
					AddNewDefaultItem(this, partyType);
				}
			}
		}

		static void AddNewDefaultItem(ChargeCodeWithTypeCollection collection, MultilingualString partyType)
		{
			var chargeCodeWithType = collection.AddNew();
			chargeCodeWithType.PartyType = partyType;
			chargeCodeWithType.ChargeCode = ZGuid.Empty;
			chargeCodeWithType.UseDefaultProfitShareChargeCode = true;
		}
	}
}
