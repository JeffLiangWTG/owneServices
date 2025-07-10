using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Freight
{
	public class DocService : DocBaseWrapper
	{
		protected DocService(JobService service, IDocServicesParent parent, BusinessObjectFactory factoryToWrap)
			: base(service, factoryToWrap)
		{
			fParent = parent;
		}

		public static DocService New(JobService service, BusinessObjectFactory factory)
		{
			return new DocService(service, null, factory);
		}

		public static DocService New(JobService service, IDocServicesParent parent, BusinessObjectFactory factoryToWrap)
		{
			if (service == null || parent == null)
			{
				return null;
			}
			else
			{
				return new DocService(service, parent, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}

		JobService Service
		{
			get { return (JobService)WrappedObject; }
		}

		#region ZString Fields

		public ZString Description
		{
			get { return Service.ES_Calc_Description; }
		}

		public ZString Details
		{
			get { return Service.ES_ServiceNote; }
		}

		public ZString Reference
		{
			get { return Service.ES_References; }
		}

		public ZString ReferenceOrOrderRef
		{
			get
			{
				ZString result;
				if (ReportName.Contains((NoResString)"Authorization"))
				{
					result = Parent.OwnerRefAndOrderRef;
				}
				else
				{
					result = Reference;
				}
				return result;
			}
		}

		public ZString ReferenceOrOrderRefHeading
		{
			get
			{
				ZString result;
				if (ReportName.Contains((NoResString)"Authorization"))
				{
					result = Parent.OwnerRefAndOrderRefHeading;
				}
				else
				{
					result = Res.GetString("1b434905-f510-4b57-9366-2478e62204f7", "REFERENCE");
				}
				return result;
			}
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime BookedDate
		{
			get { return Service.ES_Booked; }
		}

		#endregion

		#region Wrapper Fields

		public DocOrganisation Contractor
		{
			get
			{
				DocOrganisation result;
				if (Service.ES_OH_Contractor.IsEmpty)
				{
					result = DocOrganisation.New(Factory, Service.ES_OA_Location);
				}
				else
				{
					result = DocOrganisation.New(Factory, Service.ES_OH_Contractor);
				}
				return result;
			}
		}

		public DocAddress Location
		{
			get
			{
				if (Service.Location != null)
				{
					return DocAddress.New(Service.Location, Factory);
				}
				else if (Service.Contractor != null)
				{
					return DocAddress.New(Service.Contractor.MainAddress, Factory);
				}
				return null;
			}
		}

		public DocOrganisation RequestedBy
		{
			get { return GlbBranch.CurrentBranch.OrgProxy != null ? DocOrganisation.New(Factory, GlbBranch.CurrentBranch.OrgProxy.PK) : null; }
		}

		#endregion

		#region Parents

		public IDocServicesParent Parent
		{
			get { return fParent; }

			set { fParent = value; }
		}

		IDocServicesParent fParent;

		#endregion

		#region DocumentText

		public ZString ServiceDocumentTitle
		{
			get
			{
				ZString result = ZString.Empty;
				if (ReportName.Contains((NoResString)"Request"))
				{
					result = Res.GetString("e12e137d-7bb2-4fd7-9b98-c79022f9e546", "Request for {0} Service", Description);
				}
				else if (ReportName.Contains((NoResString)"Authorization"))
				{
					result = Res.GetString("0e6ba45a-e802-41cf-86c0-ba5cca83d0f1", "Authorization for {0} Service", Description);
				}
				return result;
			}
		}

		public ZString ServiceDocumentOpeningText
		{
			get
			{
				ZString result = ZString.Empty;
				if (ReportName.Contains((NoResString)"Request"))
				{
					result = DocumentsDataRegistry.Instance.RequestForServiceOpeningText.Value;
				}
				else if (ReportName.Contains((NoResString)"Authorization"))
				{
					result = DocumentsDataRegistry.Instance.AuthorisationForServiceOpeningText.Value;
				}
				return result;
			}
		}

		public ZString ServiceDocumentClosingText
		{
			get
			{
				ZString result = ZString.Empty;
				if (ReportName.Contains((NoResString)"Request"))
				{
					result = DocumentsDataRegistry.Instance.RequestForServiceClosingText.Value;
				}
				else if (ReportName.Contains((NoResString)"Authorization"))
				{
					result = DocumentsDataRegistry.Instance.AuthorisationForServiceClosingText.Value;
				}
				return result;
			}
		}

		#endregion

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get
			{
				IBODocDataProvider parentDocDataProvider = Parent as IBODocDataProvider;
				return parentDocDataProvider != null ? parentDocDataProvider.BusinessObjectToLogAgainst : base.BusinessObjectToLogAgainst;
			}
		}
	}
}
