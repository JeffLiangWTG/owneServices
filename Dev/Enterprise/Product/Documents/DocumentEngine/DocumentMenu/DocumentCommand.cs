using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Macros;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	[System.Diagnostics.DebuggerDisplay("{SU_MenuName}")]
	public class DocumentCommand : StmMenuItemBase, IDocumentCommand, IRootTypeProvider, IZFilterModuleThreadSafe
	{
		public DocumentCommand(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new DocumentCommand New(BusinessObjectFactory factory)
		{
			return (DocumentCommand)factory.New(typeof(DocumentCommand));
		}

		public static DocumentCommand GetDocumentCommand(BusinessObjectFactory factory, IDocumentSupportable documentSupportable, ZString menuName, bool onlySystemDefined = false, bool isDocBuilder = false)
		{
			DocumentCommand result = null;
			if ((factory != null) && (documentSupportable != null) && !menuName.IsEmpty)
			{
				DocumentSupporter documentSupporter = documentSupportable.DocumentSupporter;
				if (documentSupporter != null)
				{
					var filter = new DocumentZQuery(documentSupportable.DocumentSupporter.BusinessContext, menuName, onlySystemDefined);
					var commands = factory.Load<DocumentCommand>(filter);
					if (commands.Length > 1)
					{
						if (onlySystemDefined)
						{
							commands = commands.Where(x => x.SU_IsSystemDefined).ToArray();
						}

						result = isDocBuilder ?
							commands.FirstOrDefault(x => x.Documents.Cast<StmMenuTemplatePivotBase>().Any(p => p.SO_Name.StartsWith(Core.Constants.SectionRepositoryTemplateNames.System, StringComparison.OrdinalIgnoreCase))) :
							commands.FirstOrDefault(x => x.Documents.Cast<StmMenuTemplatePivotBase>().Any(p => !p.SO_Name.StartsWith(Core.Constants.SectionRepositoryTemplateNames.System, StringComparison.OrdinalIgnoreCase)));
					}
					else if (commands.Length == 1)
					{
						result = commands[0];
					}
				}
			}
			return result;
		}

		public static DocumentCommand GetDocumentCommand(BusinessObjectFactory factory, IDocumentSupportable documentSupportable, ZString menuName, ZString menuPath, ZString filterList)
		{
			DocumentCommand result = null;
			if ((factory != null) && (documentSupportable != null) && !menuName.IsEmpty)
			{
				DocumentSupporter documentSupporter = documentSupportable.DocumentSupporter;
				if (documentSupporter != null)
				{
					DocumentZQuery filter = new DocumentZQuery(documentSupportable.DocumentSupporter.BusinessContext, menuName);
					filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath);
					filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
					filter.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, ZBool.False);
					if (!filterList.IsEmpty)
					{
						filter.AddToFilter(StmMenuItemSchema.SU_FilterList, filterList);
					}

					result = factory.LoadTop1<DocumentCommand>(filter);
				}
			}
			return result;
		}

		public ControllerID ControllerId { get; set; }

		protected bool SU_IsModifiable_ReadOnly
		{
			get { return !GlbStaff.CurrentUser.GS_IsController && GetPropertyInfosReadOnly(null); }
		}

		public override ZBool SU_IsDocPack
		{
			get { return base.SU_IsDocPack; }
			set
			{
				base.SU_IsDocPack = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSU_EmailSubjectLine();
					Validation.ValidateSU_MenuDataContext();
				}
			}
		}

		public override ZString SU_MenuDataContext
		{
			get
			{
				return base.SU_MenuDataContext;
			}
			set
			{
				base.SU_MenuDataContext = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSU_EmailSubjectLine();
				}
			}
		}

		protected bool SU_MenuDataContext_ReadOnly
		{
			get { return EmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup || GetPropertyInfosReadOnly(null); }
		}

		public override ZString SU_EmailSubjectLine
		{
			get
			{
				return base.SU_EmailSubjectLine;
			}
			set
			{
				base.SU_EmailSubjectLine = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSU_MenuDataContext();
				}
			}
		}

		protected bool SU_EmailSubjectLine_ReadOnly
		{
			get
			{
				var readOnly = EmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup;
				if (!GlbStaff.CurrentUser.GS_IsController)
				{
					readOnly = readOnly || GetPropertyInfosReadOnly(null);
				}
				return readOnly;
			}
		}

		internal bool EmailSubjectAndMenuDataContextReadOnlyBasedOnDocumentSetup
		{
			get { return !SU_IsDocPack && Documents.Count == 0 && ChildMenus.Count > 0; }
		}

		protected bool SU_DeliveryRestrictionType_ReadOnly
		{
			get { return !Env.Security.ReceivablesOnCreditHoldController.IsAllowed && GetPropertyInfosReadOnly(null); }
		}

		protected override bool SU_DeliveryRestrictionMacro_ReadOnly
		{
			get { return SU_DeliveryRestrictionType_ReadOnly || base.SU_DeliveryRestrictionMacro_ReadOnly; }
		}

		protected override bool SU_DeliveryRestrictionDescription_ReadOnly
		{
			get { return SU_DeliveryRestrictionType_ReadOnly || base.SU_DeliveryRestrictionDescription_ReadOnly; }
		}

		public override ZString SU_DeliveryRestrictionType
		{
			get => base.SU_DeliveryRestrictionType;
			set
			{
				var isDeliveryRestrictionsReadOnly = false;
				if (base.SU_DeliveryRestrictionType != value && value == nameof(DeliveryRestrictionType.CNH) && DeactivateDeliveryRestrictionsCancelEventHandler != null)
				{
					var args = new CancelEventArgs();
					DeactivateDeliveryRestrictionsCancelEventHandler(this, args);
					if (args.Cancel)
					{
						return;
					}

					isDeliveryRestrictionsReadOnly = true;
					var activatedRestrictions = DeliveryRestrictions.OfType<DocumentCommandDeliveryRestriction>().Where(d => d.SDR_IsActive).ToArray();
					if (activatedRestrictions.Any())
					{
						activatedRestrictions.ForEach(x => x.SDR_IsActive = false);
					}
				}
				DeliveryRestrictions.SetReadOnlyIncludingChildren(isDeliveryRestrictionsReadOnly);
				base.SU_DeliveryRestrictionType = value;
			}
		}

		[ChildEditable(true)]
		public DocumentCommandDeliveryRestrictionCollection DeliveryRestrictions
		{
			get
			{
				if (deliveryRestrictions == null)
				{
					deliveryRestrictions = new DocumentCommandDeliveryRestrictionCollection(this);
					RegisterEditableChildObject(deliveryRestrictions);
					SetDeliveryRestrictionsReadOnlyIfNeeded();
				}
				return deliveryRestrictions;
			}
		}
		DocumentCommandDeliveryRestrictionCollection deliveryRestrictions;

		void SetDeliveryRestrictionsReadOnlyIfNeeded()
		{
			if (!IsDeleted && SU_DeliveryRestrictionType == nameof(DeliveryRestrictionType.CNH))
			{
				DeliveryRestrictions.SetReadOnlyIncludingChildren(true);
			}
		}

		public event CancelEventHandler DeactivateDeliveryRestrictionsCancelEventHandler;

		#region GetDeliveryRestrictionErrorMessage

		string IDocumentCommand.GetDeliveryRestrictionErrorMessage(BusinessObject parentBusinessObject, BusinessObject deliveryBusinessObject)
		{
			var errorMessage = new StringBuilder();
			var isAllowed = true;
			var processor = ObjectFactory.Get<ITextMacroProcessor>();

			var creditControlManager = ObjectFactory.Get<IDocumentDeliveryCreditControlManager>();
			var restrictions = DeliveryRestrictions.Where(x => x.SDR_IsActive).ToArray();

			if (DeliveryRestrictionRelatedToComplianceRisk(SU_DeliveryRestrictionType, SU_DeliveryRestrictionMacro) || HasUserDefinedRestrictionsRelatedToComplianceRisk(parentBusinessObject))
			{
				if (creditControlManager.ShouldStopDelivery(deliveryBusinessObject))
				{
					return MultilingualString.Join(" ", ResString.GetMultilingualString("2CCFC2BB-42DF-4AD4-8C5E-0C4BAB7A51AA", "Document Delivery"), SecurityLogin.CancelledText);
				}
			}

			if (SU_DeliveryRestrictionType == nameof(DeliveryRestrictionType.UDF) && !IsUserDefinedConditionMet(processor, parentBusinessObject, SU_DeliveryRestrictionMacro))
			{
				isAllowed = false;
				AppendErrorMessageIfNeeded(errorMessage, GetReplacedValue(processor, parentBusinessObject, SU_DeliveryRestrictionDescription));
			}

			if (restrictions.Length > 0)
			{
				foreach (var restriction in restrictions)
				{
					if (parentBusinessObject is IOriginDestinationForDocumentDeliveryRestriction documentDeliveryRestriction &&
						(!IsCountryCodeMatched(restriction.SDR_RN_NKOriginCountryCode, documentDeliveryRestriction.OriginCountryCode) ||
						!IsCountryCodeMatched(restriction.SDR_RN_NKDestinationCountryCode, documentDeliveryRestriction.DestinationCountryCode)) ||
						!IsUserDefinedMatched(restriction.SDR_DeliveryRestrictionType, processor, restriction.SDR_DeliveryRestrictionMacro, parentBusinessObject))
					{
						isAllowed = false;
						AppendErrorMessageIfNeeded(errorMessage, GetReplacedValue(processor, parentBusinessObject, restriction.SDR_DeliveryRestrictionDescription));
					}
				}
			}

			return isAllowed ? "" : Res.GetString("91189438-7374-4962-8ba7-ddb7c07a840a", "User defined delivery restriction condition is not met. {0}{1}", System.Environment.NewLine, errorMessage.ToString());

			bool HasUserDefinedRestrictionsRelatedToComplianceRisk(BusinessObject parentBusinessObject)
			{
				var documentDeliveryRestriction = parentBusinessObject as IOriginDestinationForDocumentDeliveryRestriction;
				return restrictions.Any(x => (documentDeliveryRestriction != null
					&& IsCountryCodeMatched(x.SDR_RN_NKOriginCountryCode, documentDeliveryRestriction.OriginCountryCode)
					&& IsCountryCodeMatched(x.SDR_RN_NKDestinationCountryCode, documentDeliveryRestriction.DestinationCountryCode)
					|| documentDeliveryRestriction == null)
					&& DeliveryRestrictionRelatedToComplianceRisk(x.SDR_DeliveryRestrictionType, x.SDR_DeliveryRestrictionMacro));
			}

			bool DeliveryRestrictionRelatedToComplianceRisk(ZString restrictionType, ZString macro)
			{
				return restrictionType == nameof(DeliveryRestrictionType.UDF)
					&& (macro.Contains(JobRiskMacro, StringComparison.OrdinalIgnoreCase) || macro.Contains(CommodityRiskMacro, StringComparison.OrdinalIgnoreCase));
			}
		}

		const string JobRiskMacro = $"<{ComplianceRiskStatusSchema.Constants.TableName}.{nameof(ComplianceRiskStatusObject.JobRisk)}>";
		const string CommodityRiskMacro = $"<{ComplianceRiskStatusSchema.Constants.TableName}.{nameof(ComplianceRiskStatusObject.CommodityRisk)}>";

		void AppendErrorMessageIfNeeded(StringBuilder errorMessage, string errorDescription)
		{
			if (!string.IsNullOrEmpty(errorDescription))
			{
				errorMessage.AppendLine(errorDescription);
			}
		}

		string GetReplacedValue(ITextMacroProcessor processor, BusinessObject businessObject, ZString macro)
		{
			try
			{
				return processor.Replace(
					macro.Trim(),
					new[] { businessObject },
					false,
					this
				);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				return macro;
			}
		}

		bool IsCountryCodeMatched(ZString countryCode, ZString documentDeliveryCountryCode)
		{
			return countryCode.IsEmpty || countryCode == documentDeliveryCountryCode;
		}

		bool IsUserDefinedMatched(string restrictionType, ITextMacroProcessor processor, string restrictionMacro, BusinessObject parentBusinessObject)
		{
			return restrictionType != nameof(DeliveryRestrictionType.UDF) || (restrictionType == nameof(DeliveryRestrictionType.UDF) && IsUserDefinedConditionMet(processor, parentBusinessObject, restrictionMacro));
		}

		bool IsUserDefinedConditionMet(ITextMacroProcessor processor, BusinessObject businessObject, ZString macro)
		{
			try
			{
				var expression = processor.Replace(
					macro.Trim(),
					new[] { businessObject },
					false,
					this
				);
				return expression.EvaluateDocEngineExpression(RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				return false;
			}
		}

		#endregion

		public IUserContext UserContextForWebServiceEnvironment { get; set; }

		CodeDescriptionPairList includeDocInArchiveList;
		public CodeDescriptionPairList SU_IncludeDocInArchiveList
		{
			get
			{
				if (includeDocInArchiveList == null)
				{
					IIncludeDocInArchiveCodeDescriptionPairListProvider provider = ObjectFactory.Get<IIncludeDocInArchiveCodeDescriptionPairListProvider>();
					includeDocInArchiveList = provider.CodeDescriptionPairList;
				}
				return includeDocInArchiveList;
			}
		}

		[List("SU_IncludeDocInArchiveList")]
		public override ZString SU_IncludeDocInArchive
		{
			get
			{
				return base.SU_IncludeDocInArchive;
			}
			set
			{
				base.SU_IncludeDocInArchive = value;
			}
		}

		public IDocumentDeliveryRestrictionGUIManager CreditControlledDocumentDeliveryGUIManager { get; set; }

		protected override bool GetPropertyInfosReadOnly(PropertyDescriptor property)
		{
			bool result;
			if (property != null &&
				(property.Name == StmMenuItemSchema.SU_IsModifiable.Name ||
				 property.Name == StmMenuItemSchema.SU_MenuDataContext.Name ||
				 property.Name == StmMenuItemSchema.SU_EmailSubjectLine.Name ||
				 property.Name == StmMenuItemSchema.SU_EmailSenderOverride.Name ||
				 property.Name == StmMenuItemSchema.SU_DeliveryRestrictionType.Name ||
				 property.Name == StmMenuItemSchema.SU_DeliveryRestrictionMacro.Name ||
				 property.Name == StmMenuItemSchema.SU_DeliveryRestrictionDescription.Name ||
				 property.Name == StmMenuItemSchema.SU_IncludeDocInArchive.Name))
			{
				result = MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}
			else
			{
				result = base.GetPropertyInfosReadOnly(property);
			}
			return result;
		}

		public ZString MenuNameTag
		{
			get
			{
				string flag = "";

				if (!SU_IsSystemDefined)
				{
					flag = SU_IsPublished ? (NoResString)" <Customized>" : (NoResString)" <Private>";
				}

				return flag;
			}
		}

		public override bool IsApplicableCore() => this.IsApplicable(Parent);

		public bool HasChildMenus
		{
			get
			{
				return ChildMenus.Count > 0;
			}
		}

		public IDocumentSupportable Parent
		{
			get { return fParent; }
			set
			{
				fParent = value;
				IDocManagerSupport docManagerSupport = value as IDocManagerSupport;

				if (docManagerSupport != null)
				{
					DocManagerCode = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManagerSupport.DocManagerInfo.DocManagerCode);
				}

				parentDocumentSupporter = null;
			}
		}

		public DocumentSupporter ParentDocumentSupporter
		{
			get { return parentDocumentSupporter ?? (parentDocumentSupporter = Parent?.DocumentSupporter); }
		}

		DocumentSupporter parentDocumentSupporter;

		#region DocManagerCode

		public override StmMenuTemplatePivotBaseCollection Documents
		{
			get
			{
				var result = base.Documents;
				result.DocManagerFilter = fDocManagerCode;
				return result;
			}
		}

		public ZString DocManagerCode
		{
			get
			{
				return fDocManagerCode;
			}
			set
			{
				fDocManagerCode = value;
				if (documents != null)
				{
					Documents.DocManagerFilter = value;
				}
			}
		}

		ZString fDocManagerCode;

		#endregion

		public DocumentDirection DocumentDirection
		{
			get
			{
				DocumentDirection direction = DocumentDirection.ANY;
				if (!SU_DocumentDirection.IsEmpty)
				{
					direction = (DocumentDirection)Enum.Parse(typeof(DocumentDirection), SU_DocumentDirection);
				}
				return direction;
			}
		}

		public override CodeDescriptionPairList SU_MenuDataContextList
		{
			get
			{
				CodeDescriptionPairList result = base.SU_MenuDataContextList;
				if (Parent != null)
				{
					result = Parent.DocumentSupporter.ListOfSupportedDataContexts;
				}
				return result;
			}
		}

		public bool IsEDocsProviderPlaceholder
		{
			get { return SU_IsSystemDefined && SU_FilterList.Contains(Core.Constants.MenuItemFilters.EDocsProviderPlaceholderTag); }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (!IsDeleted && IsEDocsProviderPlaceholder); }
			set { base.ReadOnly = value; }
		}

		public Report GetReportForMenuDataContextReplacements(UserControlProviderList uDFList)
		{
			IBODocDataProvider dataProvider = GetDocWrapperForMenuDataContext();
			if (dataProvider != null)
			{
				DocumentPack tempPack = new DocumentPack();
				Report dummyReportForGettingDefaults = new Report(tempPack, null, dataProvider, "", uDFList, DocumentDirection.ANY, false);
				dummyReportForGettingDefaults.ResetDataProvider();
				dummyReportForGettingDefaults.RegisterDocumentAndReportRelatedMacroProviders();
				return dummyReportForGettingDefaults;
			}
			return null;
		}

		public IBODocDataProvider GetDocWrapperForMenuDataContext()
		{
			IBODocDataProvider[] dataProviders = Parent.DocumentSupporter.GetBODocDataProviders(new DataContextValue(SU_MenuDataContext), this);
			return (dataProviders != null) && (dataProviders.Length > 0) ? dataProviders[0] : null;
		}

		/// <summary>
		/// Adds an eDoc to this document command. This will only add eDocs for DocTypes that aren't already in this list.
		/// So if you have an eDoc of type 'ABC' you can't add another type of 'ABC' to this list.
		/// </summary>
		public DocumentStmMenuEDocs AddEDoc(RefDocType docType)
		{
			DocumentStmMenuEDocs eDoc = EDocs[docType.RT_DocType];

			if (eDoc != null)
			{
				eDoc.SX_IsClientSupressed = false;
			}
			else
			{
				eDoc = EDocs.AddNew();
				eDoc.SX_RT_DocType = docType.PK;
			}

			return eDoc;
		}

		/// <summary>
		/// Removes an eDoc from this DocumentCommand. If the eDoc was System Defined, then it just sets the
		/// IsSupressed flag to true, otherwise if it was a user defined one it removes it from the collection.
		/// </summary>
		/// <param name="eDoc">eDoc to remove</param>
		public void RemoveEDoc(StmMenuEDocs eDoc)
		{
			if (eDoc != null)
			{
				if (eDoc.SX_IsSystemDefined)
				{
					eDoc.SX_IsClientSupressed = true;
				}
				else
				{
					EDocs.RemoveAndDelete(eDoc);
				}
			}
		}

		IDocumentSupportable fParent;

		#region Validation

		protected override StmMenuItemValidation GetNewValidation()
		{
			return new DocumentCommandValidation(this);
		}
		#endregion

		protected override ICodeDescriptionPairList GetAttachmentTypesCore()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
			result.AddPair(AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Descriptions.Xlsx);
			result.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
			result.AddPair(AttachmentTypeList.Codes.Pdfa, AttachmentTypeList.Descriptions.Pdfa);
			result.AddPair(AttachmentTypeList.Codes.Pdfc, AttachmentTypeList.Descriptions.Pdfc);
			result.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);
			result.AddPair(AttachmentTypeList.Codes.Html, AttachmentTypeList.Descriptions.Html);
			result.AddPair(AttachmentTypeList.Codes.Htmf, AttachmentTypeList.Descriptions.Htmf);

			return result;
		}

		#region IRootTypeProvider members

		Type[] IRootTypeProvider.RootTypes
		{
			get
			{
				var types = new List<Type>();
				types.Add(this.GetType());
				if (Parent is BusinessObject)
				{
					types.Add(Parent.GetType());
				}

				return types.ToArray();
			}
		}

		BusinessObject[] IRootTypeProvider.Roots
		{
			get
			{
				var bizOs = new List<BusinessObject>();
				bizOs.Add(this);
				if (Parent is BusinessObject)
				{
					bizOs.Add(Parent as BusinessObject);
				}

				return bizOs.ToArray();
			}
		}

		#endregion

		#region IZFilterModuleThreadSafe

		public bool ShowFormsFromMainThread { get; set; }

		#endregion

		#region FromMenu

		public IDisposable FromMenu()
		{
			IsFromMenu = true;
			return new DisposableAction(() => IsFromMenu = false);
		}

		public bool IsFromMenu { get; private set; }

		#endregion

		public override void Delete()
		{
			DeliveryRestrictions.DeleteAll();
			base.Delete();
		}
	}
}
