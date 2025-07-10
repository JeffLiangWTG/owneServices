using System.Data;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderDiscount : AutoEdiPriceHeaderDiscount
	{
		public EdiPriceHeaderDiscount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString NameDescription
		{
			get
			{
				return EDIDataRegistry.Instance.StlDiscountTypes.Value.GetDescriptionFromCode(PHD_Name);
			}
		}

		public ZPropertyInfo NameDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(NameDescription)); }
		}

		[List(nameof(Lookups) + "." + nameof(EdiPriceHeaderDiscountLookups.ActiveDiscountNames))]
		public override ZString PHD_Name
		{
			get { return base.PHD_Name; }
			set { base.PHD_Name = value; }
		}

		[List(nameof(Lookups) + "." + nameof(EdiPriceHeaderDiscountLookups.DiscountTypes))]
		public override ZString PHD_Type
		{
			get { return base.PHD_Type; }
			set
			{
				if (base.PHD_Type != value)
				{
					// Must clear the config before changing value so
					// value_changed listeners get the correct config type.
					if (config != null)
					{
						config.HasChangesChanged -= Config_HasChangesChanged;
						UnRegisterEditableChildObject(config);
					}
					config = null;
					PHD_ConfigXml = "";

					base.PHD_Type = value;

					if (Config != null)
					{
						PHD_Percent = 0;
					}
				}
			}
		}

		public ZString DiscountTypeDesc
		{
			get { return Lookups.DiscountTypes.GetDescriptionFromCode(PHD_Type); }
		}

		/// <summary>
		/// Configuration specific to the PHD_Type
		/// </summary>
		public BusinessObject Config
		{
			get
			{
				if (config == null)
				{
					config = CreateConfig();
					if (config != null)
					{
						if (!PHD_ConfigXml.IsEmpty)
						{
							ClearConfigHasChanges();
						}

						using (SuspendSettingHasChanges())
						{
							PHD_ConfigXml = SerializeConfig();
						}
						config.HasChangesChanged += Config_HasChangesChanged;
						RegisterEditableChildObject(config);
					}
				}

				return config;
			}
		}

		BusinessObject CreateConfig()
		{
			BusinessObject result;
			switch ((string)PHD_Type)
			{
				case BillingConstants.DiscountCalculator.DevelopingCountry: result = CountryDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.DomesticEntity: result = DomesticDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.Volume: result = VolumeDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.WiseCloud: result = WiseCloudDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.SingleCountry: result = SingleCountryDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.OrgMembership: result = OrgMembershipDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.MasterOrgDevelopingCountry: result = MasterOrgDevelopingCountryDiscount.NewFromXml(PHD_ConfigXml); break;
				case BillingConstants.DiscountCalculator.ProductBundle: result = ProductBundleDiscount.NewFromXml(PHD_ConfigXml, this); break;
				default:
					result = null; break;
			}

			return result;
		}

		void Config_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (config != null)
			{
				PHD_ConfigXml = SerializeConfig();
			}
		}

		string SerializeConfig()
		{
			var serializer = ZXmlSerializer.New(config.GetType());
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(writer, config);
				return writer.ToString();
			}
		}

		BusinessObject config;

		public bool PHD_Percent_ReadOnly
		{
			get
			{
				return PHD_Type != BillingConstants.DiscountCalculator.WiseCloud
					&& PHD_Type != BillingConstants.DiscountCalculator.SingleCountry
					&& PHD_Type != BillingConstants.DiscountCalculator.OrgMembership
					&& PHD_Type != BillingConstants.DiscountCalculator.ProductBundle
					&& Config != null;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				ClearConfigHasChanges();
			}
		}

		void ClearConfigHasChanges()
		{
			if (config != null && config.HasChanges)
			{
				var children = ((IBusiness)config).Children;
				children.ForEach(x => x.HasChanges = false);
				config.HasChanges = false;
			}
		}
	}
}

