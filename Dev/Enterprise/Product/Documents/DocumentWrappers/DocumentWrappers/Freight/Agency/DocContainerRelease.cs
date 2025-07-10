using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocContainerRelease : DocBaseWrapper
	{
		DocContainerRelease(ReleaseInstance release, BusinessObjectFactory factoryToWrap)
			: base(release, factoryToWrap)
		{
			if (release == null)
			{
				throw new ArgumentNullException(nameof(release));
			}
		}

		public static DocContainerRelease New(ReleaseInstance release, BusinessObjectFactory factoryToWrap)
		{
			return new DocContainerRelease(release, factoryToWrap);
		}

		#region Simple Properties

		public ZString ReleaseNumber
		{
			get { return Release.ReleaseNumber; }
		}

		public ZString ReleaseNote
		{
			get { return Release.Header.ContainerReleaseNote; }
		}

		#endregion

		#region Related Wrappers

		public CodeAndDescriptionWrapper ReleaseType
		{
			get { return releaseType ?? (releaseType = new CodeAndDescriptionWrapper(Release.ReleaseType, Release.Lookups.ReleaseType_List, Factory)); }
		}
		CodeAndDescriptionWrapper releaseType;

		public DocDocAddress ContainerYard
		{
			get { return DocDocAddress.New(Release.ContainerYard, Factory); }
		}

		public DocAgencyShipment Shipment
		{
			get { return DocAgencyShipment.New(Release.Shipment, Factory); }
		}

		public IDocContainerCollection ReleasedContainers
		{
			get { return releasedContainers ?? (releasedContainers = BuildReleasedContainerCollection()); }
		}
		IDocContainerCollection BuildReleasedContainerCollection()
		{
			IDocContainerCollection result = new IDocContainerCollection(Release.Factory);

			foreach (AgencyShipmentContainer container in Release.Shipment.BookedContainers)
			{
				if (container.JC_ReleaseNum == Release.ReleaseNumber)
				{
					result.Add(DocAgencyContainer.New(container, Factory));
				}
			}

			return result;
		}
		IDocContainerCollection releasedContainers;

		#endregion

		#region Implementation

		ReleaseInstance Release
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseInstance)WrappedObject; }
		}

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			return Release.Shipment;
		}

		#endregion

		#region IBODocDataProvider Members

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return Release.Shipment; }
		}

		protected override BusinessObject ParentBusinessObject
		{
			get { return Release.Shipment; }
		}

		#endregion
	}
}
