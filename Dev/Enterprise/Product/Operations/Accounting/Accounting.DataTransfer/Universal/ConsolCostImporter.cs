using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class ConsolCostImporter
	{
		public ConsolCostImporter()
		{
			addMappings();
			addFilterMapping();
		}

		void addMappings()
		{
			Mapping.Add(ConsolCostLineElementType.ChargeCode, JobConsolCostSchema.E6_AC_ChargeCode);
			Mapping.Add(ConsolCostLineElementType.CostOSCurrency, JobConsolCostSchema.E6_RX_NKCurrency);
			Mapping.Add(ConsolCostLineElementType.CostOSAmount, JobConsolCostSchema.E6_OSCostAmount);
			Mapping.Add(ConsolCostLineElementType.CostLocalAmount, JobConsolCostSchema.E6_LocalCostAmount);
			Mapping.Add(ConsolCostLineElementType.CostAPInvoiceNumber, JobConsolCostSchema.E6_InvoiceNum);
			Mapping.Add(ConsolCostLineElementType.CostInvoiceDate, JobConsolCostSchema.E6_InvoiceDate);
			Mapping.Add(ConsolCostLineElementType.CostDueDate, JobConsolCostSchema.E6_PaymentDate);
			Mapping.Add(ConsolCostLineElementType.CostGSTVATID, JobConsolCostSchema.E6_AT_TaxRate);
			Mapping.Add(ConsolCostLineElementType.CostOSGSTVATAmount, JobConsolCostSchema.E6_OSGSTAmount);
			Mapping.Add(ConsolCostLineElementType.Creditor, JobConsolCostSchema.E6_OH_Creditor);
			Mapping.Add(ConsolCostLineElementType.CostExchangeRate, JobConsolCostSchema.E6_ExchangeRate);
			Mapping.Add(ConsolCostLineElementType.SupplierReference, JobConsolCostSchema.E6_CostReference);
			Mapping.Add(ConsolCostLineElementType.PrepaidCollectFilter, JobConsolCostSchema.E6_PPDCLT);
			Mapping.Add(ConsolCostLineElementType.ApportionmentMethod, JobConsolCostSchema.E6_ApportionmentMethod);
			Mapping.Add(ConsolCostLineElementType.IncludeOnCollectInvoice, JobConsolCostSchema.E6_IsForCollectInvoice);
			Mapping.Add(ConsolCostLineElementType.RatingBehaviour, JobConsolCostSchema.E6_RatingBehaviour);
			Mapping.Add(PlaceOfSupplyElement.Location, JobConsolCostSchema.E6_PlaceOfSupply);
			Mapping.Add(PlaceOfSupplyElement.LocationType, JobConsolCostSchema.E6_PlaceOfSupplyType);
			Mapping.Add(ConsolCostLineElementType.SupplyType, JobConsolCostSchema.E6_SupplyType);

			// non persistent property
			var column_E6_ApportionToRelatedShipments = new SchemaBoolColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(JobConsolCostSchema.Constants.TableName), JobConsolCost.Schema.E6_ApportionToRelatedShipments, 0, false, false, false);
			Mapping.Add(ConsolCostLineElementType.ApportionToSubShipments, column_E6_ApportionToRelatedShipments);

			var column_E6_SellGovtChargeCode = new SchemaStringColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(JobConsolCostSchema.Constants.TableName),JobConsolCost.Schema.E6_SellGovtChargeCode, 1, SqlDbType.NVarChar, "", false, 23);
			Mapping.Add(ConsolCostLineElementType.GovernmentReportingSellChargeCode, column_E6_SellGovtChargeCode);

			var column_E6_CostGovtChargeCode = new SchemaStringColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(JobConsolCostSchema.Constants.TableName), JobConsolCost.Schema.E6_CostGovtChargeCode, 2, SqlDbType.NVarChar, "", false, 23);
			Mapping.Add(ConsolCostLineElementType.GovernmentReportingCostChargeCode, column_E6_CostGovtChargeCode);
		}

		void addFilterMapping()
		{
			FilterMapping.Add(ConsolCostLineElementType.CostIsPosted, (x, c) => x.IsPosted == new ZBool(c.Value.Value));
			FilterMapping.Add(ConsolCostLineElementType.CostAPInvoiceNumber, (x, c) => x.E6_InvoiceNum == c.Value.Value);
		}

		readonly Dictionary<Enum, SchemaColumn> Mapping = new Dictionary<Enum, SchemaColumn>();
		readonly Dictionary<Enum, ConsolCostMatcher.MatchingCriteriaDelegate> FilterMapping = new Dictionary<Enum, ConsolCostMatcher.MatchingCriteriaDelegate>();

		enum PlaceOfSupplyElement
		{
			Location,
			LocationType
		}

		#region ImportCharges

		public void ImportConsolCosts(BusinessObjectFactory factory, IXmlImportLogger logger, IConsolCostsData consol, ZGuid parentPK, ZString parentTablePrefix)
		{
			string error = string.Empty;

			GlbCompany company = null;
			ZString dataProvider = null;

			if (consol != null && consol.ConsolCosts != null && consol.ConsolCosts.ConsolCostLineCollection != null && consol.ConsolCosts.ConsolCostLineCollection.Any())
			{
				company = GetCompany(factory, logger, consol);

				// There always should be TopLevelDataContext to use DataProviderForCodeMapping for mapping MatchingCriteria codes, but it happens in Universal unit tests
				if (logger.TopLevelDataContext != null)
				{
					dataProvider = logger.TopLevelDataContext.DataProviderForCodeMapping;
				}

				ApportionmentListing apportionmentListing = null;
				BusinessObject bizO = factory.Load(parentTablePrefix, parentPK);
				var consolCostParent = bizO as IGenericJobCostPlugIn;

				if (consolCostParent != null)
				{
					apportionmentListing = new ApportionmentListing(consolCostParent.Factory, consolCostParent, false);
				}

				if (apportionmentListing != null)
				{
					var matcher = new ConsolCostMatcher(factory, company, dataProvider, logger, Mapping, FilterMapping);

					// Ensure validation runs to prevent a CriticalValidation error being generated when the business object is saved.
					var resumeValidationTemporarily = factory.IsValidationSuspended
						? new DisposableAction(() => factory.ResumeValidation(), () => factory.SuspendValidation()) 
						: null;

					using (resumeValidationTemporarily)
					using (factory.SetTempContext(BusinessContext.JobCreatedFromImporter))
					{
						var hasClosedShipmentLazy = new Lazy<bool>(() => HasAnyClosedShipment(consolCostParent, logger, company));
						foreach (var consolCostLine in consol.ConsolCosts.ConsolCostLineCollection)
						{
							if (consolCostLine.ImportMetaData != null)
							{
								var consolCostCreationErrors = ProcessConsolCostLine(consolCostLine, parentPK, parentTablePrefix
									, apportionmentListing, matcher, logger
									, hasClosedShipmentLazy);
								error = NullableExtensions.JoinExcludingEmpty(System.Environment.NewLine, new[] { error, consolCostCreationErrors });
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(error))
			{
				throw new DataObjectReadFailureException(error);
			}
		}

		string ProcessConsolCostLine(ConsolCostLine consolCostLine, ZGuid parentPK, ZString parentTablePrefix
			, ApportionmentListing apportionmentListing, ConsolCostMatcher matcher, IXmlImportLogger logger
			, Lazy<bool> hasClosedShipmentLazy)
		{
			var error = string.Empty;

			var consolCost = null as JobConsolCost;
			try
			{
				var matchingConsolCosts = GetMatchingConsolCosts();
				switch (consolCostLine.ImportMetaData.Instruction)
				{
					case InstructionType.Insert:
						if (!hasClosedShipmentLazy.Value)
						{
							consolCost = insertConsolCostLine(apportionmentListing, consolCostLine, logger);
						}
						break;
					case InstructionType.Update:
						consolCost = updateConsolCostLine(apportionmentListing, consolCostLine, matchingConsolCosts, logger);
						break;
					case InstructionType.UpdateAndInsertIfNotFound:
						if (matchingConsolCosts.Length == 0)
						{
							if (!hasClosedShipmentLazy.Value)
							{
								consolCost = insertConsolCostLine(apportionmentListing, consolCostLine, logger);
							}
						}
						else
						{
							consolCost = updateConsolCostLine(apportionmentListing, consolCostLine, matchingConsolCosts, logger);
						}
						break;
					case InstructionType.Delete:
						deleteConsolCostLine(apportionmentListing, matchingConsolCosts);
						break;
					default:
						break;
				}

				if (consolCostLine.ImportMetaData.Instruction != InstructionType.Delete && consolCost != null)
				{
					var validationErrors = ValidateCosnolCostAndCollectErrors(parentPK, parentTablePrefix, consolCostLine, consolCost);
					if (!string.IsNullOrEmpty(validationErrors))
					{
						error = NullableExtensions.JoinExcludingEmpty(System.Environment.NewLine, new[] { error, validationErrors });
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				error = logExceptionError(consolCostLine, parentPK, parentTablePrefix, ex.Message, error, logger);
			}
			catch (MatchingCriteriaException ex)
			{
				error = logExceptionError(consolCostLine, parentPK, parentTablePrefix, ex.Message, error, logger);
			}
			catch (JobCreationException ex)
			{
				error = logExceptionError(consolCostLine, parentPK, parentTablePrefix, ex.Message, error, logger);
			}
			return error;

			JobConsolCost[] GetMatchingConsolCosts()
			{
				if (consolCostLine.ImportMetaData.Instruction == InstructionType.Insert)
				{
					return null;
				}

				return matcher.GetMatchingConsolCosts(apportionmentListing, consolCostLine)
					?? throw new InvalidOperationException(Res.GetString("8e28f776-9527-4bbd-948b-d9a9a6521aeb", "No matching criteria specified."));
			}
		}

		static string ValidateCosnolCostAndCollectErrors(ZGuid parentPK, ZString parentTablePrefix, ConsolCostLine consolCostLine, JobConsolCost consolCost)
		{
			var error = new ZStringBuilder();

			consolCost.RunPreSaveValidation();
			if (consolCost.HasErrors)
			{
				error.Append(getMessage(parentPK, parentTablePrefix, consolCostLine));

				ZNotificationCollector notificationCollector = new ZNotificationCollector(consolCost, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

				foreach (string uniqueError in notificationCollector.GetUniqueMessageList())
				{
					error.Append(uniqueError);
				}
			}

			return error.ToStringWithNewLineBetweenAppends().TrimEnd();
		}

		static GlbCompany GetCompany(BusinessObjectFactory factory, IXmlImportLogger logger, IConsolCostsData consol)
		{
			GlbCompany company = null;
			if (consol.DataContext != null && !consol.DataContext.CompanyCodeToImportInto.IsEmpty)
			{
				company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, consol.DataContext.CompanyCodeToImportInto));
			}
			else if (logger.TopLevelDataContext != null && !logger.TopLevelDataContext.CompanyCodeToImportInto.IsEmpty)
			{
				company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, logger.TopLevelDataContext.CompanyCodeToImportInto));
			}

			if (company == null)
			{
				throw new DataObjectReadFailureException(Res.GetString("cd15ba4c-5c19-42a0-83f8-5ee7a37cca41", "You must specify a Company Code in the Shipment's DataContext."));
			}

			return company;
		}

		JobConsolCost insertConsolCostLine(ApportionmentListing apportionmentListing, ConsolCostLine consolCostLine, IXmlImportLogger importLogger)
		{
			JobConsolCost consolCost = apportionmentListing.CostsCollection.TryAddNew();
			if (consolCost != null)
			{
				setValues(apportionmentListing, consolCost, consolCostLine, importLogger);
			}
			else
			{
				var error = Res.GetString("c8c15dcf-b5b6-418d-8bdc-847125fd0483", "Consol with Master Bill number '{0}' does not support adding new Consol Costs.",
					apportionmentListing.CostsCollection.GenericJobCostPlugIn.CostSupporter.MasterBillNum);
				throw new InvalidOperationException(error);
			}
			return consolCost;
		}

		JobConsolCost updateConsolCostLine(ApportionmentListing apportionmentListing, ConsolCostLine consolCostLine, JobConsolCost[] consolCosts, IXmlImportLogger importLogger)
		{
			JobConsolCost consolCost = null;
			var error = string.Empty;

			if (apportionmentListing.CostsCollection.ReadOnly)
			{
				error = Res.GetString("0d257614-4875-4b09-b364-d0b97631b24a", "Consol with Master Bill number '{0}' does not support updating its Consol Costs.",
					apportionmentListing.CostsCollection.GenericJobCostPlugIn.CostSupporter.MasterBillNum);
			}
			else
			{
				if (consolCosts.Length > 1)
				{
					error = Res.GetString("f4768ea4-cafb-4806-9bc9-b648cbbd24d5", "Multiple consol costs found when updating consol cost Line.");
				}
				else if (consolCosts.Length == 0)
				{
					error = Res.GetString("d0b17744-14bd-47cb-a86f-35edbbe00024", "Consol cost not found when updating consol cost Line.");
				}
				else
				{
					consolCost = consolCosts[0];
					setValues(apportionmentListing, consolCost, consolCostLine, importLogger);
				}
			}

			if (!string.IsNullOrEmpty(error))
			{
				throw new InvalidOperationException(error);
			}

			return consolCost;
		}

		static void deleteConsolCostLine(ApportionmentListing apportionmentListing, JobConsolCost[] matchingConsolCosts)
		{
			string error = string.Empty;
			if (apportionmentListing.CostsCollection.ReadOnly)
			{
				error = Res.GetString("c2f4fe76-7f32-48c9-9449-93db4f03627c", "Consol with Master Bill number '{0}' does not support deleting its Consol Costs.",
					apportionmentListing.CostsCollection.GenericJobCostPlugIn.CostSupporter.MasterBillNum);
			}
			else
			{
				if (matchingConsolCosts.Length == 0)
				{
					error += Res.GetString("fa656a1f-a901-4531-b728-80abdb7bd84a", "No consol costs found when deleting consol cost Line.");
				}
				else if (matchingConsolCosts.Length > 0)
				{
					if (matchingConsolCosts.All(consolCost => consolCost.CanDelete))
					{
						foreach (var consolCost in matchingConsolCosts)
						{
							consolCost.Delete();
						}
					}
					else
					{
						foreach (var consolCost in matchingConsolCosts)
						{
							if (!consolCost.CanDelete)
							{
								error += consolCost.ReasonForNotAbleToDelete + "\r\n";
							}
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(error))
			{
				throw new InvalidOperationException(error);
			}
		}

		void setValues(ApportionmentListing apportionmentListing, JobConsolCost jobConsolCost, ConsolCostLine consolCostLine, IXmlImportLogger importLogger)
		{
			if (consolCostLine.ChargeCode != null)
			{
				setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.ChargeCode, (ZString?)consolCostLine.ChargeCode.Code, importLogger);
			}

			if (consolCostLine.Creditor != null)
			{
				setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.Creditor, (ZString?)consolCostLine.Creditor.Key, importLogger);
			}

			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostAPInvoiceNumber, consolCostLine.CostAPInvoiceNumber, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostInvoiceDate, consolCostLine.CostInvoiceDate, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostDueDate, consolCostLine.CostDueDate, importLogger);

			if (consolCostLine.CostOSCurrency != null)
			{
				setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostOSCurrency, consolCostLine.CostOSCurrency.Code, importLogger);
			}

			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostOSAmount, consolCostLine.CostOSAmount, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostLocalAmount, consolCostLine.CostLocalAmount, importLogger);

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				if (consolCostLine.SupplyType != null)
				{
					setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.SupplyType, consolCostLine.SupplyType.Code, importLogger);
				}
			}

			if (consolCostLine.CostGSTVATID != null)
			{
				setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostGSTVATID, consolCostLine.CostGSTVATID.TaxCode, importLogger);
			}

			TryToSetGSTValue();
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.SupplierReference, consolCostLine.SupplierReference, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.ApportionmentMethod, consolCostLine.ApportionmentMethod, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.PrepaidCollectFilter, consolCostLine.PrepaidCollectFilter, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.IncludeOnCollectInvoice, consolCostLine.IncludeOnCollectInvoice, importLogger);
			setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.ApportionToSubShipments, consolCostLine.ApportionToSubShipments, importLogger);

			if (consolCostLine.RatingBehaviour != null)
			{
				setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.RatingBehaviour, consolCostLine.RatingBehaviour.Code, importLogger);
			}

			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				if (consolCostLine.GovernmentReportingSellChargeCode.HasValue)
				{
					setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.GovernmentReportingSellChargeCode, consolCostLine.GovernmentReportingSellChargeCode, importLogger);
				}

				if (consolCostLine.GovernmentReportingCostChargeCode.HasValue)
				{
					setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.GovernmentReportingCostChargeCode, consolCostLine.GovernmentReportingCostChargeCode, importLogger);
				}
			}

			if (consolCostLine.CostIsPosted.HasValue)
			{
				importLogger.Log(Enterprise.Integration.LogType.Warning,
									Res.GetString("7b701473-4372-4572-a28e-d35c90344397", "'{0}' should not be populated for Import. {1}", "CostIsPosted",
									getMessage(apportionmentListing.ConsolPK, apportionmentListing.ConsolType, consolCostLine)));
			}

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(jobConsolCost.Company) && consolCostLine.PlaceOfSupply != null)
			{
				setValue(apportionmentListing, consolCostLine, jobConsolCost, PlaceOfSupplyElement.Location, consolCostLine.PlaceOfSupply.Location.Code, importLogger);
				setValue(apportionmentListing, consolCostLine, jobConsolCost, PlaceOfSupplyElement.LocationType, consolCostLine.PlaceOfSupply.LocationType.Code, importLogger);
			}

			void TryToSetGSTValue()
			{
				var isValueSet = false;
				var oldFlagValue =	jobConsolCost.E6_IsTaxAmountOverridden;
				var oldGSTAmt = jobConsolCost.E6_OSGSTAmount_Calc;
				try
				{
					jobConsolCost.E6_IsTaxAmountOverridden = true;
					isValueSet = setValue(apportionmentListing, consolCostLine, jobConsolCost, ConsolCostLineElementType.CostOSGSTVATAmount, consolCostLine.CostOSGSTVATAmount, importLogger);
				}
				finally
				{
					if (!isValueSet)
					{
						jobConsolCost.E6_IsTaxAmountOverridden = oldFlagValue;
						if (jobConsolCost.E6_IsTaxAmountOverridden)
						{
							jobConsolCost.E6_OSGSTAmount_Calc = oldGSTAmt;
						}
					}
				}
			}
		}

		bool setValue<T>(ApportionmentListing apportionmentListing, ConsolCostLine consolCostLine, JobConsolCost businessObj, Enum xmlFieldName, T? value, IXmlImportLogger importLogger)
			where T : struct, IZType
		{
			Action<ZString> onPropertyReadOnlyAction = propertyName =>
				{
					string postedMessage = string.Empty;

					if (businessObj.IsPosted)
					{
						postedMessage += "  " + Res.GetString("2eaec816-4848-47b7-a16a-06b9f73de98c", "The consol cost is posted.");
					}

					importLogger.Log(Enterprise.Integration.LogType.Warning,
									Res.GetString("f355f26d-df6f-4004-afc6-747ab788f182", "{0}\r\n{1} is read-only and was not updated.{2}",
									getMessage(apportionmentListing.ConsolPK, apportionmentListing.ConsolType, consolCostLine), propertyName, postedMessage));
				};

			if (Mapping[xmlFieldName].Name == JobConsolCostSchema.Constants.E6_OSGSTAmount)
			{
				return Helpers.SetValue(businessObj, xmlFieldName, value, Mapping, onPropertyReadOnlyAction: onPropertyReadOnlyAction, customValueSetter: (val) => businessObj.E6_OSGSTAmount_Calc = new ZDecimal(val));
			}
			else
			{
				return Helpers.SetValue(businessObj, xmlFieldName, value, Mapping, onPropertyReadOnlyAction: onPropertyReadOnlyAction);
			}
		}

		//string getMessage(IGenericJobCostPlugIn consol, ConsolCostLine consolCostLine) // will be better to use IGenericJobCostPlugIn but need to extend the interface to have a ConsolNumber field!
		static string getMessage(ZGuid parentPK, ZString parentTablePrefix, ConsolCostLine consolCostLine)
		{
			return Res.GetString("0532e624-8954-4c67-8f2c-59565b21bb80", "Whilst importing Consol Cost Line: Parent PK={0} Parent Table Code={1} Charge Code={2} Creditor={3}, Cost OS Amount={4}",
									parentPK, parentTablePrefix,
									consolCostLine.ChargeCode != null ? consolCostLine.ChargeCode.Code : ZString.Empty,
									consolCostLine.Creditor != null ? consolCostLine.Creditor.Key : ZString.Empty,
									consolCostLine.CostOSAmount);
		}

		string logExceptionError(ConsolCostLine consolCostLine, ZGuid parentPK, ZString parentTablePrefix, string exceptionMessage, string errorString, IXmlImportLogger importLogger)
		{
			errorString = NullableExtensions.JoinExcludingEmpty(System.Environment.NewLine, new[] { errorString, getMessage(parentPK, parentTablePrefix, consolCostLine), exceptionMessage });
			importLogger.Log(Enterprise.Integration.LogType.Error, errorString);
			return errorString;
		}

		bool HasAnyClosedShipment(IGenericJobCostPlugIn consol, IXmlImportLogger importLogger, GlbCompany company)
		{
			var closedShipmentJobs = GetClosedShipmentJobs(consol, company);
			if (closedShipmentJobs.Count > 0)
			{
				var closedShipments = consol.CostSupporter.ShipmentsList
					.Where(shipment => closedShipmentJobs.ContainsKey(shipment.PK));

				var message = Res.GetString("677B5ED5-397C-40CE-822C-A10B927E1981"
					, "The Consol Cost Charges cannot be inserted as at least one of the shipments' Job is closed. Closed Shipment(s):[{0}]"
					, string.Join(",", closedShipments.Select(shipment => $"'{shipment.JobNumber}'"))
				);
				importLogger.Log(Enterprise.Integration.LogType.Warning, message);

				return true;
			}

			return false;
		}

		IDictionary<ZGuid, Job> GetClosedShipmentJobs(IGenericJobCostPlugIn consol, GlbCompany company)
		{
			var shipmentPKs = consol.CostSupporter.ShipmentsList.Select(x => x.PK).ToArray();
			var jobQuery = new ZQuery();
			jobQuery.AddToFilter(JobHeaderSchema.JH_ParentID, shipmentPKs);
			jobQuery.AddToFilter(JobHeaderSchema.JH_Status, JobHeaderStatus.Closed.Code);
			jobQuery.AddToFilter(JobHeaderSchema.JH_GC, company.PK);

			return consol.Factory.GetCachedReadOnlyFactory()
				.Load<Job>(jobQuery)
				.ToDictionary(job => job.JH_ParentID, job => job);
		}

		#endregion
	}
}
