using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	class GLHeadersAndChargeCodesDataAdapter : BaseAccountingDataAdapter<BusinessObjectThatDoesntSave, Xsd.GLHeadersAndChargeCodes>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "GLHeadersAndChargeCodes"; }
		}

		public override string RootElementName
		{
			get { return "SingleGLHeadersAndChargeCodesElement"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeadersAndChargeCodesSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeadersAndChargeCodesCollectionSchema; }
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSave bizObj, Xsd.GLHeadersAndChargeCodes value, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting GLAccounts is currently not supported");
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSave bizObj, Xsd.GLHeadersAndChargeCodes value, IValueObjectImportContext context)
		{
			ImportGLHeaders(value.SingleGLHeadersAndChargeCodesElement.GLHeaders, bizObj, new GLHeaderDataAdapter(), context, out Dictionary<ZString, AccGLHeader> dictGLHeaders);
			ImportChargeCodes(value.SingleGLHeadersAndChargeCodesElement.ChargeCodes, bizObj, new ChargeCodeDataAdapter(), context);
			ImportGLHeaderMultiLanguageMappings(value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings, bizObj, new GLAccountDescriptorDataAdapter(), context, dictGLHeaders);
			ImportGLHeaderMultiLanguageReportSetups(value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups, bizObj, new GLMappingReportSetupDataAdapter(), context);
		}

		void ImportGLHeaders(GLHeadersGLHeaderCollection glHeaders, BusinessObjectThatDoesntSave bizObj, GLHeaderDataAdapter glHeaderDataAdapter, IValueObjectImportContext context, out Dictionary<ZString, AccGLHeader> dictGLHeaders)
		{
			dictGLHeaders = bizObj.Factory.Load<AccGLHeader>(new ZQuery()).ToDictionary(x => x.AG_AccountNum, x => x);

			foreach (GLHeadersGLHeader header in glHeaders)
			{
				if (dictGLHeaders.ContainsKey(header.AccNumber))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("2293d322-395e-4470-8a46-79cd1bd72da8", "The Account Number '{0}' is already in use by another GL Account.", header.AccNumber)));
				}
				else
				{
					var newGLHeader = bizObj.Factory.New<AccGLHeader>();
					glHeaderDataAdapter.ImportFromValueObject(newGLHeader, header, context);
					dictGLHeaders.Add(header.AccNumber, newGLHeader);
				}
			}

			foreach (GLHeadersGLHeader header in glHeaders)
			{
				dictGLHeaders.TryGetValue(header.AccNumber, out AccGLHeader glHeader);
				if (glHeader != null && !glHeader.Notifications.HasErrors())
				{
					glHeaderDataAdapter.SetReferencesToGLAccounts(glHeader, header, context);
				}
			}
		}

		void ImportChargeCodes(ChargeCodesChargeCodeCollection chargeCodes, BusinessObjectThatDoesntSave bizObj, ChargeCodeDataAdapter chargeCodeDataAdapter, IValueObjectImportContext context)
		{
			foreach (ChargeCodesChargeCode chargeCode in chargeCodes)
			{
				var newChargeCode = bizObj.Factory.New<AccChargeCode>();
				chargeCodeDataAdapter.ImportFromValueObject(newChargeCode, chargeCode, context);

				var uniqueCheck = new ZQuery(AccChargeCodeSchema.AC_Code, newChargeCode.AC_Code);
				uniqueCheck.AddToFilter(AccChargeCodeSchema.AC_GC, newChargeCode.AC_GC);
				uniqueCheck.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, newChargeCode.PK);
				uniqueCheck.FetchOnlyFromLocalCache = true;
				if (bizObj.Factory.LoadTop1<AccChargeCode>(uniqueCheck) != null)
				{
					context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("8f050ea0-286b-4372-89c7-a36245a42054", "The Code '{0}' is already in use by another Charge Code in this file.", newChargeCode.AC_Code)));
				}
			}
		}

		void ImportGLHeaderMultiLanguageMappings(GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection glHeaderMultiLanguageMappings, BusinessObjectThatDoesntSave bizObj, GLAccountDescriptorDataAdapter glAccountDescriptorDataAdapter, IValueObjectImportContext context, Dictionary<ZString, AccGLHeader> dictGLHeaders)
		{
			foreach (GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping glHeaderMapping in glHeaderMultiLanguageMappings)
			{
				var uniqueCheck = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, glHeaderMapping.LocalAccountNumber);
				uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, glHeaderMapping.Language);
				uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, glHeaderMapping.CountryOfCompliance);
				uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount);

				if (bizObj.Factory.LoadTop1<AccGLAccountDescriptor>(uniqueCheck) != null)
				{
					context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4D4B247F-849C-43E8-BEAA-D4D966BE5C46", "The local account number '{0}' for the language '{1}' is already in mapping by another GL Account in this file.", glHeaderMapping.LocalAccountNumber, glHeaderMapping.Language)));
				}
				else if (DoesLocalAccountReferToSameParentAccount(bizObj.Factory, glHeaderMapping, dictGLHeaders, out ZString errorLocalAccountNumber))
				{
					context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("08EF8DCC-EEE4-4E7A-91F7-FF00B2BBFA88", @"The Parent Account '{0}' for the language '{1}' is already in mapping by another Local Account '{2}'.", glHeaderMapping.ParentAccount, glHeaderMapping.Language, errorLocalAccountNumber)));
				}
				else
				{
					var newAccGLAccountDescriptor = bizObj.Factory.New<AccGLAccountDescriptor>();
					if (newAccGLAccountDescriptor.Factory.IsValidationSuspended)
					{
						newAccGLAccountDescriptor.Factory.ResumeValidation();
					}

					glAccountDescriptorDataAdapter.ImportFromValueObject(newAccGLAccountDescriptor, glHeaderMapping, context);
				}
			}

			foreach (GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping glHeaderMapping in glHeaderMultiLanguageMappings)
			{
				var glAccountQuery = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, glHeaderMapping.LocalAccountNumber);
				glAccountQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, glHeaderMapping.Language);
				glAccountQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, glHeaderMapping.CountryOfCompliance);
				var glAccountDescriptor = bizObj.Factory.LoadTop1<AccGLAccountDescriptor>(glAccountQuery);
				if (glAccountDescriptor != null)
				{
					glAccountDescriptorDataAdapter.SetReferencesToGLAccounts(glAccountDescriptor, glHeaderMapping, context);
				}
			}
		}

		void ImportGLHeaderMultiLanguageReportSetups(GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetupCollection glHeaderMultiLanguageReportSetups, BusinessObjectThatDoesntSave bizObj, GLMappingReportSetupDataAdapter glMappingReportSetupDataAdapter, IValueObjectImportContext context)
		{
			foreach (GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup glReportSetup in glHeaderMultiLanguageReportSetups)
			{
				var rfilter = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, glReportSetup.LocalAccountNumber);
				rfilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, glReportSetup.Language);
				rfilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, glReportSetup.Country);
				rfilter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount);
				var accGLAccountDescriptors = bizObj.Factory.Load<AccGLAccountDescriptor>(rfilter);
				if (accGLAccountDescriptors.Length > 0)
				{
					var uniqueCheck = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, glReportSetup.LocalAccountNumber);
					uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, glReportSetup.Language);
					uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, glReportSetup.Country);
					uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, glReportSetup.ReportType);
					uniqueCheck.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportCategory, glReportSetup.ReportCategory);
					if (bizObj.Factory.LoadTop1<AccGLAccountDescriptor>(uniqueCheck) != null)
					{
						context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("750C19FE-3F83-4BEC-B273-263894A031DD", "The local account number '{0}' for the language '{1}' is already in mapping to Report Category '{2}' in Report Type '{3}'.", glReportSetup.LocalAccountNumber, glReportSetup.Language, glReportSetup.ReportCategory, glReportSetup.ReportType)));
					}
					else
					{
						var accGLAccountDescriptor = (AccGLAccountDescriptor)accGLAccountDescriptors[0].Clone();
						var newGLDescriptorPivot = bizObj.Factory.New<GLDescriptorPivot>();
						if (newGLDescriptorPivot.Factory.IsValidationSuspended)
						{
							newGLDescriptorPivot.Factory.ResumeValidation();
						}

						if (!newGLDescriptorPivot.ReportType_List.CodesAsString.Contains(glReportSetup.ReportType, StringComparison.OrdinalIgnoreCase))
						{
							context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("aaaf3fbb-6d83-7101-8056-96e7dc698e49",
													"The report type '{0}' is missing in the current language '{1}'.",
													glReportSetup.ReportType, glReportSetup.Language)));
						}
						else
						{
							if (newGLDescriptorPivot.ReportType_List.GetReportTypeCategoriesFromCode(glReportSetup.ReportType)[glReportSetup.ReportCategory] == null)
							{
								context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("ccbf3fbb-5d83-4101-9056-46e7dc698849",
																									"The category '{0}' is missing in the report type '{1}' for the current language '{2}'.",
																									glReportSetup.ReportCategory,
																									glReportSetup.ReportType,
																									glReportSetup.Language)));
							}
							else
							{
								newGLDescriptorPivot.YJ_AG = accGLAccountDescriptors[0].ParentGLHeaderPK;
								newGLDescriptorPivot.YJ_AJ = accGLAccountDescriptor.PK;
								glMappingReportSetupDataAdapter.ImportFromValueObject(newGLDescriptorPivot, glReportSetup, context);
							}
						}
					}
				}
				else
				{
					context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("1233d322-395e-2270-8a46-79cd1bd77899", "The local account number '{0}' for the language '{1}' cannot be found.", glReportSetup.LocalAccountNumber, glReportSetup.Language)));
				}
			}
		}

		bool DoesLocalAccountReferToSameParentAccount(BusinessObjectFactory factory, GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping glHeaderMapping, Dictionary<ZString, AccGLHeader> dictGLHeaders, out ZString errorLocalAccountNumber)
		{
			errorLocalAccountNumber = ZString.Empty;

			if (dictGLHeaders.TryGetValue(glHeaderMapping.ParentAccount, out AccGLHeader parentAccount))
			{
				var pivotQuery = new ZQuery(AccGLDescriptorPivotSchema.YJ_AG, parentAccount.PK);
				var pivots = factory.Load<AccGLDescriptorPivot>(pivotQuery);

				foreach (var pk in pivots.Select(p => p.YJ_AJ))
				{
					var descriptor = factory.Load<AccGLAccountDescriptor>(pk);
					if (descriptor.AJ_Language == glHeaderMapping.Language &&
						descriptor.AJ_RN_NKCountryOfCompliance == glHeaderMapping.CountryOfCompliance &&
						descriptor.AJ_ReportCategory == glHeaderMapping.ReportCategory &&
						descriptor.AJ_ReportType == AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount)
					{
						errorLocalAccountNumber = descriptor.AJ_LocalAccountNumber;
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Test metheds/property wrapper
		public void ImportFromValueObjectCore_ForTestOnly(BusinessObjectThatDoesntSave bizObj, Xsd.GLHeadersAndChargeCodes value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(bizObj, value, context);
		}

		public void NotifyBizObjCreatedOrUpdated_ForTestOnly(INotifications notifications, BusinessObject bizObj)
		{
			NotifyBizObjCreatedOrUpdated(notifications, bizObj);
		}
		#endregion
	}
}
