using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentNotesWithoutDescription : Notes
	{
		public IncidentNotesWithoutDescription(ProfessionalServicesQuote parent)
			: base(parent)
		{
		}

		protected override Type ElementType
		{
			get { return typeof(IncidentNoteWithoutDescription); }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new IncidentNoteWithoutDescriptionDependentCollection(Parent, Parent.Factory);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new IncidentNoteWithoutDescriptionCollection(Parent);
		}

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new StmNoteCollectionView(Parent, ElementType);
		}

		public new ProfessionalServicesQuote Parent
		{
			get { return (ProfessionalServicesQuote)base.Parent; }
		}

		#region class IncidentNoteWithoutDescriptionDependentCollection

		internal class IncidentNoteWithoutDescriptionDependentCollection : StmNoteCollection
		{
			public IncidentNoteWithoutDescriptionDependentCollection(ProfessionalServicesQuote parent, BusinessObjectFactory factory)
				: base(parent, factory)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(IncidentNoteWithoutDescription);
			}
		}

		#endregion

		#region class IncidentNoteWithoutDescriptionCollection

		internal class IncidentNoteWithoutDescriptionCollection : StmNoteCollectionWithRelatedElements
		{
			public IncidentNoteWithoutDescriptionCollection(ProfessionalServicesQuote parent)
				: base(parent)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(IncidentNoteWithoutDescription);
			}
		}

		#endregion
	}
}

