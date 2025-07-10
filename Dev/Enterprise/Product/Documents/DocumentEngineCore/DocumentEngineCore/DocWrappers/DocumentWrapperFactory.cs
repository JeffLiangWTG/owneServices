using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class DocumentWrapperFactory
	{
		#region Create Wrapper with Parent

		public static DocumentWrapper CreateWrapperWithParent(Constants.DataContext dataContext, BusinessObject businessObjectToWrap, EnterpriseBusinessObject parentObject)
		{
			var factory = GetFactoryFromBusinessObjects(businessObjectToWrap, parentObject);
			var @params = new Object[] { businessObjectToWrap, parentObject, factory };

			return CreateWrapper(DocumentWrapperClassNamespace + (NoResString)"Doc" + dataContext.ToString(), @params);
		}

		#endregion

		#region Create Wrapper For Shipment Object

		public static DocumentWrapper CreateContainerWrapperWithShipment(BusinessObject container, BusinessObject baseShipment)
		{
			var factory = GetFactoryFromBusinessObjects(container, baseShipment);
			var @params = new Object[] { container, baseShipment, factory };

			return CreateWrapper(DocumentWrapperClassNamespace + "DocContainer", @params);
		}

		public static DocumentWrapper CreateFCLContainerWrapperWithShipment(BusinessObject fCLContainer, BusinessObject baseShipment)
		{
			var factory = GetFactoryFromBusinessObjects(fCLContainer, baseShipment);
			var @params = new Object[] { fCLContainer, baseShipment, factory };

			return CreateWrapper(DocumentWrapperClassNamespace + "DocFCLContainer", @params);
		}

		public static DocumentWrapper CreateIMOShipmentWrapper(BusinessObject shipment, BusinessObject consol)
		{
			var factory = GetFactoryFromBusinessObjects(shipment, consol);
			var @params = new Object[] { shipment, consol, factory };

			return CreateWrapper(DocumentWrapperClassNamespace + "DocIMOShipment", @params);
		}

		public static DocumentWrapper CreateServiceWrapperWithParent(BusinessObject service, DocumentWrapper parentWrapper)
		{
			var @params = new Object[] { service, parentWrapper, service.Factory };
			return CreateWrapper(DocumentWrapperClassNamespace + "Freight.DocService", @params);
		}

		#endregion

		#region Create Cartage Wrappers

		public static DocumentWrapper CreateContainerWrapperWithCartage(BusinessObject container, BusinessObject cartage)
		{
			var @params = new Object[] { container, cartage, container.Factory };
			return CreateWrapper(DocumentWrapperClassNamespace + "DocCommonContainer", @params);
		}

		#endregion

		public static DocumentWrapper CreateWrapper(Constants.DataContext dataContext, BusinessObject businessObjectToWrap, string assemblyName = null)
		{
			Argument.NotNull(businessObjectToWrap, "BusinessObject");
			return CreateWrapper(dataContext, DocumentWrapperClassNamespace, businessObjectToWrap, assemblyName);
		}

		public static DocumentWrapper CreateWrapper(Constants.DataContext dataContext, string nameSpace, BusinessObject businessObjectToWrap, string assemblyName = null)
		{
			Argument.NotNull(businessObjectToWrap, "BusinessObject");
			if (!nameSpace.EndsWith("."))
			{
				nameSpace += ".";
			}

			return CreateWrapper(nameSpace + (NoResString)"Doc" + dataContext.ToString(), businessObjectToWrap, assemblyName);
		}

		public static DocumentWrapper[] CreateWrappers(Constants.DataContext dataContext, BusinessObject businessObjectToWrap, DocWrapperCopyInfo[] copiesToCreate)
		{
			var docWrappers = new DocumentWrapper[copiesToCreate.Length + 1];
			docWrappers[0] = CreateWrapper(dataContext, businessObjectToWrap);

			if (copiesToCreate != null)
			{
				for (var i = 0; i < copiesToCreate.Length; i++)
				{
					var wrapper = CreateWrapper(dataContext, businessObjectToWrap);
					wrapper.SetAdditionalCopyInfo(copiesToCreate[i]);
					docWrappers[i + 1] = wrapper;
				}
			}

			return docWrappers;
		}

		public static DocumentWrapper CreateAccountingWrapper(Constants.DataContext dataContext, BusinessObject businessObjectToWrap, string countryCode = null)
		{
			Argument.NotNull(businessObjectToWrap, "BusinessObject");

			if (!string.IsNullOrWhiteSpace(countryCode))
			{
				return CreateWrapper(DocumentWrapperClassNamespace + countryCode + ".Doc" + dataContext.ToString(), businessObjectToWrap);
			}
			else
			{
				return CreateWrapper(dataContext, businessObjectToWrap);
			}
		}

		public static DocumentWrapper CreateCustomsWrapper(Constants.DataContext dataContext, BusinessObject businessObjectToWrap, ZString countryCode)
		{
			var className = $"Doc{dataContext}";
			var declarationType = businessObjectToWrap.GetType();
			return CreateWrapperWithoutException($"{DocumentWrapperCustomsNamespace}{GetCustomsNamespaceForCountryCode(countryCode)}.{className}", businessObjectToWrap) ??
				CreateWrapperWithoutException($"{declarationType.Namespace}.{className}", businessObjectToWrap, declarationType.Assembly) ??
				CreateWrapperWithoutException($"{declarationType.Namespace}.DocumentWrappers.{className}", businessObjectToWrap, declarationType.Assembly) ??
				CreateWrapper($"{DocumentWrapperCustomsNamespace}General.{className}", businessObjectToWrap);
		}

		public static DocumentWrapper CreateCustomsWrapperZA(Constants.DataContext dataContext, BusinessObject businessObjectToWrap)
		{
			return CreateWrapperWithoutException($"Enterprise.Customs.ZA.Business.DocumentWrappers.Doc{dataContext}", businessObjectToWrap, "Enterprise.Customs.ZA.Business");
		}

		public static DocumentWrapper[] GenerateGenericWrappers(Constants.DataContext dataContext, BusinessObject objectToWrap)
		{
			DocumentWrapper[] result = null;
			var loader = GenericWrapperLoader.GetFromDataContext(dataContext);
			if (loader != null)
			{
				result = GetDocumentWrapper(dataContext, objectToWrap, loader);
			}
			return result;
		}

#if DEBUG
		internal
#endif
 static DocumentWrapper[] GetDocumentWrapper(Constants.DataContext dataContext, BusinessObject objectToWrap, GenericWrapperLoader loader, BusinessObject childObjectToWrap = null)
		{
			try
			{
				return childObjectToWrap == null ? loader.GetWrappers(objectToWrap, objectToWrap.Factory) : loader.GetWrappers(objectToWrap, childObjectToWrap, objectToWrap.Factory);
			}
			catch (InvalidCastException)
			{
				DisplayInvalidCastExceptionMessage(dataContext);
			}
			return null;
		}

		static void DisplayInvalidCastExceptionMessage(Constants.DataContext dataContext)
		{
			Globals.Message.ShowError(Res.GetString("8ff169dc-1145-4b5d-9323-248c9c4b01ae", "An error happened when generating the document. Data context \"{0}\" could be improper for this document. Please try modifying your customized document configuration and choose an appropriate data context before running again.",
				dataContext), Res.GetString("791a9f42-2638-45e0-93c8-7f003f961dfe", "Incorrect Document Data Context"));
		}

		public static DocumentWrapper[] GenerateGenericWrappers(Constants.DataContext dataContext, BusinessObject objectToWrap, DocWrapperCopyInfo[] copiesToCreate)
		{
			DocumentWrapper[] result = null;
			var loader = GenericWrapperLoader.GetFromDataContext(dataContext);
			if (loader != null)
			{
				result = GetDocumentWrapper(dataContext, objectToWrap, loader);
			}

			if (copiesToCreate != null && copiesToCreate.Length > 0 && result != null && result.Length == 1)
			{
				var resultWithCopies = new DocumentWrapper[copiesToCreate.Length + 1];
				resultWithCopies[0] = result[0];
				for (int i = 0; i < copiesToCreate.Length; i++)
				{
					var wrapper = loader.GetWrappers(objectToWrap, objectToWrap.Factory)[0];
					wrapper.SetAdditionalCopyInfo(copiesToCreate[i]);
					resultWithCopies[i + 1] = wrapper;
				}
				return resultWithCopies;
			}
			else
			{
				return result;
			}
		}

		public static DocumentWrapper[] GenerateGenericWrappers(Constants.DataContext dataContext, BusinessObject parentObjectToWrap, BusinessObject childObjectToWrap)
		{
			DocumentWrapper[] result = null;
			var loader = GenericWrapperLoader.GetFromDataContext(dataContext);
			if (loader != null)
			{
				result = GetDocumentWrapper(dataContext, parentObjectToWrap, loader, childObjectToWrap);
			}
			return result;
		}

		public static DocumentWrapper CreateNZConsolWrapper(BusinessObject consol)
		{
			return CreateWrapper(DocumentWrapperClassNamespace + "Freight.NZ.DocForwardingConsol", consol);
		}

		public static DocumentWrapper CreateCHConsolWrapper(BusinessObject consol)
		{
			return CreateWrapper((NoResString)"Enterprise.Customs.CH.Business.DocForwardingConsol, Enterprise.Customs.CH.Business", consol);
		}

		public static DocumentWrapper CreateCustomsContainerWrapperWithDeclaration(BusinessObject cusContainer, BusinessObject declaration, ZString countryCode)
		{
			DocumentWrapper result = null;
			var @namespace = DocumentWrapperClassNamespace + "Customs." + GetCustomsNamespaceForCountryCode(countryCode);
			var factory = GetFactoryFromBusinessObjects(cusContainer, declaration);
			var @params = new Object[] { cusContainer, declaration, factory };
			result = CreateWrapperWithoutException(@namespace + ".DocCusContainer", @params);
			if (result == null)
			{
				var declarationType = declaration.GetType();
				result = CreateWrapperWithoutException(declarationType.Namespace + ".DocCusContainer", @params, declarationType.Assembly);
			}
			if (result == null)
			{
				result = CreateWrapper(DocumentWrapperClassNamespace + "Customs.General.DocCusContainer", @params);
			}

			return result;
		}

		public static DocumentWrapper CreateOrganisationWrapperWithSupplierBuyer(BusinessObject orgHeader, DocumentWrapper orgSupplier, DocumentWrapper orgBuyer)
		{
			DocumentWrapper result = null;
			var wrapperNamespace = DocumentWrapperClassNamespace + "GenericWrappers";
			var factory = GetFactoryFromBusinessObjects(orgHeader, orgSupplier);
			var parameters = new Object[] { orgHeader, orgSupplier, orgBuyer, factory };
			result = CreateWrapperWithoutException(wrapperNamespace + ".FreightWrapperFromOrgBO", parameters);
			return result;
		}

		public static DocumentWrapper CreateTransportWrapper(BusinessObject transport, BusinessObject parent)
		{
			var factory = GetFactoryFromBusinessObjects(transport, parent);
			var @params = new Object[] { parent, transport, factory };

			return CreateWrapper(DocumentWrapperClassNamespace + "DocTransport", @params);
		}

		#region Implementation

		static string GetCustomsNamespaceForCountryCode(string countryCode)
		{
			if (CargoWise.Application.ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
			{
				return "AsycudaCustoms";
			}
#if DEBUG
			if (countryCode == Constants.CountryCodes._TemplateCountryName_)
			{
				return "_CustomsTemplate_";
			}
#endif
			return countryCode;
		}

#if DEBUG
		public
#endif
		static string DocumentWrapperClassNamespace => "Enterprise.DocumentWrappers.";

		static string DocumentWrapperCustomsNamespace => "Enterprise.DocumentWrappers.Customs.";

		static BusinessObjectFactory GetFactoryFromBusinessObjects(BusinessObject businessObjectOne, BusinessObject businessObjectTwo)
		{
			BusinessObjectFactory result = null;

			if (businessObjectOne != null)
			{
				result = businessObjectOne.Factory;
			}
			else if (businessObjectTwo != null)
			{
				result = businessObjectTwo.Factory;
			}

			return result;
		}

		internal static DocumentWrapper CreateWrapper(string fullNamespaceForWrapperClass, BusinessObject businessObjectToWrap, string assemblyName = null)
		{
			var @params = new Object[] { businessObjectToWrap, businessObjectToWrap?.Factory };
			return CreateWrapper(fullNamespaceForWrapperClass, @params, assemblyName);
		}

		public static DocumentWrapper CreateWrapperWithoutException(string fullNamespaceForWrapperClass, BusinessObject businessObjectToWrap, string assemblyName = null)
		{
			var @params = new Object[] { businessObjectToWrap, businessObjectToWrap?.Factory };
			return CreateWrapperWithoutException(fullNamespaceForWrapperClass, @params, assemblyName);
		}

		public static DocumentWrapper CreateWrapper(ZString wrapperType, Object[] @params, string assemblyName = null)
		{
			return CreateWrapperWithoutException(wrapperType, @params, assemblyName);
		}

		static DocumentWrapper CreateWrapperWithoutException(ZString wrapperType, Object[] @params, string assemblyName = null)
		{
			Type targetType;
			if (wrapperType.Contains(","))
			{
				targetType = Type.GetType(wrapperType);
			}
			else
			{
				var wrapperAssembly = Assembly.Load(assemblyName ?? "DocumentWrappers");
				targetType = wrapperAssembly.GetType(wrapperType);
			}

			if (targetType == null && ClientHookLoader.Instance.ClientHook != null)
			{
				targetType = ClientHookLoader.Instance.ClientHook.GetType().Assembly.GetType(wrapperType);
			}

			return CreateWrapper(targetType, @params);
		}

		static DocumentWrapper CreateWrapperWithoutException(string wrapperType, BusinessObject businessObjectToWrap, Assembly assembly)
		{
			var @params = new Object[] { businessObjectToWrap, businessObjectToWrap?.Factory };
			return CreateWrapperWithoutException(wrapperType, @params, assembly);
		}

		static DocumentWrapper CreateWrapperWithoutException(string wrapperType, Object[] @params, Assembly assembly)
		{
			return CreateWrapper(assembly.GetType(wrapperType), @params);
		}

		static DocumentWrapper CreateWrapper(Type targetType, Object[] @params)
		{
			if (targetType != null)
			{
				var bindingFlag = BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod;
				return (DocumentWrapper)targetType.InvokeMember("New", bindingFlag, null, null, @params);
			}

			return null;
		}

		#endregion

#if DEBUG
		[TypeFactoryAnnotationMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			foreach (var dataContext in Enum.GetNames(typeof(Constants.DataContext)))
			{
				yield return "Enterprise.DocumentWrappers.Doc" + dataContext + ",DocumentWrappers";
			}

			yield return DocumentWrapperFactory.DocumentWrapperClassNamespace + "DocContainer,DocumentWrappers";
			yield return DocumentWrapperFactory.DocumentWrapperClassNamespace + "DocFCLContainer,DocumentWrappers";
			yield return DocumentWrapperFactory.DocumentWrapperClassNamespace + "DocIMOShipment,DocumentWrappers";
			yield return DocumentWrapperFactory.DocumentWrapperClassNamespace + "Freight.DocService,DocumentWrappers";
		}
#endif
	}
}
