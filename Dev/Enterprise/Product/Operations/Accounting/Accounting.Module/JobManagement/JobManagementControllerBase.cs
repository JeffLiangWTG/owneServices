using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for JobManagement.
	/// </summary>
	public class JobManagementControllerBase : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public JobManagementControllerBase()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobHeader; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobManagement); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			JobManagementForm form = null;
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Job loadedJob = newFactory.Load<Job>(((BusinessObject)businessEntity).PK);

			if (loadedJob != null)
			{
				GenericJob genericJob = loadedJob.LoadGenericJob<GenericJob>();
				if (genericJob != null)
				{
					Type consumerType = genericJob.GetConsumerType();
					if (consumerType != null)
					{
						IJobInvoicingPlugIn pluginData = (IJobInvoicingPlugIn)newFactory.Load(consumerType, loadedJob.JH_ParentID);

						if (pluginData != null)
						{
							loadedJob.PlugInData = pluginData;
							if (loadedJob.ProfitLoss.Any())
							{
								JobProfitLoss profitLoss = (JobProfitLoss)loadedJob.ProfitLoss[0];
								form = new JobManagementForm(profitLoss);
							}
							else
							{
								form = GetFallbackJobManagementForm(newFactory, businessEntity);
							}
						}
						else
						{
							form = GetFallbackJobManagementForm(newFactory, businessEntity);
						}
					}
					else
					{
						form = GetFallbackJobManagementForm(newFactory, businessEntity);
					}
				}
				else
				{
					form = GetFallbackJobManagementForm(newFactory, businessEntity);
				}
			}

			return form;
		}

		public override IZForm ShowNewForm()
		{
			throw new NewJobManagementNotSupportedException("Job Management does not support new");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (sourceEntity is Job && ((Job)sourceEntity).JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix)
			{
				Globals.Message.Show(Res.GetString("25d305d9-22da-4181-a05b-61e16d8ab5e2", "You are not allowed to view / edit this job"));
				return null;
			}
			else
			{
				return base.ShowEditForm(sourceEntity);
			}
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("EE48D22E-047F-4C6D-A09C-0BFB556F6D27", "The selected record has been deleted or deactivated by another user. It cannot be displayed.");
			}
		}

		[Serializable]
		class NewJobManagementNotSupportedException : ModuleFeatureNotSupportedException
		{
			public NewJobManagementNotSupportedException(string message) : base(message) { }
#if NETFRAMEWORK
			protected NewJobManagementNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{ }
#endif
		}

		JobManagementForm GetFallbackJobManagementForm(BusinessObjectFactory newFactory, IBusiness businessEntity)
		{
			JobProfitLoss profitLoss = new JobProfitLoss(newFactory);
			profitLoss.SetJobPKs(new ZGuid[] { ((BusinessObject)businessEntity).PK });
			return new JobManagementForm(profitLoss);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewJobProfitLoss; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			// You cannot new this Job Management
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			// There is no edit menu either
			get { return Env.Security.ViewJobProfitLoss; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DeleteInvoicingJobHeader; }
		}

		#endregion

		#region CRM Security

		readonly JobManagementCRMSecurityProvider SecurityProvider = new JobManagementCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as JobHeader, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as JobHeader, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as JobHeader, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
