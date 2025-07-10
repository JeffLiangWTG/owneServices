using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class SourceModulesControl : RegistryZUserControl
	{
		public SourceModulesControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetCleanupButtonVisibility();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.SourceModulesGrid.ReadOnly = readOnly;
			this.AddCurrentButton.ReadOnly = readOnly;
			this.AddClientSpecificButton.ReadOnly = readOnly;
			this.CleanUpButton.ReadOnly = readOnly;
		}

		void SetCleanupButtonVisibility()
		{
			var sourceModuleCollection = DataSource as SourceModuleCollection;
			if (sourceModuleCollection != null)
			{
				this.CleanUpButton.Visible = !sourceModuleCollection.Cast<SourceModule>().Any(s => s.ModuleListType == ModuleListType.DetectedMenuItem);
			}
		}

		void AddCurrentButton_Click(object sender, EventArgs e)
		{
			var result = Sweep(ModuleTree.Tree);

			PromptAddSourceModules("Add Current Menu Items", result.SourceModulesToAdd,
				result.SourceModulesToUpdate);
		}

		void AddClientSpecificButton_Click(object sender, EventArgs e)
		{
			var sourceModuleCollection = DataSource as SourceModuleCollection;
			if (sourceModuleCollection != null)
			{
				var folderBrowserDialog = new ZFolderBrowserDialog();
				folderBrowserDialog.SelectedPath = Path.GetDirectoryName(GetType().Assembly.Location);
				folderBrowserDialog.Description = "Select Folder containing ZClient dlls";
				var dialogResult = folderBrowserDialog.ShowDialog();
				if (dialogResult == DialogResult.OK)
				{
					var sourceModulesToAdd = new SourceModuleCollection();
					var sourceModulesToUpdate = new SourceModuleCollection();
					var moduleTreeLoader = CargoWise.Application.ObjectFactory.Get<IModuleTreeLoader>();
					var clientFiles = Directory.GetFiles(folderBrowserDialog.MappedSelectedPath, "ZClient???.dll");
					foreach (var clientSpecificAssemblyFileName in clientFiles)
					{
						ClientHook clientHook;
						Assembly clientSpecificAssembly = ClientHookLoader.Instance.GetAssemblyFromFileName(clientSpecificAssemblyFileName);

						try
						{
							clientHook = ClientHookLoader.Instance.GetClientHookFromAssembly(clientSpecificAssembly);
						}
						catch (TypeLoadException)
						{
							continue;
						}

						var moduleTree = new ModuleTree();
						moduleTreeLoader.Initialise(moduleTree, Enterprise.Environment.Env.Security, clientHook);
						moduleTreeLoader.LoadModules();

						var result = Sweep(moduleTree);

						sourceModulesToAdd.AddRange(result.SourceModulesToAdd);
						sourceModulesToUpdate.AddRange(result.SourceModulesToUpdate);
					}

					PromptAddSourceModules("Add Client Specific Menu Items", sourceModulesToAdd, sourceModulesToUpdate);
				}
			}
		}

		(SourceModuleCollection SourceModulesToAdd, SourceModuleCollection SourceModulesToUpdate) Sweep(ModuleTree tree)
		{
			var sourceModulesToAdd = new SourceModuleCollection();
			var sourceModulesToUpdate = new SourceModuleCollection();

			if (DataSource is SourceModuleCollection sourceModuleCollection)
			{
				foreach (var category in tree.Categories.ValuesIncludingHidden.Where(c => !c.Name.Equals("Jump")))
				{
					foreach (var section in category.Sections.ValuesIncludingHidden)
					{
						foreach (var module in section.Modules.ValuesIncludingHidden)
						{
							var existingSourceModule = sourceModuleCollection.GetSourceModule(module.ModuleTreeID,
								ModuleListType.DetectedMenuItem, "ENT");

							if (existingSourceModule == null)
							{
								sourceModulesToAdd.AddNew(module, true, true, ProductTypes.Codes.Enterprise);
							}
							else
							{
								if (!existingSourceModule.Description.Equals(module.Description.GetUnresolvedValue()) || !existingSourceModule.Path.EqualsIgnoringCase(SourceModule.GetPath(module)))
								{
									sourceModulesToUpdate.AddNew(module, true, true, ProductTypes.Codes.Enterprise);
								}
							}
						}
					}
				}
			}

			return (sourceModulesToAdd, sourceModulesToUpdate);
		}

		void PromptAddSourceModules(string caption, SourceModuleCollection sourceModulesToAdd, SourceModuleCollection sourceModulesToUpdate)
		{
			var dataSource = (SourceModuleCollection)DataSource;
			if (sourceModulesToAdd.Any() || sourceModulesToUpdate.Any())
			{
				var messageBuilder = new StringBuilder();
				if (sourceModulesToAdd.Any())
				{
					messageBuilder.AppendLine($"[{sourceModulesToAdd.Count}] Menu Item(s) have been found that are are currently not in this list and will be added.");
				}

				if (sourceModulesToUpdate.Any())
				{
					messageBuilder.AppendLine($"[{sourceModulesToUpdate.Count}] Menu Item(s) have a different Description or Module Tree Path and will be updated.");
				}

				var dialogResult = Globals.Message.Show(
						messageBuilder.ToString(),
						caption,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);

				if (dialogResult == DialogResult.Yes)
				{
					foreach (var sourceModule in sourceModulesToAdd)
					{
						dataSource.Add(sourceModule);
					}

					foreach (var sourceModule in sourceModulesToUpdate)
					{
						var record = sourceModule as SourceModule;
						if (record != null)
						{
							var existingRecord = dataSource.GetSourceModule(record.Code, record.Product);
							if (existingRecord != null)
							{
								existingRecord.Description = record.Description;
								existingRecord.Path = record.Path;
							}
						}
					}
				}
			}
			else
			{
				Globals.Message.Show("List already contains all Menu Items.", caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		protected void CleanUpButton_Click(object sender, EventArgs e)
		{
			var sourceModuleCollection = DataSource as SourceModuleCollection;
			if (sourceModuleCollection != null)
			{
				var missingSourceModules = new SourceModuleCollection();
				var moduleTreeLoader = CargoWise.Application.ObjectFactory.Get<IModuleTreeLoader>();

				var moduleTree = new ModuleTree();
				moduleTreeLoader.Initialise(moduleTree, Environment.Env.Security);
				moduleTreeLoader.LoadModules();

				var duplicatedSourceModules = new List<SourceModule>();

				foreach (var category in moduleTree.Categories.ValuesIncludingHidden)
				{
					foreach (var section in category.Sections.ValuesIncludingHidden)
					{
						foreach (var module in section.Modules.ValuesIncludingHidden)
						{
							var sourceModule = sourceModuleCollection.GetSourceModule(module);
							if (sourceModule != null)
							{
								sourceModule.ModuleListType = ModuleListType.DetectedMenuItem;

								if (!sourceModule.ModuleListTypeDescriptions.ContainsOnly(nameof(ModuleListType.DetectedMenuItem)))
								{
									sourceModule.ModuleListTypeDescriptions.AddPair(EnumExtensions.GetCaption(DetectedType), DetectedType.ToString());
								}

								var duplicatedList = sourceModuleCollection.Where(s => s.Code == sourceModule.Code && s.ModuleListType != ModuleListType.DetectedMenuItem);
								if (duplicatedList.Any())
								{
									duplicatedSourceModules.AddRange(duplicatedList);
								}
							}
						}
					}
				}

				if (duplicatedSourceModules.Count > 0)
				{
					sourceModuleCollection.RemoveRange(duplicatedSourceModules);
				}

				SetCleanupButtonVisibility();
			}
		}

		ModuleListType DetectedType
		{
			get
			{
				if (detectedType != ModuleListType.DetectedMenuItem)
				{
					detectedType = ((ModuleListType[])Enum.GetValues(typeof(ModuleListType))).First(m => m == ModuleListType.DetectedMenuItem);
				}
				return detectedType;
			}
		}
		ModuleListType detectedType;
	}
}
