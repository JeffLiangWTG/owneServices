using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.URLHandler;

namespace Enterprise.ZArchitecture.Modules
{
	public sealed class ZControllerFactory : IDCountryFactory<ControllerID, ControllerInfo, ControllerList>, IControllerFactory
	{
		ZControllerFactory()
		{
		}

		public static readonly ZControllerFactory Instance = new ZControllerFactory();

		public static ZControllerFactory GetInstance() => Instance;

		/// <summary>
		/// Creates a new ZController.
		/// </summary>
		/// <param name="iD">The ControllerID which represents the ZController to create.</param>
		public static ZController Create(ControllerID iD)
		{
			return (ZController)Instance.CreateNew(iD);
		}

		public static ZController CreateWithCountry(ControllerID iD, string countryCode)
		{
			return (ZController)Instance.CreateNewWithCountry(iD, countryCode);
		}

		public ZController GetControllerForBizo(IBusiness bizo)
		{
			return GetControllerForType(bizo.GetType());
		}

		public IEnumerable<ZController> GetControllersForBizo(IBusiness bizo)
		{
			return GetControllersForType(bizo.GetType());
		}

		public ZController GetControllerForType(Type type, string countryCode = null)
		{
			foreach (var registrationIdentifier in this.RegistrationList.All)
			{
				try
				{
					var controller = string.IsNullOrEmpty(countryCode) ? Create(registrationIdentifier.ID) : CreateWithCountry(registrationIdentifier.ID, countryCode);
					if (IsForBusinessObject(controller, type))
					{
						return controller;
					}
				}
				catch (ModuleIDIsNullException ex)
				{
					ErrorReporter.ReportOnce("ZController_GetControllerForType_ModuleIDIsNullException", string.Format(CultureInfo.InvariantCulture, "registrationIdentifier debugger info\r\n{0}", registrationIdentifier.GetDebuggerInfo()), ex);
				}
			}
			return null;
		}

		public ZController GetControllerForTypeOrItsBaseTypes(Type type, string countryCode = null)
		{
			if (!typeof(BusinessObject).IsAssignableFrom(type))
			{
				ErrorReporter.ReportOnce("ZController_GetControllerForTypeOrItsBaseTypes_TypeNotBusinessObject", $"Type:{type.FullName}");
			}
			while (type != null && type != typeof(BusinessObject))
			{
				var controller = GetControllerForType(type, countryCode);
				if (controller != null)
				{
					return controller;
				}
				type = type.BaseType;
			}
			return null;
		}

		public IEnumerable<ZController> GetControllersForType(Type type)
		{
			var result = new List<ZController>();
			foreach (var registrationIdentifier in this.RegistrationList.All)
			{
				var controller = Create(registrationIdentifier.ID);
				if (IsForBusinessObject(controller, type))
				{
					result.Add(controller);
				}
			}
			return result;
		}

		bool IsForBusinessObject(ZController controller, Type bizoType)
		{
			try
			{
				return controller?.TypeOfTopLevelBusinessObject == bizoType;
			}
			catch (ModuleGuiNotSupportedException) { return false; }
			catch (NotSupportedException) { return false; }
			catch (NotImplementedException) { return false; }
		}

		IController IControllerFactory.Create(ControllerID controllerId) => Create(controllerId);

		IController IControllerFactory.GetControllerForType(Type type) => GetControllerForType(type);

		ControllerID IControllerFactory.GetRegisteredIdentifierByName(string identifier) => GetRegisteredIdentifierByName(identifier);

		#region Get Correct Controller

		(IController Controller, BusinessObject BusinessObject) IControllerFactory.GetCorrectControllerAndBusinessObject(ControllerID controllerId, ZGuid pk, bool shouldReportError)
		{
			return GetCorrectControllerAndBusinessObject(controllerId, pk, shouldReportError);
		}

		public static (ZController Controller, BusinessObject BusinessObject) GetCorrectControllerAndBusinessObject(ControllerID controllerId, ZGuid pk, bool shouldReportError)
		{
			var result = CreateControllerAndBizO(controllerId, pk);
			var controller = result.Controller;
			var bizO = result.BusinessObject;

			if (controller is INavigationControllerIDProvider provider)
			{
				var newControllerId = provider.GetValidControllerID(bizO);
				if (!Equals(newControllerId, controller.ID))
				{
					if (provider.ShouldLoadBusinessObject)
					{
						result = CreateControllerAndBizO(newControllerId, bizO?.PK ?? pk, bizO?.Factory);
						controller = result.Controller;
					}
					else
					{
						result.Controller = Create(newControllerId);
					}
				}
			}

			if (shouldReportError && !controller.ID.Equals(controllerId))
			{
				throw new EnterpriseUrlHandlerException("This is not a valid " + Enterprise.Core.Constants.ProductName + " shortcut or hyperlink. Record state was changed and this operation is now invalid.");
			}

			return result;
		}

		static (ZController Controller, BusinessObject BusinessObject) CreateControllerAndBizO(ControllerID controllerId, ZGuid pk, BusinessObjectFactory factory = null)
		{
			var controller = Create(controllerId);
			factory ??= controller.Factory;
			BusinessObject bizO;

			if (controller is IPluginControllerBusinessObjectProvider pluginControllerProvider)
			{
				bizO = (BusinessObject)pluginControllerProvider.LoadBusinessEntityForPlugIn(factory, pk);
			}
			else
			{
				bizO = (BusinessObject)controller.LoadBusinessEntity(factory, pk);
			}

			if (bizO == null)
			{
				throw new EnterpriseUrlHandlerException("The system has searched all open " + Enterprise.Core.Constants.ProductName + " programs and could not find the record");
			}

			return (controller, bizO);
		}

		#endregion
	}
}
