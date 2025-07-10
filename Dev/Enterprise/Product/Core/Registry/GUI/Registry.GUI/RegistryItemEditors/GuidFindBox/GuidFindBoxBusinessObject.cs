using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Registry.GUI
{
	public class GuidFindBoxBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string SelectedPK = "SelectedPK";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public GuidFindBoxBusinessObject(FallbackLevel fallback, IRegistryEditorInfo editorInfo)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GuidFindBoxRegistryEditorInfo guidEditorInfo = (GuidFindBoxRegistryEditorInfo)editorInfo;
			RegistryFindBoxCollection findBoxCollection = guidEditorInfo.FindBoxCollection;

			#region AccBankAccount

			if (findBoxCollection == RegistryFindBoxCollection.AccBankAccount)
			{
				if (fallback.BranchPK != Guid.Empty)
				{
					GlbBranch branch = (GlbBranch)factory.Load(typeof(GlbBranch), fallback.BranchPK);
					fChoicesCollection = new AccBankAccountCollection(factory, branch);
				}
				else if (fallback.CompanyPK(true) != Guid.Empty)
				{
					GlbCompany company1 = (GlbCompany)factory.Load(typeof(GlbCompany), fallback.CompanyPK(true));
					fChoicesCollection = new AccBankAccountCollection(factory, company1);
				}
				else
				{
					throw new ArgumentException("AccBankAccount findboxes must be used on either the branch level or the company level.");
				}
				fModuleID = ModuleIDs.AccBankAccount;
			}

			#endregion

			#region AccChargeCode

			else if (findBoxCollection == RegistryFindBoxCollection.AccChargeCode)
			{
				ZQuery filter1 = new ZQuery();
				switch (guidEditorInfo.FindBoxFilter)
				{
					case RegistryFindBoxFilter.DisbursementChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Disbursement);
						break;

					case RegistryFindBoxFilter.CustomDeferredChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Comment);
						break;

					case RegistryFindBoxFilter.MrgDsbOrMjaChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Disbursement);
						filter1.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, ChargeType.Margin);
						filter1.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, ChargeType.ManualJobAccrual);
						break;
					case RegistryFindBoxFilter.FreightChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);
						break;
					case RegistryFindBoxFilter.NonJobRelatedChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Overhead);
						filter1.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, ChargeType.NonAccrual);
						break;
					case RegistryFindBoxFilter.AUCustomsQuarantineChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "DSB");
						break;
					case RegistryFindBoxFilter.RevenueOrNonJobRelatedChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Revenue);
						filter1.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, ChargeType.NonAccrual);
						break;
					case RegistryFindBoxFilter.RevenueChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Revenue);
						break;
					case RegistryFindBoxFilter.GlobalDSBOrMRGChargeCode:
						filter1 = new ZQuery(AccChargeCodeSchema.AC_ChargeType, ChargeType.Disbursement);
						filter1.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, ChargeType.Margin);
						filter1.AddToFilter(AccChargeCodeSchema.AC_GC, null);
						break;
				}

				filter1.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_IsActive, true);
				if (fallback.CompanyPK(false) != Guid.Empty)
				{
					filter1.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_GC, fallback.CompanyPK(false));
				}

				var accChargeCodeCollection = new AccChargeCodeCollection(factory, filter1, fallback.CompanyPK(true));

				if (guidEditorInfo.FindBoxFilter == RegistryFindBoxFilter.AUCustomsQuarantineChargeCode)
				{
					accChargeCodeCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Charge Type", "Property", (ZString)"DSB", false));
				}
				fChoicesCollection = accChargeCodeCollection;

				fModuleID = ModuleIDs.AccChargeCodeForRegistry;
			}

			#endregion

			#region AccGLHeader

			else if (findBoxCollection == RegistryFindBoxCollection.AccGLHeader)
			{
				ZQuery filter1 = new ZQuery();
				switch (guidEditorInfo.FindBoxFilter)
				{
					case RegistryFindBoxFilter.BSHAndControl:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.True);
						filter1.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "BSH");
						break;

					case RegistryFindBoxFilter.BSHAndNonControl:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
						filter1.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "BSH");
						break;

					case RegistryFindBoxFilter.HDR:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_AccountType, "HDR");
						break;

					case RegistryFindBoxFilter.PandL:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_AccountType, "P&L");
						break;

					case RegistryFindBoxFilter.PandLOrBSH:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_AccountType, "P&L");
						filter1.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "BSH");
						break;

					case RegistryFindBoxFilter.PandLOrBSHAndNonControl_AllowDirectPost:
					case RegistryFindBoxFilter.BSHAndNonControl_AllowDirectPost:
						filter1 = GetPandLOrBSHAndNonControlWithDisallowDirectPostFilter(guidEditorInfo, RegistryFindBoxFilter.PandLOrBSHAndNonControl_AllowDirectPost, false);
						break;

					case RegistryFindBoxFilter.PandLOrBSHandNonControl:
						filter1.AddToFilter(AccGLHeaderSchema.AG_AccountType, AccountType.ProfitAndLossAccount);
						filter1.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, AccountType.BalanceSheetAccount);
						filter1.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
						break;

					case RegistryFindBoxFilter.TTL:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_AccountType, "TTL");
						break;

					case RegistryFindBoxFilter.BSH:
						filter1 = new ZQuery(AccGLHeaderSchema.AG_AccountType, "BSH");
						break;
					case RegistryFindBoxFilter.PandLOrBSHAndNonControl_DisallowDirectPost:
					case RegistryFindBoxFilter.BSHAndNonControlDisallowDirectPost:
						filter1 = GetPandLOrBSHAndNonControlWithDisallowDirectPostFilter(guidEditorInfo, RegistryFindBoxFilter.PandLOrBSHAndNonControl_DisallowDirectPost, true);
						break;
				}

				if (!filter1.IsEmpty)
				{
					ZQuery tmpFilter = new ZQuery(filter1);
					tmpFilter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_IsActive, true);
					tmpFilter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_IsGlobal, true);
					filter1 = tmpFilter;
				}
				fChoicesCollection = new AccGLHeaderCollection(factory, filter1);

				fModuleID = ModuleIDs.AccGLHeader;
			}

			#endregion

			#region AccountDescriptors

			else if (findBoxCollection == RegistryFindBoxCollection.AccountDescriptors)
			{
				fModuleID = ModuleIDs.AccGLAccountDescriptor;

				switch (guidEditorInfo.FindBoxFilter)
				{
					case RegistryFindBoxFilter.ChineseSimplifiedAccountDescriptor:
						ZQuery cHSFilter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, "HDR");
						cHSFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Core.SharedConstants.Languages.ChineseSimplified);
						cHSFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
						fChoicesCollection = new AccGLAccountDescriptorCollection(factory, cHSFilter);
						break;

					case RegistryFindBoxFilter.ChineseTraditionalAccountDescriptor:
						ZQuery cHTFilter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, "HDR");
						cHTFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Core.SharedConstants.Languages.ChineseTraditional);
						cHTFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
						fChoicesCollection = new AccGLAccountDescriptorCollection(factory, cHTFilter);
						break;

					case RegistryFindBoxFilter.VietnameseAccountDescriptor:
						ZQuery vTNFilter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, "HDR");
						vTNFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Constants.Languages.Vietnamese);
						vTNFilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
						fChoicesCollection = new AccGLAccountDescriptorCollection(factory, vTNFilter);
						break;

					default:
						fChoicesCollection = new AccGLAccountDescriptorCollection(factory);
						break;
				}
			}

			#endregion

			#region Broker

			else if (findBoxCollection == RegistryFindBoxCollection.Broker)
			{
				fChoicesCollection = new BrokerCollection(factory);
				fModuleID = ModuleIDs.Organisation;
			}

			#endregion

			#region Debtor

			else if (findBoxCollection == RegistryFindBoxCollection.Debtor)
			{
				fChoicesCollection = new DebtorCollection(factory);
				fModuleID = ModuleIDs.Organisation;
			}

			#endregion

			#region FumigationContractors

			else if (findBoxCollection == RegistryFindBoxCollection.FumigationContractors)
			{
				fChoicesCollection = new FumigationContractorCollection(factory);
				fModuleID = ModuleIDs.Organisation;
			}

			#endregion

			#region GlbBranchNotCurrentCompanyRelated

			else if (findBoxCollection == RegistryFindBoxCollection.GlbBranchNotCurrentCompanyRelated)
			{
				fChoicesCollection = new GlbBranchNotCurrentCompanyRelatedCollection(factory);
				fModuleID = ModuleIDs.GlbBranchNotCurrentCompanyRelated;
			}

			#endregion

			#region GlbBranch

			else if (findBoxCollection == RegistryFindBoxCollection.GlbBranch)
			{
				var companyPK = fallback.CompanyPK(false);
				var filter = companyPK == Guid.Empty ? new ZQuery() : new ZQuery(GlbBranchSchema.GB_GC, companyPK);
				var collection = new GlbBranchCollection(factory, filter);
				var filterDefault = new FilterBusinessObjectDefault("Company", "Property", new ZGuid(companyPK), false);
				collection.FilterBusinessObjectDefaults.Add(filterDefault);
				fChoicesCollection = collection;
				fModuleID = ModuleIDs.GlbBranch;
			}

			#endregion

			#region GlbCompany

			else if (findBoxCollection == RegistryFindBoxCollection.GlbCompany)
			{
				fChoicesCollection = new GlbCompanyCollection(factory);
				fModuleID = ModuleIDs.GlbCompany;
			}

			#endregion

			#region GlbDepartment

			else if (findBoxCollection == RegistryFindBoxCollection.GlbDepartment)
			{
				switch (guidEditorInfo.FindBoxFilter)
				{
					case (RegistryFindBoxFilter.NonMiscDepartment):
						ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbDepartment));

						string sqlText = string.Format(@"
								{0} = 0
								AND
								(
									{1} IS NULL OR
									{1} NOT IN
									(
										SELECT {2}
										FROM {3}
										WHERE {0} = 1
									)
								)",
							GlbDepartmentSchema.Constants.GE_Misc,    // 0
							GlbDepartmentSchema.Constants.GE_GE,      // 1
							GlbDepartmentSchema.Constants.PK,         // 2
							GlbDepartmentSchema.Constants.TableName); // 3

						query.AddFilterAndZSQLParameterCollection(sqlText, new ZSqlParameterCollection());
						fChoicesCollection = new GlbDepartmentCollection(factory, query);
						break;

					default:
						fChoicesCollection = new GlbDepartmentCollection(factory);
						break;
				}

				fModuleID = ModuleIDs.GlbDepartment;
			}

			#endregion

			#region GlbGroup

			else if (findBoxCollection == RegistryFindBoxCollection.GlbGroup)
			{
				fChoicesCollection = new GlbGroupCollection(factory);
				fModuleID = ModuleIDs.GlbGroup;
			}

			#endregion

			#region GlbStaff

			else if (findBoxCollection == RegistryFindBoxCollection.GlbStaff)
			{
				fChoicesCollection = new GlbStaffCollection(factory);
				fModuleID = ModuleIDs.GlbStaff;
			}

			#endregion

			#region OrgCreditorGroup

			else if (findBoxCollection == RegistryFindBoxCollection.OrgCreditorGroup)
			{
				fChoicesCollection = new OrgCreditorGroupCollection(factory);
				fModuleID = ModuleIDs.OrgCreditorGroup;
			}

			#endregion

			#region OrgDebtorGroup

			else if (findBoxCollection == RegistryFindBoxCollection.OrgDebtorGroup)
			{
				fChoicesCollection = new OrgDebtorGroupCollection(factory);
				fModuleID = ModuleIDs.OrgDebtorGroup;
			}

			#endregion

			#region OrgHeader

			else if (findBoxCollection == RegistryFindBoxCollection.OrgHeader)
			{
				fChoicesCollection = new OrgHeaderCollection(factory);
				fModuleID = ModuleIDs.Organisation;
			}

			#endregion

			#region RefCommodityCode

			else if (findBoxCollection == RegistryFindBoxCollection.RefCommodityCode)
			{
				fChoicesCollection = new RefCommodityCodeCollection(factory);
				fModuleID = ModuleIDs.RefCommodityCode;
			}

			#endregion

			#region RefCurrency

			else if (findBoxCollection == RegistryFindBoxCollection.RefCurrency)
			{
				fModuleID = ModuleIDs.RefCurrency;
				fChoicesCollection = new RefCurrencyCollection(factory);
			}

			#endregion

			#region RefServiceLevel

			else if (findBoxCollection == RegistryFindBoxCollection.RefServiceLevel)
			{
				fChoicesCollection = new RefServiceLevelCollection(factory);
				fModuleID = ModuleIDs.ServiceLevel;
			}

			#endregion

			#region ShippingProvider

			else if (findBoxCollection == RegistryFindBoxCollection.ShippingProvider)
			{
				fChoicesCollection = new ShippingProviderCollection(factory);
				fModuleID = ModuleIDs.Organisation;
			}

			#endregion

			#region StmPrintQueue

			else if (findBoxCollection == RegistryFindBoxCollection.StmPrintQueue)
			{
				ZQuery filter1 = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);
				fChoicesCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { factory, filter1 });
				fModuleID = ModuleIDs.PrintQueue;
			}

			#endregion

			#region StmMenuItem

			else if (findBoxCollection == RegistryFindBoxCollection.ARInvoiceMenuItem)
			{
				ZQuery filter = new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.ARInvoice));
				filter.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
				StmMenuItemCollection collection = new StmMenuItemCollection(factory, filter);
				FilterBusinessObjectDefault filterDefault = new FilterBusinessObjectDefault("Business Context", "Property", new ZString(nameof(BusinessContext.ARInvoice)));
				collection.FilterBusinessObjectDefaults.Add(filterDefault);
				fChoicesCollection = collection;
				fModuleID = ModuleIDs.StmMenuItem;
			}

			#endregion

			#region TagMagnitude

			else if (findBoxCollection == RegistryFindBoxCollection.TagMagnitude)
			{
				fChoicesCollection = ObjectFactory.Get<ITagMagnitudeCollection>(nameof(ITagMagnitudeCollection), factory);
				fModuleID = ModuleIDs.BMTagMagnitude;
			}

			#endregion

			#region PackingDocument

			else if (findBoxCollection == RegistryFindBoxCollection.PackingDocument)
			{
				var filter = new ZDBOnlyQuery(typeof(StmMenuItem));
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Package));
				filter.AddToFilter(StmMenuItemSchema.SU_PreventAutoDelivery, false);

				var pivotQuery = new ZDBOnlySubQuery(typeof(StmMenuTemplatePivot), StmMenuTemplatePivotSchema.SI_SU);
				pivotQuery.AddFilterAndZSQLParameterCollection(string.Format(@"
						SI_PK IN
						(
							SELECT S3_SI
							FROM dbo.StmMenuDocumentConfig
							WHERE S3_OverrideDataContext NOT IN ({0})
						)", string.Join(",", RegistryConstants.Packing.NonAutoPrintingDataContexts.Select(d => string.Format("'{0}'", d)))), new ZSqlParameterCollection());
				filter.AddSubQuery(pivotQuery, JoinCondition.And);

				var collection = new PackingStmMenuItemCollection(factory, filter);
				var filterDefault = new FilterBusinessObjectDefault("Business Context", "Property", new ZString(nameof(BusinessContext.Package)), isRemovable: false);
				collection.FilterBusinessObjectDefaults.Add(filterDefault);
				fChoicesCollection = collection;
				fModuleID = ModuleIDs.StmMenuItem;
			}

			#endregion

			#region WhsWarehouse

			else if (findBoxCollection == RegistryFindBoxCollection.WhsWarehouse)
			{
				var collectionType = ObjectFactory.GetType<IWhsWarehouseCollection>();
				var filter = new ZQuery(WhsWarehouseSchema.WW_IsActive, true);
				fChoicesCollection = (IBusinessObjectCollection)Activator.CreateInstance(collectionType, new object[] { factory, filter });
				fModuleID = ModuleIDs.WhsConfigWarehouse;
			}

			#endregion

			#region OrgContact

			else if (findBoxCollection == RegistryFindBoxCollection.OrgContact)
			{
				fChoicesCollection = new OrgContactCollection(factory, new ZQuery(OrgContactSchema.OC_IsActive, ZBool.True));
				fModuleID = ModuleIDs.OrgContacts;
			}

			#endregion

			#region ZACustomsOffice

			else if (findBoxCollection == RegistryFindBoxCollection.ZACustomsOffice)
			{
				fModuleID = ModuleIDs.Customs.Universal.ZZRefCusCodeList;

				Type collectionType = ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IZARefCusCodeListCollection>();
				fChoicesCollection = (IBusinessObjectCollection)Activator.CreateInstance(collectionType, new object[] { factory, (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today });
			}

			#endregion

			#region AccTaxRate

			else if (findBoxCollection == RegistryFindBoxCollection.AccTaxRate)
			{
				ZQuery filter1 = new ZQuery();
				switch (guidEditorInfo.FindBoxFilter)
				{
					case RegistryFindBoxFilter.AccTaxRateTypeRated:
						filter1 = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
						break;

					case RegistryFindBoxFilter.AccTaxRateTypeReverseRated:
						filter1 = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.ReverseRated);
						break;

					case RegistryFindBoxFilter.AccTaxRateTypeCapitalRated:
						filter1 = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.CapitalRated);
						break;

					case RegistryFindBoxFilter.AccTaxRateTypeExempt:
						filter1 = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Exempt);
						break;

					case RegistryFindBoxFilter.AccTaxRateTypeNotReportable:
						filter1 = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.NotReportable);
						break;
				}

				if (!filter1.IsEmpty)
				{
					ZQuery tmpFilter = new ZQuery(filter1);
					tmpFilter.AddToFilter(JoinCondition.And, AccTaxRateSchema.AT_IsActive, true);
					filter1 = tmpFilter;
				}

				GlbCompany company = factory.Load<GlbCompany>(fallback.CompanyPK(false));

				if (guidEditorInfo.FindBoxFilter == RegistryFindBoxFilter.VATTaxSystem)
				{
					fChoicesCollection = new VATAccTaxRateCollectionForRegistry(factory, filter1, company ?? GlbCompany.CurrentCompany);
				}
				else
				{
					fChoicesCollection = new AccTaxRateCollectionForRegistry(factory, filter1, company ?? GlbCompany.CurrentCompany);
				}

				fModuleID = ModuleIDs.AccTaxRateForRegistry;
			}

			#endregion

			#region AccInvMsg

			else if (findBoxCollection == RegistryFindBoxCollection.AccInvMsg)
			{
				var company = factory.Load<GlbCompany>(fallback.CompanyPK(false));
				fChoicesCollection = new AccInvMsgCollection(factory, company?.GC_RN_NKCountryCode ?? ZString.Empty);
				fModuleID = ModuleIDs.AccInvMsg;
			}

			#endregion

			#region RefDocType

			else if (findBoxCollection == RegistryFindBoxCollection.RefDocType)
			{
				if (guidEditorInfo.FindBoxFilter == RegistryFindBoxFilter.RefDocTypeForCommunicationParsedEmail)
				{
					var filter = new ZQuery();
					filter.AddToFilter(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, "All");
					filter.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, "CSR");
					fChoicesCollection = new RefDocTypeCollection(factory, filter);
				}
				else
				{
					fChoicesCollection = new RefDocTypeCollection(factory);
				}
				fModuleID = ModuleIDs.RefDocType;
			}

			#endregion

			#region AccAlternateChart

			else if (findBoxCollection == RegistryFindBoxCollection.AccAlternateChart)
			{
				var query = new ZQuery(AccAlternateChartSchema.AAC_GC_Company, fallback.CompanyPK(false));
				query.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, null);
				fChoicesCollection = new AccAlternateChartCollection(factory, query, fallback.CompanyPK(false));
				fModuleID = ModuleIDs.AlternateChartofAccounts;
			}

			#endregion

			else
			{
				throw new ArgumentException("The FindBoxCollection is not set. RegistryItem that uses ZGuidFindBox must call SetFindBoxCollection in RawDataRegistry.");
			}
		}

		#region PackingStmMenuItemCollection
#if DEBUG
		public
#endif
		class PackingStmMenuItemCollection : StmMenuItemCollection
		{
			internal PackingStmMenuItemCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
				: base(factory, additionalFilter)
			{
			}

			protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(notifications, selectedBusinessObject);

				var packingMenuItem = (StmMenuItem)selectedBusinessObject;
				if (packingMenuItem.SU_PreventAutoDelivery)
				{
					notifications.Add(Res.GetString("2286a40a-6262-43b2-914f-c0ee8387fea7", "This Document cannot be chosen because 'Prevent Auto Delivery' is checked."));
				}
				else
				{
					var pivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, selectedBusinessObject.PK);
					pivotQuery.AddFilterAndZSQLParameterCollection(string.Format(@"
					SI_PK IN
					(
						SELECT S3_SI
						FROM dbo.StmMenuDocumentConfig
						WHERE S3_OverrideDataContext NOT IN ({0})
					)", string.Join(",", RegistryConstants.Packing.NonAutoPrintingDataContexts.Select(d => string.Format("'{0}'", d)))), new ZSqlParameterCollection());

					if (!Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(StmMenuTemplatePivot)), pivotQuery))
					{
						notifications.Add(Res.GetString("fb4d4634-5519-4936-8e82-cf491131dfbc", "This Document cannot be chosen because it prints multiple labels."));
					}
				}
			}
		}

		#endregion

		#region SelectedPK

		public ZGuid SelectedPK
		{
			get { return fSelectedPK; }
			set
			{
				if (SelectedPK != value)
				{
					fSelectedPK = value;
					HasChanges = true;
				}

				if (!IsValidationSuspended)
				{
					ValidateSelectedPK();
				}

				SelectedPKInfo.RefreshBinding();
			}
		}

		public virtual void ValidateSelectedPK()
		{
			SelectedPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(SelectedPKInfo);
		}

		public ZPropertyInfo SelectedPKInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedPK); }
		}

		ZGuid fSelectedPK;

		#endregion

		ZQuery GetPandLOrBSHAndNonControlWithDisallowDirectPostFilter(GuidFindBoxRegistryEditorInfo guidEditorInfo, RegistryFindBoxFilter findBoxFilter, bool disallowDirectPost)
		{
			ZQuery bshAccountFilter = new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			bshAccountFilter.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "BSH");

			if (guidEditorInfo.FindBoxFilter == findBoxFilter)
			{
				ZQuery pl_bsh_Filter = new ZQuery(AccGLHeaderSchema.AG_AccountType, "P&L");
				bshAccountFilter.AddToFilter(pl_bsh_Filter, JoinCondition.Or);
			}

			ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_DisallowDirectPosting, disallowDirectPost);
			filter.AddToFilter(bshAccountFilter);
			return filter;
		}

		public IBusinessObjectCollection ChoicesCollection
		{
			get { return fChoicesCollection; }
		}

		public ModuleIdentifier ModuleID
		{
			get { return fModuleID; }
		}

		readonly IBusinessObjectCollection fChoicesCollection;
		readonly ModuleIdentifier fModuleID;
	}
}
