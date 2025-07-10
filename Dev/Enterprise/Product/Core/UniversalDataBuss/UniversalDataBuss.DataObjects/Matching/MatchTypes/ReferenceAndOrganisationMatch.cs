using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Res = Enterprise.UniversalDataBuss.DataObjects.Res;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	/// <summary>
	/// Complete Reference and Organisation match. Defines Source and Destination Reference, 
	/// has an OrganisationMatch as well and defines the score on the pair.
	/// </summary>
	class ReferenceAndOrganisationMatch : IMatchWithFilterAndScore
	{
		internal ReferenceAndOrganisationMatch(ReferenceElementName referenceElementName, SchemaStringColumn dbFieldName, IOrganizationAddressMatch organisationMatch, Score score)
		{
			this.referenceElementName = referenceElementName;
			this.dbFieldName = Argument.NotNull(dbFieldName, "SchemaStringColumn dbFieldName");
			this.organisationMatch = Argument.NotNull(organisationMatch, "IOrganisationMatch organisationMatch");
			this.scoringValues = score;
		}

		readonly ReferenceElementName referenceElementName;
		readonly SchemaStringColumn dbFieldName;
		readonly IOrganizationAddressMatch organisationMatch;
		readonly Score scoringValues;

		ZString referenceElementValue;

		public int Score
		{
			get;
			private set;
		}

		public bool HasDataForMatching(IShipmentDataObjectReader shipmentReader)
		{
			return HasDataForMatchingCore(
						shipmentReader.DataObject,
						() => organisationMatch.HasDataForMatching(shipmentReader)
						);
		}

		bool HasDataForMatchingCore(ITopLevelDataObject dataObject, Func<bool> hasDataForMatching)
		{
			var propertyInfo = dataObject.GetType().GetProperty(referenceElementName.ToString()) ?? throw new InvalidOperationException("Property [" + referenceElementName.ToString() + "] does not exist on type [" + dataObject.GetType().FullName + "].");
			if (propertyInfo.PropertyType != typeof(ZString?))
			{
				throw new InvalidOperationException("Property [" + referenceElementName.ToString() + "] on type [" + dataObject.GetType().FullName + "] must be a Nullable<ZString> to be used for Reference Matching.");
			}

			referenceElementValue = ((ZString?)propertyInfo.GetValue(dataObject, null)).GetValueOrDefault();

			if (referenceElementValue.Length >= 6)
			{
				return hasDataForMatching();
			}

			return false;
		}

		public ZQuery GetFilter()
		{
			var result = new ZQuery(dbFieldName, referenceElementValue);

			var organisationFilterSource = organisationMatch as IMatchWithFilter;
			if (organisationFilterSource != null)
			{
				result.AddToFilter(organisationFilterSource.GetFilter());
			}

			return result;
		}

		public bool IsMatch(PotentialMatch matchTarget, ISimpleLogger logger)
		{
			var referenceFromBO = (ZString)matchTarget.TargetData[dbFieldName];
			if (!referenceFromBO.IsEmpty)
			{
				if (referenceFromBO.EqualsIgnoringCase(referenceElementValue))
				{
					if (organisationMatch.IsMatch(matchTarget, logger))
					{
						logger.LogVerboseOnly(LogType.Information, Res.GetString("08f812b9-9fed-4fa5-ba30-4fbb6cbfdc3d", "Match on {0} and {1}, {2} points.", IncomingValueName, organisationMatch.IncomingValueName, scoringValues.FullMatch.ToString()));
						this.Score = scoringValues.FullMatch;
						return true;
					}
					else
					{
						logger.LogVerboseOnly(LogType.Information, Res.GetString("9238510e-23ba-4a09-b8e1-e6c8b49da90d", "Partial Match on on {0} without {1}, {2} points.", IncomingValueName, organisationMatch.IncomingValueName, scoringValues.ReferenceOnlyMatch.ToString()));
						this.Score = scoringValues.ReferenceOnlyMatch;
						return true;
					}
				}
				else
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("59b27833-d3a0-4ad4-a273-b15aa69bc91a", "Conflict on {0}, {1} points.", IncomingValueName, scoringValues.Conflict.ToString()));
					this.Score = scoringValues.Conflict;
					return true;
				}
			}

			this.Score = 0;
			return false;
		}

		public string IncomingValueName
		{
			get { return referenceElementName.ToString(); }
		}
	}
}
