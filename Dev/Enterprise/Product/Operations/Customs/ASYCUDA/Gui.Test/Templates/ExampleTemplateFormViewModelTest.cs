using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ExampleTemplateFormViewModel))]
	sealed class ExampleTemplateFormViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new ExampleTemplateFormViewModel(header);
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (excludedProperties.Contains(info.Name))
			{
				return;
			}

			base.TestBizObjectField(info);
		}

		static readonly ImmutableHashSet<string> excludedProperties = ImmutableHashSet.Create
			(nameof(ExampleTemplateFormViewModel.Layout1Selected),
			nameof(ExampleTemplateFormViewModel.Layout2Selected),
			nameof(ExampleTemplateFormViewModel.Layout3Selected),
			nameof(ExampleTemplateFormViewModel.Layout4Selected));
	}
}
