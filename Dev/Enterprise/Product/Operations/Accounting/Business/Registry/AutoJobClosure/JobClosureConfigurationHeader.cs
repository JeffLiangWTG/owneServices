using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobClosureConfigurationHeader : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string CloseJobsWithOpenWipAcr = "CloseJobsWithOpenWipAcr";
			public const string BackPostWipAcr = "BackPostWipAcr";
		}

		#endregion

		public JobClosureConfigurationHeader()
		{
		}

		public JobClosureConfigurationHeader(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobClosureConfigurationHeader(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			JobClosureConfigurationHeader castedClone = (JobClosureConfigurationHeader)clone;
			if (jobConfigurationCollection != null)
			{
				castedClone.jobConfigurationCollection = (JobClosureConfigurationCollection)ConfigurationCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.ConfigurationCollection);
			}
		}

		#region Bound Properties

		#region JobClosureConfigurationCollection

		public JobClosureConfigurationCollection ConfigurationCollection
		{
			get
			{
				if (jobConfigurationCollection == null)
				{
					jobConfigurationCollection = new JobClosureConfigurationCollection();
					RegisterEditableChildObject(jobConfigurationCollection);
				}
				return jobConfigurationCollection;
			}
		}
		JobClosureConfigurationCollection jobConfigurationCollection;

		ZXmlSerializer fJobClosureConfigurationCollectionSerialiser;
		ZXmlSerializer JobClosureConfigurationCollectionSerialiser
		{
			get
			{
				return fJobClosureConfigurationCollectionSerialiser ?? (fJobClosureConfigurationCollectionSerialiser = ZXmlSerializer.New(typeof(JobClosureConfigurationCollection)));
			}
		}

		#endregion

		#endregion

		#region Validation

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			JobClosureConfigurationCollectionSerialiser.Serialize(writer, ConfigurationCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			jobConfigurationCollection = (JobClosureConfigurationCollection)JobClosureConfigurationCollectionSerialiser.Deserialize(reader);
			RegisterEditableChildObject(jobConfigurationCollection);
		}

		#endregion
	}
}
