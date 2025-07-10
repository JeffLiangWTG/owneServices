using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.Common.CA;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class LVXController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CALVXJobs; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CALVXJobs; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CALVXJobsView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CALVXJobsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CALVXJobsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CALVXJobsEdit; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return CreateNewBusinessObject(Factory);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobDeclarationForm((JobDeclaration)businessEntity);
		}

		public static JobDeclaration CreateNewBusinessObject(BusinessObjectFactory factory, JobComInvoiceHeader invoiceHeader = null)
		{
			var result = factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			result.JE_MessageSubType = B3EntryTypeList.Codes.NoB3;
			result.JE_MergeBy = B3MergeByList.Codes.NotMerge;
			if (invoiceHeader != null)
			{
				using (invoiceHeader.SuspendMarkingAsNeedingValidation())
				{
					result.Invoices.Add(invoiceHeader);
					result.JE_OH_Importer = invoiceHeader.JZ_OH_Buyer;
				}
			}
			return result;
		}
	}
}
