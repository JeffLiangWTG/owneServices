using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEADPScoring : AutoClientADPScoring
	{
		public UPEADPScoring(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (User != null)
			{
				T4_IsStaffWorkingDay = User.IsWorkingToday;
			}
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public UPEADPScoring LoadOrCreate()
			{
				return LoadOrCreate(GlbStaff.CurrentUser, ZDateTime.Today);
			}

			public UPEADPScoring LoadOrCreate(GlbStaff user, ZDateTime date)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(ClientADPScoringSchema.T4_GS_NKUser, SQLComparisonOperator.Equal, user.GS_Code);
				query.AddToFilter(ClientADPScoringSchema.T4_Date, SQLComparisonOperator.Equal, date.Date);
				query.IsNoLock = false;

				UPEADPScoring result = Factory.LoadTop1<UPEADPScoring>(query);
				if (result == null)
				{
					result = Factory.New<UPEADPScoring>();
					result.T4_GS_NKUser = user.GS_Code;
					result.T4_Date = date.Date;
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(UPEADPScoring);
			}
		}

		#endregion
	}
}
