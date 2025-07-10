using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class TestRigRegistryOptionsValidation : ZValidation
	{
		public TestRigRegistryOptionsValidation(TestRigRegistryOptions parent)
			: base(parent)
		{
			this.parent = parent;
		}

		protected void CheckProduct()
		{
			MandatoryValidation.CheckEntered(parent.ProductInfo);
			ListValidation.ErrorIfInvalidCode(parent.ProductInfo);
			ValidateIdentifiersAreNotDuplicated(parent.ProductInfo);
		}

		protected void CheckProductArea()
		{
			MandatoryValidation.CheckEntered(parent.ProductAreaInfo);
			ListValidation.ErrorIfInvalidCode(parent.ProductAreaInfo);
			ValidateIdentifiersAreNotDuplicated(parent.ProductAreaInfo);
		}

		protected void CheckModule()
		{
			ListValidation.ErrorIfInvalidCode(parent.ModuleInfo);
			ValidateIdentifiersAreNotDuplicated(parent.ModuleInfo);
		}

		protected void CheckChangeType()
		{
			ListValidation.ErrorIfInvalidCode(parent.ChangeTypeInfo);
			ValidateIdentifiersAreNotDuplicated(parent.ChangeTypeInfo);
		}

		protected void CheckBackupFile()
		{
			MandatoryValidation.CheckEntered(parent.BackupFileInfo);
			var regex = new Regex(@"\\\\.+\\.+\.bak");

			if (!regex.IsMatch(parent.BackupFile))
			{
				parent.BackupFileInfo.AddError((NoResString)@"Please enter a properly formatted file path to a .bak file (e.g. \\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\TeamFolder\backup.bak).");
			}
		}

		protected void CheckAdditionalOptions()
		{
		}

		void ValidateIdentifiersAreNotDuplicated(ZPropertyInfo info)
		{
			if (!parent.Product.IsEmpty && !parent.ProductArea.IsEmpty)
			{
				var collection = parent.ParentCollections.FirstOrDefault() as TestRigRegistryCollection;
				var matches = collection?.GetOptionsForWorkItemCombination(parent.Product, parent.ProductArea, parent.Module, parent.ChangeType);

				if (matches?.Count() > 1)
				{
					info.AddError((NoResString)"The Product/Product Area/Module/Change Type combination has already been specified. Each combination can only be specified once.");
				}
			}
		}

		public override void ValidateAll()
		{
			ValidateProduct();
			ValidateProductArea();
			ValidateModule();
			ValidateChangeType();
			ValidateBackupFile();
			ValidateAdditionalOptions();
		}

		public override Type AutoValidationType => GetType();

		readonly TestRigRegistryOptions parent;

		public void ValidateProduct()
		{
			ValidateCalculatedProperty(parent.ProductInfo);
		}

		public void ValidateProductArea()
		{
			ValidateCalculatedProperty(parent.ProductAreaInfo);
		}

		public void ValidateModule()
		{
			ValidateCalculatedProperty(parent.ModuleInfo);
		}

		public void ValidateChangeType()
		{
			ValidateCalculatedProperty(parent.ChangeTypeInfo);
		}

		public void ValidateBackupFile()
		{
			ValidateCalculatedProperty(parent.BackupFileInfo);
		}

		public void ValidateAdditionalOptions()
		{
			ValidateCalculatedProperty(parent.AdditionalOptionsInfo);
		}
	}
}

