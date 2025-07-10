using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Customs.Manifest
{
	public class ManifestJobNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public ManifestJobNumberCustomisationRegistryDataType()
			: base(new ManifestJobNumberCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("04D2F707-0304-4834-AF77-23FC017A5165", "Manifest Job Number");
			MaxLength = AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength;
		}

		protected override Type DataTypeCore
		{
			get { return typeof(ManifestJobNumberCustomisation); }
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ManifestJobNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public ManifestJobNumberCustomisation()
		{
			UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "7";

			foreach (BillOfLadingNumberCustomisationElement unFilteredElement in UnFilteredElements.ToArray())
			{
				if (!unFilteredElement.Matches(NumberCustomisationElementCategories.Standard))
				{
					UnFilteredElements.Remove(unFilteredElement);
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ManifestJobNumberCustomisation();
		}
	}
}
