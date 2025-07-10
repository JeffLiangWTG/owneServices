using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.CustomerService.Module.Test
{
	public class ModuleCustomerServiceCodeTest : TestCaseWithFactory
	{
		enum ModuleType
		{
			Filter,
			Popup,
			Report,
			Url,
			UnKnown
		}

		static class FailureReasons
		{
			public const string Skip = "[Ignorable]This kind of module cannot submit eRequest";
			public const string NoForm = "[Manual inspection is required]Unit test cannot create module's form";
			public const string CannotCreateBizo = "[Manual inspection is required]Unit test Cannot create the business object what the module needs";
			public const string CannotCreateController = "[Manual inspection is required]Unit test cannot create this module's controller";
			public const string CannotCreateModuleInstance = "[Manual inspection is required]Unit test cannot create this module instance";

			public const string MissingModuleID = "Cannot get ModuleID from Parent Form";
			public const string NoModuleFound = "Cannot find module object with ModuleID";
			public const string ModuleParentSectionMissingCode = "The module's parent section doesn't have CustomerServiceMenuSectionCode";
		}

		readonly ModuleList moduleInfoList = new ModuleList();
		readonly ControllerList controllerInfoList = new ControllerList();

		class ModuleTestResult
		{
			public IMainFormModule Module { get; set; }

			public ModuleType ModuleType { get; set; }

			public string ModuleTypeName { get; set; }

			public string[] AvailableCountries { get; set; }

			public string ParentSectionName => Module?.ParentSection?.Name ?? string.Empty;
			public string Description => Module?.Description ?? string.Empty;
			public string ParentCategory => Module?.ParentSection?.ParentCategory?.Name ?? string.Empty;
			public string CustomerServiceMenuSectionCode => Module?.ParentSection?.CustomerServiceMenuSectionCode ?? string.Empty;
			public bool SectionCodeOverridable { get; set; }

			public string FormTypeFullName { get; set; } = cellDefaultValue;

			public string ModuleIDControllerValue { get; set; } = cellDefaultValue;

			public string SectionCodeControllerValue { get; set; } = cellDefaultValue;

			public bool DataCheckPassed { get; set; }

			public string Comments { get; set; }

			const string cellDefaultValue = "";

			public override string ToString()
			{
				return base.ToString();
			}
		}

		void CheckModuleForm(IncidentApprovalController approvalController, Form form, ref ModuleTestResult result)
		{
			result.FormTypeFullName = form?.GetType()?.FullName ?? string.Empty;
			((IServiceRequestController)approvalController).SetParentForm(form);
			result.ModuleIDControllerValue = approvalController.GetModuleId(form);
			if (string.IsNullOrEmpty(result.ModuleIDControllerValue))
			{
				var zForm = form as IZForm;
				result.Comments = FailureReasons.MissingModuleID;
				if (zForm == null)
				{
					result.Comments += "(This form is not zForm)";
				}
				else if (zForm.ControllerID == null)
				{
					result.Comments += "(Form is missing ControllerID)";
				}

				return;
			}

			result.SectionCodeOverridable = form is ICustomerServiceMenuSectionCodeOverridable;
			result.SectionCodeControllerValue = approvalController.GetCustomerServiceMenuSectionCode(result.ModuleIDControllerValue);
			if (string.IsNullOrEmpty(result.SectionCodeControllerValue) || result.SectionCodeControllerValue == "OTH")
			{
				result.Comments = FailureReasons.ModuleParentSectionMissingCode;
				return;
			}

			result.DataCheckPassed = result.SectionCodeOverridable || (result.SectionCodeControllerValue != "OTH");
		}

		ModuleTestResult CheckModule(IMainFormModule module)
		{
			var approvalController = new IncidentApprovalControllerTestOnly();
			var result = new ModuleTestResult() { Module = module };

			result.AvailableCountries = moduleInfoList.GetCountryOverridesRegistered(module.ModuleID).ToArray();
			using (var moduleInstance = ZModuleFactory.Instance.Create(module.ModuleID, result.AvailableCountries.FirstOrDefault() ?? Core.Constants.CountryCodes.Australia))
			{
				if (moduleInstance == null)
				{
					result.Comments = FailureReasons.CannotCreateModuleInstance;
					return result;
				}

				var referenceName = moduleInstance?.GetType().Assembly.GetName().Name ?? "";
				result.ModuleTypeName = referenceName.Replace("Enterprise.", "").Replace(".Module", "");
				if (module.ParentSection.CustomerServiceMenuSectionCode == "OTH")
				{
					result.Comments = FailureReasons.ModuleParentSectionMissingCode;
					return result;
				}

				var form = CheckModuleTypeAndGetModuleForm(moduleInstance, ref result);
				if (form != null)
				{
					CheckModuleForm(approvalController, form as Form, ref result);
				}

				try
				{
					form?.Dispose();
				}
				catch { }

				return result;
			}
		}

		IZForm CheckModuleTypeAndGetModuleForm(ZModule module, ref ModuleTestResult result)
		{
			switch (module)
			{
				case ZFilterModule moduleObject:
					result.ModuleType = ModuleType.Filter;
					var controller = moduleObject.GetNewController();
					if (controller == null && moduleObject.TypeOfTopLevelBusinessObject != null)
					{
						var tempBizo = new BusinessObjectFactory().NewWithValidTestData(moduleObject.TypeOfTopLevelBusinessObject);
						var method = typeof(ZFilterModule).GetMethod("GetNewController", BindingFlags.NonPublic | BindingFlags.Instance);
						controller = method.Invoke(moduleObject, new[] { tempBizo }) as ZController;
						tempBizo.Delete();
					}

					return GetModuleControllerForm(controller, ref result);

				case ZPopupModule moduleObject:
					result.ModuleType = ModuleType.Popup;
					var popForm = moduleObject.ShowNew(null);
					result.Comments = popForm == null ? FailureReasons.NoForm : string.Empty;
					return popForm;

				case ZReportModule moduleObject:
					result.ModuleType = ModuleType.Report;
					var reportForm = moduleObject.ShowPopup();
					result.Comments = reportForm == null ? FailureReasons.NoForm : string.Empty;
					return reportForm;

				case ZSimpleUrlLauncherModule:
					result.ModuleType = ModuleType.Url;
					result.Comments = FailureReasons.Skip;
					return null;
				case null:
					result.ModuleType = ModuleType.UnKnown;
					return null;
				default:
					result.ModuleType = ModuleType.UnKnown;
					IZForm form = null;
					try
					{
						form = module.ShowPopup();
					}
					catch { }

					var controllerInfo = controllerInfoList.GetRegisteredIdentifierByName(module.ID.ToString());
					if (controllerInfo != null)
					{
						var controllerCountryCode = controllerInfoList.GetCountryOverridesRegistered(controllerInfo);
						if (controllerCountryCode.FirstOrDefault() != null)
						{
							controller = ZControllerFactory.CreateWithCountry(controllerInfo, controllerCountryCode.FirstOrDefault());
							form = GetModuleControllerForm(controller, ref result);
						}
					}

					return form;
			}
		}

		BusinessObject CreateUsableFilterModuleDataSource(ZController controller, bool testClassOnly)
		{
			var methodGetBusinessObjectWithoutValidationErrors = typeof(ZControllerBasherTest).GetMethod("GetBusinessObjectWithoutValidationErrors", BindingFlags.NonPublic | BindingFlags.Instance);
			var testClassControllerField = typeof(ZControllerBasherTest).GetField("Controller", BindingFlags.NonPublic | BindingFlags.Instance);
			var controllerType = controller.GetType();
			var controllerTestClass = controllerType.Name + "Test";
			Assembly assembly;
			try
			{
				assembly = Assembly.LoadFrom($"{Env.ApplicationStartupPath.Trim('\\')}\\{controllerType.Assembly.GetName().Name}.Test.dll");
			}
			catch
			{
				assembly = Assembly.LoadFrom($"{Env.ApplicationStartupPath.Trim('\\')}\\{controllerType.Assembly.GetName().Name}.Testing.dll");
			}

			foreach (var type in assembly?.GetTypes() ?? Array.Empty<Type>())
			{
				if (type.Name == controllerTestClass || (Attribute.IsDefined(type, typeof(TestedTypeAttribute)) && TestedTypeHelper.GetTestedType(type) == controllerType))
				{
					var testClassObj = Activator.CreateInstance(type);
					testClassControllerField.SetValue(testClassObj, controller);
					var parentData = (BusinessObject)methodGetBusinessObjectWithoutValidationErrors.Invoke(testClassObj, Array.Empty<object>());
					parentData.Factory.Save();
					return parentData;
				}
			}

			return null;
		}

		IZForm GetModuleControllerForm(ZController controller, ref ModuleTestResult result)
		{
			if (controller == null)
			{
				result.Comments = FailureReasons.CannotCreateController;
				return null;
			}

			try
			{
				var newForm = controller.ShowNewForm();
				if (newForm != null)
				{
					return newForm;
				}
			}
			catch { }

			BusinessObject moduleData;
			try
			{
				moduleData = CreateUsableFilterModuleDataSource(controller, testClassOnly: false);
				if (moduleData == null)
				{
					result.Comments = FailureReasons.CannotCreateBizo;
					return null;
				}
			}
			catch (Exception ex)
			{
				result.Comments = $"{FailureReasons.CannotCreateBizo}({ex.Message})";
				return null;
			}

			var form = ShowEditOrViewForm(controller, moduleData, ref result);

			try
			{
				moduleData.Delete();
				moduleData.Factory.Save();
			}
			catch { }
			return form;
		}

		IZForm ShowEditOrViewForm(ZController controller, BusinessObject datasource, ref ModuleTestResult result)
		{
			IZForm resultForm = null;
			try
			{
				resultForm = controller.ShowEditForm(datasource);
			}
			catch { }

			if (resultForm == null)
			{
				try
				{
					resultForm = controller.ShowViewForm(datasource);
				}
				catch (Exception ex) { result.Comments = ex.Message; }
			}

			if (resultForm == null)
			{
				result.Comments = $"{FailureReasons.NoForm}({result.Comments})";
			}

			return resultForm;
		}

		[DeveloperOnlyTest]
		public void TestModuleChildForm()
		{
			GlbStaff.CurrentUser.GS_IsController = true;
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var moduleTreeLoader = ObjectFactory.Get<IModuleTreeLoader>();
				var moduleTreeForTest = new ModuleTree();
				using (ModuleTree.OverrideTreeForTest(moduleTreeForTest))
				{
					var invalidModule = "";
					var resultData = new Dictionary<string, ModuleTestResult>();
					var waitingProcessQueue = new Dictionary<string, bool>();
					moduleTreeLoader.Initialise(moduleTreeForTest, Env.Security);
					moduleTreeLoader.LoadModules();

					foreach (var category in ModuleTree.Tree.Categories.ValuesIncludingHidden.Where(x => x.DisplayText != "Jump"))
					{
						foreach (var section in category.Sections.ValuesIncludingHidden)
						{
							foreach (var currentModule in section.Modules.ValuesIncludingHidden)
							{
								try
								{
									var moduleCountryCode = moduleInfoList.GetCountryOverridesRegistered(currentModule.ModuleID).FirstOrDefault(x => !string.IsNullOrEmpty(x));
									if (moduleCountryCode != null)
									{
										GlbCompany.CurrentCompany.SetCountry(moduleCountryCode);
									}
									else
									{
										GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
									}

									var result = CheckModule(currentModule);
									if (resultData.ContainsKey(currentModule.ID))
									{
										var counter = 0;
										while (resultData.ContainsKey($"{currentModule.ID}({counter})"))
										{
											counter++;
										}

										resultData.Add($"{currentModule.ID}({counter})", result);
									}
									else
									{
										resultData.Add(currentModule.ID, result);
									}
								}
								catch (Exception ex)
								{
									invalidModule += $";{currentModule.ID}({ex.Message})";
								}
							}
						}
					}

					ExceptionReporterTestListener.Instance.Clear();
					GenerateReport(invalidModule, resultData);
				}
			}
		}

		void GenerateReport(string invalidModules, Dictionary<string, ModuleTestResult> dataSet)
		{
			var result = new StringBuilder();
			result.AppendLine(invalidModules);
			result.AppendLine("Module Type$Module Assembly$From$Module ID$Customer Service Menu Section Code$Module ID(IncidentApprovalController.GetModuleID)$Section Code(IncidentApprovalController.GetCustomerServiceMenuSectionCode)$Comment$Passed?");
			foreach (var line in dataSet)
			{
				var moduleData = line.Value;
				var sectionCode = (moduleData.SectionCodeOverridable ? "[Overrode]" : "") + $"{moduleData.SectionCodeControllerValue}";
				result.AppendLine($"{moduleData.ModuleType}${moduleData.ModuleTypeName}${moduleData.FormTypeFullName}${line.Key}${moduleData.CustomerServiceMenuSectionCode}${moduleData.ModuleIDControllerValue}${sectionCode}${moduleData.Comments}${moduleData.DataCheckPassed}");
			}
			Assert(result.ToString(), false);
		}

		class IncidentApprovalControllerTestOnly : IncidentApprovalController
		{
			protected override bool IncludeHiddenModule => true;

			protected override ModuleTree GetModuleTree()
			{
				return ModuleTree.Tree;
			}
		}
	}
}
