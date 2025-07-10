using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public interface IOrganizationAddressMatch : IMatch { }

	public class OrganizationAddressMatch<T> : OrganizationAddressMatchBase where T : BusinessObject
	{
		public OrganizationAddressMatch(MatchableOrganizationType organisationAddressType, OrganizationAddressMatchPool matchPool, TargetOrgHeaderPKGetter<T> targetOrgHeaderPksGetter, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
			: base(organisationAddressType, matchPool, unmatchedOrgNoteType, unmatchedOrgNoteSubType)
		{
			this.getTargetOrgHeaderPKs = Argument.NotNull(targetOrgHeaderPksGetter, "TargetOrgHeaderPKGetter targetOrgHeaderPksGetter");
		}
		readonly TargetOrgHeaderPKGetter<T> getTargetOrgHeaderPKs;

		protected override ZGuid[] GetTargetOrgHeaderPKsToMatchTo(BusinessObject matchingBO)
		{
			return getTargetOrgHeaderPKs((T)matchingBO);
		}
	}

	public delegate ZGuid[] TargetOrgHeaderPKGetter<T>(T matchingBO) where T : BusinessObject;

	/// <summary>
	/// Defines how a type of Organisation is matched. Knows how to get the Organisation from the 
	/// incoming data, and knows which Organisation to match to in CargoWise One in the target module.
	/// </summary>
	public abstract class OrganizationAddressMatchBase : IOrganizationAddressMatch
	{
		protected OrganizationAddressMatchBase(MatchableOrganizationType organisationAddressType, OrganizationAddressMatchPool matchPool, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
		{
			this.organisationAddressType = Argument.NotNull(organisationAddressType, "MatchableOrganizationType organisationAddressType");
			this.OrgHeaderMatchPool = Argument.NotNull(matchPool, "OrganisationMatchPool matchPool");
			this.unmatchedOrgNoteType = unmatchedOrgNoteType;
			this.unmatchedOrgNoteSubType = unmatchedOrgNoteSubType;
		}

		readonly MatchableOrganizationType organisationAddressType;
		protected readonly OrganizationAddressMatchPool OrgHeaderMatchPool;
		readonly OrganisationTypes unmatchedOrgNoteType;
		readonly string unmatchedOrgNoteSubType;

		protected OrganizationAddress OrganisationAddress
		{
			get;
			private set;
		}

		bool HasDataForMatching(ITopLevelDataObject dataObject)
		{
			var organisationsDataSource = dataObject as IOrganizationAddressCollectionParent ?? throw new InvalidOperationException("DataObject used for matching must implement IOrganizationAddressCollectionParent to be able to use a ReferenceAndOrganisationMatch.");

			var organisations = organisationsDataSource.OrganizationAddressCollection;

			OrganisationAddress = organisations != null
				? organisations.FirstOrDefault(o => o.AddressType.GetValueOrDefault() == organisationAddressType.ToString())
				: null;
			return OrganisationAddress != null;
		}

		public bool HasDataForMatching(IShipmentDataObjectReader shipmentReader)
		{
			var currentShipmentReader = shipmentReader;

			while (currentShipmentReader != null)
			{
				if (HasDataForMatching(currentShipmentReader.DataObject))
				{
					return true;
				}

				currentShipmentReader = currentShipmentReader.ParentReader;
			}

			return false;
		}

		public bool IsMatch(PotentialMatch matchTarget, ISimpleLogger logger)
		{
			var targetOrgHeaderPKs = GetTargetOrgHeaderPKsToMatchTo(matchTarget.TargetData);
			if (targetOrgHeaderPKs != null)
			{
				var unmatchedOrgRegistry = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
				var unmatchedOrganisationPK = unmatchedOrgRegistry.Organisation;

				if (unmatchedOrganisationPK.IsValid)
				{
					targetOrgHeaderPKs = targetOrgHeaderPKs.Where(orgHeaderPK => orgHeaderPK != unmatchedOrganisationPK).ToArray();
				}

				if (targetOrgHeaderPKs.Length > 0)
				{
					var matchResult = OrgHeaderMatchPool.GetMatch(OrganisationAddress, matchTarget.TargetData.Factory);

					if (matchResult.Success && matchResult.MatchFound.PK != unmatchedOrganisationPK)
					{
						return targetOrgHeaderPKs.Contains(matchResult.MatchFound.PK);
					}
				}
			}

			return IsFallbackMatch(OrganisationAddress, matchTarget.TargetData);
		}

		protected virtual bool IsFallbackMatch(OrganizationAddress organisationAddress, BusinessObject matchingBO)
		{
			if (unmatchedOrgNoteType != OrganisationTypes.None)
			{
				var matcher = ObjectFactory.New<IOrganisationAddressUnmatchedNoteMatcher>();
				return matcher.IsMatchToOrgInUnmatchedNote(organisationAddress, matchingBO, unmatchedOrgNoteType, unmatchedOrgNoteSubType);
			}

			return false;
		}

		protected abstract ZGuid[] GetTargetOrgHeaderPKsToMatchTo(BusinessObject matchingBO);

		public string IncomingValueName
		{
			get { return organisationAddressType.ToString(); }
		}
	}
}
