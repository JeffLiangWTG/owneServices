using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class NotificationForwardingPartySynchroniser : BusinessObjectSynchroniser
	{
		public NotificationForwardingPartySynchroniser(NotificationForwardingParty destination, JobDocAddress source, JPAFRHeader parentHeader)
			: base(destination, source)
		{
			Argument.NotNull(parentHeader, "Parent Header");
			this.parentHeader = parentHeader;
		}
		readonly JPAFRHeader parentHeader;

		public new JobDocAddress Source
		{
			get
			{
				return (JobDocAddress)base.Source;
			}
		}

		public new NotificationForwardingParty Destination
		{
			get
			{
				return (NotificationForwardingParty)base.Destination;
			}
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.CY_DataInfo, GetData, GetInfosAffects));
			Synchronisers.Add(new FieldSynchroniser(Destination.CY_OrderInfo, GetOrder, GetInfosAffects));
		}

		internal static OrgCusCode GetRelatedCCDCodeFromAddress(JobDocAddress sourceJobDocAddress, GlbCompany company)
		{
			OrgCusCode result = null;
			company = company ?? GlbCompany.CurrentCompany;
			var consigneeOrg = sourceJobDocAddress != null ? sourceJobDocAddress.Organisation : null;
			if (consigneeOrg != null)
			{
				var relatedOrg = consigneeOrg.GetJapanNotificationParty(company);
				var relatedParty = relatedOrg != null ? relatedOrg.RelatedParty : null;
				result = relatedParty != null ? relatedParty.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.Japan) : null;
			}
			return result;
		}

		#region Implementation

		IEnumerable<ZPropertyInfo> GetInfosAffects()
		{
			yield return Source.E2_OA_AddressInfo;
			yield return parentHeader.JPH_GB_BranchInfo;
		}

		IZType GetData()
		{
			ZString result = ZString.Empty;
			var cusCode = GetRelatedCCDCodeFromAddress(Source, parentHeader.Company);
			if (cusCode != null)
			{
				result = cusCode.OK_CustomsRegNo;
			}

			return result;
		}

		IZType GetOrder()
		{
			return ZShort.Zero;
		}

		#endregion
	}
}
