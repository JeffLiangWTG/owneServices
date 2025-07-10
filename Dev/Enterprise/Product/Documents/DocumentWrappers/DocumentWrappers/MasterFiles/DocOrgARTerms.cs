using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgARTerms : DocumentWrapper
	{
		DocOrgARTerms(OrgARTerms orgARTerms, BusinessObjectFactory factoryForWrapper)
			: base(orgARTerms, factoryForWrapper)
		{
		}

		public static DocOrgARTerms New(OrgARTerms orgARTerms, BusinessObjectFactory factoryForWrapper)
		{
			if (orgARTerms == null)
			{
				return null;
			}
			else
			{
				return new DocOrgARTerms(orgARTerms, factoryForWrapper);
			}
		}

		OrgARTerms OrgARTerms
		{
			get { return (OrgARTerms)WrappedObject; }
		}

		public ZString AgreedPaymentMethod
		{
			get
			{
				return OrgARTerms.PY_AgreedPaymentMethod;
			}
		}

		public ZString Direction
		{
			get
			{
				return OrgARTerms.PY_Direction;
			}
		}

		public ZString InvoiceClass
		{
			get
			{
				return OrgARTerms.PY_InvoiceClass;
			}
		}

		public ZString InvoiceTerm
		{
			get
			{
				return OrgARTerms.PY_InvoiceTerm;
			}
		}

		public ZString JobType
		{
			get
			{
				return OrgARTerms.PY_JobType;
			}
		}

		public ZString TransportMode
		{
			get
			{
				return OrgARTerms.PY_TransportMode;
			}
		}

		public ZByte InvoiceDays
		{
			get
			{
				return OrgARTerms.PY_InvoiceDays;
			}
		}

		public DocDepartment Department
		{
			get
			{
				return fDepartment ?? (fDepartment = DocDepartment.New(OrgARTerms.Department, Factory));
			}
		}
		DocDepartment fDepartment;

		public DocBranch Branch
		{
			get
			{
				return fBranch ?? (fBranch = DocBranch.New(OrgARTerms.Branch, Factory));
			}
		}
		DocBranch fBranch;
	}
}
