using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DocBuilder
{
	internal static class TemplateLanguageSelector
	{
		public static ZString SelectLanguage(OrgHeaderContact orgHeaderContact, string documentGroup = "", string transportMode = "", StmMenuItem stmMenuItem = null)
		{
			var client = orgHeaderContact?.OrgHeader;
			var defaultLanguageOrder = DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.Value;
			var selectedLanguage = DataRegistry.Instance.EnglishSpelling;

			if (defaultLanguageOrder != null && defaultLanguageOrder.Count > 0)
			{
				var newLanguage = SelectDefaultLanguage(orgHeaderContact, defaultLanguageOrder, documentGroup, transportMode, stmMenuItem?.PK ?? Guid.Empty);
				if (!string.IsNullOrEmpty(newLanguage))
				{
					selectedLanguage = newLanguage;
				}
			}
			else
			{
				var languageIsForeign = client != null && GlbCompany.CurrentCompany.OrgProxy != null && client.OH_Language == GlbCompany.CurrentCompany.OrgProxy.OH_Language && !Res.IsEnglish(client.OH_Language);
				if (languageIsForeign)
				{
					selectedLanguage = client.OH_Language.ToString();
				}
			}

			return selectedLanguage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string SelectDefaultLanguage(OrgHeaderContact orgHeaderContact, DocumentDeliveryDefaultLanguagesCollection defaultLanguageOrder, string documentGroup, string transportMode, ZGuid stmMenuItemPk)
		{
			var selectedLanguage = string.Empty;
			var client = orgHeaderContact?.OrgHeader;

			foreach (var item in defaultLanguageOrder.Cast<DocumentDeliveryDefaultLanguages>().OrderBy(d => d.Order))
			{
				if (Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact.Equals(item.Fallback, StringComparison.OrdinalIgnoreCase))
				{
					var contactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(ContactType.Find(documentGroup), transportMode);
					if (contactType != null)
					{
						var contactLanguage = GetContactLanguage(client, stmMenuItemPk, contactType.Code);
						if (!string.IsNullOrEmpty(contactLanguage))
						{
							selectedLanguage = contactLanguage;
							break;
						}
					}
				}
				else if (Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address.Equals(item.Fallback, StringComparison.OrdinalIgnoreCase))
				{
					if (client != null)
					{
						var selectedAddressLanguage = orgHeaderContact?.OrgAddress?.OA_Language;
						if (!string.IsNullOrEmpty(selectedAddressLanguage))
						{
							selectedLanguage = selectedAddressLanguage;
							break;
						}
						else if (!string.IsNullOrEmpty(client.MainAddress?.OA_Language))
						{
							selectedLanguage = client.MainAddress.OA_Language;
							break;
						}
					}
				}
				else if (Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization.Equals(item.Fallback, StringComparison.OrdinalIgnoreCase))
				{
					if (client != null && !string.IsNullOrEmpty(client.OH_Language))
					{
						selectedLanguage = client.OH_Language;
						break;
					}
				}
				else if (Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch.Equals(item.Fallback, StringComparison.OrdinalIgnoreCase))
				{
					if (!string.IsNullOrEmpty(GlbBranch.CurrentBranch.OrgProxy?.OH_Language))
					{
						selectedLanguage = GlbBranch.CurrentBranch.OrgProxy.OH_Language;
						break;
					}
				}
				else if (Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company.Equals(item.Fallback, StringComparison.OrdinalIgnoreCase))
				{
					if (!string.IsNullOrEmpty(GlbCompany.CurrentCompany.OrgProxy?.OH_Language))
					{
						selectedLanguage = GlbCompany.CurrentCompany.OrgProxy.OH_Language;
						break;
					}
				}
				else if (Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.System.Equals(item.Fallback, StringComparison.OrdinalIgnoreCase))
				{
					selectedLanguage = DataRegistry.Instance.EnglishSpelling;
					break;
				}
			}

			return selectedLanguage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static string GetContactLanguage(OrgHeader client, ZGuid stmMenuItemPk, string contactType)
		{
			var result = string.Empty;
			var activeContacts = client?.Contacts.Cast<OrgContact>().Where(c => c.OC_IsActive);

			if (activeContacts != null && activeContacts.Any())
			{
				var orgDocuments = activeContacts.SelectMany(c => c.Documents.Cast<OrgDocument>().Where(d => !d.OD_DeliverBy.EqualsIgnoringCase(Core.Constants.ContactNotifyModes.DoNotDeliver)));

				//Contact Priority 1 => Match the specific document and is official
				//Contact Priority 2 => Match the specific document and is not official
				var documentMatched = orgDocuments.Where(d => stmMenuItemPk != Guid.Empty && d.OD_SU_MenuItem == stmMenuItemPk);
				if (documentMatched.Any())
				{
					var matchedOrgDocument = documentMatched.FirstOrDefault(d => d.OD_DefaultContact);
					result = matchedOrgDocument != null ? matchedOrgDocument.Contact.OC_Language : documentMatched.First().Contact.OC_Language;
				}
				else
				{
					//Contact Priority 3 => Match the specific document group and is official
					//Contact Priority 4 => Match the specific document group and is not official
					var documentGroupMatched = FindDocumentGroupMatched(orgDocuments, contactType);
					if (documentGroupMatched.Any())
					{
						var matchedOrgDocument = documentGroupMatched.FirstOrDefault(d => d.OD_DefaultContact);
						result = matchedOrgDocument != null ? matchedOrgDocument.Contact.OC_Language : documentGroupMatched.First().Contact.OC_Language;
					}
					else
					{
						//Contact Priority 5 => Document group is ALL and is official
						//Contact Priority 6 => Document group is ALL and is not official
						var allGroupMatched = orgDocuments.Where(d => d.OD_DocumentGroup.EqualsIgnoringCase(ContactType.All.Code));
						if (allGroupMatched.Any())
						{
							var matchedOrgDocument = allGroupMatched.FirstOrDefault(d => d.OD_DefaultContact);
							result = matchedOrgDocument != null ? matchedOrgDocument.Contact.OC_Language : allGroupMatched.First().Contact.OC_Language;
						}
					}
				}
			}

			return result;
		}

		static IEnumerable<OrgDocument> FindDocumentGroupMatched(IEnumerable<OrgDocument> orgDocuments, string contactType)
		{
			var documentGroupMatched = orgDocuments.Where(d => d.OD_DocumentGroup.EqualsIgnoringCase(contactType));
			if (documentGroupMatched.Any())
			{
				return documentGroupMatched;
			}

			var parentContactType = ContactType.Find(contactType);
			if (parentContactType != null && !string.IsNullOrEmpty(parentContactType.AggregateParentType))
			{
				documentGroupMatched = FindDocumentGroupMatched(orgDocuments, parentContactType.AggregateParentType);
			}

			return documentGroupMatched;
		}
	}
}
