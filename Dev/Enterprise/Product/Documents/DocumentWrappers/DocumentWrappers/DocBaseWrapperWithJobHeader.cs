using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocBaseWrapperWithJobHeader : DocBaseWrapper
	{
		public DocBaseWrapperWithJobHeader(object objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		#region Company Tariff Level

		protected override ClientTariffAndLevel TariffAndLevelRegistry
		{
			get
			{
				ClientTariffAndLevel result = null;
				if (JobHeader != null && JobHeader.LocalCharges != null)
				{
					ZString tariffLevelCode = JobHeader.LocalCharges.MiscServ.ARGlobalRateBase.ToString();
					result = (ClientTariffAndLevel)DocumentsDataRegistry.Instance.ClientTariffAndLevels.Value.FindByCode(tariffLevelCode);
				}
				else
				{
					result = base.TariffAndLevelRegistry;
				}

				return result;
			}
		}

		#endregion

		#region Branding

		protected virtual Image JobHeaderBranchLogo
		{
			get
			{
				return JobHeader != null && JobHeader.Branch != null
					? JobHeader.Branch.GetDepartmentBranchLogo(JobHeader.Department != null ? JobHeader.Department.DepartmentPK.ToGuid() : Guid.Empty)
					: null;
			}
		}

		protected override Image GetCompanyLogoFallback()
		{
			var result = !SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.Value ? JobHeaderBranchLogo : null;
			return result ?? base.GetCompanyLogoFallback();
		}

		public Image GetHouseBillBrandImage(DocOrganisation brandingAgent, DocOrganisation brandingClient, ClientAndAgentBrandingRegistryItem registryCollectionToUse)
		{
			ZString code = ZString.Empty;

			if (DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.Value == HBLAndHAWBBrandingOptionEditorInfo.AgentBranded)
			{
				if (DocumentsDataRegistry.Instance.EnableAgentBranding.Value && brandingAgent != null && brandingAgent != null)
				{
					code = brandingAgent.MiscServ.FWAgentCategory;
				}
			}
			else
			{
				if (DocumentsDataRegistry.Instance.EnableClientBranding.Value)
				{
					if (JobHeader != null && JobHeader.LocalCharges != null)
					{
						code = JobHeader.LocalCharges.MiscServ.ARGlobalRateBase.ToString();
					}

					if (code.IsEmpty && brandingClient != null)
					{
						code = brandingClient.MiscServ.ARGlobalRateBase.ToString();
					}
				}
			}

			if (!code.IsEmpty)
			{
				ClientAndAgentBrandingBusinessObject brandingObj = registryCollectionToUse.Value.FindByCode(code);
				if (brandingObj != null)
				{
					return brandingObj.Image;
				}
			}

			return null;
		}

		#endregion

		#region Job Details

		public abstract DocJobHeader JobHeader { get; }

		protected DocJobInvoicingJob GenericInvoicingJob
		{
			get
			{
				DocJobInvoicingJob genericInvoicingJob = null;
				if (JobHeader != null && JobHeader.JobHeader != null)
				{
					Job jobInvoice = Factory.Load<Job>(JobHeader.JobHeader.PK);
					genericInvoicingJob = DocJobInvoicingJob.New(jobInvoice, Factory);
				}
				return genericInvoicingJob;
			}
		}

		protected ZString Mode
		{
			get
			{
				if (GenericInvoicingJob != null)
				{
					return GenericInvoicingJob.TransportMode == Core.Constants.TransportModes.Sea ? GenericInvoicingJob.ContainerMode : GenericInvoicingJob.TransportMode;
				}
				return ZString.Empty;
			}
		}

		#endregion
	}
}
