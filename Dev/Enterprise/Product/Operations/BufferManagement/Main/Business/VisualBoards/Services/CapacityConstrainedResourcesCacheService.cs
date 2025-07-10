using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class CapacityConstrainedResourcesCacheService : ResettableService<Dictionary<ZGuid, Dictionary<string, CapacityConstrainedResourceStatus>>>
	{
		public CapacityConstrainedResourcesCacheService(IVisualBoardProvider boardProvider)
			: base(GetServiceDataCreator(boardProvider))
		{
		}

		internal CapacityConstrainedResourceStatus GetCapacityConstrainedResourceStatus(string staffCode, ZGuid componentPK)
		{
			CapacityConstrainedResourceStatus result = null;
			Dictionary<string, CapacityConstrainedResourceStatus> byStaff;

			if (ServiceData.TryGetValue(componentPK, out byStaff))
			{
				result = byStaff.GetValueSafe(staffCode);
			}

			return result ?? new CapacityConstrainedResourceStatus(staffCode, false, false, ZDateTime.Empty);
		}

		protected override BoardServiceStalenessPolicy StalenessPolicy => BoardServiceStalenessPolicy.StaleBeforeBoardRefresh | BoardServiceStalenessPolicy.StaleBeforeSavingTicket;

		#region Implementation

		static Func<Dictionary<ZGuid, Dictionary<string, CapacityConstrainedResourceStatus>>> GetServiceDataCreator(IVisualBoardProvider boardProvider)
		{
			var boardPKs = boardProvider.Boards.Select(b => b.BoardPK);
			var boards = boardProvider.Factory.Load<BMBoard>(new ZQuery(BMBoardSchema.PK, boardPKs));

			var bufferPKs = BMBoardSlideshow.GetAllComponentsAcrossMultipleBoards(boards).Where(component => component.IsBuffer).Select(b => b.PK).ToArray();

			return delegate
			{ return GetCCRStaffCodesPerBuffer(bufferPKs); };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static Dictionary<ZGuid, Dictionary<string, CapacityConstrainedResourceStatus>> GetCCRStaffCodesPerBuffer(ZGuid[] componentPKs)
		{
			var result = new Dictionary<ZGuid, Dictionary<string, CapacityConstrainedResourceStatus>>();
			const string sql = @"
				SELECT
					FD_FC_Component,
					FD_GS_NKResource,
					FD_IsCapacityConstrained,
					FD_IsPersistentlyOverloaded,
					FD_CapacityConstraintDetectedUtc
				FROM dbo.BMComponentResourceLink
				WHERE FD_FC_Component IN (SELECT VALUE FROM @Buffers)
				";

			using (var command = Db.Connection.Command(sql)) // It's faster than loading many business objects, sadly.
			{
				command.AddTableValuedParameter("@Buffers", CargoWise.Schema.Schema.GenericGuidSchemaColumn, componentPKs);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var bufferPK = reader.GetGuid(0);
						var dictionary = result.GetOrAdd(bufferPK, () => new Dictionary<string, CapacityConstrainedResourceStatus>());

						var staffCode = reader.GetString(1);
						var ccrDetectedTime = reader.IsDBNull(4) ? ZDateTime.Empty : reader.GetDateTime(4);

						dictionary.Add(staffCode,
							new CapacityConstrainedResourceStatus(
								staffCode,
								reader.GetBoolean(2),
								reader.GetBoolean(3),
								ccrDetectedTime
							));
					}
				}
			}

			return result;
		}

		#endregion
	}
}
