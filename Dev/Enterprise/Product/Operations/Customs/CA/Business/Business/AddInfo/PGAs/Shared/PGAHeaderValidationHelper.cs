using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class PGAHeaderValidationHelper
	{
		public static ZString ValidateRequiredLPCOs(IPGAProgramRequirementProvider pgaheader, string programCode)
		{
			var lpcoviews = GetRelatedLpcos(pgaheader);

			if (lpcoviews != null && pgaheader.GetProgramCodesList().ContainsCode(programCode))
			{
				var defaultDocumentTypes = pgaheader.GetDefaultDocumentTypes(programCode);
				var pgaHeaderBO = (BusinessObject)pgaheader;
				var pgaInfo = Res.GetString("d0f6fe91-5f3c-4d57-ac9b-085272320cc3", "program: {0}", pgaheader.GetProgramCodesList()[programCode, StringComparison.OrdinalIgnoreCase].Description);
				var groupedDefaultDocumentTypes = defaultDocumentTypes.GroupBy(x => x.Category);
				var groupedMandatoryTypes = groupedDefaultDocumentTypes.Where(x => x.Key.RequieredType == DocumentTypeRequieredType.Mandatory).ToArray();
				var groupedAlternativeTypes = groupedDefaultDocumentTypes.Where(x => x.Key.RequieredType == DocumentTypeRequieredType.Alternative).ToArray();

				if (groupedMandatoryTypes.Any())
				{
					var groupMandatoryLPCOsInfo = groupedMandatoryTypes
						.Select(x => GetGroupLPCOsMessage(pgaHeaderBO.Factory, x.ToArray(), lpcoviews, DocumentTypeRequieredType.Mandatory))
						.Where(x => !string.IsNullOrEmpty(x)).ToArray();
					if (groupMandatoryLPCOsInfo.Any())
					{
						return GetRequiredLPCOsMessageError(groupMandatoryLPCOsInfo, "and", pgaInfo);
					}
				}

				if (groupedAlternativeTypes.Any())
				{
					var isMultilAlternativeExist = groupedAlternativeTypes.Count(x => x.Any(y => lpcoviews.Any(z => z.CLP_Type == y.Code))) > 1;
					var groupAlternativeLPCOsInfo = groupedAlternativeTypes
						.Select(x => GetGroupLPCOsMessage(pgaHeaderBO.Factory, x.ToArray(), isMultilAlternativeExist ? null : lpcoviews, DocumentTypeRequieredType.Alternative))
						.Where(x => !string.IsNullOrEmpty(x)).ToArray();
					if (groupAlternativeLPCOsInfo.Any() && groupAlternativeLPCOsInfo.Length == groupedAlternativeTypes.Length)
					{
						return GetRequiredLPCOsMessageError(groupAlternativeLPCOsInfo, "or", pgaInfo);
					}
				}
			}

			return ZString.Empty;
		}

		static IEnumerable<LPCOView> GetRelatedLpcos(IPGAProgramRequirementProvider pgaHeader)
		{
			var lpcoParents = new[]
			{
				pgaHeader as ILPCOCollectionParent,
				pgaHeader?.GetParentInvoiceLine()?.Declaration
			};

			foreach (var parent in lpcoParents)
			{
				if (parent != null)
				{
					foreach (var lpcoview in parent.LPCOViews.Cast<LPCOView>())
					{
						yield return lpcoview;
					}
				}
			}
		}

		static string GetRequiredLPCOsMessageError(string[] groupsInfo, string separator, string pgaInfo)
		{
			return groupsInfo.Length == 1
				? Res.GetString("c2ad06c9-ef61-48d6-a3e3-7fd721a246c4", "LPCO(s): {0} should be added for {1}.", groupsInfo.First(), pgaInfo)
				: Res.GetString("dd6ca0e8-9b70-40f3-8cc7-3db119fe4282", "LPCO(s): [{0}] should be added for {1}.", string.Join(string.Format(CultureInfo.InvariantCulture, " {0} ", separator), groupsInfo), pgaInfo);
		}

		static string GetGroupLPCOsMessage(BusinessObjectFactory factory, IRequiredDocumentType[] types, IEnumerable<LPCOView> lpcoviews, DocumentTypeRequieredType categoryRequieredType)
		{
			var mandatoryTypes = types.Where(x => x.RequiredType == DocumentTypeRequieredType.Mandatory).ToArray();
			if (mandatoryTypes.Any())
			{
				return GetMandatoryGroupLPCOsMessage(factory, mandatoryTypes, lpcoviews, categoryRequieredType);
			}

			if (types.All(x => x.RequiredType == DocumentTypeRequieredType.AtLeastOne))
			{
				return GetAtLeastOneLPCOsMessage(factory, types, lpcoviews);
			}

			if (types.All(x => x.RequiredType == DocumentTypeRequieredType.Alternative))
			{
				return GetAlternativeGroupLPCOsMessage(factory, types, lpcoviews);
			}

			return string.Empty;
		}

		static string GetMandatoryGroupLPCOsMessage(BusinessObjectFactory factory, IRequiredDocumentType[] types, IEnumerable<LPCOView> lpcoviews, DocumentTypeRequieredType categoryRequieredType)
		{
			var result = string.Empty;
			var missingMandatoryTypes = lpcoviews != null ? types.Where(x => lpcoviews.All(y => y.CLP_Type != x.Code)).ToArray() : types;

			if (missingMandatoryTypes.Any())
			{
				var lpcoDocumentTypes = factory.GetCachedValue<LPCODocumentTypeQualifier>();
				var messageTypes = categoryRequieredType == DocumentTypeRequieredType.Mandatory ? missingMandatoryTypes : types;
				var lpcosInfo = messageTypes.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", x.Code, lpcoDocumentTypes.GetDescriptionFromCode(x.Code)));

				result = string.Format(CultureInfo.InvariantCulture, "[{0}]", string.Join(" and ", lpcosInfo));
			}

			return result;
		}

		static string GetAtLeastOneLPCOsMessage(BusinessObjectFactory factory, IRequiredDocumentType[] types, IEnumerable<LPCOView> lpcoviews)
		{
			var result = string.Empty;

			if (!types.Any(x => lpcoviews.Any(y => y.CLP_Type == x.Code)))
			{
				var lpcoDocumentTypes = factory.GetCachedValue<LPCODocumentTypeQualifier>();
				var lpcosInfo = types.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", x.Code, lpcoDocumentTypes.GetDescriptionFromCode(x.Code)));

				result = string.Format(CultureInfo.InvariantCulture, "at least one of {0}", string.Join(", ", lpcosInfo));
			}

			return result;
		}

		static string GetAlternativeGroupLPCOsMessage(BusinessObjectFactory factory, IRequiredDocumentType[] types, IEnumerable<LPCOView> lpcoviews)
		{
			var result = string.Empty;
			var alternativeTypes = types.Where(x => x.RequiredType == DocumentTypeRequieredType.Alternative).ToArray();
			var lpcoDocumentTypes = factory.GetCachedValue<LPCODocumentTypeQualifier>();

			if (alternativeTypes.Any() && (lpcoviews == null || alternativeTypes.Count(x => lpcoviews.Any(y => y.CLP_Type == x.Code)) != 1))
			{
				var lpcosInfo = alternativeTypes.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", x.Code, lpcoDocumentTypes.GetDescriptionFromCode(x.Code))).ToArray();
				if (lpcosInfo.Length > 2)
				{
					result = string.Format(CultureInfo.InvariantCulture, "[one of {0}]", string.Join(", ", lpcosInfo));
				}
				else
				{
					result = string.Format(CultureInfo.InvariantCulture, "[{0}]", string.Join(" or ", lpcosInfo));
				}
			}

			return result;
		}
	}
}
