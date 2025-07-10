using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PurgeSettings : RegistryBusinessObjectTemplate
	{
		PurgeSettingsRegistryDataType dataType;

		public PurgeSettings()
		{
		}

		public PurgeSettings(PurgeSettingsRegistryDataType dataType)
		{
			this.dataType = dataType;
		}

		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string BillingMonths = "BillingMonths";
			public const string BatchSize = "BatchSize";
		}

		#endregion

		#region Properties

		#region ApplicationCodes

		[ChildEditable]
		public ApplicationCodeObjCollection ApplicationCodes
		{
			get
			{
				if (applicationCodes == null)
				{
					applicationCodes = new ApplicationCodeObjCollection();
					RegisterEditableChildObject(applicationCodes);
				}
				return applicationCodes;
			}
		}

		ApplicationCodeObjCollection applicationCodes;

		public void SetApplicationCodes(ApplicationCodeObjCollection applicationCodes)
		{
			UnRegisterEditableChildObject(ApplicationCodes);
			this.applicationCodes = applicationCodes;
			RegisterEditableChildObject(ApplicationCodes);
		}

		#endregion

		#region HiddenApplicationCodes

		public ApplicationCodeObjCollection HiddenApplicationCodes
		{
			get { return hiddenApplicationCodes ?? (hiddenApplicationCodes = new ApplicationCodeObjCollection()); }
		}

		ApplicationCodeObjCollection hiddenApplicationCodes;

		public void SetHiddenApplicationCodes(ApplicationCodeObjCollection applicationCodes)
		{
			hiddenApplicationCodes = applicationCodes;
		}

		#endregion HiddenApplicationCodes

		#region BatchSize

		public ZInt BatchSize
		{
			get
			{
				return batchSize;
			}
			set
			{
				SetNonPersistentPropertyValue(BatchSizeInfo, ref batchSize, value);
				if (!IsValidationSuspended)
				{
					ValidateBatchSize();
				}
			}
		}

		public ZPropertyInfo BatchSizeInfo
		{
			get { return GetZPropertyInfo(Schema.BatchSize, "Message Purge Batch Size"); }
		}

		public void ValidateBatchSize()
		{
			BatchSizeInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(BatchSizeInfo, DefaultBatchSize, MaxBatchSize);
		}

		ZInt batchSize = DefaultBatchSize;
		const int DefaultBatchSize = 100;
		const int MaxBatchSize = 10000;

		#endregion

		PurgeSettingsRegistryDataType DataType
		{
			get
			{
				if (this.dataType == null)
				{
					this.dataType = new PurgeSettingsRegistryDataType();
				}

				return this.dataType;
			}
		}

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var bytes = this.DataType.Serialise(this);
			return this.DataType.Deserialise(bytes);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			CollectionSerialiser.Serialize(writer, ApplicationCodes);
			writer.WriteElementString(Schema.BillingMonths, string.Empty);
			CollectionSerialiser.Serialize(writer, HiddenApplicationCodes);
			writer.WriteElementString(Schema.BatchSize, BatchSize.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var applicationCodes = new ApplicationCodeObjCollectionMergeManager().MergeApplicationCodeObjCollections(eHubMessagingRegistry.GetDefaultPurgeSettings().ApplicationCodes, (ApplicationCodeObjCollection)CollectionSerialiser.Deserialize(reader));
			SetApplicationCodes(applicationCodes);
			reader.ReadElementString(Schema.BillingMonths);
			hiddenApplicationCodes = (ApplicationCodeObjCollection)CollectionSerialiser.Deserialize(reader);
			batchSize = reader.ReadElementStringAsZInt(Schema.BatchSize);
			if (batchSize == ZInt.Zero)
			{
				batchSize = DefaultBatchSize;
			}
		}

		ZXmlSerializer CollectionSerialiser
		{
			get { return collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(ApplicationCodeObjCollection))); }
		}

		ZXmlSerializer collectionSerialiser;

		#endregion
	}
}
