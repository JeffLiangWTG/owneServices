#if WINZOR
using System;
using System.Collections.Generic;
using System.Linq;
#endif
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Forwarding;

namespace BusinessObjectSecurityCheckpointExtractor.Test
{
	public class CheckpointExtractorTest : TestCaseWithFactory
	{
#if !WINZOR
		static readonly HashSet<string> Whitelist = new HashSet<string>
		{
			$"Enterprise.Freight.Forwarding.Business.ForwardingProfitShareRedistribution, Enterprise.Freight.Forwarding.Business",
			$"Enterprise.Core.DialogDefault.StmDialogDefault, Enterprise.ZArchitecture.GUI",
			$"Enterprise.Customs.US.InBond.Business.CusInBondHeader, Enterprise.Customs.US.InBond.Business",
			$"Enterprise.Freight.Forwarding.Business.ForwardingShipment, Enterprise.Freight.Forwarding.Business",
			$"Enterprise.Customs.CA.Business.CusStatementHeader, Enterprise.Customs.CA.Business",
			$"Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader, Enterprise.Customs.ASYCUDA.Business",
			$"Enterprise.Customs.ASYCUDA.Business.AsycudaBill, Enterprise.Customs.ASYCUDA.Business",
			$"Enterprise.Packing.Business.PkgPackageJob, Enterprise.Packing.Business"
		};

		public void TestBusinessObjectSecurityCheckpointExtractor()
		{
			var testDiscrepancies = new Dictionary<string, BusinessObjectCheckpoint>();
			var implementationDiscrepancies = new Dictionary<string, BusinessObjectCheckpoint>();

			var extractor = new CheckpointExtractor();
			var implementationMethodCheckpoints = extractor.GenerateBusinessObjectSecurityCheckpoints();

			var testMethodCheckpoints = GetBusinessObjectCheckpointsThroughControllers();

			foreach (var item in testMethodCheckpoints)
			{
				if (Whitelist.Contains(item.Key))
				{
					continue;
				}
				if (implementationMethodCheckpoints.TryGetValue(item.Key, out var implementationCheckpoints))
				{
					var testDiscrepancy = new BusinessObjectCheckpoint();
					var implementationDiscrepancy = new BusinessObjectCheckpoint();
					var hasDiscrepancy = false;

					CompareCheckpoints($"Checkpoint", "SecurityCheckpoint", item.Value.Module, item.Value, implementationCheckpoints, testDiscrepancy, implementationDiscrepancy, ref hasDiscrepancy);
					CompareCheckpoints($"Delete", "CheckPointForDelete", item.Value.Controller, item.Value, implementationCheckpoints, testDiscrepancy, implementationDiscrepancy, ref hasDiscrepancy);
					CompareCheckpoints($"Edit", "CheckPointForEdit", item.Value.Controller, item.Value, implementationCheckpoints, testDiscrepancy, implementationDiscrepancy, ref hasDiscrepancy);
					CompareCheckpoints($"New", "CheckPointForNew", item.Value.Controller, item.Value, implementationCheckpoints, testDiscrepancy, implementationDiscrepancy, ref hasDiscrepancy);
					CompareCheckpoints($"View", "CheckPointForView", item.Value.Controller, item.Value, implementationCheckpoints, testDiscrepancy, implementationDiscrepancy, ref hasDiscrepancy);

					if (hasDiscrepancy)
					{
						testDiscrepancies[item.Key] = testDiscrepancy;
						implementationDiscrepancies[item.Key] = implementationDiscrepancy;
					}
				}
			}

			AssertMultilineASCIIEquals($"Mismatch found. Please run 'BusinessObjectSecurityCheckpointExtractor' to extract the latest checkpoint file.", Utility.DictionaryToString(testDiscrepancies), Utility.DictionaryToString(implementationDiscrepancies));
		}

		void CompareCheckpoints(string propertyName, string checkpointPropertyName, string controllerName, BusinessObjectCheckpoint testCheckpoint,
			BusinessObjectCheckpoint implementationCheckpoint, BusinessObjectCheckpoint testDiscrepancy, BusinessObjectCheckpoint implementationDiscrepancy, ref bool hasDiscrepancy)
		{
			var implementationCheckpointValue = GetPropertyValue(implementationCheckpoint, propertyName) ?? "";
			var testCheckpointValue = GetPropertyValue(testCheckpoint, propertyName) ?? "";

			if (implementationCheckpointValue != null)
			{
				implementationCheckpointValue = implementationCheckpointValue.Replace("m:", "").Replace("c:", "");
			}

			var shouldFlagDiscrepancy = false;

			if (!Equals(implementationCheckpointValue, testCheckpointValue))
			{
				if (!string.IsNullOrEmpty(implementationCheckpointValue) && !string.IsNullOrEmpty(testCheckpointValue))
				{
					var implementationParts = implementationCheckpointValue.Split('.');
					var testParts = testCheckpointValue.Split('.');
					var checkpointType = Type.GetType("Enterprise.Security.SecurityCore, Enterprise.Security.Core").GetField(implementationParts[implementationParts.Length - 1], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (checkpointType is null)
					{
						var value = GetCheckpointCode(implementationCheckpointValue);
						if (value is null || value != testParts[testParts.Length - 1])
						{
							shouldFlagDiscrepancy = true;
						}
					}
					else
					{
						var retrievedCheckpoint = GetControllerSecurityCheckpoint(controllerName, checkpointPropertyName);
						if (retrievedCheckpoint is not null)
						{
							if (retrievedCheckpoint.Code != testParts[testParts.Length - 1])
							{
								shouldFlagDiscrepancy = true;
							}
						}
					}
				}
				else
				{
					shouldFlagDiscrepancy = true;
				}
			}

			if (shouldFlagDiscrepancy)
			{
				SetPropertyValue(testDiscrepancy, propertyName, testCheckpointValue);
				SetPropertyValue(implementationDiscrepancy, propertyName, implementationCheckpointValue);
				hasDiscrepancy = true;
			}

			string GetPropertyValue(BusinessObjectCheckpoint obj, string propName)
			{
				return (string)typeof(BusinessObjectCheckpoint).GetProperty(propName).GetValue(obj);
			}

			void SetPropertyValue(BusinessObjectCheckpoint obj, string propName, string value)
			{
				typeof(BusinessObjectCheckpoint).GetProperty(propName).SetValue(obj, value);
			}

			string GetCheckpointCode(string implementationCheckpointValue)
			{
				var lastDotIndex = implementationCheckpointValue.LastIndexOf('.');
				var assemblyName = lastDotIndex >= 0 ? implementationCheckpointValue.Substring(0, lastDotIndex) : implementationCheckpointValue;

				var type = Type.GetType($"{implementationCheckpointValue}, {assemblyName}");

				if (type is null)
				{
					return null;
				}

				return GetInstancePropertyValue(type, "Code").ToString();
			}
		}

		Dictionary<string, BusinessObjectCheckpoint> GetBusinessObjectCheckpointsThroughControllers()
		{
			var data = new Dictionary<string, BusinessObjectCheckpoint>();
			var controllerInfoList = new ControllerList().All.Select(c => (c.ID, c.CountryCodeForTest));
			foreach (var controllerInfo in controllerInfoList)
			{
				if (!TryGetControllerInfo(controllerInfo.ID, out var controller, out var moduleId, out var typeOfTopLevelBusinessObject))
				{
					continue;
				}

				var bizObj = TryCreateBusinessObject(typeOfTopLevelBusinessObject);
				if (bizObj is null)
				{
					continue;
				}

				var moduleCheckpoint = GetModuleSecurityCheckpoint(moduleId, controllerInfo.CountryCodeForTest, out var moduleType);
				if (string.IsNullOrEmpty(moduleType))
				{
					continue;
				}

				var controllerFullName = $"{controller.GetType().FullName}, {controller.GetType().Module.Name.Replace(".dll", "")}";
				var businessObjectCheckpoint = GetBusinessObjectCheckpoint(controller, moduleCheckpoint, bizObj, controllerFullName);

				var parts = typeOfTopLevelBusinessObject.AssemblyQualifiedName.Split(',');
				if (parts is not null)
				{
					var key = string.Join(", ", parts.Take(2).Select(p => p.Trim()));
					if (data.TryGetValue(key, out var value))
					{
						CheckpointExtractor.MergeCheckpoints(value, businessObjectCheckpoint);
					}
					else
					{
						businessObjectCheckpoint.Controller = controllerFullName;
						businessObjectCheckpoint.Module = moduleType;
						data[key] = businessObjectCheckpoint;
					}
				}
			}

			return data;
		}

		BusinessObject TryCreateBusinessObject(Type bizObjType)
		{
			try
			{
				if (bizObjType.FullName == "Enterprise.Accounting.Business.JobInvoicing.Job")
				{
					var parent = (IJobHeaderParent)Factory.New<IForwardingShipment>();
					var loader = new JobHeader.Loader(Factory, parent);
					return loader.TryCreateWithoutMutexForTestOnly();
				}
				return Factory.New(bizObjType);
			}
			catch
			{
				return null;
			}
		}

		static SecurityCheckpoint GetModuleSecurityCheckpoint(ModuleIdentifier moduleId, string countryCode, out string moduleType)
		{
			moduleType = "";
			if (moduleId is null || (ModuleId)moduleId.ID == ModuleId.NotAssigned)
			{
				return null;
			}

			SecurityCheckpoint checkpoint = null;
			using (var module = ZModuleFactory.Instance.Create(moduleId, countryCode) ?? ZModuleFactory.Instance.Create(moduleId, moduleId.Name.Substring(0, 2)))
			{
				if (module is not null)
				{
					moduleType = $"{module.GetType().FullName}, {module.GetType().Module.Name.Replace(".dll", "")}";
					if (module.SecurityCheckpoint != Env.Security.None)
					{
						checkpoint = module.SecurityCheckpoint;
					}
				}
			}

			return checkpoint;
		}

		static bool TryGetControllerInfo(ControllerID controllerId, out ZController controller, out ModuleIdentifier moduleId, out Type typeOfTopLevelBusinessObject)
		{
			controller = null;
			moduleId = null;
			typeOfTopLevelBusinessObject = null;
			try
			{
				controller = ZControllerFactory.Create(controllerId);
				if (controller is null)
				{
					return false;
				}
				moduleId = controller.ModuleID;
				if (moduleId is null)
				{
					return false;
				}
				typeOfTopLevelBusinessObject = controller.TypeOfTopLevelBusinessObject;
				if (typeOfTopLevelBusinessObject is null)
				{
					return false;
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		static SecurityCheckpoint GetControllerSecurityCheckpoint(string className, string propertyName)
		{
			var type = Type.GetType(className);
			if (type == null)
			{
				return null;
			}

			return GetInstancePropertyValue(type, propertyName) as SecurityCheckpoint;
		}

		static object GetInstancePropertyValue(Type type, string propertyName)
		{
			var propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			if (propInfo == null)
			{
				return null;
			}

			var instance = Activator.CreateInstance(type);

			using (instance as IDisposable)
			{
				return propInfo.GetValue(instance);
			}
		}

		BusinessObjectCheckpoint GetBusinessObjectCheckpoint(ZController controller, SecurityCheckpoint moduleCheckpoint, BusinessObject bizObj, string controllerFullName)
		{
			var instance = new BusinessObjectCheckpoint();

			if (moduleCheckpoint != null)
			{
				instance.Checkpoint = GetCheckpointFullName(moduleCheckpoint);
			}

			instance.Delete = GetCheckpointValue(() => controller.GetCheckPointForDelete(bizObj), controllerFullName, "GetCheckPointForDelete");
			instance.Edit = GetCheckpointValue(() => controller.GetCheckPointForEdit(bizObj), controllerFullName, "GetCheckPointForEdit");
			instance.New = GetCheckpointValue(() => controller.GetCheckPointForNew(bizObj), controllerFullName, "GetCheckPointForNew");
			instance.View = GetCheckpointValue(() => controller.GetCheckPointForView(bizObj), controllerFullName, "GetCheckPointForView");

			return instance;

			string GetCheckpointValue(Func<SecurityCheckpoint> method, string controllerFullName, string methodName)
			{
				if (ControllerHasCheckpointMethod(controllerFullName, methodName))
				{
					return null;
				}
				try
				{
					var checkpoint = method();
					return (checkpoint != null && checkpoint != Env.Security.None) ? GetCheckpointFullName(checkpoint) : null;
				}
				catch
				{
					return null;
				}
			}

			string GetCheckpointFullName(SecurityCheckpoint checkpoint)
			{
				return $"Enterprise.Security.SecurityCore.{checkpoint.Code}";
			}

			bool ControllerHasCheckpointMethod(string className, string methodName)
			{
				var type = Type.GetType(className);
				var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				return method.DeclaringType != typeof(ZController);
			}
		}
#endif
	}
}
