using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingUnitCountAdjustment : AutoBillingUnitCountAdjustment
	{
		public BillingUnitCountAdjustment()
		{
		}

		public BillingUnitCountAdjustment(BusinessObjectFactory factory)
			: base(factory) { }

		public BillingUnitCountAdjustment(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public BillingUnitCountAdjustment(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		[BusinessObjectTestExclude] // These properties implement setters on collections, which is generally not advised.
		public BillingUnitCountAdjustmentSettingCollection AdjustmentSettings
		{
			get
			{
				if (adjustmentSettings == null)
				{
					adjustmentSettings = new BillingUnitCountAdjustmentSettingCollection();
				}
				return adjustmentSettings;
			}
			private set { adjustmentSettings = value; }
		}

		BillingUnitCountAdjustmentSettingCollection adjustmentSettings;

		protected override void ReadAdjustmentSettings(XmlReader reader)
		{
			AdjustmentSettings.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("BillingUnitCountAdjustmentSetting"))
				{
					var setting = AdjustmentSettings.AddNew();
					((IXmlSerializable)setting).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WriteAdjustmentSettings(XmlWriter writer)
		{
			foreach (BillingUnitCountAdjustmentSetting setting in AdjustmentSettings)
			{
				writer.WriteStartElement("BillingUnitCountAdjustmentSetting");
				((IXmlSerializable)setting).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			BillingUnitCountAdjustment clone = new BillingUnitCountAdjustment(fallbackLevel, factory);
			clone.AdjustmentSettings = (BillingUnitCountAdjustmentSettingCollection)this.AdjustmentSettings.Clone(fallbackLevel, factory);
			return clone;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();

			if (AdjustmentSettings.Any<BillingUnitCountAdjustmentSetting>() && !AdjustmentSettings.OfType<BillingUnitCountAdjustmentSetting>()
				.Select(x => (int)x.OriginalUnitCount).OrderBy(z => z)
				.SequenceEqual(Enumerable.Range(1, AdjustmentSettings.OfType<BillingUnitCountAdjustmentSetting>().Count())))
			{
				AddRowError("Original Values must be in sequential order from 1.");
			}
		}

		public override void ValidatePriceCode()
		{
			base.ValidatePriceCode();
			MandatoryValidation.CheckEntered(PriceCodeInfo);
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(PriceCodeInfo);
			}
		}

		public static BillingUnitCountAdjustment GetDefaultValue()
		{
			return new BillingUnitCountAdjustment();
		}
	}
}
