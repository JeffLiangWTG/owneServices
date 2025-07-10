using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class QualityIterationAssignmentCollection : RegistryBusinessObjectCollectionTemplate
	{
		public QualityIterationAssignmentCollection()
		{
		}

		public QualityIterationAssignmentCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new QualityIterationAssignment this[int index]
		{
			get { return (QualityIterationAssignment)Elements[index]; }
		}

		public QualityIterationAssignment this[string releaseGroup]
		{
			get
			{
				return Elements.Cast<QualityIterationAssignment>().SingleOrDefault(x => x.ReleaseGroup == releaseGroup) ?? throw new ArgumentException(@"There is no setting for the release group specified.", nameof(releaseGroup));
			}
		}

		public new QualityIterationAssignment AddNew()
		{
			return (QualityIterationAssignment)base.AddNew();
		}

		public QualityIterationAssignment AddNew(ZString releaseGroup, ZBool isQiEnabled)
		{
			if (IsReleaseGroupSpecified(releaseGroup))
			{
				throw new InvalidOperationException("Each release group may only be specified once.");
			}

			var assignment = AddNew();
			assignment.ReleaseGroup = releaseGroup;
			assignment.IsQiEnabled = isQiEnabled;
			return assignment;
		}

		public ZBool IsQiEnabledForReleaseGroup(ZString releaseGroup)
		{
			return this[releaseGroup].IsQiEnabled;
		}

		public ZBool IsReleaseGroupSpecified(ZString releaseGroup)
		{
			return Elements.Cast<QualityIterationAssignment>().Any(x => x.ReleaseGroup == releaseGroup);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new QualityIterationAssignment(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new QualityIterationAssignmentCollection(fallbackLevel, factory);
		}

		public void CopyTo(QualityIterationAssignment[] array, int index)
		{
			Elements.ToArray().CopyTo(array, index);
		}
	}
}

