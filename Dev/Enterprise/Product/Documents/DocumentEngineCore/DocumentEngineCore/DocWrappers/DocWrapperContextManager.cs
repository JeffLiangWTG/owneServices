using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public static class FactoryExtension
	{
		public static DocWrapperContextManager GetDocWrapperContextManager(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DocWrapperContextKey", () => new DocWrapperContextManager());
		}
	}

	public class DocWrapperContextManager : IDocWrapperContext
	{
		internal DocWrapperContextManager()
		{
		}

		DocWrapperContext wrapperContext;
		DocumentPackContext documentPackContext;

		public StackTrace callstackWhenWrapperContextBecomeNull;

		public void SetupDocWrapperContextFromDocumentPack(ZString documentDirection, ZGuid menuItemPK, ZString menuTitle, ZString contactTypeCode, IOrganisationData brandedOrganisation, IOrganisationData contactOrganisation)
		{
			documentPackContext = new DocumentPackContext(documentDirection
				, menuItemPK
				, menuTitle
				, contactTypeCode
				, brandedOrganisation == null ? ZGuid.Empty : brandedOrganisation.PK
				, contactOrganisation == null ? ZGuid.Empty : contactOrganisation.PK);

			wrapperContext = null;

			callstackWhenWrapperContextBecomeNull = new StackTrace();
		}

		class DocumentPackContext
		{
			internal DocumentPackContext(ZString documentDirection, ZGuid menuItemPK, ZString menuTitle, ZString contactTypeCode, ZGuid brandedOrganisationPK, ZGuid contactOrganisationPK)
			{
				this.DocumentDirection = documentDirection;
				this.MenuItemPK = menuItemPK;
				this.MenuTitle = menuTitle;
				this.ContactTypeCode = contactTypeCode;
				this.BrandedOrganisationPK = brandedOrganisationPK;
				this.ContactOrganisationPK = contactOrganisationPK;
			}

			internal readonly ZString DocumentDirection;
			internal readonly ZGuid MenuItemPK;
			internal readonly ZString MenuTitle;
			internal readonly ZString ContactTypeCode;
			internal readonly ZGuid BrandedOrganisationPK;
			internal readonly ZGuid ContactOrganisationPK;
		}

		public void UpdateDocWrapperContextFromReportConstants(Dictionary<string, object> constants)
		{
			if (wrapperContext == null)
			{
				if (documentPackContext != null)
				{
					wrapperContext = new DocWrapperContext(new Dictionary<string, object>
					{
						{ Constants.TemplateDefined.DocumentDirection, documentPackContext.DocumentDirection },
						{ Constants.TemplateDefined.MenuItemPK, documentPackContext.MenuItemPK },
						{ Constants.TemplateDefined.MenuTitle, documentPackContext.MenuTitle },
						{ Constants.TemplateDefined.ContactType, documentPackContext.ContactTypeCode },
						{ Constants.TemplateDefined.BrandedOrganisationPK, documentPackContext.BrandedOrganisationPK },
						{ Constants.TemplateDefined.ContactOrganisationPK, documentPackContext.ContactOrganisationPK },
					});
				}
				else
				{
					wrapperContext = new DocWrapperContext(null);
				}
			}

			wrapperContext.MergeConstants(constants);

			documentPackContext = new DocumentPackContext(wrapperContext.DocumentDirection, wrapperContext.MenuItemPK, wrapperContext.MenuTitle, wrapperContext.DocumentContactTypeCode, wrapperContext.BrandedOrganisationPK, wrapperContext.ContactOrganisationPK);
		}

		static void ThrowExceptionIfContextIsNull(object context)
		{
			if (context == null)
			{
				throw new InvalidOperationException("You cannot access the DocWrapperContext properties until they have been setup by the Report. If you are accessing this property from the Constructor of your DocumentWrapper, try converting to a lazy loading pattern so that it gets accessed after it's been setup.");
			}
		}

		#region IDocWrapperContext Members

		ZString IDocWrapperContext.DocumentContactTypeCode
		{
			get
			{
				ThrowExceptionIfContextIsNull(documentPackContext);
				return documentPackContext.ContactTypeCode;
			}
		}

		ZGuid IDocWrapperContext.ContactOrganisationPK
		{
			get
			{
				ThrowExceptionIfContextIsNull(documentPackContext);
				return documentPackContext.ContactOrganisationPK;
			}
		}

		ZGuid IDocWrapperContext.BrandedOrganisationPK
		{
			get
			{
				ThrowExceptionIfContextIsNull(documentPackContext);
				return documentPackContext.BrandedOrganisationPK;
			}
		}

		ZString IDocWrapperContext.ReportName
		{
			get
			{
				ThrowExceptionIfContextIsNull(wrapperContext);
				return wrapperContext.ReportName;
			}
		}

		ZString IDocWrapperContext.DocumentDirection
		{
			get
			{
				ThrowExceptionIfContextIsNull(documentPackContext);
				return documentPackContext.DocumentDirection;
			}
		}

		ZString IDocWrapperContext.MenuTitle
		{
			get
			{
				ThrowExceptionIfContextIsNull(documentPackContext);
				return documentPackContext.MenuTitle;
			}
		}

		ZGuid IDocWrapperContext.MenuItemPK
		{
			get
			{
				ThrowExceptionIfContextIsNull(documentPackContext);
				return documentPackContext.MenuItemPK;
			}
		}

		ZString IDocWrapperContext.DocumentDeliveryMode
		{
			get
			{
				ThrowExceptionIfContextIsNull(wrapperContext);
				return wrapperContext.DocumentDeliveryMode;
			}
		}

		T IDocWrapperContext.GetTemplateConstantValue<T>(string key, out bool isFound)
		{
			ThrowExceptionIfContextIsNull(wrapperContext);
			return wrapperContext.GetTemplateConstantValue<T>(key, out isFound);
		}

		#endregion

		public IDisposable SuspendContextSetCheck()
		{
			return new ContextTemporarySetter(this);
		}

		public IDisposable SetContextValuesTemporarily(DocumentDirection direction, IContactType contactType)
		{
			Argument.NotNull(contactType, "contactType");

			Dictionary<string, object> contextValues = new Dictionary<string, object>();
			contextValues.Add(Constants.TemplateDefined.DocumentDirection, direction.ToString());
			contextValues.Add(Constants.TemplateDefined.ContactType, contactType.ToString());

			return new ContextTemporarySetter(this, contextValues);
		}

		public IDisposable ClearMenuContextValuesTemporarily()
		{
			var contextValues = new Dictionary<string, object>
			{
				{ Constants.TemplateDefined.DocumentDirection, ZString.Empty },
				{ Constants.TemplateDefined.MenuItemPK, ZGuid.Empty },
				{ Constants.TemplateDefined.MenuTitle, ZString.Empty },
				{ Constants.TemplateDefined.ContactType, ZString.Empty },
				{ Constants.TemplateDefined.BrandedOrganisationPK, ZGuid.Empty },
				{ Constants.TemplateDefined.ContactOrganisationPK, ZGuid.Empty }
			};

			return new ContextTemporarySetter(this, contextValues);
		}

		class ContextTemporarySetter : IDisposable
		{
			public ContextTemporarySetter(DocWrapperContextManager manager, Dictionary<string, object> constants = null)
			{
				contextManager = manager;
				menuContext = manager.documentPackContext;
				wrapperContext = manager.wrapperContext;
				contextManager.UpdateDocWrapperContextFromReportConstants(constants);
			}

			readonly DocWrapperContextManager contextManager;
			readonly DocWrapperContext wrapperContext;
			readonly DocumentPackContext menuContext;

			void IDisposable.Dispose()
			{
				contextManager.documentPackContext = menuContext;
				contextManager.wrapperContext = wrapperContext;
			}
		}
	}

	public interface IDocWrapperContext
	{
		ZString DocumentContactTypeCode { get; }
		ZGuid ContactOrganisationPK { get; }
		ZGuid BrandedOrganisationPK { get; }
		ZString ReportName { get; }
		ZString DocumentDirection { get; }
		ZString MenuTitle { get; }
		ZGuid MenuItemPK { get; }
		ZString DocumentDeliveryMode { get; }
		T GetTemplateConstantValue<T>(string key, out bool isFound);
	}
}
