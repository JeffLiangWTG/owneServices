using System;
using CargoWise.Definitions;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public sealed class LVXOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CALVX; }
		}

		public override BusinessContext DocumentBusinessContext
		{
			get { return BusinessContext.Customs; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.CALVXJobs;

		public override Type RootType
		{
			get { return typeof(JobDeclaration); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("5a12197c-331c-48ab-997a-2bf808e928ce", "Courier LVS Declaration Job"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("113624c5-0079-4a09-8f34-b3e6bb4eda6d", "Courier LVS Declaration Jobs"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.CAJobDeclaration);
		}

		protected override bool SupportsBulkUpdatesCore
		{
			get { return true; }
		}

		public override ResourceStringData AllowAllResourceStringData
		{
			get
			{
				return Res.GetData("8dbe2441-3e08-4079-8cdb-f56cd985556b", "Select All");
			}
		}
	}
}
