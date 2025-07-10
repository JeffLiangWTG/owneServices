using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgRematch : AutoClientOrgRematch
	{
		public UPEOrgRematch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class OrgTypes
		{
			public const string Importer = "IMP";
			public const string Supplier = "SUP";
			public const string Broker = "BRK";
		}

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			T5_RematchedToDate = ZDateTime.Now;
			T5_GS_RematchedBy = Env.CurrentUser.PK;
		}

		#endregion

		#region Related Business Objects

		public UPEJobDeclaration Declaration
		{
			get { return Factory.Load<UPEJobDeclaration>(T5_JE); }
		}

		#endregion

		#region LogRematch

		public static void LogRematch(UPEJobDeclaration declaration, ZPropertyInfo organisationFKProperty, ZString organisationType)
		{
			try
			{
				if (declaration.IsInDatabase)
				{
					bool rematchRequired = organisationFKProperty.HasChanges;

					UPEOrgRematch rematch = GetRematchPendingSave(declaration, organisationType);
					if (!rematchRequired && rematch != null)
					{
						rematch.Delete();
						rematch = null;
					}
					else if (rematchRequired && rematch == null)
					{
						rematch = declaration.Factory.New<UPEOrgRematch>();
					}
					if (rematch != null)
					{
						rematch.PopulateFromDeclaration(declaration, organisationFKProperty, organisationType);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("UPEOrgRematch.LogRematch" + ex.Message, ex.Message, ex);
			}
		}

		static UPEOrgRematch GetRematchPendingSave(UPEJobDeclaration declaration, ZString organisationType)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ClientOrgRematchSchema.T5_JE, SQLComparisonOperator.Equal, declaration.PK);
			query.AddToFilter(JoinCondition.And, ClientOrgRematchSchema.T5_OrganisationType, SQLComparisonOperator.Equal, organisationType);
			query.FetchOnlyFromLocalCache = true;
			UPEOrgRematch[] rematches = declaration.Factory.Load<UPEOrgRematch>(query);

			foreach (UPEOrgRematch rematch in rematches)
			{
				if (!rematch.IsInDatabase)
				{
					return rematch;
				}
			}
			return null;
		}

		#endregion

		#region Implementation

		void PopulateFromDeclaration(UPEJobDeclaration declaration, ZPropertyInfo organisationFKProperty, ZString organisationType)
		{
			T5_JE = declaration.PK;
			T5_OrganisationType = organisationType;

			UPEOrgRematch previousRematch = GetPreviousRematch();
			T5_RematchedFromDate = GetPreviousRematchDate(previousRematch);
			T5_OH_RematchedFromOrg = (ZGuid)organisationFKProperty.OriginalValue;
			T5_OH_RematchedToOrg = (ZGuid)organisationFKProperty.Value;
		}

		ZDateTime GetPreviousRematchDate(UPEOrgRematch previousRematch)
		{
			ZDateTime result = ZDateTime.Empty;
			if (previousRematch != null)
			{
				result = previousRematch.T5_RematchedToDate;
			}
			else if (Declaration.IsInDatabase)
			{
				result = Declaration.Logs.AddedLog.SL_EventTime;
			}
			return result;
		}

		UPEOrgRematch GetPreviousRematch()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(ClientOrgRematchSchema.T5_JE, SQLComparisonOperator.Equal, T5_JE);
			query.AddToFilter(JoinCondition.And, ClientOrgRematchSchema.T5_OrganisationType, SQLComparisonOperator.Equal, T5_OrganisationType);
			query.OrderBy = ClientOrgRematchSchema.T5_RematchedToDate.Name + " DESC";

			UPEOrgRematch[] allRematches = Factory.Load<UPEOrgRematch>(query);
			foreach (UPEOrgRematch rematch in allRematches)
			{
				if (PK != rematch.PK)
				{
					return rematch;
				}
			}
			return null;
		}

		#endregion
	}
}
