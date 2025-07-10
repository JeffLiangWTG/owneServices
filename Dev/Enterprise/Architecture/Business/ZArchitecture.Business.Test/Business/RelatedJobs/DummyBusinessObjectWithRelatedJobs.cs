using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyBusinessObjectWithRelatedJobs : DummyBusinessObject, IRelatedJob, ISupportRelatedJobs
	{
		public DummyBusinessObjectWithRelatedJobs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public RelatedJobCollection RelatedJobs
		{
			get
			{
				if (relatedJobs == null)
				{
					relatedJobs = new RelatedJobCollection(Factory);
					relatedJobs.Add(this);
				}
				return relatedJobs;
			}
		}

		RelatedJobCollection relatedJobs;

		#region IRelatedJob Members

		public ZString JobDescription
		{
			get { return "Dummy Job Description"; }
		}

		public ZString JobNumber
		{
			get { return "Dummy Job No."; }
		}

		public ZString JobStatus
		{
			get { return "Dummy Job Status"; }
		}

		#endregion

		#region IControllerIDProvider Members

		public Guid BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		public ControllerID ControllerID
		{
			get { return DummyControllerIDs.Dummy; }
		}

		#endregion
	}
}
