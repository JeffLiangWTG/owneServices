using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Enumeration;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	class TextTemplatesFactory
	{
		public TextTemplatesFactory(IDataBoundControl dataBoundControl)
		{
			this.dataBoundControl = dataBoundControl;
			control = dataBoundControl as Control;
		}

		public TextTemplatesFactory(CurrencyManager dataSource, string mappingName, TextBoxBase textBox)
		{
			this.dataSource = dataSource;
			this.mappingName = mappingName;
			control = textBox;
		}

		public void Bind(CurrencyManager dataSource, string mappingName)
		{
			this.dataSource = dataSource;
			this.mappingName = mappingName;
			ResetContext();
		}

		readonly IDataBoundControl dataBoundControl;
		readonly Control control;

		internal CurrencyManager dataSource;
		string mappingName;

		public bool TemplatesSupported
		{
			get
			{
				return ContextBusinessObjects != null && bindingMemberSupported;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public StmNoteTemplate[] GetAllTemplatesForControl()
		{
			if (ContextId == null)
			{
				return null;
			}

			var query = new ZQuery();
			query.AddToFilter(StmNoteTemplateSchema.S8_ContextID, ContextId);

			if (alternateContextIds != null)
			{
				foreach (var alternateContextId in alternateContextIds)
				{
					var alternateContext = new ZQuery();
					alternateContext.AddToFilter(StmNoteTemplateSchema.S8_ContextID, alternateContextId);
					query.AddToFilter(alternateContext, JoinCondition.Or);
				}
			}

			if (!string.IsNullOrEmpty(fallbackContextId))
			{
				var fallbackContext = new ZQuery(StmNoteTemplateSchema.S8_ContextID, fallbackContextId);
				query.AddToFilter(fallbackContext, JoinCondition.Or);
			}

			var securityAccessFilter = new ZQuery();
			securityAccessFilter.AddToFilter(StmNoteTemplateSchema.S8_GS_NKStaff, EnvProxy.Instance.CurrentUser.Initials);
			var publishedFilter = new ZQuery();
			publishedFilter.AddToFilter(StmNoteTemplateSchema.S8_GS_NKStaff, ZString.Empty);
			publishedFilter.AddToFilter(JoinCondition.And, StmNoteTemplateSchema.S8_GC, EnvProxy.Instance.CurrentCompany.PK);
			securityAccessFilter.AddToFilter(publishedFilter, JoinCondition.Or);
			var allCompaniesFilter = new ZQuery();
			allCompaniesFilter.AddToFilter(StmNoteTemplateSchema.S8_GS_NKStaff, ZString.Empty);
			allCompaniesFilter.AddToFilter(JoinCondition.And, StmNoteTemplateSchema.S8_GC, null);
			securityAccessFilter.AddToFilter(allCompaniesFilter, JoinCondition.Or);
			query.AddToFilter(securityAccessFilter);

			query.OrderBy = StmNoteTemplateSchema.S8_Description.Name;

			query.IncludeBlob(StmNoteTemplateSchema.S8_Description);
			query.IncludeBlob(StmNoteTemplateSchema.S8_TemplateText);

			var notesCollection = QueryFactory.Load<StmNoteTemplate>(query);

			foreach (var noteTemplate in notesCollection.Where(x => x.S8_ContextID != ContextId && x.S8_ContextID != fallbackContextId))
			{
				noteTemplate.S8_ContextID = ContextId;
			}

			return notesCollection;
		}

		BusinessObjectFactory QueryFactory
		{
			get
			{
				return ContextBusinessObjects[0].Factory ?? queryFactory ?? (queryFactory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory queryFactory;

		public BusinessObject[] ContextBusinessObjects
		{
			get
			{
				InitializeContext();
				return contextBusinessObjects;
			}
		}

		public string ContextId
		{
			get
			{
				InitializeContext();

				contextId = null;
				if (customContextBusinessObject is ICustomTextTemplateContext customContext)
				{
					contextId = customContext.GetTextTemplateContextID(dataBoundControl != null ? dataBoundControl.DataSource : dataSource, bindingMemberInfo);
				}
				else if (customContextBusinessObject != null)
				{
					contextId = customContextBusinessObject.TableName + "." + bindingMemberInfo.BindingField;
				}

#if WINZOR
				if (!string.IsNullOrEmpty(contextId))
				{
					contextId = contextId.Replace("_HTML", "");
				}
#endif

				return contextId;
			}
		}

		KBindingMemberInfo bindingMemberInfo;
		void InitializeContext()
		{
			if (!contextInitialized)
			{
				contextInitialized = true;
				if (dataBoundControl != null && dataBoundControl.DataSource != null)
				{
					bindingMemberInfo = new KBindingMemberInfo(dataBoundControl.DataMember);
					var bindingManager = ((Control)dataBoundControl).BindingContext[dataBoundControl.DataSource, bindingMemberInfo.BindingPath];
					if (bindingManager.Count > 0 && bindingManager.GetCurrent() is BusinessObject currentObject)
					{
						contextBusinessObjects = new[] { currentObject };
					}
					bindingManager.CurrentChanged += (sender, args) => ResetContext();
				}
				else if (dataSource != null && dataSource.GetCurrent() is BusinessObject currentObject)
				{
					bindingMemberInfo = new KBindingMemberInfo(mappingName);
					contextBusinessObjects = new[] { currentObject };
					dataSource.CurrentChanged += (sender, args) => ResetContext();
				}
				else //kludge - The default constructor is no longer allowed to be used, which makes it think there's a possible code path where bindingMemberInfo.BindingField could be accessed on uninitialized.
				{
					bindingMemberInfo = new KBindingMemberInfo("");
				}

				var form = control?.FindForm();
				IEnumerable<BusinessObject> ownerBizos = null;

				if (form is ZForm)
				{
					ownerBizos = ZEnumerable.Iterate(form, f => f.Owner, null)
						.OfType<ZForm>()
						.Select(f => f.BusinessEntity)
						.OfType<BusinessObject>();
					contextBusinessObjects = contextBusinessObjects?.Concat(ownerBizos).Distinct().ToArray();
				}

				if (contextBusinessObjects != null)
				{
					var bindingMemberType = contextBusinessObjects[0].GetPropertyType(bindingMemberInfo.BindingField);
					bindingMemberSupported = bindingMemberType == typeof(string) || bindingMemberType == typeof(ZString) || bindingMemberType == typeof(ZBlob);

					customContextBusinessObject = contextBusinessObjects[0];
					if (customContextBusinessObject is ICustomTextTemplateContext customContext)
					{
						var customContextBusinessObjects = customContext.GetTextTemplateContextBusinessObject(dataBoundControl != null ? dataBoundControl.DataSource : dataSource, bindingMemberInfo);
						contextBusinessObjects = customContextBusinessObjects?.Concat(ownerBizos).Distinct().ToArray();

						if (customContext is ICustomTextTemplateAlternateContexts alternateContext)
						{
							alternateContextIds = alternateContext.GetAlternateTextTemplateContextIDs(dataBoundControl != null ? dataBoundControl.DataSource : dataSource, bindingMemberInfo);
						}

						if (customContext is ICustomTextTemplateFallbackContext fallbackContext)
						{
							fallbackContextId = fallbackContext.GetFallbackTextTemplateContextID(dataBoundControl != null ? dataBoundControl.DataSource : dataSource, bindingMemberInfo);
						}
					}
				}
			}
		}

		public void ResetContext()
		{
			contextInitialized = false;

			customContextBusinessObject = null;
			alternateContextIds = null;
			fallbackContextId = null;
		}

		bool contextInitialized;
		BusinessObject[] contextBusinessObjects;
		BusinessObject customContextBusinessObject;
		string contextId;
		IReadOnlyCollection<string> alternateContextIds;
		string fallbackContextId;
		bool bindingMemberSupported;

		public StmNoteTemplate New()
		{
			var template = new BusinessObjectFactory().New<StmNoteTemplate>();
			template.S8_ContextID = ContextId;
			return template;
		}

		public StmNoteTemplate Edit(StmNoteTemplate template)
		{
			return (StmNoteTemplate)new BusinessObjectFactory().ImportFromAnotherFactory(template);
		}

#if DEBUG
		internal IReadOnlyCollection<string> AlternateContextIds_Exposed => alternateContextIds;
#endif
	}
}
