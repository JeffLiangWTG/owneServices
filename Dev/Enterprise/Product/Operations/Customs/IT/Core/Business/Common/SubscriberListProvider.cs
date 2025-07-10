using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public class SubscriberListProvider
{
	public SubscriberListProvider(BusinessObjectFactory factory, ICustomsProfileDataProvider customsProfileDataProvider)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.customsProfileDataProvider = Argument.NotNull(customsProfileDataProvider, nameof(customsProfileDataProvider));
	}

	readonly BusinessObjectFactory factory;
	readonly ICustomsProfileDataProvider customsProfileDataProvider;

	public GlbStaffCollection GetSubscribers()
	{
		var node = CustomsCredentialHelper.GetNodeFromInternalCode(customsProfileDataProvider.CustomsProfile);
		if (node.IsEmpty)
		{
			return new GlbStaffCollection(factory, ZQuery.NoResultQuery);
		}
		else
		{
			var cacheKey = FormattableString.Invariant($"IT.SubscriberListLookups|SubscribersFor_{node}");
			return factory.GetCachedValue(cacheKey, () =>
			{
				const int staffCollectionCountThreshold = 50;
				var glbStaffCollection = new GlbStaffCollection(factory, GetGlbStaffAdditionalQuery(node));
				if (glbStaffCollection.Count > staffCollectionCountThreshold)
				{
					ErrorReporter.ReportOnce(key: cacheKey, message: FormattableString.Invariant($"More than {staffCollectionCountThreshold} staff records have been loaded."));
				}
				return glbStaffCollection;
			});
		}
	}

	ZDBOnlyQuery GetGlbStaffAdditionalQuery(ZString node)
	{
		var glbStaffAdditionalQuery = new ZDBOnlyQuery(typeof(GlbStaff));
		var glbExternalPasswordSubquery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GS);
		glbExternalPasswordSubquery.AddToFilter(GlbExternalPasswordSchema.GP_UserID, node);
		glbExternalPasswordSubquery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.ITB);
		glbStaffAdditionalQuery.AddSubQuery(GlbStaffSchema.PK, glbExternalPasswordSubquery, JoinCondition.And);
		return glbStaffAdditionalQuery;
	}
}
