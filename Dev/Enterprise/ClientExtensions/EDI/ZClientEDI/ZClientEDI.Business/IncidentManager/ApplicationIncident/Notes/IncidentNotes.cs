using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentNotes : Notes
	{
		public IncidentNotes(IStmNoteParent parentBizO)
			: base(parentBizO)
		{
		}

		protected override Type ElementType
		{
			get { return typeof(IncidentNote); }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new IncidentNoteDependantCollection(Parent, ((BusinessObject)Parent).Factory);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new IncidentNoteCollection(Parent);
		}

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new StmNoteCollectionView(Parent, ElementType);
		}

		#region Incident Note Dependant & Non-Dependant Collections

		public class IncidentNoteDependantCollection : StmNoteCollection
		{
			public IncidentNoteDependantCollection(IStmNoteParent parentBizO, BusinessObjectFactory factory)
				: base(parentBizO, factory)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(IncidentNote);
			}
		}

		public class IncidentNoteCollection : StmNoteCollectionWithRelatedElements
		{
			public IncidentNoteCollection(IStmNoteParent parentBizO)
				: base(parentBizO)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(IncidentNote);
			}
		}

		#endregion
	}
}

