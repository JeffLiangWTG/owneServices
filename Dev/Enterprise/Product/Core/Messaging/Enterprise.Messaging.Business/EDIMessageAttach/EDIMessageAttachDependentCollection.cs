using System;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageAttachDependentCollection : DependentBusinessObjectCollection<EDIMessageAttach, EDIMessage>
	{
		public EDIMessageAttachDependentCollection(EDIMessage ediMessage, BusinessObjectFactory factory)
			: base(ediMessage, factory)
		{
		}

		public new EDIMessageAttach AddNew(Type bizoType)
		{
			return base.AddNew(bizoType);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			EDIMessageAttach ediMessageAttach = child as EDIMessageAttach;
			if (ediMessageAttach != null)
			{
				ediMessageAttach.EG_EM = Master.PK;
			}
		}
	}
}
