using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	public class ChargeCodeMatcher : Matcher<ZGuid>
	{
		public ChargeCodeMatcher(BusinessObjectFactory factory, ZGuid mappingOrgPK, ZString unmappedChargeCode, INotifications notifications)
			: this(factory, mappingOrgPK, unmappedChargeCode, notifications, GlbCompany.CurrentCompany.PK) { }

		public ChargeCodeMatcher(BusinessObjectFactory factory, ZGuid mappingOrgPK, ZString unmappedChargeCode, INotifications notifications, ZGuid companyPK)
		{
			this.factory = factory;
			this.mappingOrgPK = mappingOrgPK;
			this.unmappedChargeCode = unmappedChargeCode;
			this.notifications = notifications;
			this.companyPK = companyPK;
		}

		protected override MatchDelegate[] matchDelegates
		{
			get
			{
				return new MatchDelegate[]
				{
					ByChargeCode,
					ByOrgPattern,
					Else,
				};
			}
		}

		MatchResult ByOrgPattern()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, mappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.ChargeCodes);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, unmappedChargeCode);

			OrgPatternMatchOverride match = factory.LoadTop1<OrgPatternMatchOverride>(filter);

			if (match == null)
			{
				return new MatchResult();
			}
			else
			{
				MatchResult result = ByChargeCode(match.OO_LocalCode);

				if (!result.HasMatched)
				{
					notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError,
						Res.GetString("892884a9-a061-4e74-8869-dccb297cc383", "'{0}' was mapped to the non-existent charge code '{1}'",
						unmappedChargeCode, match.OO_LocalCode)));
				}

				return result;
			}
		}

		MatchResult ByChargeCode()
		{
			return ByChargeCode(unmappedChargeCode);
		}
		MatchResult ByChargeCode(string code)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK);
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, code);

			AccChargeCode match = factory.LoadTop1<AccChargeCode>(filter);

			if (match == null)
			{
				return new MatchResult();
			}
			else
			{
				return new MatchResult(true, match.PK);
			}
		}

		MatchResult Else()
		{
			OrgHeader mappingOrg = factory.Load<OrgHeader>(mappingOrgPK);

			if (mappingOrg == null)
			{
				notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError,
					Res.GetString("5507e2f9-8de3-43d4-9626-85846ccce772", "'{0}' is not a recognized charge code.",
						unmappedChargeCode)));
			}
			else
			{
				notifications.Notify(new ErrorNotification(ErrorType.DataTypeConversionError,
					Res.GetString("7762d4ab-8280-4f6e-b558-9ed969f620cc", "'{0}' is not a recognized charge code, you can add a mapping from this code to a valid charge code on the organization '{1}'.",
						unmappedChargeCode, mappingOrg.OH_Code)));
			}

			return new MatchResult();
		}

		readonly INotifications notifications;
		readonly BusinessObjectFactory factory;
		readonly ZGuid companyPK;
		readonly ZGuid mappingOrgPK;
		readonly ZString unmappedChargeCode;
	}
}
