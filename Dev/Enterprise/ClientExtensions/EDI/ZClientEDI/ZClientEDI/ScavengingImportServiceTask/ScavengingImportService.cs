using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ScavengingImportServiceTask.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	sealed class ScavengingImportService : IImportService
	{
		internal ScavengingImportService(SqlOrganizationScavengingRepository repository, IProcessor processor, INotifications notifier)
		{
			this.repository = Argument.NotNull(repository, "repository");
			this.processor = Argument.NotNull(processor, "processor");
			this.notifier = Argument.NotNull(notifier, "notifier");
		}

		public void Process(CancellationToken token)
		{
			try
			{
				ScavengingItem[] items;
				do
				{
					token.ThrowIfCancellationRequested();
					items = RetrieveItems(BatchSize);
					if (items.Length > 0)
					{
						OnBatchStarted();
						try
						{
							foreach (var item in items)
							{
								token.ThrowIfCancellationRequested();
								ProcessItemSafe(item, DefaultElementsToRemovePKs, Array.Empty<XElement>(), Array.Empty<XElement>());
								if (!item.IsProcessed)
								{
									break;
								}
							}
						}
						finally
						{
							OnBatchFinished(items.Count(item => item.IsImported));
						}
					}
				}
				while (items.Length == BatchSize && !items.Any(item => !item.IsProcessed));
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifier.AddError(ex.ToString());
				throw;
			}
		}

		ScavengingItem[] RetrieveItems(int count)
		{
			Notify("Retrieving messages...");
			var items = repository.Select(count).ToArray();
			Notify(string.Format("{0} messages retrieved.", items.Length));
			return items;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		OrgHeader ProcessItemSafe(ScavengingItem item, IEnumerable<string> elementsToRemovePKs, XElement[] elementsToReplaceArray, XElement[] elementsToModifyArray)
		{
			OrgHeader result = null;
			XElement originalXml = null;
			XElement modifiedXml = null;
			try
			{
				originalXml = CreateXElementFromOtherXElement(item.Content);
				modifiedXml = CreateXElementFromOtherXElement(originalXml);
				ModifyElements(modifiedXml, new[] { originalXml });

				Connection.RunTransactioned(delegate
				{
					result = ProcessItem(item, modifiedXml, elementsToRemovePKs, elementsToReplaceArray, elementsToModifyArray);
				});
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				var nL = System.Environment.NewLine;
				if (ex.Message.Contains("Constraint_ShortCode"))
				{
					notifier.AddError("Org code generated was empty. Please adjust the Org Code generation setting in the registry at 'Organisation > Codes > Organisation Code - Default Set'.");
				}
				else if (ex.Message.Contains("The value of Organisation Code must be unique on Organisation. The duplicate value(s) are:"))
				{
					notifier.AddWarning(string.Format(CultureInfo.CurrentCulture, "Message [PK:'{0}'] Auto-generated Organisation Code is duplicate. The item will be reprocessed in a later run. {2}{1}", item.ID, ex, nL));
				}
				else
				{
					try
					{
						var reprocess = false;
						var reprocessWarning = "";
						var elementsToRemovePKsArray = elementsToRemovePKs as string[] ?? elementsToRemovePKs.ToArray();

						if (ex.Message.Contains("Database parent does not match entity parent.") && !elementsToRemovePKsArray.Contains("*"))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK:'{0}'] Database parent does not match entity parent. All PKs will be stripped and item will be reprocessed.{2}{1}", item.ID, ex, nL);
							elementsToRemovePKsArray = new[] { "*" };
						}
						else if (DuplicateCompositeKeys(ex.Message, elementsToRemovePKsArray, "OrgAddress", OrgAddressSchema.OA_OH.Name, OrgAddressSchema.OA_Code.Name))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK:'{0}'] Address PK and Code mismatch. All addresses PKs will be stripped and item will be reprocessed.{2}{1}", item.ID, ex, nL);
							elementsToRemovePKsArray = elementsToRemovePKsArray.Concat(new[] { "OrgAddress" }).ToArray();
						}
						else if (DuplicateCompositeKeys(ex.Message, elementsToRemovePKsArray, "OrgCusCode", OrgCusCodeSchema.OK_CodeType.Name, OrgCusCodeSchema.OK_OH.Name, OrgCusCodeSchema.OK_RN_NKCodeCountry.Name, OrgCusCodeSchema.OK_OA_PremisesAddress.Name))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK:'{0}'] CusCode PK and Code mismatch. All CusCode PKs will be stripped and item will be reprocessed.{2}{1}", item.ID, ex, nL);
							elementsToRemovePKsArray = elementsToRemovePKsArray.Concat(new[] { "OrgCusCode" }).ToArray();
						}
						else if (DuplicateCompositeKeys(ex.Message, elementsToRemovePKsArray, "OrgContact", OrgContactSchema.OC_ContactName.Name, OrgContactSchema.OC_OH.Name))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK:'{0}'] Contact PK and Code mismatch. All Contact PKs will be stripped and item will be reprocessed.{2}{1}", item.ID, ex, nL);
							elementsToRemovePKsArray = elementsToRemovePKsArray.Concat(new[] { "OrgContact" }).ToArray();
						}
						else if (DuplicateCompositeKeys(ex.Message, elementsToRemovePKsArray, "OrgCarrierServiceLevel", OrgCarrierServiceLevelSchema.PL_CarrierServiceLevelDescription.Name, OrgCarrierServiceLevelSchema.PL_OM.Name))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK: '{0}'] CarrierServiceLevel PK and Code mismatch. All CarrierServiceLevel PKs will be stripped and item will be reprocessed. {2}{1}", item.ID, ex, nL);
							elementsToRemovePKsArray = elementsToRemovePKsArray.Concat(new[] { "OrgCarrierServiceLevel" }).ToArray();
						}
						else if (DuplicateCompositeKeys(ex.Message, elementsToRemovePKsArray, "OrgCarrierServiceLevel", OrgCarrierServiceLevelSchema.PL_Code.Name, OrgCarrierServiceLevelSchema.PL_OM.Name))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK: '{0}'] CarrierServiceLevel PK and Code mismatch. All CarrierServiceLevel PKs will be stripped and item will be reprocessed. {2}{1}", item.ID, ex, nL);
							elementsToRemovePKsArray = elementsToRemovePKsArray.Concat(new[] { "OrgCarrierServiceLevel" }).ToArray();
						}
						else if (ex.Message.Contains("Could not insert/update") && ex.Message.Contains("with the following values:"))
						{
							reprocess = true;
							reprocessWarning = string.Format(CultureInfo.CurrentCulture, "Message [PK:'{0}'] {1} Invalid code will be replaced, and the item will be reprocessed.{2}", item.ID, ex.Message, nL);
							elementsToReplaceArray = elementsToReplaceArray.Concat(new[] { CreateElementToReplace(ex.Message, originalXml) }).ToArray();
						}
						else
						{
							repository.ProcessItemAsDefect(item);
							notifier.AddError(CreateErrorMessage(item, ex, modifiedXml));
						}
						if (reprocess)
						{
							notifier.AddWarning(reprocessWarning);
							result = ProcessItemSafe(item, elementsToRemovePKsArray, elementsToReplaceArray, elementsToModifyArray);
							if (result != null)
							{
								result.Notes.AddNew(true, "Scavenging Import Conflict", ex.Message);
								result.Factory.Save();
							}
						}
					}
					catch (Exception e)
					{
						if (e.IsCriticalException())
						{
							throw;
						}

						repository.ProcessItemAsDefect(item);
						notifier.AddError(CreateErrorMessage(item, e, modifiedXml));
					}
				}
			}
			return result;
		}

		static XElement CreateXElementFromOtherXElement(XElement element)
		{
			XElement originalXml;
			var xmlString = element.ToString(SaveOptions.DisableFormatting).Replace("\r\n", " ");
			using (var streamReader = new XmlTextReader(new StringReader(xmlString)))
			{
				originalXml = XElement.Load(streamReader);
			}

			return originalXml;
		}

		string CreateErrorMessage(ScavengingItem item, Exception ex, XElement xml)
		{
			var nL = System.Environment.NewLine;
			var errorMessage = string.Format(CultureInfo.InvariantCulture, "Message [PK:'{0}'] {1}", item.ID, ex);
			if (xml != null)
			{
				errorMessage += nL + nL + "Message Content (modified):" + nL + xml;
			}
			return errorMessage;
		}

		bool DuplicateCompositeKeys(string message, string[] elementsToRemovePKsArray, string tableName, params string[] columnNames)
		{
			IDataBoundResourceStrings dataResourceStringHelper = new DataBoundResourceStrings();
			var readableTableName = dataResourceStringHelper.GetStringForTable(tableName);
			var readableColumnNames = String.Join(" + ", columnNames.Select(x => dataResourceStringHelper.GetStringForProperty(tableName, x)));
			return message.Contains(string.Format("The value of {0} must be unique on {1}.", readableColumnNames, readableTableName)) && !elementsToRemovePKsArray.Contains(tableName);
		}

		static XElement CreateElementToReplace(string exMessage, XElement xml)
		{
			string message = Regex.Replace(exMessage, @"\t|\n|\r", "");
			var invalidValue = Regex.Match(message, @"Code:(.*?)].$").Groups[1].Value;
			var nodeName = Regex.Match(message, @"\(([^)]*)\)").NextMatch().Groups[1].Value;
			var element = xml.Descendants().First(x => x.Name.LocalName == nodeName &&
				x.Descendants(xml.Name.Namespace + "Code").Any() &&
				x.Descendants(xml.Name.Namespace + "Code").First().Value == invalidValue);
			var tableName = element.Attribute("TableName").Value;

			var elementFirstDescendant = element.Descendants().First();
			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(tableName);
			var columnName = schema.PK.Name.Split('_')[0] + "_" + elementFirstDescendant.Name.LocalName;
			var column = schema.All.Cast<SchemaColumn>().FirstOrDefault(c => c.Name == columnName);
			if (column != null)
			{
				elementFirstDescendant.Value = new string('X', column.MaxLength);
			}
			element.Add(CreateXElementFromOtherXElement(new XElement(element.Name.Namespace + "OldCode", invalidValue)));

			if (elementFirstDescendant.Value == invalidValue)
			{
				elementFirstDescendant.Value = new string('X', invalidValue.Length);
			}
			if (element.Attribute("Action") == null)
			{
				element.Add(new XAttribute("Action", "MERGE"));
			}

			return element;
		}

		OrgHeader ProcessItem(ScavengingItem item, XElement parsedXml, IEnumerable<string> elementsToRemovePKs, XElement[] elementsToReplaceArray, XElement[] elementsToModifyArray)
		{
			OrgHeader org = null;
			if (IsXMLOldFormat(parsedXml))
			{
				Notify("Skipping message in old format which is no longer supported.");
				item.IsProcessed = true;
			}
			else if (!IsSystemOrg(parsedXml))
			{
				var factory = CreateFactoryForImport();
				var orgPk = GetOrganizationPK(parsedXml);
				var firstHistory = LoadFirstImportHistory(factory, orgPk);
				var isOrgOwner = IsOrgOwner(firstHistory, item.Client);
				CleanXml(parsedXml, isOrgOwner, elementsToRemovePKs, elementsToReplaceArray, elementsToModifyArray);
				AddClientKey(parsedXml, item.Client);

				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(parsedXml.ToString())))
				{
					processor.Process(stream);
				}

				org = factory.Load<OrgHeader>(orgPk);
				if (org != null)
				{
					AddClientOrgImportHistory(org, item.Client);
					AddEHubClientIDCodeMapping(org, item.Client);
					factory.Save();

					item.IsProcessed = true;
					item.IsImported = true;
				}
			}
			else
			{
				Notify("Skipping system organization.");
				item.IsProcessed = true;
			}
			if (item.IsProcessed)
			{
				repository.Delete(item.ID);
			}
			return org;
		}

		static bool IsXMLOldFormat(XElement xml)
		{
			var element = xml.XPathSelectElement("//*[local-name()='CompactOrganization']");
			return element != null;
		}

		static bool IsSystemOrg(XElement xml)
		{
			var orgCode = xml.XPathSelectElement("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='Code']").Value;

			return "UNMATCHED".Equals(orgCode) || "MISC".Equals(orgCode);
		}

		static ClientOrgImportHistory LoadFirstImportHistory(BusinessObjectFactory factory, ZGuid orgPK)
		{
			var query = new ZQuery(ClientOrgImportHistorySchema.O2_TargetOrgPK, orgPK);
			query.OrderBy = ClientOrgImportHistory.Schema.O2_ImportedDate + " ASC";
			return factory.LoadTop1<ClientOrgImportHistory>(query);
		}

		static bool IsOrgOwner(ClientOrgImportHistory history, ZString clientID)
		{
			return
				history == null ||
				(history.O2_ClientKey.Left(3) == clientID.Left(3) &&
				 history.O2_ClientKey.Right(3) == clientID.Right(3));
		}

		static void CleanXml(XElement xml, bool isOrgOwner, IEnumerable<string> elementsToRemovePKs, XElement[] elementsToReplaceArray, XElement[] elementsToModifyArray)
		{
			if (!isOrgOwner)
			{
				var orgHeader = xml.XPathSelectElement("//*[local-name()='Organization']/*[local-name()='OrgHeader']");
				orgHeader.Elements().Where(x => x.Name.LocalName != "PK" && x.Name.LocalName != "ClientOrgConsolCollection" && x.Name.LocalName != "ClosestPort" && x.Name.LocalName != "FullName").Remove();
			}
			else
			{
				CleanOrg(xml);
			}

			// Remove the Password node from CusBondDetailCollection
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='CusBondDetailCollection']/*[local-name()='CusBondDetail']/*[local-name()='Password']").Remove();

			CleanClosestPort(xml);

			// FullName is mandatory on Native XML Import
			var fullName = xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='FullName']").First();
			if (string.IsNullOrWhiteSpace(fullName.Value))
			{
				fullName.Value = "XXXXX";
			}

			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgMiscServ']/*[local-name()='CMLeadSourcePerson']").Remove();

			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgMiscServ']/*[local-name()='EXDefaultDGContact']").Remove();

			ReplaceElements(xml, elementsToReplaceArray);

			ModifyElements(xml, elementsToModifyArray);

			RemovePKs(xml, elementsToRemovePKs);
		}

		static void CleanOrg(XElement xml)
		{
			// GlbCompany
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgCompanyDataCollection']").Remove();
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgRateTariffLevelCollection']").Remove();
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgStaffAssignmentsCollection']").Remove();
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgLandedCostingPrefsCollection']").Remove();

			// OrgHeader
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgSupplierBuyerLink_SupplierCollection' or local-name()='OrgSupplierBuyerLink_BuyerCollection']").Remove();
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgSales_SupplierCollection' or local-name()='OrgSales_BuyerCollection']").Remove();
			// GlbCompany & OrgHeader
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgRelatedPartyCollection']").Remove();
			// OrgHeader
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgAppointedAgentPortsCollection']").Remove();
			// OrgHeader
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='JobRequiredDocumentCollection']/*[local-name()='JobRequiredDocument']/*[local-name()='DocumentOwner']").Remove();
			// OrgAddress & OrgContact
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgOpportunityCollection']").Remove();

			// OrgAddress
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgContactCollection']/*[local-name()='OrgContact']/*[local-name()='OrgAddress' or local-name()='AddressOverride']").Remove();

			// OrgDocument
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgContactCollection']/*[local-name()='OrgContact']/*[local-name()='OrgDocumentCollection']").Remove();
			// OrgSecurity
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgContactCollection']/*[local-name()='OrgContact']/*[local-name()='OrgSecurityContactsCollection']").Remove();
			// OrgContact Password
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgContactCollection']/*[local-name()='OrgContact']/*[local-name()='Password']").Remove();
			// OrgSecurityCollection
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgSecurityCollection']").Remove();

			//OrgServiceLevelCollection
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgServiceLevelCollection']").Remove();
			//OrgSalesCallCollection
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgSalesCallCollection']").Remove();

			//OrgCountryDataCollection
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgCountryDataCollection']").Remove();

			//OrgCarrierServiceLevelCollection
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgMiscServ']/*[local-name()='OrgCarrierServiceLevelCollection']").Remove();

			// GlbGroupOrgContactLink
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgContactCollection']/*[local-name()='OrgContact']/*[local-name()='GlbGroupOrgContactLinkCollection']").Remove();

			// GlbGroupOrgLink
			xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='OrgContactCollection']/*[local-name()='OrgContact']/*[local-name()='GlbGroupOrgLinkCollection']").Remove();
		}

		static void CleanClosestPort(XElement xml)
		{
			// UNLOCO is mandatory on Native XML Import
			if (!xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']").Any())
			{
				var ns = xml.Name.Namespace;
				var closestPort = CreateXElementFromOtherXElement(new XElement(ns + OrgHeader.Schema.OH_RL_NKClosestPort.Substring(8),
					new XAttribute("TableName", RefUNLOCO.Schema.TableName), CreateXElementFromOtherXElement(new XElement(ns + RefUNLOCO.Schema.RL_Code.Substring(3), "XXXXX")),
					new XAttribute("Action", "MERGE")));

				xml.XPathSelectElement("//*[local-name()='Organization']/*[local-name()='OrgHeader']").Add(closestPort);
			}
			else if (!xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']/*[local-name()='Code']").Any())
			{
				var ns = xml.Name.Namespace;
				var code = CreateXElementFromOtherXElement(new XElement(ns + RefUNLOCO.Schema.RL_Code.Substring(3), "XXXXX"));
				xml.XPathSelectElement("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']").Add(code);
				xml.XPathSelectElement("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']").SetAttributeValue("Action", "MERGE");
			}
			else if (string.IsNullOrWhiteSpace(xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']/*[local-name()='Code']").First().Value))
			{
				xml.XPathSelectElements("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']/*[local-name()='Code']").First().Value = "XXXXX";
				xml.XPathSelectElement("//*[local-name()='Organization']/*[local-name()='OrgHeader']/*[local-name()='ClosestPort']").SetAttributeValue("Action", "MERGE");
			}
		}

		static void ReplaceElements(XElement xml, XElement[] elementsToReplaceArray)
		{
			if (elementsToReplaceArray.Length > 0)
			{
				foreach (var elementToReplace in elementsToReplaceArray)
				{
					var ns = elementToReplace.Name.Namespace;
					var elements = xml.Descendants().Where(x => x.Name == elementToReplace.Name &&
						x.Descendants(ns + "Code").Any() &&
						x.Descendants(ns + "Code").First().Value == elementToReplace.Descendants(ns + "OldCode").First().Value).ToArray();
					foreach (var element in elements)
					{
						var e = CreateXElementFromOtherXElement(new XElement(elementToReplace));
						e.Descendants(ns + "OldCode").First().Remove();
						if (e.Descendants(ns + "PK").Any())
						{
							e.Descendants(ns + "PK").First().Remove();
						}
						element.ReplaceWith(e);
					}
				}
			}
		}

		static void ModifyElements(XElement xml, XElement[] elementsToModifyArray)
		{
			if (elementsToModifyArray.Length > 0)
			{
				foreach (var elementToModify in elementsToModifyArray)
				{
					var elements = xml.Descendants().Where(x => x.Name.LocalName == elementToModify.Name.LocalName && x.Value.Replace("\r", "") == elementToModify.Value.Replace("\r", "")).ToArray();
					foreach (var element in elements)
					{
						element.Value = element.Value.Replace("\r", "");
					}
				}
			}
		}

		static void RemovePKs(XElement xml, IEnumerable<string> elementsToRemovePKs)
		{
			var toRemovePKs = elementsToRemovePKs as string[] ?? elementsToRemovePKs.ToArray();
			if (toRemovePKs.Length > 0)
			{
				var pkParentCondition = toRemovePKs.Contains("*")
					? "local-name()!='OrgHeader'"
					: string.Join(" or ", toRemovePKs.Select(element => "local-name()='" + element + "'"));
				var pkXPath = string.Format(CultureInfo.InvariantCulture, "//*[local-name()='Organization']/*[local-name()='OrgHeader']//*[{0}]/*[local-name()='PK']", pkParentCondition);
				xml.XPathSelectElements(pkXPath).Remove();
			}
		}

		static IEnumerable<string> DefaultElementsToRemovePKs
		{
			get
			{
				yield return "OrgAddressCapability";
				yield return "OrgMiscServ";
			}
		}

		void OnBatchStarted()
		{
			Notify("Importing messages...");
		}

		void OnBatchFinished(int importedOrgsCount)
		{
			Notify(importedOrgsCount == 1
				? "1 organization has been imported to database."
				: string.Format("{0} organizations have been imported to database.", importedOrgsCount));
		}

		static void AddEHubClientIDCodeMapping(OrgHeader org, string clientKey)
		{
			var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, org.PK)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, clientKey);

			if (!org.Factory.Exists(typeof(OrgPatternMatchOverride), query))
			{
				var mapping = org.Factory.New<OrgPatternMatchOverride>();
				using (mapping.SuspendSettingHasChanges())
				using (mapping.GetValidationSuspender())
				{
					mapping.OO_OH = org.PK;
					mapping.OO_ForeignCode = clientKey;
					mapping.OO_Relationship = EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID;
				}
			}
		}

		internal void AddClientOrgImportHistory(OrgHeader org, string clientKey)
		{
			var operationTime = org.OH_SystemLastEditTimeUtc;

			var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID)
				.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, org.PK);
			var isExisting = org.Factory.Exists(typeof(OrgPatternMatchOverride), query);
			if (isExisting)
			{
				if (operationTime.IsEmpty || IsTooOld(operationTime) || operationTime.IsInTheFutureDatePartOnly)
				{
					return;
				}

				ScavengingPartitioner.EnsureDBPartitionExists(operationTime);
			}

			var importHistory = org.Factory.New<ClientOrgImportHistory>();
			importHistory.O2_OrgCode = org.OH_Code;
			importHistory.O2_TargetOrgPK = org.PK;
			importHistory.O2_ImportedDate = operationTime;
			importHistory.O2_ActionType = !isExisting ? "CreateNew" : "Merge (Native)";
			importHistory.O2_MatchCount = !isExisting ? 0 : 1;
			importHistory.O2_ClientKey = clientKey;
		}

		bool IsTooOld(ZDateTime operationTime)
		{
			var setting = EDIDataRegistry.Instance.ScavengingPurgeSettings.Value.OfType<ScavengingPurgeItem>().First(s => s.Code == ScavengingPurgeSettings.ClientOrgImportHistory);
			var oldestTime = ScavengingPurgeSettings.GetMaxPurgeDateTime(setting);
			return operationTime < oldestTime;
		}

		void Notify(string message)
		{
			Thread.Sleep(1); // Required for a proper notifications order in DB
			notifier.Notify(new InfoNotification(message));
		}

		static BusinessObjectFactory CreateFactoryForImport()
		{
			var factory = new BusinessObjectFactory(Connection) { NameForDebugging = "Scavenging Item Import" };
			factory.SuspendValidation();
			return factory;
		}

		static DbConnection Connection
		{
			get { return Db.Connection; }
		}

		static ZGuid GetOrganizationPK(XElement nativeXElement)
		{
			var resolver = new XmlNamespaceManager(new NameTable());
			resolver.AddNamespace("ns", nativeXElement.Name.NamespaceName);
			var pkElement = nativeXElement.XPathSelectElement("./ns:Body/ns:Organization/ns:OrgHeader/ns:PK", resolver);
			return new ZGuid(pkElement.Value);
		}

		static void AddClientKey(XElement nativeXElement, string clientKey)
		{
			var ns = nativeXElement.Name.Namespace;
			foreach (var clientOrgConsol in nativeXElement.Descendants(ns + ClientOrgConsol.Schema.TableName))
			{
				clientOrgConsol.Add(CreateXElementFromOtherXElement(new XElement(ns + ClientOrgConsol.Schema.O7_ClientKey.Substring(3), clientKey)));
			}
		}

		const int BatchSize = 100;

		readonly SqlOrganizationScavengingRepository repository;
		readonly IProcessor processor;
		readonly INotifications notifier;
	}
}
