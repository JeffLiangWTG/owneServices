using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMZoneCapacityMultiplier : AutoBMZoneCapacityMultiplier,
		IAuditParent
	{
		public BMZoneCapacityMultiplier(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Component")]
		[List("Lookups.Components")]
		public override ZGuid BZC_FC_Component
		{
			get { return base.BZC_FC_Component; }
			set { base.BZC_FC_Component = value; }
		}

		public BMComponent Component
		{
			get { return Factory.Load<BMComponent>(BZC_FC_Component); }
		}

		[RelatedBusinessObject("ReleaseGroup")]
		[List("Lookups.ReleaseGroups")]
		public override ZGuid BZC_GG_ReleaseGroup
		{
			get { return base.BZC_GG_ReleaseGroup; }
			set { base.BZC_GG_ReleaseGroup = value; }
		}

		#endregion

		#region Get Multipliers

		public static ZDecimal GetZoneMultiplier(BMZoneCapacityMultiplier multiplier, int zoneId)
		{
			var useDefault = multiplier == null;

			switch (zoneId)
			{
				case 0:
					return useDefault ? BMConstants.Zone0DefaultMultiplier : multiplier.BZC_Zone0Multiplier;
				case 1:
					return useDefault ? BMConstants.Zone1DefaultMultiplier : multiplier.BZC_Zone1Multiplier;
				case 2:
					return useDefault ? BMConstants.Zone2DefaultMultiplier : multiplier.BZC_Zone2Multiplier;
				case 3:
					return useDefault ? BMConstants.Zone3DefaultMultiplier : multiplier.BZC_Zone3Multiplier;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The zone id {0} is not within the valid range [0-3]", zoneId));
			}
		}

		public static ZDecimal GetZoneMultiplier(BusinessObjectFactory factory, ZGuid destinationPK, ZGuid releaseGroupPK, int zoneId)
		{
			var multiplier = GetMultiplierForReleaseGroup(factory, destinationPK, releaseGroupPK);

			if (multiplier == null)
			{
				var noReleaseGroupQuery = new ZQuery(BMZoneCapacityMultiplierSchema.BZC_FC_Component, destinationPK);
				noReleaseGroupQuery.AddToFilter(BMZoneCapacityMultiplierSchema.BZC_GG_ReleaseGroup, null);
				multiplier = factory.LoadTop1<BMZoneCapacityMultiplier>(noReleaseGroupQuery);
			}

			return GetZoneMultiplier(multiplier, zoneId);
		}

		static BMZoneCapacityMultiplier GetMultiplierForReleaseGroup(BusinessObjectFactory factory, ZGuid destinationPK, ZGuid releaseGroupPK)
		{
			if (releaseGroupPK != Guid.Empty)
			{
				var releaseGroupExistsQuery = new ZQuery(BMZoneCapacityMultiplierSchema.BZC_FC_Component, destinationPK);
				releaseGroupExistsQuery.AddToFilter(BMZoneCapacityMultiplierSchema.BZC_GG_ReleaseGroup, releaseGroupPK);
				return factory.LoadTop1<BMZoneCapacityMultiplier>(releaseGroupExistsQuery);
			}

			return null;
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BZC_Zone0Multiplier = BMConstants.Zone0DefaultMultiplier;
			BZC_Zone1Multiplier = BMConstants.Zone1DefaultMultiplier;
			BZC_Zone2Multiplier = BMConstants.Zone2DefaultMultiplier;
			BZC_Zone3Multiplier = BMConstants.Zone3DefaultMultiplier;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
